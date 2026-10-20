using WarrantHedge.API.Services;

namespace WarrantHedge.API.Models.Dtos;

/// <summary>權證資料（清單與單筆共用）</summary>
public sealed record WarrantDto(
    string WarrantId,
    decimal StrikePrice,
    decimal ConversionRatio,
    string WarrantType,
    int PositionQty);

/// <summary>試算 / 存檔共用的請求內容</summary>
/// <remarks>用 decimal? 而非 decimal：沒傳值時才能分辨「沒輸入」和「輸入 0」。</remarks>
public sealed record CalculateRequest(decimal? MarketPrice);

/// <summary>試算結果（不存檔）</summary>
public sealed record CalculationResultDto(
    string WarrantId,
    string WarrantType,
    decimal MarketPrice,
    decimal StrikePrice,
    decimal TheoryPrice,
    Moneyness Moneyness,
    decimal Delta,
    decimal HedgeQty,
    HedgeDirection HedgeDirection);

/// <summary>試算紀錄（歷史明細區、存檔回傳）</summary>
public sealed record TrialLogDto(
    int LogId,
    string WarrantId,
    decimal MarketPrice,
    decimal TheoryPrice,
    decimal HedgeQty,
    DateTime CreatedTime);

/// <summary>存檔結果：IsReplay = true 代表同一個 Idempotency-Key 已存過，回傳的是既有那一筆</summary>
public sealed record SaveTrialResult(TrialLogDto Log, bool IsReplay);
