namespace WarrantHedge.API.Services;

public enum Moneyness
{
    /// <summary>價內</summary>
    ITM,
    /// <summary>價平</summary>
    ATM,
    /// <summary>價外</summary>
    OTM
}

public enum HedgeDirection
{
    /// <summary>買入現股</summary>
    BUY,
    /// <summary>賣出現股</summary>
    SELL
}

public sealed record HedgeResult(
    decimal TheoryPrice,
    Moneyness Moneyness,
    decimal Delta,
    decimal HedgeQty,
    HedgeDirection Direction);

/// <summary>
/// 權證理論價值與 Delta 避險張數計算。
/// 設計重點：
/// 1. 純函式（無 DB、無狀態），方便單元測試。
/// 2. 全程使用 decimal，不使用 double，避免二進位浮點誤差。
/// 3. 捨入一律 MidpointRounding.AwayFromZero（四捨五入），
///    與 SQL Server ROUND() 行為一致；C# 預設的 Math.Round 是銀行家捨入 (ToEven)。
/// </summary>
public static class HedgeCalculator
{
    public const decimal DeltaItm = 0.8m;
    public const decimal DeltaAtm = 0.5m;
    public const decimal DeltaOtm = 0.2m;

    /// <summary>理論價值小數位數，對應 DECIMAL(18,4)</summary>
    public const int TheoryPriceScale = 4;

    /// <summary>避險張數小數位數，對應 DECIMAL(18,2)</summary>
    public const int HedgeQtyScale = 2;

    public static HedgeResult Calculate(
        string warrantType,
        decimal strikePrice,
        decimal conversionRatio,
        int positionQty,
        decimal marketPrice)
    {
        // 防禦性檢查：Service 層已先擋過，這裡再擋一次，確保計算函式被單獨呼叫時也安全
        if (marketPrice <= 0)
            throw new ArgumentOutOfRangeException(nameof(marketPrice), "標的股價必須大於 0。");

        bool isCall = warrantType switch
        {
            "CALL" => true,
            "PUT" => false,
            _ => throw new ArgumentException($"不支援的權證類型：{warrantType}", nameof(warrantType))
        };

        // 內含價值方向：CALL = 標的價 − 履約價；PUT = 履約價 − 標的價
        // > 0 即價內、= 0 即價平、< 0 即價外，CALL / PUT 共用同一套判斷
        decimal intrinsic = isCall ? marketPrice - strikePrice : strikePrice - marketPrice;

        decimal theoryPrice = Math.Round(
            Math.Max(0m, intrinsic) * conversionRatio,
            TheoryPriceScale,
            MidpointRounding.AwayFromZero);

        // decimal 比較是精確比較：100.0000m == 100m 為 true
        Moneyness moneyness = intrinsic > 0 ? Moneyness.ITM
                            : intrinsic == 0 ? Moneyness.ATM
                            : Moneyness.OTM;

        decimal delta = moneyness switch
        {
            Moneyness.ITM => DeltaItm,
            Moneyness.ATM => DeltaAtm,
            _ => DeltaOtm
        };

        decimal hedgeQty = Math.Round(
            positionQty * conversionRatio * delta,
            HedgeQtyScale,
            MidpointRounding.AwayFromZero);

        // 假設：發行商為權證賣方（對投資人為空方部位）。
        // 賣出認購 → 標的上漲有風險 → 買入現股避險；
        // 賣出認售 → 標的下跌有風險 → 賣出（放空）現股避險。
        HedgeDirection direction = isCall ? HedgeDirection.BUY : HedgeDirection.SELL;

        return new HedgeResult(theoryPrice, moneyness, delta, hedgeQty, direction);
    }
}
