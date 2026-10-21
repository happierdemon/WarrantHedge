using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WarrantHedge.WinForm.Models;
using WarrantHedge.WinForm.Services;

namespace WarrantHedge.WinForm
{
    /// <summary>
    /// 主畫面：左側權證清單 + 右上試算面板 + 右下歷史明細區。
    /// 原則：畫面只負責「輸入、顯示」，所有計算都交給 API。
    /// </summary>
    public partial class MainForm : Form
    {
        private const int HistoryTop = 10;
        private const string Placeholder = "—";

        private readonly ApiClient _api;
        private readonly BindingSource _warrantSource = new BindingSource();

        /// <summary>從 API 載入的全部權證（搜尋時在記憶體中篩選，不重複呼叫 API）</summary>
        private List<WarrantDto> _allWarrants = new List<WarrantDto>();

        /// <summary>右側目前顯示的權證</summary>
        private WarrantDto _current;

        /// <summary>畫面上目前顯示的試算結果；null 代表沒有有效結果，不能存檔</summary>
        private CalculationResultDto _lastResult;

        /// <summary>這一筆試算結果專屬的冪等鍵；重新試算才會換新的</summary>
        private Guid _saveKey;

        /// <summary>進行中的試算請求，有新輸入時取消舊的</summary>
        private CancellationTokenSource _calcCts;

        /// <summary>歷史紀錄的請求版本號：快速切換權證時，只採用最後一次請求的結果</summary>
        private int _historyVersion;

        private bool _isSaving;

        /// <summary>程式設定清單資料來源時，暫停處理選取事件</summary>
        private bool _suppressSelection;

        public MainForm(ApiClient api)
        {
            InitializeComponent();
            _api = api;

            // 只顯示設計工具裡定義的 4 個欄位，不要自動產生 LogId、WarrantId 等欄位
            dgvHistory.AutoGenerateColumns = false;

            _warrantSource.DataSource = typeof(WarrantDto);
            lstWarrants.DataSource = _warrantSource;
            lstWarrants.DisplayMember = nameof(WarrantDto.DisplayText);

            ShowWarrant(null);
        }

        // =====================================================================
        // 1. 載入權證清單
        // =====================================================================

        /// <summary>用 Shown 而不是 Load：畫面先顯示出來，再開始載入，使用者不會看到一片空白</summary>
        private async void MainForm_Shown(object sender, EventArgs e)
        {
            await LoadWarrantsAsync();
        }

        private async Task LoadWarrantsAsync()
        {
            while (true)
            {
                SetStatus("載入權證清單中…（雲端伺服器第一次連線可能需要較久）");
                UseWaitCursor = true;
                splitContainer.Enabled = false;

                try
                {
                    _allWarrants = await _api.GetWarrantsAsync(CancellationToken.None);
                    ApplyFilter();
                    SetStatus("已載入 " + _allWarrants.Count + " 筆權證");

                    // 預設選取第一筆
                    if (lstWarrants.Items.Count > 0)
                        lstWarrants.SelectedIndex = 0;
                    return;
                }
                catch (ApiException ex)
                {
                    SetStatus("載入失敗：" + ex.Message);
                    var answer = MessageBox.Show(this, ex.Message + "\n\n要重試嗎？", "載入權證清單失敗",
                        MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                    if (answer != DialogResult.Retry)
                        return;
                }
                finally
                {
                    UseWaitCursor = false;
                    splitContainer.Enabled = true;
                }
            }
        }

        // =====================================================================
        // 2. 搜尋：在記憶體中篩選 800 筆，不呼叫 API
        // =====================================================================

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var keyword = txtSearch.Text.Trim();

            var filtered = keyword.Length == 0
                ? _allWarrants
                : _allWarrants
                    .Where(w => w.WarrantId.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

            // 換資料來源時 ListBox 會自動選第一筆並觸發事件，
            // 先暫停事件，避免使用者一打字，右邊的權證就被換掉
            _suppressSelection = true;
            try
            {
                _warrantSource.DataSource = filtered;

                // 目前的權證若還在篩選結果中，維持選取；不在就取消選取（右側內容保持不變）
                var index = _current == null ? -1 : filtered.FindIndex(w => w.WarrantId == _current.WarrantId);
                lstWarrants.SelectedIndex = index;
            }
            finally
            {
                _suppressSelection = false;
            }

            lblCount.Text = "共 " + filtered.Count + " / " + _allWarrants.Count + " 筆";
        }

        // =====================================================================
        // 3. 選取權證：顯示資料、重新試算、載入歷史紀錄
        // =====================================================================

        private async void lstWarrants_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressSelection) return;

            var selected = lstWarrants.SelectedItem as WarrantDto;
            if (selected == null) return;
            if (_current != null && selected.WarrantId == _current.WarrantId) return;

            ShowWarrant(selected);

            // 股價欄有值就直接用新權證重算；試算和歷史紀錄同時發出，不用互相等待
            await Task.WhenAll(RecalculateAsync(), LoadHistoryAsync(selected.WarrantId));
        }

        private void ShowWarrant(WarrantDto warrant)
        {
            _current = warrant;
            ResetCalculation();

            grpInput.Enabled = warrant != null;

            if (warrant == null)
            {
                lblWarrantInfo.Text = "請從左側選擇權證";
                grpHistory.Text = "最近 " + HistoryTop + " 筆試算紀錄";
                dgvHistory.DataSource = null;
                return;
            }

            lblWarrantInfo.Text = string.Format(
                "{0}｜{1}｜履約價 {2:N4}｜行使比例 {3:0.####}｜庫存 {4:N0} 張",
                warrant.WarrantId,
                warrant.WarrantType == "CALL" ? "認購 CALL" : "認售 PUT",
                warrant.StrikePrice,
                warrant.ConversionRatio,
                warrant.PositionQty);

            grpHistory.Text = "最近 " + HistoryTop + " 筆試算紀錄 - " + warrant.WarrantId;
        }

        // =====================================================================
        // 4. 即時試算：停止輸入 300ms 後才呼叫 API（Debounce）
        // =====================================================================

        private void txtMarketPrice_TextChanged(object sender, EventArgs e)
        {
            // 輸入一變，舊結果就和輸入不符，立刻清掉，避免存到錯的結果
            ResetCalculation();

            // 重新計時：連續輸入時 Timer 會一直被重設，停下來 300ms 後才觸發
            debounceTimer.Stop();
            debounceTimer.Start();
        }

        private async void txtMarketPrice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            // 按 Enter：不等 300ms，立刻試算；SuppressKeyPress 避免系統發出「叮」聲
            e.SuppressKeyPress = true;
            debounceTimer.Stop();
            await RecalculateAsync();
        }

        private async void debounceTimer_Tick(object sender, EventArgs e)
        {
            debounceTimer.Stop();
            await RecalculateAsync();
        }

        private async Task RecalculateAsync()
        {
            ResetCalculation();
            if (_current == null) return;

            decimal price;
            string error;
            if (!TryParsePrice(txtMarketPrice.Text, out price, out error))
            {
                lblPriceError.Text = error ?? string.Empty;
                return;
            }

            var cts = new CancellationTokenSource();
            _calcCts = cts;
            var warrantId = _current.WarrantId;

            SetStatus("試算中…");
            try
            {
                var result = await _api.CalculateAsync(warrantId, price, cts.Token);

                // 等待期間若有新輸入或切換權證，這個結果已經過時，直接丟掉
                if (cts.IsCancellationRequested) return;

                ShowResult(result);
                SetStatus("試算完成");
            }
            catch (OperationCanceledException)
            {
                // 被新的輸入取消，屬於正常情況，不用顯示
            }
            catch (ApiException ex)
            {
                if (cts.IsCancellationRequested) return;
                lblPriceError.Text = ex.Message;
                SetStatus("試算失敗");
            }
            finally
            {
                if (_calcCts == cts) _calcCts = null;
                cts.Dispose();
            }
        }

        /// <summary>
        /// 前端先做基本檢查，減少不必要的 API 呼叫；API 端仍會再檢查一次（不信任 Client）。
        /// 空白時回傳 false 但不顯示錯誤，因為使用者可能還沒開始輸入。
        /// </summary>
        private static bool TryParsePrice(string text, out decimal price, out string error)
        {
            price = 0m;
            error = null;
            text = text.Trim();

            if (text.Length == 0) return false;

            // 用 decimal.TryParse，不用 double；InvariantCulture 固定以「.」作為小數點
            if (!decimal.TryParse(text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out price))
            {
                error = "請輸入數字";
                return false;
            }

            if (price <= 0m)
            {
                error = "股價必須大於 0";
                return false;
            }

            if (price != Math.Round(price, 4))
            {
                error = "最多 4 位小數";
                return false;
            }

            return true;
        }

        private void ShowResult(CalculationResultDto result)
        {
            _lastResult = result;
            _saveKey = Guid.NewGuid();   // 新的試算結果 → 新的冪等鍵

            lblTheory.Text = result.TheoryPrice.ToString("N4");
            lblMoneyness.Text = MoneynessText(result.Moneyness) + "（Delta " + result.Delta.ToString("0.0") + "）";

            bool isBuy = result.HedgeDirection == "BUY";
            lblHedge.Text = (isBuy ? "買入 " : "賣出 ") + result.HedgeQty.ToString("N2") + " 張";
            
            lblHedge.ForeColor = isBuy ? Color.Firebrick : Color.SeaGreen;

            btnSave.Enabled = !_isSaving;
        }

        private void ResetCalculation()
        {
            if (_calcCts != null)
            {
                _calcCts.Cancel();
                _calcCts = null;
            }

            _lastResult = null;
            lblPriceError.Text = string.Empty;
            lblTheory.Text = Placeholder;
            lblMoneyness.Text = Placeholder;
            lblHedge.Text = Placeholder;
            lblHedge.ForeColor = SystemColors.ControlText;
            btnSave.Enabled = false;
        }

        private static string MoneynessText(string moneyness)
        {
            switch (moneyness)
            {
                case "ITM": return "價內";
                case "ATM": return "價平";
                case "OTM": return "價外";
                default: return moneyness;
            }
        }

        // =====================================================================
        // 5. 儲存：防連點 + 冪等鍵
        // =====================================================================

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (_lastResult == null || _isSaving) return;

            // 先把要存的資料記下來：等待 API 期間使用者可能又改了輸入
            var result = _lastResult;
            var key = _saveKey;
            bool succeeded = false;

            _isSaving = true;
            btnSave.Enabled = false;   // 第一道防線：按鈕停用，防止連點
            btnSave.Text = "儲存中…";

            try
            {
                var saved = await _api.SaveTrialAsync(result.WarrantId, result.MarketPrice, key, CancellationToken.None);
                succeeded = true;

                ToastForm.ShowToast(this, saved.IsReplay ? "這筆試算已儲存過，未重複寫入" : "儲存成功");

                if (_current != null && _current.WarrantId == result.WarrantId)
                    await LoadHistoryAsync(result.WarrantId);
            }
            catch (ApiException ex)
            {
                // 失敗時保留同一個 Key：若其實已經寫入、只是回應沒收到（例如逾時），
                // 使用者按重試時 API 會回傳既有資料，不會重複新增（第二道防線）
                ToastForm.ShowToast(this, "儲存失敗", success: false);
                MessageBox.Show(this, ex.Message, "儲存失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isSaving = false;
                btnSave.Text = "儲存試算結果";

                // 存檔成功：同一筆結果不再開放儲存，直到重新試算
                // 存檔失敗：開放重試
                btnSave.Enabled = _lastResult != null && !(succeeded && _lastResult == result);
            }
        }

        // =====================================================================
        // 6. 歷史明細區
        // =====================================================================

        private async Task LoadHistoryAsync(string warrantId)
        {
            var version = ++_historyVersion;

            try
            {
                var logs = await _api.GetRecentTrialsAsync(warrantId, HistoryTop, CancellationToken.None);

                // 等待期間使用者已切換到別的權證，這份結果已經過時
                if (version != _historyVersion) return;

                dgvHistory.DataSource = logs;
            }
            catch (ApiException ex)
            {
                if (version != _historyVersion) return;
                dgvHistory.DataSource = null;
                SetStatus("歷史紀錄載入失敗：" + ex.Message);
            }
        }

        // =====================================================================
        // 其他
        // =====================================================================

        private void SetStatus(string message)
        {
            lblStatus.Text = message;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            debounceTimer.Stop();
            if (_calcCts != null) _calcCts.Cancel();
        }
    }
}
