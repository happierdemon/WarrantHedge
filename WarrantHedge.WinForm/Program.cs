using System;
using System.Configuration;
using System.Windows.Forms;
using WarrantHedge.WinForm.Services;

namespace WarrantHedge.WinForm
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 最後防線：任何沒被攔到的例外都顯示訊息，而不是讓程式直接閃退
            Application.ThreadException += (s, e) => ShowFatalError(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => ShowFatalError(e.ExceptionObject as Exception);

            // API 網址放在 App.config，切換本機 / 雲端不用改程式
            var baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                MessageBox.Show("App.config 缺少 ApiBaseUrl 設定。", "設定錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int timeoutSeconds;
            if (!int.TryParse(ConfigurationManager.AppSettings["ApiTimeoutSeconds"], out timeoutSeconds))
                timeoutSeconds = 60;

            using (var api = new ApiClient(baseUrl, TimeSpan.FromSeconds(timeoutSeconds)))
            {
                Application.Run(new MainForm(api));
            }
        }

        private static void ShowFatalError(Exception ex)
        {
            MessageBox.Show(
                "發生未預期的錯誤：\n" + (ex != null ? ex.Message : "未知錯誤"),
                "系統錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
