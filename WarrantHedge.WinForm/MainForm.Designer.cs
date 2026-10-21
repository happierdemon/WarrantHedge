namespace WarrantHedge.WinForm
{
    partial class MainForm
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.lstWarrants = new System.Windows.Forms.ListBox();
            this.lblCount = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.grpHistory = new System.Windows.Forms.GroupBox();
            this.dgvHistory = new System.Windows.Forms.DataGridView();
            this.colCreatedTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMarketPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTheoryPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHedgeQty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grpInput = new System.Windows.Forms.GroupBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.lblHedge = new System.Windows.Forms.Label();
            this.lblHedgeCaption = new System.Windows.Forms.Label();
            this.lblMoneyness = new System.Windows.Forms.Label();
            this.lblMoneynessCaption = new System.Windows.Forms.Label();
            this.lblTheory = new System.Windows.Forms.Label();
            this.lblTheoryCaption = new System.Windows.Forms.Label();
            this.lblPriceError = new System.Windows.Forms.Label();
            this.txtMarketPrice = new System.Windows.Forms.TextBox();
            this.lblPriceCaption = new System.Windows.Forms.Label();
            this.lblWarrantInfo = new System.Windows.Forms.Label();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.debounceTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.grpHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistory)).BeginInit();
            this.grpInput.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            //
            // splitContainer
            //
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            //
            // splitContainer.Panel1
            //
            this.splitContainer.Panel1.Controls.Add(this.lstWarrants);
            this.splitContainer.Panel1.Controls.Add(this.lblCount);
            this.splitContainer.Panel1.Controls.Add(this.txtSearch);
            this.splitContainer.Panel1.Controls.Add(this.lblSearch);
            this.splitContainer.Panel1.Padding = new System.Windows.Forms.Padding(8);
            //
            // splitContainer.Panel2
            //
            this.splitContainer.Panel2.Controls.Add(this.grpHistory);
            this.splitContainer.Panel2.Controls.Add(this.grpInput);
            this.splitContainer.Panel2.Padding = new System.Windows.Forms.Padding(8);
            this.splitContainer.Size = new System.Drawing.Size(1040, 678);
            this.splitContainer.Panel1MinSize = 200;
            this.splitContainer.SplitterDistance = 260;
            this.splitContainer.TabIndex = 0;
            //
            // lstWarrants
            //
            this.lstWarrants.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstWarrants.Font = new System.Drawing.Font("Consolas", 11F);
            this.lstWarrants.IntegralHeight = false;
            this.lstWarrants.ItemHeight = 18;
            this.lstWarrants.Location = new System.Drawing.Point(8, 61);
            this.lstWarrants.Name = "lstWarrants";
            this.lstWarrants.Size = new System.Drawing.Size(244, 585);
            this.lstWarrants.TabIndex = 2;
            this.lstWarrants.SelectedIndexChanged += new System.EventHandler(this.lstWarrants_SelectedIndexChanged);
            //
            // lblCount
            //
            this.lblCount.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblCount.ForeColor = System.Drawing.Color.DimGray;
            this.lblCount.Location = new System.Drawing.Point(8, 646);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(244, 24);
            this.lblCount.TabIndex = 3;
            this.lblCount.Text = "共 0 筆";
            this.lblCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // txtSearch
            //
            this.txtSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtSearch.Location = new System.Drawing.Point(8, 32);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(244, 29);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            //
            // lblSearch
            //
            this.lblSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSearch.Location = new System.Drawing.Point(8, 8);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(244, 24);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "搜尋權證代碼";
            this.lblSearch.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // grpHistory
            //
            this.grpHistory.Controls.Add(this.dgvHistory);
            this.grpHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpHistory.Location = new System.Drawing.Point(8, 298);
            this.grpHistory.Name = "grpHistory";
            this.grpHistory.Padding = new System.Windows.Forms.Padding(8);
            this.grpHistory.Size = new System.Drawing.Size(760, 372);
            this.grpHistory.TabIndex = 1;
            this.grpHistory.TabStop = false;
            this.grpHistory.Text = "最近 10 筆試算紀錄";
            //
            // dgvHistory
            //
            this.dgvHistory.AllowUserToAddRows = false;
            this.dgvHistory.AllowUserToDeleteRows = false;
            this.dgvHistory.AllowUserToResizeRows = false;
            this.dgvHistory.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvHistory.BackgroundColor = System.Drawing.Color.White;
            this.dgvHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvHistory.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCreatedTime,
            this.colMarketPrice,
            this.colTheoryPrice,
            this.colHedgeQty});
            this.dgvHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvHistory.Location = new System.Drawing.Point(8, 30);
            this.dgvHistory.MultiSelect = false;
            this.dgvHistory.Name = "dgvHistory";
            this.dgvHistory.ReadOnly = true;
            this.dgvHistory.RowHeadersVisible = false;
            this.dgvHistory.RowTemplate.Height = 28;
            this.dgvHistory.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvHistory.Size = new System.Drawing.Size(744, 334);
            this.dgvHistory.TabIndex = 0;
            //
            // colCreatedTime
            //
            this.colCreatedTime.DataPropertyName = "CreatedTime";
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.Format = "yyyy/MM/dd HH:mm:ss";
            this.colCreatedTime.DefaultCellStyle = dataGridViewCellStyle1;
            this.colCreatedTime.FillWeight = 130F;
            this.colCreatedTime.HeaderText = "試算時間";
            this.colCreatedTime.Name = "colCreatedTime";
            this.colCreatedTime.ReadOnly = true;
            //
            // colMarketPrice
            //
            this.colMarketPrice.DataPropertyName = "MarketPrice";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle2.Format = "N4";
            this.colMarketPrice.DefaultCellStyle = dataGridViewCellStyle2;
            this.colMarketPrice.HeaderText = "標的股價";
            this.colMarketPrice.Name = "colMarketPrice";
            this.colMarketPrice.ReadOnly = true;
            //
            // colTheoryPrice
            //
            this.colTheoryPrice.DataPropertyName = "TheoryPrice";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle3.Format = "N4";
            this.colTheoryPrice.DefaultCellStyle = dataGridViewCellStyle3;
            this.colTheoryPrice.HeaderText = "理論價值";
            this.colTheoryPrice.Name = "colTheoryPrice";
            this.colTheoryPrice.ReadOnly = true;
            //
            // colHedgeQty
            //
            this.colHedgeQty.DataPropertyName = "HedgeQty";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle4.Format = "N2";
            this.colHedgeQty.DefaultCellStyle = dataGridViewCellStyle4;
            this.colHedgeQty.HeaderText = "避險張數";
            this.colHedgeQty.Name = "colHedgeQty";
            this.colHedgeQty.ReadOnly = true;
            //
            // grpInput
            //
            this.grpInput.Controls.Add(this.btnSave);
            this.grpInput.Controls.Add(this.lblHedge);
            this.grpInput.Controls.Add(this.lblHedgeCaption);
            this.grpInput.Controls.Add(this.lblMoneyness);
            this.grpInput.Controls.Add(this.lblMoneynessCaption);
            this.grpInput.Controls.Add(this.lblTheory);
            this.grpInput.Controls.Add(this.lblTheoryCaption);
            this.grpInput.Controls.Add(this.lblPriceError);
            this.grpInput.Controls.Add(this.txtMarketPrice);
            this.grpInput.Controls.Add(this.lblPriceCaption);
            this.grpInput.Controls.Add(this.lblWarrantInfo);
            this.grpInput.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpInput.Location = new System.Drawing.Point(8, 8);
            this.grpInput.Name = "grpInput";
            this.grpInput.Size = new System.Drawing.Size(760, 290);
            this.grpInput.TabIndex = 0;
            this.grpInput.TabStop = false;
            this.grpInput.Text = "避險試算";
            //
            // btnSave
            //
            this.btnSave.Enabled = false;
            this.btnSave.Location = new System.Drawing.Point(110, 240);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(180, 36);
            this.btnSave.TabIndex = 10;
            this.btnSave.Text = "儲存試算結果";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            //
            // lblHedge
            //
            this.lblHedge.AutoSize = true;
            this.lblHedge.Font = new System.Drawing.Font("Microsoft JhengHei UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblHedge.Location = new System.Drawing.Point(108, 198);
            this.lblHedge.Name = "lblHedge";
            this.lblHedge.Size = new System.Drawing.Size(24, 25);
            this.lblHedge.TabIndex = 9;
            this.lblHedge.Text = "—";
            //
            // lblHedgeCaption
            //
            this.lblHedgeCaption.AutoSize = true;
            this.lblHedgeCaption.Location = new System.Drawing.Point(20, 202);
            this.lblHedgeCaption.Name = "lblHedgeCaption";
            this.lblHedgeCaption.Size = new System.Drawing.Size(73, 20);
            this.lblHedgeCaption.TabIndex = 8;
            this.lblHedgeCaption.Text = "建議避險";
            //
            // lblMoneyness
            //
            this.lblMoneyness.AutoSize = true;
            this.lblMoneyness.Location = new System.Drawing.Point(110, 164);
            this.lblMoneyness.Name = "lblMoneyness";
            this.lblMoneyness.Size = new System.Drawing.Size(20, 20);
            this.lblMoneyness.TabIndex = 7;
            this.lblMoneyness.Text = "—";
            //
            // lblMoneynessCaption
            //
            this.lblMoneynessCaption.AutoSize = true;
            this.lblMoneynessCaption.Location = new System.Drawing.Point(20, 164);
            this.lblMoneynessCaption.Name = "lblMoneynessCaption";
            this.lblMoneynessCaption.Size = new System.Drawing.Size(41, 20);
            this.lblMoneynessCaption.TabIndex = 6;
            this.lblMoneynessCaption.Text = "價性";
            //
            // lblTheory
            //
            this.lblTheory.AutoSize = true;
            this.lblTheory.Font = new System.Drawing.Font("Microsoft JhengHei UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTheory.Location = new System.Drawing.Point(108, 122);
            this.lblTheory.Name = "lblTheory";
            this.lblTheory.Size = new System.Drawing.Size(24, 25);
            this.lblTheory.TabIndex = 5;
            this.lblTheory.Text = "—";
            //
            // lblTheoryCaption
            //
            this.lblTheoryCaption.AutoSize = true;
            this.lblTheoryCaption.Location = new System.Drawing.Point(20, 126);
            this.lblTheoryCaption.Name = "lblTheoryCaption";
            this.lblTheoryCaption.Size = new System.Drawing.Size(73, 20);
            this.lblTheoryCaption.TabIndex = 4;
            this.lblTheoryCaption.Text = "理論價值";
            //
            // lblPriceError
            //
            this.lblPriceError.AutoSize = true;
            this.lblPriceError.ForeColor = System.Drawing.Color.Firebrick;
            this.lblPriceError.Location = new System.Drawing.Point(305, 82);
            this.lblPriceError.Name = "lblPriceError";
            this.lblPriceError.Size = new System.Drawing.Size(0, 20);
            this.lblPriceError.TabIndex = 3;
            //
            // txtMarketPrice
            //
            this.txtMarketPrice.Font = new System.Drawing.Font("Consolas", 12F);
            this.txtMarketPrice.Location = new System.Drawing.Point(110, 77);
            this.txtMarketPrice.Name = "txtMarketPrice";
            this.txtMarketPrice.Size = new System.Drawing.Size(180, 31);
            this.txtMarketPrice.TabIndex = 2;
            this.txtMarketPrice.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtMarketPrice.TextChanged += new System.EventHandler(this.txtMarketPrice_TextChanged);
            this.txtMarketPrice.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtMarketPrice_KeyDown);
            //
            // lblPriceCaption
            //
            this.lblPriceCaption.AutoSize = true;
            this.lblPriceCaption.Location = new System.Drawing.Point(20, 82);
            this.lblPriceCaption.Name = "lblPriceCaption";
            this.lblPriceCaption.Size = new System.Drawing.Size(73, 20);
            this.lblPriceCaption.TabIndex = 1;
            this.lblPriceCaption.Text = "標的股價";
            //
            // lblWarrantInfo
            //
            this.lblWarrantInfo.AutoSize = true;
            this.lblWarrantInfo.Font = new System.Drawing.Font("Microsoft JhengHei UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblWarrantInfo.Location = new System.Drawing.Point(18, 34);
            this.lblWarrantInfo.Name = "lblWarrantInfo";
            this.lblWarrantInfo.Size = new System.Drawing.Size(150, 23);
            this.lblWarrantInfo.TabIndex = 0;
            this.lblWarrantInfo.Text = "請從左側選擇權證";
            //
            // statusStrip
            //
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip.Location = new System.Drawing.Point(0, 678);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1040, 22);
            this.statusStrip.TabIndex = 1;
            //
            // lblStatus
            //
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(1025, 17);
            this.lblStatus.Spring = true;
            this.lblStatus.Text = "就緒";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // debounceTimer
            //
            this.debounceTimer.Interval = 300;
            this.debounceTimer.Tick += new System.EventHandler(this.debounceTimer_Tick);
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1040, 700);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.statusStrip);
            this.Font = new System.Drawing.Font("Microsoft JhengHei UI", 10F);
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "權證發行風險監控與避險試算系統";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.grpHistory.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistory)).EndInit();
            this.grpInput.ResumeLayout(false);
            this.grpInput.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.ListBox lstWarrants;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.GroupBox grpInput;
        private System.Windows.Forms.Label lblWarrantInfo;
        private System.Windows.Forms.Label lblPriceCaption;
        private System.Windows.Forms.TextBox txtMarketPrice;
        private System.Windows.Forms.Label lblPriceError;
        private System.Windows.Forms.Label lblTheoryCaption;
        private System.Windows.Forms.Label lblTheory;
        private System.Windows.Forms.Label lblMoneynessCaption;
        private System.Windows.Forms.Label lblMoneyness;
        private System.Windows.Forms.Label lblHedgeCaption;
        private System.Windows.Forms.Label lblHedge;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.GroupBox grpHistory;
        private System.Windows.Forms.DataGridView dgvHistory;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCreatedTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMarketPrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTheoryPrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHedgeQty;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.Timer debounceTimer;
    }
}
