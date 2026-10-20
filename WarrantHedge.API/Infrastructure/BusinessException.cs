namespace WarrantHedge.API.Infrastructure;

/// <summary>
/// 可預期的業務錯誤（輸入錯誤、查無資料、冪等鍵衝突…），
/// 由 GlobalExceptionHandler 統一轉成 ProblemDetails 回傳。
/// </summary>
public class BusinessException(int statusCode, string title, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Title { get; } = title;

    public static BusinessException BadRequest(string message) =>
        new(StatusCodes.Status400BadRequest, "輸入資料錯誤", message);

    public static BusinessException NotFound(string message) =>
        new(StatusCodes.Status404NotFound, "查無資料", message);

    public static BusinessException Unprocessable(string message) =>
        new(StatusCodes.Status422UnprocessableEntity, "請求無法處理", message);
}
