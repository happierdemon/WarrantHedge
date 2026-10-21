/* =========================================================
   權證發行風險監控與避險試算系統 - 測試資料 Script
   - 800 筆權證（400 CALL + 400 PUT），以 Set-based 一次產生，不使用迴圈
   - 每檔權證 3 筆試算紀錄（價外 / 價平 / 價內各一），共 2,400 筆
   - 數值以固定公式產生（非亂數），每次執行結果一致，方便驗證
   ========================================================= */
USE WarrantHedgeDB;
GO
SET NOCOUNT ON;

-- 測試資料的時間基準：台灣時間（與 01_schema.sql 的 Created_Time 預設值一致，
-- Azure SQL Database 的 GETDATE() 是 UTC，不能直接用）
DECLARE @Now DATETIME = CONVERT(DATETIME, SYSDATETIMEOFFSET() AT TIME ZONE 'Taipei Standard Time');

DELETE FROM dbo.Warrant_Trial_Log;
DELETE FROM dbo.Warrant_Master;

/* ---------- 1. 權證基本檔：800 筆 ---------- */
;WITH Nums AS (
    SELECT TOP (400) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT INTO dbo.Warrant_Master (Warrant_ID, Strike_Price, Conversion_Ratio, Warrant_Type, Position_Qty)
-- 認購權證：030001 ~ 030400
SELECT
    '03' + RIGHT('0000' + CAST(n AS VARCHAR(4)), 4),
    CAST(20 + (n * 37) % 980 AS DECIMAL(18,4)) + (n % 4) * 0.25,
    CAST(CHOOSE(n % 5 + 1, 0.05, 0.1, 0.2, 0.5, 1.0) AS DECIMAL(18,4)),
    'CALL',
    500 + (n * 137) % 9500
FROM Nums
UNION ALL
-- 認售權證：03001P ~ 03400P
SELECT
    '03' + RIGHT('000' + CAST(n AS VARCHAR(3)), 3) + 'P',
    CAST(20 + (n * 53) % 980 AS DECIMAL(18,4)) + (n % 4) * 0.25,
    CAST(CHOOSE(n % 5 + 1, 0.05, 0.1, 0.2, 0.5, 1.0) AS DECIMAL(18,4)),
    'PUT',
    500 + (n * 211) % 9500
FROM Nums;

/* ---------- 2. 試算日誌：每檔 3 筆（股價 = 履約價 × 0.9 / 1.0 / 1.1）----------
   計算規則與 C# API 相同；SQL ROUND 為「四捨五入 (遠離零)」，
   C# 端需對應使用 MidpointRounding.AwayFromZero，兩邊結果才會一致。 */
INSERT INTO dbo.Warrant_Trial_Log (Warrant_ID, Market_Price, Theory_Price, Hedge_Qty, Created_Time, Request_ID)
SELECT
    w.Warrant_ID,
    p.Market_Price,
    ROUND(
        CASE w.Warrant_Type
            WHEN 'CALL' THEN IIF(p.Market_Price > w.Strike_Price, p.Market_Price - w.Strike_Price, 0)
            ELSE             IIF(w.Strike_Price > p.Market_Price, w.Strike_Price - p.Market_Price, 0)
        END * w.Conversion_Ratio, 4),
    ROUND(w.Position_Qty * w.Conversion_Ratio * d.Delta, 2),
    DATEADD(MINUTE, -15 * s.Seq, @Now),
    NEWID()
FROM dbo.Warrant_Master AS w
CROSS JOIN (VALUES (1, 0.90), (2, 1.00), (3, 1.10)) AS s (Seq, Factor)
CROSS APPLY (SELECT CAST(ROUND(w.Strike_Price * s.Factor, 4) AS DECIMAL(18,4)) AS Market_Price) AS p
CROSS APPLY (
    SELECT CAST(
        CASE
            WHEN p.Market_Price = w.Strike_Price THEN 0.5                         -- 價平 ATM
            WHEN (w.Warrant_Type = 'CALL' AND p.Market_Price > w.Strike_Price)
              OR (w.Warrant_Type = 'PUT'  AND p.Market_Price < w.Strike_Price) THEN 0.8  -- 價內 ITM
            ELSE 0.2                                                               -- 價外 OTM
        END AS DECIMAL(3,1)) AS Delta
) AS d;

/* ---------- 3. 驗證 ---------- */
SELECT Warrant_Type, COUNT(*) AS Cnt FROM dbo.Warrant_Master GROUP BY Warrant_Type;  -- 預期 CALL 400 / PUT 400
SELECT COUNT(*) AS LogCnt FROM dbo.Warrant_Trial_Log;                                -- 預期 2400

-- 歷史明細區查詢範例（走 IX_Trial_Log_Warrant_Time）
SELECT TOP (10) Log_ID, Market_Price, Theory_Price, Hedge_Qty, Created_Time
FROM dbo.Warrant_Trial_Log
WHERE Warrant_ID = '030001'
ORDER BY Created_Time DESC, Log_ID DESC;
GO