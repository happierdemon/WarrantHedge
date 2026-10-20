using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using WarrantHedge.API.Data;
using WarrantHedge.API.Infrastructure;
using WarrantHedge.API.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        // Enum 以字串輸出（"ITM" 而非 0），API 回應較易讀
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// 連線字串：本機用 appsettings；部署到 Azure 時改用 App Service 的連接字串設定覆蓋，不寫進 Git
var connectionString = builder.Configuration.GetConnectionString("WarrantHedgeDb")
    ?? throw new InvalidOperationException("找不到連線字串 ConnectionStrings:WarrantHedgeDb。");

builder.Services.AddDbContext<WarrantDbContext>(options =>
    options.UseSqlServer(connectionString, sql =>
        // 雲端資料庫可能短暫斷線或自動暫停，遇到暫時性錯誤自動重試
        sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)));

builder.Services.AddScoped<IWarrantService, WarrantService>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi();

var app = builder.Build();

// ---------- Pipeline ----------
app.UseExceptionHandler();

// 為了讓面試官能直接在瀏覽器測試，所有環境都開啟 OpenAPI 與 Swagger UI（/swagger）
app.MapOpenApi();
app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("/openapi/v1.json", "WarrantHedge API v1");
    o.DocumentTitle = "權證避險試算 API";
});

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
