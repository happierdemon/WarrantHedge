using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WarrantHedge.WinForm.Models;

namespace WarrantHedge.WinForm.Services
{
    /// <summary>
    /// 呼叫 API 失敗時丟出的例外。Message 已轉成可以直接顯示給使用者的中文訊息。
    /// </summary>
    public class ApiException : Exception
    {
        public ApiException(string message, HttpStatusCode? statusCode = null, Exception inner = null)
            : base(message, inner)
        {
            StatusCode = statusCode;
        }

        /// <summary>HTTP 狀態碼；連線失敗或逾時時為 null</summary>
        public HttpStatusCode? StatusCode { get; }
    }

    /// <summary>
    /// 集中管理所有 API 呼叫。
    /// 1. 整個程式共用一個 HttpClient：每次 new HttpClient 會耗盡 Socket（Socket Exhaustion）。
    /// 2. 錯誤統一轉成 ApiException，畫面只需要處理一種例外。
    /// 3. 內部使用 ConfigureAwait(false)：這裡不碰 UI，不需要切回 UI 執行緒。
    /// </summary>
    public sealed class ApiClient : IDisposable
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            // 小數一律解析成 decimal，不經過 double，避免精度誤差
            FloatParseHandling = FloatParseHandling.Decimal,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private readonly HttpClient _http;

        public ApiClient(string baseUrl, TimeSpan timeout)
        {
            // BaseAddress 結尾一定要有 "/"，且後面的相對路徑開頭不能有 "/"，
            // 否則 Uri 合併時會把 BaseAddress 的路徑吃掉（例如部署在子路徑時）
            if (!baseUrl.EndsWith("/")) baseUrl += "/";

            _http = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                // 雲端免費方案可能冷啟動，逾時設長一點
                Timeout = timeout
            };
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<List<WarrantDto>> GetWarrantsAsync(CancellationToken ct)
        {
            var response = await SendAsync(HttpMethod.Get, "api/warrants", null, null, ct).ConfigureAwait(false);
            return await ReadAsync<List<WarrantDto>>(response).ConfigureAwait(false);
        }

        public async Task<CalculationResultDto> CalculateAsync(string warrantId, decimal marketPrice, CancellationToken ct)
        {
            var body = new CalculateRequest { MarketPrice = marketPrice };
            var response = await SendAsync(HttpMethod.Post, WarrantPath(warrantId, "calculate"), body, null, ct).ConfigureAwait(false);
            return await ReadAsync<CalculationResultDto>(response).ConfigureAwait(false);
        }

        public async Task<SaveTrialResult> SaveTrialAsync(string warrantId, decimal marketPrice, Guid idempotencyKey, CancellationToken ct)
        {
            var body = new CalculateRequest { MarketPrice = marketPrice };
            var response = await SendAsync(HttpMethod.Post, WarrantPath(warrantId, "trials"), body, idempotencyKey, ct).ConfigureAwait(false);

            // 201 = 新寫入；200 + Header「Idempotent-Replayed」= 同一個 Key 重送，回傳的是既有資料
            bool isReplay = response.Headers.Contains("Idempotent-Replayed");
            var log = await ReadAsync<TrialLogDto>(response).ConfigureAwait(false);

            return new SaveTrialResult { Log = log, IsReplay = isReplay };
        }

        public async Task<List<TrialLogDto>> GetRecentTrialsAsync(string warrantId, int top, CancellationToken ct)
        {
            var path = WarrantPath(warrantId, "trials") + "?top=" + top;
            var response = await SendAsync(HttpMethod.Get, path, null, null, ct).ConfigureAwait(false);
            return await ReadAsync<List<TrialLogDto>>(response).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _http.Dispose();
        }

        // ---------- private helpers ----------

        private static string WarrantPath(string warrantId, string action)
        {
            // EscapeDataString：權證代碼放進網址前先編碼，避免特殊字元破壞路徑
            return "api/warrants/" + Uri.EscapeDataString(warrantId) + "/" + action;
        }

        /// <summary>送出請求；成功回傳 Response，失敗一律轉成 ApiException</summary>
        private async Task<HttpResponseMessage> SendAsync(
            HttpMethod method, string path, object body, Guid? idempotencyKey, CancellationToken ct)
        {
            using (var request = new HttpRequestMessage(method, path))
            {
                if (body != null)
                {
                    var json = JsonConvert.SerializeObject(body, JsonSettings);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                if (idempotencyKey.HasValue)
                    request.Headers.Add("Idempotency-Key", idempotencyKey.Value.ToString());

                HttpResponseMessage response;
                try
                {
                    response = await _http.SendAsync(request, ct).ConfigureAwait(false);
                }
                // HttpClient 逾時也是丟 TaskCanceledException；
                // 用 ct 是否被取消來分辨「逾時」和「我們自己取消」
                catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
                {
                    throw new ApiException("連線逾時，伺服器可能正在啟動中，請稍後再試。", null, ex);
                }
                catch (HttpRequestException ex)
                {
                    throw new ApiException("無法連線到伺服器，請確認 API 是否已啟動。", null, ex);
                }

                if (response.IsSuccessStatusCode)
                    return response;

                // 失敗：讀出 ProblemDetails 的 detail 當作錯誤訊息
                using (response)
                {
                    var message = await TryReadProblemAsync(response).ConfigureAwait(false);
                    throw new ApiException(
                        message ?? "伺服器回應錯誤（HTTP " + (int)response.StatusCode + "）。",
                        response.StatusCode);
                }
            }
        }

        private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
        {
            using (response)
            {
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<T>(json, JsonSettings);
            }
        }

        private static async Task<string> TryReadProblemAsync(HttpResponseMessage response)
        {
            try
            {
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var problem = JsonConvert.DeserializeObject<ProblemDetailsDto>(json, JsonSettings);
                if (problem == null) return null;
                return !string.IsNullOrWhiteSpace(problem.Detail) ? problem.Detail : problem.Title;
            }
            catch (JsonException)
            {
                // 回應不是 JSON（例如 IIS 錯誤頁），改用通用訊息
                return null;
            }
        }
    }
}
