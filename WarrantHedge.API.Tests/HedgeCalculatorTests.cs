using WarrantHedge.API.Services;
using Xunit;

namespace WarrantHedge.API.Tests;

/// <summary>
/// HedgeCalculator 單元測試。
/// 每個測試都依照 AAA 結構：Arrange（準備資料）→ Act（執行）→ Assert（驗證）。
/// 注意：C# Attribute 參數不能是 decimal，所以用 TheoryData 傳入 decimal 測資，
/// 而不是 [InlineData]（InlineData 只能用 double，會把精度問題帶進測試本身）。
/// </summary>
public class HedgeCalculatorTests
{
    // ============================================================
    // 1. 價內 / 價平 / 價外：CALL 與 PUT 各三種情境
    //    共用條件：履約價 100、行使比例 0.1、庫存 1000 張
    //    理論價值 = MAX(0, 內含價值) × 0.1
    //    避險張數 = 1000 × 0.1 × Delta
    // ============================================================
    public static TheoryData<string, decimal, decimal, Moneyness, decimal, decimal, HedgeDirection> MoneynessCases => new()
    {
        // 類型,  股價,   理論價值, 價性,           Delta, 避險張數, 方向
        { "CALL", 120m, 2.0000m, Moneyness.ITM, 0.8m, 80.00m, HedgeDirection.BUY  }, // (120-100)×0.1 = 2
        { "CALL", 100m, 0.0000m, Moneyness.ATM, 0.5m, 50.00m, HedgeDirection.BUY  },
        { "CALL",  80m, 0.0000m, Moneyness.OTM, 0.2m, 20.00m, HedgeDirection.BUY  }, // 內含價值為負 → 取 0
        { "PUT",   80m, 2.0000m, Moneyness.ITM, 0.8m, 80.00m, HedgeDirection.SELL }, // (100-80)×0.1 = 2
        { "PUT",  100m, 0.0000m, Moneyness.ATM, 0.5m, 50.00m, HedgeDirection.SELL },
        { "PUT",  120m, 0.0000m, Moneyness.OTM, 0.2m, 20.00m, HedgeDirection.SELL },
    };

    [Theory]
    [MemberData(nameof(MoneynessCases))]
    public void 依價性計算理論價值與避險張數(
        string type, decimal marketPrice,
        decimal expectedTheory, Moneyness expectedMoneyness, decimal expectedDelta,
        decimal expectedHedge, HedgeDirection expectedDirection)
    {
        // Act
        var r = HedgeCalculator.Calculate(type, strikePrice: 100m, conversionRatio: 0.1m, positionQty: 1000, marketPrice);

        // Assert
        Assert.Equal(expectedTheory, r.TheoryPrice);
        Assert.Equal(expectedMoneyness, r.Moneyness);
        Assert.Equal(expectedDelta, r.Delta);
        Assert.Equal(expectedHedge, r.HedgeQty);
        Assert.Equal(expectedDirection, r.Direction);
    }

    // ============================================================
    // 2. 價平判斷：decimal 是精確比較
    // ============================================================
    [Fact]
    public void 小數位數不同但數值相同_應判定為價平()
    {
        // 100.0000m 與 100m 的 Scale 不同（4 vs 0），但數值相等 → 價平
        var r = HedgeCalculator.Calculate("CALL", strikePrice: 100m, conversionRatio: 0.1m, positionQty: 1000, marketPrice: 100.0000m);

        Assert.Equal(Moneyness.ATM, r.Moneyness);
        Assert.Equal(0.5m, r.Delta);
    }

    [Fact]
    public void 股價只差萬分之一_應判定為價內而非價平()
    {
        // 不使用「誤差容忍值」模糊比較：差 0.0001 就是價內
        var r = HedgeCalculator.Calculate("CALL", strikePrice: 100m, conversionRatio: 0.1m, positionQty: 1000, marketPrice: 100.0001m);

        Assert.Equal(Moneyness.ITM, r.Moneyness);
        Assert.Equal(0.8m, r.Delta);
    }

    // ============================================================
    // 3. 捨入規則：必須是「四捨五入（AwayFromZero）」，與 SQL ROUND() 一致
    //    以下每個案例都剛好落在 5 的中間值，
    //    若程式誤用 C# 預設的銀行家捨入（ToEven），結果會不同，測試就會失敗。
    // ============================================================
    public static TheoryData<string, decimal, decimal> TheoryPriceRoundingCases => new()
    {
        // 類型,  股價,        預期理論價值        （履約價 100、行使比例 0.5）
        { "CALL", 100.0001m, 0.0001m }, // 0.0001×0.5 = 0.00005 → 0.0001（銀行家捨入會得到 0.0000）
        { "CALL", 100.0005m, 0.0003m }, // 0.0005×0.5 = 0.00025 → 0.0003（銀行家捨入會得到 0.0002）
        { "PUT",   99.9995m, 0.0003m }, // 0.0005×0.5 = 0.00025 → 0.0003（PUT 同樣規則）
    };

    [Theory]
    [MemberData(nameof(TheoryPriceRoundingCases))]
    public void 理論價值四位小數_中間值應四捨五入(string type, decimal marketPrice, decimal expected)
    {
        var r = HedgeCalculator.Calculate(type, strikePrice: 100m, conversionRatio: 0.5m, positionQty: 1000, marketPrice);

        Assert.Equal(expected, r.TheoryPrice);
    }

    public static TheoryData<decimal, decimal, int, decimal> HedgeQtyRoundingCases => new()
    {
        // 股價,  行使比例,   庫存, 預期避險張數     （CALL、履約價 100）
        { 100m, 0.05m,     1, 0.03m }, // 價平：1×0.05×0.5   = 0.025 → 0.03（銀行家捨入會得到 0.02）
        {  90m, 0.0625m,   2, 0.03m }, // 價外：2×0.0625×0.2 = 0.025 → 0.03
    };

    [Theory]
    [MemberData(nameof(HedgeQtyRoundingCases))]
    public void 避險張數兩位小數_中間值應四捨五入(decimal marketPrice, decimal ratio, int position, decimal expected)
    {
        var r = HedgeCalculator.Calculate("CALL", strikePrice: 100m, conversionRatio: ratio, positionQty: position, marketPrice);

        Assert.Equal(expected, r.HedgeQty);
    }

    [Fact]
    public void 對照組_CSharp預設的MathRound是銀行家捨入()
    {
        // 這個測試不測我們的程式，而是記錄「為什麼一定要指定 AwayFromZero」
        Assert.Equal(0.02m, Math.Round(0.025m, 2));                                  // 預設：ToEven
        Assert.Equal(0.03m, Math.Round(0.025m, 2, MidpointRounding.AwayFromZero));   // 指定：四捨五入
    }

    // ============================================================
    // 4. 邊界值
    // ============================================================
    [Fact]
    public void 庫存為零_避險張數為零但理論價值照常計算()
    {
        var r = HedgeCalculator.Calculate("CALL", strikePrice: 100m, conversionRatio: 0.1m, positionQty: 0, marketPrice: 120m);

        Assert.Equal(2m, r.TheoryPrice);
        Assert.Equal(0m, r.HedgeQty);
    }

    // ============================================================
    // 5. 防禦性檢查：不合法的輸入必須丟出例外，不能算出錯誤結果
    // ============================================================
    public static TheoryData<decimal> InvalidMarketPrices => new() { 0m, -1m, -0.0001m };

    [Theory]
    [MemberData(nameof(InvalidMarketPrices))]
    public void 股價小於等於零_應丟出ArgumentOutOfRangeException(decimal marketPrice)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            HedgeCalculator.Calculate("CALL", strikePrice: 100m, conversionRatio: 0.1m, positionQty: 1000, marketPrice));

        Assert.Equal("marketPrice", ex.ParamName);
    }

    [Theory]
    [InlineData("XXX")]
    [InlineData("call")]  // DB 約束只允許大寫，計算函式也不接受小寫，避免靜默算錯
    [InlineData("")]
    public void 不支援的權證類型_應丟出ArgumentException(string type)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            HedgeCalculator.Calculate(type, strikePrice: 100m, conversionRatio: 0.1m, positionQty: 1000, marketPrice: 100m));

        Assert.Equal("warrantType", ex.ParamName);
    }
}