using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WarrantHedge.API.Infrastructure;

/// <summary>
/// 全域例外處理：所有錯誤統一回傳 RFC 9457 ProblemDetails 格式，
/// WinForm 端只要解析一種錯誤格式。未預期的錯誤不把內部訊息回給 Client。
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // Client 中斷連線（例如 WinForm 取消請求），不需要當成錯誤
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            return true;

        var (status, title, detail) = exception switch
        {
            BusinessException be => (be.StatusCode, be.Title, be.Message),
            _ => (StatusCodes.Status500InternalServerError, "系統錯誤", "系統發生未預期的錯誤，請稍後再試。")
        };

        if (status >= 500)
            logger.LogError(exception, "未預期的錯誤：{Path}", httpContext.Request.Path);
        else
            logger.LogWarning("業務錯誤 {Status}：{Detail}（{Path}）", status, detail, httpContext.Request.Path);

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail
            }
        });
    }
}
