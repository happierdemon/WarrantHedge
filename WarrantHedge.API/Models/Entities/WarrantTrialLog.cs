namespace WarrantHedge.API.Models.Entities;

/// <summary>試算日誌表 (dbo.Warrant_Trial_Log)</summary>
public class WarrantTrialLog
{
    public int LogId { get; set; }

    public string WarrantId { get; set; } = null!;

    /// <summary>試算時輸入的標的股價</summary>
    public decimal MarketPrice { get; set; }

    /// <summary>理論價值</summary>
    public decimal TheoryPrice { get; set; }

    /// <summary>建議避險張數</summary>
    public decimal HedgeQty { get; set; }

    /// <summary>存檔時間（由 DB 預設值 GETDATE() 產生）</summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>冪等鍵：同一個 Request_ID 只能寫入一次</summary>
    public Guid RequestId { get; set; }

    public WarrantMaster Warrant { get; set; } = null!;
}
