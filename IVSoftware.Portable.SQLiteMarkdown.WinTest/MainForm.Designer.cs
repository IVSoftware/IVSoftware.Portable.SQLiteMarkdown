using IVSoftware.Portable.SQLiteMarkdown.WinTest.OP;

namespace IVSoftware.Portable.SQLiteMarkdown.WinTest
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            Grid = new TableLayoutPanel();
            vcView = new CollectionView();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            textInputText = new TextBox();
            contextMenuQueryFilter = new ContextMenuStrip(components);
            tsmiQuery = new ToolStripMenuItem();
            tsmiFilter = new ToolStripMenuItem();
            tsmiSeparator1 = new ToolStripSeparator();
            tsmiEvaluate = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            tsmiCombo = new ToolStripComboBox();
            tsmiPromptEachStep = new ToolStripMenuItem();
            buttonClear = new ButtonEx();
            labelSearchIcon = new Label();
            Grid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)vcView).BeginInit();
            contextMenuQueryFilter.SuspendLayout();
            SuspendLayout();
            // 
            // Grid
            // 
            Grid.ColumnCount = 3;
            Grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
            Grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            Grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
            Grid.Controls.Add(vcView, 0, 1);
            Grid.Controls.Add(textInputText, 1, 0);
            Grid.Controls.Add(buttonClear, 2, 0);
            Grid.Controls.Add(labelSearchIcon, 0, 0);
            Grid.Dock = DockStyle.Fill;
            Grid.Location = new Point(5, 5);
            Grid.Name = "Grid";
            Grid.RowCount = 2;
            Grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            Grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            Grid.Size = new Size(508, 894);
            Grid.TabIndex = 0;
            // 
            // vcView
            // 
            vcView.AllowUserToAddRows = false;
            vcView.AllowUserToDeleteRows = false;
            vcView.BackgroundColor = Color.LightBlue;
            vcView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            vcView.ColumnHeadersVisible = false;
            vcView.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1 });
            Grid.SetColumnSpan(vcView, 3);
            vcView.Dock = DockStyle.Fill;
            vcView.Location = new Point(3, 53);
            vcView.Name = "vcView";
            vcView.RowHeadersVisible = false;
            vcView.RowHeadersWidth = 62;
            vcView.Size = new Size(502, 838);
            vcView.TabIndex = 0;
            vcView.VirtualMode = true;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewTextBoxColumn1.MinimumWidth = 8;
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // textInputText
            // 
            textInputText.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            textInputText.ContextMenuStrip = contextMenuQueryFilter;
            textInputText.Font = new Font("Segoe UI", 12F);
            textInputText.Location = new Point(53, 5);
            textInputText.Name = "textInputText";
            textInputText.PlaceholderText = "Search Items";
            textInputText.Size = new Size(402, 39);
            textInputText.TabIndex = 1;
            // 
            // contextMenuQueryFilter
            // 
            contextMenuQueryFilter.Font = new Font("Segoe UI", 12F);
            contextMenuQueryFilter.ImageScalingSize = new Size(24, 24);
            contextMenuQueryFilter.Items.AddRange(new ToolStripItem[] { tsmiQuery, tsmiFilter, tsmiSeparator1, tsmiEvaluate, toolStripSeparator2, tsmiCombo, tsmiPromptEachStep });
            contextMenuQueryFilter.Name = "contextMenuStrip1";
            contextMenuQueryFilter.Size = new Size(311, 212);
            // 
            // tsmiQuery
            // 
            tsmiQuery.Checked = true;
            tsmiQuery.CheckOnClick = true;
            tsmiQuery.CheckState = CheckState.Checked;
            tsmiQuery.Name = "tsmiQuery";
            tsmiQuery.Size = new Size(310, 38);
            tsmiQuery.Text = "Query Expr";
            // 
            // tsmiFilter
            // 
            tsmiFilter.CheckOnClick = true;
            tsmiFilter.Name = "tsmiFilter";
            tsmiFilter.Size = new Size(310, 38);
            tsmiFilter.Text = "Filter Expr";
            // 
            // tsmiSeparator1
            // 
            tsmiSeparator1.Name = "tsmiSeparator1";
            tsmiSeparator1.Size = new Size(307, 6);
            // 
            // tsmiEvaluate
            // 
            tsmiEvaluate.Name = "tsmiEvaluate";
            tsmiEvaluate.Size = new Size(310, 38);
            tsmiEvaluate.Text = "Evaluate";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(307, 6);
            // 
            // tsmiCombo
            // 
            tsmiCombo.Font = new Font("Segoe UI", 11F);
            tsmiCombo.Items.AddRange(new object[] { "[animal]", "\"\"\"Tom Tester's\"\"\"", "\"Tom Tester's\"", "'Tom Tester'" });
            tsmiCombo.Name = "tsmiCombo";
            tsmiCombo.Size = new Size(250, 38);
            tsmiCombo.Text = "Select";
            // 
            // tsmiPromptEachStep
            // 
            tsmiPromptEachStep.CheckOnClick = true;
            tsmiPromptEachStep.Name = "tsmiPromptEachStep";
            tsmiPromptEachStep.Size = new Size(310, 38);
            tsmiPromptEachStep.Text = "Prompt Each Step";
            tsmiPromptEachStep.Visible = false;
            // 
            // buttonClear
            // 
            buttonClear.Dock = DockStyle.Fill;
            buttonClear.ForeColor = Color.FromArgb(0, 0, 0, 0);
            buttonClear.Location = new Point(461, 3);
            buttonClear.Name = "buttonClear";
            buttonClear.Size = new Size(44, 44);
            buttonClear.TabIndex = 2;
            buttonClear.TabStop = false;
            buttonClear.Text = "X";
            buttonClear.UseVisualStyleBackColor = false;
            // 
            // labelSearchIcon
            // 
            labelSearchIcon.AutoSize = true;
            labelSearchIcon.Dock = DockStyle.Fill;
            labelSearchIcon.Location = new Point(3, 0);
            labelSearchIcon.Name = "labelSearchIcon";
            labelSearchIcon.Size = new Size(44, 50);
            labelSearchIcon.TabIndex = 3;
            labelSearchIcon.Text = "🔍";
            labelSearchIcon.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(518, 904);
            Controls.Add(Grid);
            Name = "MainForm";
            Padding = new Padding(5);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MainForm";
            Grid.ResumeLayout(false);
            Grid.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)vcView).EndInit();
            contextMenuQueryFilter.ResumeLayout(false);

            Controls.Add(new InfoOverlay
            {
                Dock = DockStyle.Fill,
                Visible = true,
            });
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel Grid;
        private CollectionView vcView;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private TextBox textInputText;
        private ButtonEx buttonClear;
        private Label labelSearchIcon;
        private ContextMenuStrip contextMenuQueryFilter;
        private ToolStripMenuItem tsmiQuery;
        private ToolStripMenuItem tsmiFilter;
        private ToolStripSeparator tsmiSeparator1;
        private ToolStripMenuItem tsmiEvaluate;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripComboBox tsmiCombo;
        private ToolStripMenuItem tsmiPromptEachStep;
    }
}
