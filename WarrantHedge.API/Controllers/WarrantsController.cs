using Microsoft.AspNetCore.Mvc;
using WarrantHedge.API.Infrastructure;
using WarrantHedge.API.Models.Dtos;
using WarrantHedge.API.Services;

namespace WarrantHedge.API.Controllers;

[ApiController]
[Route("api/warrants")]
[Produces("application/json")]
public class WarrantsController(IWarrantService service) : ControllerBase
{
    /// <summary>取得權證清單（可用代碼關鍵字篩選）</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<WarrantDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWarrants([FromQuery] string? keyword, CancellationToken ct)
        => Ok(await service.GetWarrantsAsync(keyword, ct));

    /// <summary>取得單一權證</summary>
    [HttpGet("{warrantId:maxlength(10)}")]
    [ProducesResponseType<WarrantDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWarrant(string warrantId, CancellationToken ct)
        => Ok(await service.GetWarrantAsync(warrantId, ct));

    /// <summary>試算理論價值與避險張數（不存檔）</summary>
    [HttpPost("{warrantId:maxlength(10)}/calculate")]
    [ProducesResponseType<CalculationResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Calculate(string warrantId, [FromBody] CalculateRequest request, CancellationToken ct)
        => Ok(await service.CalculateAsync(warrantId, request.MarketPrice, ct));

    /// <summary>
    /// 儲存試算結果。需帶 Header「Idempotency-Key: &lt;GUID&gt;」。
    /// 新寫入回 201；同一個 Key 重送回 200 並附 Header「Idempotent-Replayed: true」。
    /// </summary>
    [HttpPost("{warrantId:maxlength(10)}/trials")]
    [ProducesResponseType<TrialLogDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<TrialLogDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SaveTrial(
        string warrantId,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        [FromBody] CalculateRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey) || !Guid.TryParse(idempotencyKey, out var requestId) || requestId == Guid.Empty)
            throw BusinessException.BadRequest("請在 Header 帶入有效的 Idempotency-Key（GUID 格式）。");

        var result = await service.SaveTrialAsync(warrantId, request.MarketPrice, requestId, ct);

        if (result.IsReplay)
        {
            Response.Headers["Idempotent-Replayed"] = "true";
            return Ok(result.Log);
        }

        return CreatedAtAction(nameof(GetRecentTrials), new { warrantId = result.Log.WarrantId }, result.Log);
    }

    /// <summary>取得該權證最近 N 筆試算紀錄（預設 10，最多 50）</summary>
    [HttpGet("{warrantId:maxlength(10)}/trials")]
    [ProducesResponseType<IReadOnlyList<TrialLogDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecentTrials(string warrantId, [FromQuery] int top = 10, CancellationToken ct = default)
        => Ok(await service.GetRecentTrialsAsync(warrantId, top, ct));
}
