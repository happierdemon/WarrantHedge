namespace WarrantHedge.API.Models.Entities;

/// <summary>權證基本檔 (dbo.Warrant_Master)</summary>
public class WarrantMaster
{
    public string WarrantId { get; set; } = null!;

    /// <summary>履約價格</summary>
    public decimal StrikePrice { get; set; }

    /// <summary>行使比例（例：0.1 代表 10 換 1）</summary>
    public decimal ConversionRatio { get; set; }

    /// <summary>類型：CALL / PUT</summary>
    public string WarrantType { get; set; } = null!;

    /// <summary>庫存張數</summary>
    public int PositionQty { get; set; }

    public ICollection<WarrantTrialLog> TrialLogs { get; set; } = new List<WarrantTrialLog>();
}
