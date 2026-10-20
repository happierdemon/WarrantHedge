using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WarrantHedge.API.Data;
using WarrantHedge.API.Infrastructure;
using WarrantHedge.API.Models.Dtos;
using WarrantHedge.API.Models.Entities;

namespace WarrantHedge.API.Services;

public interface IWarrantService
{
    Task<IReadOnlyList<WarrantDto>> GetWarrantsAsync(string? keyword, CancellationToken ct);
    Task<WarrantDto> GetWarrantAsync(string warrantId, CancellationToken ct);
    Task<CalculationResultDto> CalculateAsync(string warrantId, decimal? marketPrice, CancellationToken ct);
    Task<SaveTrialResult> SaveTrialAsync(string warrantId, decimal? marketPrice, Guid requestId, CancellationToken ct);
    Task<IReadOnlyList<TrialLogDto>> GetRecentTrialsAsync(string warrantId, int top, CancellationToken ct);
}

public class WarrantService(WarrantDbContext db) : IWarrantService
{
    /// <summary>DECIMAL(18,4) 可存的最大值（整數 14 位）</summary>
    private const decimal MaxMarketPrice = 99_999_999_999_999.9999m;

    /// <summary>SQL Server 違反唯一索引 / 唯一約束的錯誤碼</summary>
    private static readonly int[] UniqueViolationErrors = [2601, 2627];

    public async Task<IReadOnlyList<WarrantDto>> GetWarrantsAsync(string? keyword, CancellationToken ct)
    {
        var query = db.WarrantMasters.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim().ToUpperInvariant();
            query = query.Where(w => w.WarrantId.Contains(kw));
        }

        return await query
            .OrderBy(w => w.WarrantId)
            .Select(w => new WarrantDto(w.WarrantId, w.StrikePrice, w.ConversionRatio, w.WarrantType, w.PositionQty))
            .ToListAsync(ct);
    }

    public async Task<WarrantDto> GetWarrantAsync(string warrantId, CancellationToken ct)
    {
        var w = await FindWarrantAsync(warrantId, ct);
        return new WarrantDto(w.WarrantId, w.StrikePrice, w.ConversionRatio, w.WarrantType, w.PositionQty);
    }

    public async Task<CalculationResultDto> CalculateAsync(string warrantId, decimal? marketPrice, CancellationToken ct)
    {
        var price = ValidateMarketPrice(marketPrice);
        var w = await FindWarrantAsync(warrantId, ct);
        var r = HedgeCalculator.Calculate(w.WarrantType, w.StrikePrice, w.ConversionRatio, w.PositionQty, price);

        return new CalculationResultDto(
            w.WarrantId, w.WarrantType, price, w.StrikePrice,
            r.TheoryPrice, r.Moneyness, r.Delta, r.HedgeQty, r.Direction);
    }

    /// <summary>
    /// 儲存試算結果（具冪等性）。
    /// 1. 同一個 requestId 已存過 → 不重複寫入，直接回傳既有那一筆。
    /// 2. 計算結果一律在 Server 端重算，不採用 Client 傳來的數值。
    /// 3. 兩個相同請求同時進來時，由 DB 唯一索引擋下第二筆，再改回傳既有資料。
    /// </summary>
    public async Task<SaveTrialResult> SaveTrialAsync(
        string warrantId, decimal? marketPrice, Guid requestId, CancellationToken ct)
    {
        var price = ValidateMarketPrice(marketPrice);
        var normalizedId = Normalize(warrantId);

        // 第一道：先查是否已處理過
        var existing = await FindLogByRequestIdAsync(requestId, ct);
        if (existing is not null)
            return Replay(existing, normalizedId, price);

        var w = await FindWarrantAsync(normalizedId, ct);
        var r = HedgeCalculator.Calculate(w.WarrantType, w.StrikePrice, w.ConversionRatio, w.PositionQty, price);

        var log = new WarrantTrialLog
        {
            WarrantId = w.WarrantId,
            MarketPrice = price,
            TheoryPrice = r.TheoryPrice,
            HedgeQty = r.HedgeQty,
            RequestId = requestId
            // CreatedTime 由 DB 預設值 GETDATE() 產生
        };

        db.WarrantTrialLogs.Add(log);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // 第二道：併發情境，另一個相同請求剛好先寫入成功
            db.ChangeTracker.Clear();
            var winner = await FindLogByRequestIdAsync(requestId, ct)
                         ?? throw new InvalidOperationException("唯一索引衝突但查無既有資料。");
            return Replay(winner, normalizedId, price);
        }

        return new SaveTrialResult(ToDto(log), IsReplay: false);
    }

    public async Task<IReadOnlyList<TrialLogDto>> GetRecentTrialsAsync(string warrantId, int top, CancellationToken ct)
    {
        var id = Normalize(warrantId);
        top = Math.Clamp(top, 1, 50);

        if (!await db.WarrantMasters.AnyAsync(w => w.WarrantId == id, ct))
            throw BusinessException.NotFound($"查無權證代碼：{id}");

        // 對應索引 IX_Trial_Log_Warrant_Time (Warrant_ID, Created_Time DESC) INCLUDE (...)
        // → Index Seek + 不需回表；Log_ID 為同一時間的第二排序，確保結果穩定
        return await db.WarrantTrialLogs.AsNoTracking()
            .Where(l => l.WarrantId == id)
            .OrderByDescending(l => l.CreatedTime)
            .ThenByDescending(l => l.LogId)
            .Take(top)
            .Select(l => new TrialLogDto(l.LogId, l.WarrantId, l.MarketPrice, l.TheoryPrice, l.HedgeQty, l.CreatedTime))
            .ToListAsync(ct);
    }

    // ---------- private helpers ----------

    /// <summary>防禦性檢查：股價必填、> 0、最多 4 位小數、不超過 DB 欄位上限</summary>
    private static decimal ValidateMarketPrice(decimal? marketPrice)
    {
        if (marketPrice is null)
            throw BusinessException.BadRequest("請輸入標的股價。");

        var p = marketPrice.Value;

        if (p <= 0)
            throw BusinessException.BadRequest("標的股價必須大於 0。");

        // 用值比較而非看 Scale：100.50000 視為合法（尾數 0 不影響精度）
        if (p != Math.Round(p, 4))
            throw BusinessException.BadRequest("標的股價最多 4 位小數。");

        if (p > MaxMarketPrice)
            throw BusinessException.BadRequest("標的股價超出可接受範圍。");

        return p;
    }

    private async Task<WarrantMaster> FindWarrantAsync(string warrantId, CancellationToken ct)
    {
        var id = Normalize(warrantId);
        return await db.WarrantMasters.AsNoTracking().FirstOrDefaultAsync(w => w.WarrantId == id, ct)
               ?? throw BusinessException.NotFound($"查無權證代碼：{id}");
    }

    private Task<WarrantTrialLog?> FindLogByRequestIdAsync(Guid requestId, CancellationToken ct) =>
        db.WarrantTrialLogs.AsNoTracking().FirstOrDefaultAsync(l => l.RequestId == requestId, ct);

    /// <summary>
    /// 重送檢查：同一個 Idempotency-Key 必須搭配相同的權證與股價，
    /// 否則代表 Client 誤用了舊的 Key，回 422 提醒而不是默默回傳舊資料。
    /// </summary>
    private static SaveTrialResult Replay(WarrantTrialLog existing, string warrantId, decimal price)
    {
        if (existing.WarrantId != warrantId || existing.MarketPrice != price)
            throw BusinessException.Unprocessable("此 Idempotency-Key 已用於另一筆不同內容的試算，請產生新的 Key。");

        return new SaveTrialResult(ToDto(existing), IsReplay: true);
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException sql && UniqueViolationErrors.Contains(sql.Number);

    private static string Normalize(string warrantId) => warrantId.Trim().ToUpperInvariant();

    private static TrialLogDto ToDto(WarrantTrialLog l) =>
        new(l.LogId, l.WarrantId, l.MarketPrice, l.TheoryPrice, l.HedgeQty, l.CreatedTime);
}
