/* =========================================================
   權證發行風險監控與避險試算系統 - 建表 Script
   執行順序：01_schema.sql → 02_seed.sql
   ========================================================= */
IF DB_ID(N'WarrantHedgeDB') IS NULL
    CREATE DATABASE WarrantHedgeDB;
GO
USE WarrantHedgeDB;
GO

-- 可重複執行：先刪子表再刪主表
IF OBJECT_ID(N'dbo.Warrant_Trial_Log', N'U') IS NOT NULL DROP TABLE dbo.Warrant_Trial_Log;
IF OBJECT_ID(N'dbo.Warrant_Master',    N'U') IS NOT NULL DROP TABLE dbo.Warrant_Master;
GO

/* ---------- A. 權證基本檔 ---------- */
CREATE TABLE dbo.Warrant_Master (
    Warrant_ID       VARCHAR(10)   NOT NULL,
    Strike_Price     DECIMAL(18,4) NOT NULL,
    Conversion_Ratio DECIMAL(18,4) NOT NULL,
    Warrant_Type     VARCHAR(4)    NOT NULL,
    Position_Qty     INT           NOT NULL,
    CONSTRAINT PK_Warrant_Master        PRIMARY KEY CLUSTERED (Warrant_ID),
    CONSTRAINT CK_Warrant_Master_Type   CHECK (Warrant_Type IN ('CALL', 'PUT')),
    CONSTRAINT CK_Warrant_Master_Strike CHECK (Strike_Price > 0),
    CONSTRAINT CK_Warrant_Master_Ratio  CHECK (Conversion_Ratio > 0),
    CONSTRAINT CK_Warrant_Master_Qty    CHECK (Position_Qty >= 0)
);
GO

/* ---------- B. 試算日誌表 ---------- */
CREATE TABLE dbo.Warrant_Trial_Log (
    Log_ID       INT IDENTITY(1,1) NOT NULL,
    Warrant_ID   VARCHAR(10)       NOT NULL,
    Market_Price DECIMAL(18,4)     NOT NULL,
    Theory_Price DECIMAL(18,4)     NOT NULL,
    Hedge_Qty    DECIMAL(18,2)     NOT NULL,
    Created_Time DATETIME          NOT NULL CONSTRAINT DF_Trial_Log_Created DEFAULT (GETDATE()),
    Request_ID   UNIQUEIDENTIFIER  NOT NULL,  -- 擴充欄位：API 冪等鍵 (Idempotency-Key)
    CONSTRAINT PK_Warrant_Trial_Log     PRIMARY KEY CLUSTERED (Log_ID),
    CONSTRAINT FK_Trial_Log_Warrant     FOREIGN KEY (Warrant_ID) REFERENCES dbo.Warrant_Master (Warrant_ID),
    CONSTRAINT CK_Trial_Log_MarketPrice CHECK (Market_Price > 0)  -- DB 層最後防線：股價 <= 0 禁止存檔
);
GO

-- 冪等性：同一個 Request_ID 只能寫入一次（重複點擊 / 網路重送不會產生重複資料）
CREATE UNIQUE NONCLUSTERED INDEX UX_Trial_Log_Request_ID
    ON dbo.Warrant_Trial_Log (Request_ID);

-- 歷史明細區「該權證最近 10 筆」查詢用：Index Seek + 免回表 (Covering Index)
CREATE NONCLUSTERED INDEX IX_Trial_Log_Warrant_Time
    ON dbo.Warrant_Trial_Log (Warrant_ID, Created_Time DESC)
    INCLUDE (Market_Price, Theory_Price, Hedge_Qty);
GO