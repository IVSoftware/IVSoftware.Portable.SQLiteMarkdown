namespace IVSoftware.Portable.SQLiteMarkdown.WinTest.OP
{
    partial class InfoContentForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InfoContentForm));
            gridInfo = new TableLayoutPanel();
            iconInfo = new PictureBox(); 
            labelWelcome = new RichTextBox();
            labelInfo = new Label();
            labelVR = new Label();
            labelHR = new Label();
            checkBoxDSA = new CheckBox();
            labelDSA = new Label();
            gridInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)iconInfo).BeginInit();
            SuspendLayout();
            // 
            // gridInfo
            // 
            gridInfo.BackColor = Color.FromArgb(178, 223, 219);
            gridInfo.ColumnCount = 4;
            gridInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11.9047623F));
            gridInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 2F));
            gridInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75.10822F));
            gridInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.7489157F)); 
            
            gridInfo.Controls.Add(labelWelcome, 2, 0);   // Welcome banner
            gridInfo.SetColumnSpan(labelWelcome, 2);     // Span columns 1, 2, and 3
            gridInfo.Controls.Add(iconInfo, 0, 1);       // Icon
            gridInfo.Controls.Add(labelVR, 1, 1);        // Divider next to icon
            gridInfo.Controls.Add(labelInfo, 2, 1);      // Main body text (row 1)

            gridInfo.Controls.Add(labelHR, 0, 2);        // Horizontal rule (row 2)
            gridInfo.SetColumnSpan(labelHR, 4);          // Make sure it still spans all columns

            gridInfo.Controls.Add(labelDSA, 2, 3);       // DSA label (row 3)
            gridInfo.Controls.Add(checkBoxDSA, 3, 3);    // DSA checkbox (row 3)

            gridInfo.Dock = DockStyle.Fill;
            gridInfo.Location = new Point(0, 0);
            gridInfo.Name = "gridInfo";
            gridInfo.RowCount = 4;
            gridInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));   // Row 0: Welcome
            gridInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));   // Row 1: Main body
            gridInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));    // Row 2: Horizontal line
            gridInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));   // Row 3: DSA
            gridInfo.Size = new Size(500, 350);
            gridInfo.TabIndex = 0;
            // 
            // iconInfo
            // 
            iconInfo.Anchor = AnchorStyles.None;
            iconInfo.Location = new Point(8, 130);
            iconInfo.Margin = new Padding(5, 0, 0, 0);
            iconInfo.Name = "iconInfo";
            iconInfo.Size = new Size(48, 48);
            iconInfo.SizeMode = PictureBoxSizeMode.AutoSize;
            iconInfo.TabIndex = 1;
            iconInfo.TabStop = false; 
            iconInfo.Image = SystemIcons.Information.ToBitmap();
            // 
            // labelWelcome
            // 
            labelWelcome.BackColor = Color.FromArgb(178, 223, 219);  // Match grid background
            labelWelcome.BorderStyle = BorderStyle.None;
            labelWelcome.Dock = DockStyle.Fill;
            labelWelcome.Font = new Font("Segoe UI", 12F);
            labelWelcome.ReadOnly = true;
            labelWelcome.ScrollBars = RichTextBoxScrollBars.None;
            labelWelcome.Text = "Welcome!";
            labelWelcome.ForeColor = ColorTranslator.FromHtml("#444444");
            labelWelcome.Margin = new Padding(10, 8, 0, 0);
            // 
            // labelInfo
            // 
            gridInfo.SetColumnSpan(labelInfo, 2);
            labelInfo.Dock = DockStyle.Fill;
            labelInfo.Font = new Font("Segoe UI", 8.5F);
            labelInfo.Location = new Point(71, 0);
            labelInfo.Margin = new Padding(10, 3, 3, 0);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new Size(426, 309);
            labelInfo.TabIndex = 2; 
            labelInfo.Text = @"
Try these simple searches:
    • animal
    • color

Compare:
    • app vs [app]
    • gre app vs gre [app]

💡 Hint: Tap [×] twice to reset:
    • First clears filter text.
    • Second returns to query mode.".Trim();


            labelInfo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelVR
            // 
            labelVR.Anchor = AnchorStyles.Top|AnchorStyles.Bottom;
            labelVR.BackColor = Color.FromArgb(170, 170, 170);
            labelVR.Location = new Point(62, 154);
            labelVR.Name = "labelVR";
            labelVR.Size = new Size(1, 1);
            labelVR.TabIndex = 3;
            // 
            // labelHR
            // 
            labelHR.BackColor = Color.FromArgb(170, 170, 170);
            gridInfo.SetColumnSpan(labelHR, 4);
            labelHR.Dock = DockStyle.Fill;
            labelHR.Location = new Point(10, 309);
            labelHR.Margin = new Padding(10, 0, 10, 0);
            labelHR.Name = "labelHR";
            labelHR.Size = new Size(480, 1);
            labelHR.TabIndex = 3;
            labelHR.Visible = false;
            // 
            // checkBoxDSA
            // 
            checkBoxDSA.Anchor = AnchorStyles.None;
            checkBoxDSA.AutoSize = true;
            checkBoxDSA.Font = new Font("Segoe UI", 8F);
            checkBoxDSA.Location = new Point(453, 319);
            checkBoxDSA.Margin = new Padding(3, 3, 10, 3);
            checkBoxDSA.Name = "checkBoxDSA";
            checkBoxDSA.Size = new Size(22, 21);
            checkBoxDSA.TabIndex = 0;
            checkBoxDSA.UseVisualStyleBackColor = true;
            checkBoxDSA.Visible = false;
            // 
            // labelDSA
            // 
            labelDSA.Anchor = AnchorStyles.Right;
            labelDSA.AutoSize = true;
            labelDSA.Font = new Font("Microsoft Sans Serif", 8.25F);
            labelDSA.ForeColor = Color.FromArgb(34, 34, 34);
            labelDSA.Location = new Point(178, 317);
            labelDSA.Margin = new Padding(3, 0, 10, 0);
            labelDSA.Name = "labelDSA";
            labelDSA.Padding = new Padding(0, 0, 0, 5);
            labelDSA.Size = new Size(247, 25);
            labelDSA.TabIndex = 4;
            labelDSA.Text = "Don't show this message again.";
            labelDSA.Visible = false;
            // 
            // InfoContentForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(500, 350);
            Controls.Add(gridInfo);
            FormBorderStyle = FormBorderStyle.None;
            MaximumSize = new Size(500, 350);
            Name = "InfoContentForm";
            StartPosition = FormStartPosition.Manual;
            Text = "InfoContentForm";
            gridInfo.ResumeLayout(false);
            gridInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)iconInfo).EndInit();
            ResumeLayout(false);
        }
        #endregion

        private TableLayoutPanel gridInfo = null!;
        private CheckBox checkBoxDSA = null!;
        private PictureBox iconInfo = null!;
        private RichTextBox labelWelcome = null!;
        private Label labelInfo = null!;
        private Label labelVR = null!;
        private Label labelHR = null!;
        private Label labelDSA = null!;
    }
}