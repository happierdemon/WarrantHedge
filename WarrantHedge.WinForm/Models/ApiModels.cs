using System;

namespace WarrantHedge.WinForm.Models
{
    // 注意：.NET Framework 4.8 預設 C# 7.3，不支援 record，所以用一般 class。
    // 屬性名稱與 API 回傳的 JSON 對應（Newtonsoft.Json 比對時不分大小寫）。

    /// <summary>權證資料</summary>
    public class WarrantDto
    {
        public string WarrantId { get; set; }
        public decimal StrikePrice { get; set; }
        public decimal ConversionRatio { get; set; }
        public string WarrantType { get; set; }
        public int PositionQty { get; set; }

        /// <summary>左側清單顯示用文字（唯讀屬性，反序列化時會被略過）</summary>
        public string DisplayText
        {
            get { return WarrantId + "  " + WarrantType; }
        }
    }

    /// <summary>試算 / 存檔的請求內容：只傳股價，計算一律由 API 負責</summary>
    public class CalculateRequest
    {
        public decimal MarketPrice { get; set; }
    }

    /// <summary>試算結果</summary>
    public class CalculationResultDto
    {
        public string WarrantId { get; set; }
        public string WarrantType { get; set; }
        public decimal MarketPrice { get; set; }
        public decimal StrikePrice { get; set; }
        public decimal TheoryPrice { get; set; }

        /// <summary>ITM / ATM / OTM（API 以字串回傳 enum）</summary>
        public string Moneyness { get; set; }

        public decimal Delta { get; set; }
        public decimal HedgeQty { get; set; }

        /// <summary>BUY / SELL</summary>
        public string HedgeDirection { get; set; }
    }

    /// <summary>試算紀錄（歷史明細區）</summary>
    public class TrialLogDto
    {
        public int LogId { get; set; }
        public string WarrantId { get; set; }
        public decimal MarketPrice { get; set; }
        public decimal TheoryPrice { get; set; }
        public decimal HedgeQty { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    /// <summary>存檔結果：IsReplay = true 代表同一個 Idempotency-Key 已存過</summary>
    public class SaveTrialResult
    {
        public TrialLogDto Log { get; set; }
        public bool IsReplay { get; set; }
    }

    /// <summary>API 錯誤回傳格式（RFC 9457 ProblemDetails）</summary>
    public class ProblemDetailsDto
    {
        public string Title { get; set; }
        public string Detail { get; set; }
        public int? Status { get; set; }
    }
}
