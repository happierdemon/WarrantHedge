using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WarrantHedge.WinForm
{
    /// <summary>
    /// 右下角自動消失的提示訊息（WinForm 沒有內建 Toast，自己做一個）。
    /// - 無邊框小視窗，顯示 2.5 秒後自動關閉，點一下也會關閉
    /// - 不搶焦點：使用者正在輸入股價時跳出，游標不會被搶走
    /// - 純程式碼建立畫面，不使用設計工具
    /// </summary>
    [DesignerCategory("Code")]
    public sealed class ToastForm : Form
    {
        private const int WS_EX_TOOLWINDOW = 0x00000080;  // 不出現在 Alt+Tab
        private const int WS_EX_NOACTIVATE = 0x08000000;  // 顯示時不取得焦點

        /// <summary>目前顯示中的 Toast；連續觸發時先關掉舊的，避免疊在一起</summary>
        private static ToastForm _current;

        private readonly Timer _closeTimer;

        private ToastForm(string message, bool success)
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            BackColor = success ? Color.FromArgb(46, 125, 50) : Color.FromArgb(198, 40, 40);
            Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Bold);

            var label = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = message
            };
            label.Click += (s, e) => Close();
            Controls.Add(label);

            // 依文字長度決定視窗大小，左右各留 24px、上下各留 14px
            var textSize = TextRenderer.MeasureText(message, Font);
            ClientSize = new Size(textSize.Width + 48, textSize.Height + 28);

            _closeTimer = new Timer { Interval = 2500 };
            _closeTimer.Tick += (s, e) => Close();
        }

        /// <summary>顯示 Toast，位置在 owner 視窗的右下角</summary>
        public static void ShowToast(Form owner, string message, bool success = true)
        {
            if (_current != null && !_current.IsDisposed)
                _current.Close();

            var toast = new ToastForm(message, success);
            var area = owner.RectangleToScreen(owner.ClientRectangle);
            toast.Location = new Point(area.Right - toast.Width - 20, area.Bottom - toast.Height - 40);

            _current = toast;
            toast.Show(owner);  // 設定 owner：Toast 永遠在主視窗上方，主視窗關閉時一起關閉
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
                return cp;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _closeTimer.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _closeTimer.Stop();
            if (_current == this) _current = null;
            base.OnFormClosed(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _closeTimer.Dispose();
            base.Dispose(disposing);
        }
    }
}
