using IVSoftware.Portable.SQLiteMarkdown;
using IVSoftware.Portable.SQLiteMarkdown.WinTest.OP;

namespace IVSoftware.Portable.SQLiteMarkdown.WinTest.OP
{
    public partial class InfoOverlay
        : Control
        , IMessageFilter
    {
        public InfoOverlay()
        {
            InitializeComponent();

            Application.AddMessageFilter(this);
            // Zero size this actual control so that the
            // translucent overlay shows content beneath it.
            SizeChanged += (sender, e) => Size = new Size();
            _overlay = new Form
            {
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.None,
                Opacity = 0.4F,
                BackColor = Color.Black,
            };
            _overlayContent = new InfoContentForm
            {
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.None,
            };
            Disposed += (sender, e) =>
            {
                Application.RemoveMessageFilter(this);
                _overlay.Dispose();
                _overlayContent.Dispose();
            };
        }

        public string InfoText
        {
            get => _infoText;
            set
            {
                if (!Equals(_infoText, value))
                {
                    _infoText = value;
                }
            }
        }
        string _infoText = string.Empty;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InfoOverlay));
            tableLayoutPanelOverlayMock = new TableLayoutPanel();
            gridInfo = new InfoLayoutPanel();
            iconInfo = new PictureBox();
            labelInfo = new Label();
            labelVR = new Label();
            label1 = new Label();
            checkBoxDSA = new CheckBox();
            label2 = new Label();
            tableLayoutPanelOverlayMock.SuspendLayout();
            gridInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)iconInfo).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanelOverlayMock
            // 
            tableLayoutPanelOverlayMock.ColumnCount = 3;
            tableLayoutPanelOverlayMock.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 4.545455F));
            tableLayoutPanelOverlayMock.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 90.90909F));
            tableLayoutPanelOverlayMock.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 4.5454545F));
            tableLayoutPanelOverlayMock.Controls.Add(gridInfo, 1, 1);
            tableLayoutPanelOverlayMock.Dock = DockStyle.Fill;
            tableLayoutPanelOverlayMock.Location = new Point(0, 0);
            tableLayoutPanelOverlayMock.Name = "tableLayoutPanelOverlayMock";
            tableLayoutPanelOverlayMock.RowCount = 3;
            tableLayoutPanelOverlayMock.RowStyles.Add(new RowStyle(SizeType.Percent, 33.29646F));
            tableLayoutPanelOverlayMock.RowStyles.Add(new RowStyle(SizeType.Percent, 36.7256622F));
            tableLayoutPanelOverlayMock.RowStyles.Add(new RowStyle(SizeType.Percent, 29.9778767F));
            tableLayoutPanelOverlayMock.Size = new Size(518, 904);
            tableLayoutPanelOverlayMock.TabIndex = 0;
            // 
            // gridInfo
            // 
            gridInfo.BackColor = Color.FromArgb(178, 223, 219);
            gridInfo.ColumnCount = 4;
            gridInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11.9047623F));
            gridInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 2F));
            gridInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75.10822F));
            gridInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.7489157F));
            gridInfo.Controls.Add(iconInfo, 0, 0);
            gridInfo.Controls.Add(labelInfo, 2, 0);
            gridInfo.Controls.Add(labelVR, 1, 0);
            gridInfo.Controls.Add(label1, 0, 1);
            gridInfo.Controls.Add(checkBoxDSA, 3, 2);
            gridInfo.Controls.Add(label2, 2, 2);
            gridInfo.Dock = DockStyle.Fill;
            gridInfo.Location = new Point(26, 304);
            gridInfo.Name = "gridInfo";
            gridInfo.RowCount = 3;
            gridInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            gridInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
            gridInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            gridInfo.Size = new Size(464, 326);
            gridInfo.TabIndex = 0;
            // 
            // iconInfo
            // 
            iconInfo.Anchor = AnchorStyles.None; 
            iconInfo.Image = SystemIcons.Information.ToBitmap();
            iconInfo.Location = new Point(6, 118);
            iconInfo.Margin = new Padding(5, 0, 0, 0);
            iconInfo.Name = "iconInfo";
            iconInfo.Size = new Size(48, 48);
            iconInfo.SizeMode = PictureBoxSizeMode.AutoSize;
            iconInfo.TabIndex = 1;
            iconInfo.TabStop = false;
            // 
            // labelInfo
            // 
            gridInfo.SetColumnSpan(labelInfo, 2);
            labelInfo.Dock = DockStyle.Fill;
            labelInfo.Font = new Font("Segoe UI", 8.5F);
            labelInfo.Location = new Point(67, 0);
            labelInfo.Margin = new Padding(10, 0, 3, 0);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new Size(394, 285);
            labelInfo.TabIndex = 2;
            labelInfo.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelVR
            // 
            labelVR.Anchor = AnchorStyles.None;
            labelVR.BackColor = Color.FromArgb(170, 170, 170);
            labelVR.Location = new Point(58, 27);
            labelVR.Name = "labelVR";
            labelVR.Size = new Size(1, 230);
            labelVR.TabIndex = 3;
            // 
            // label1
            // 
            label1.BackColor = Color.FromArgb(170, 170, 170);
            gridInfo.SetColumnSpan(label1, 4);
            label1.Dock = DockStyle.Fill;
            label1.Location = new Point(10, 285);
            label1.Margin = new Padding(10, 0, 10, 0);
            label1.Name = "label1";
            label1.Size = new Size(444, 1);
            label1.TabIndex = 3;
            // 
            // checkBoxDSA
            // 
            checkBoxDSA.Anchor = AnchorStyles.None;
            checkBoxDSA.AutoSize = true;
            checkBoxDSA.Font = new Font("Segoe UI", 8F);
            checkBoxDSA.Location = new Point(419, 295);
            checkBoxDSA.Margin = new Padding(3, 3, 10, 3);
            checkBoxDSA.Name = "checkBoxDSA";
            checkBoxDSA.Size = new Size(22, 21);
            checkBoxDSA.TabIndex = 0;
            checkBoxDSA.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 8.25F);
            label2.ForeColor = Color.FromArgb(34, 34, 34);
            label2.Location = new Point(147, 293);
            label2.Margin = new Padding(3, 0, 10, 0);
            label2.Name = "label2";
            label2.Padding = new Padding(0, 0, 0, 5);
            label2.Size = new Size(247, 25);
            label2.TabIndex = 4;
            label2.Text = "Don't show this message again.";
            label2.Visible = false;

            Controls.Add(tableLayoutPanelOverlayMock);
            Name = "InfoOverlay";
            tableLayoutPanelOverlayMock.ResumeLayout(false);
            gridInfo.ResumeLayout(false);
            gridInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)iconInfo).EndInit();
            ResumeLayout(false);
        }

        public bool PreFilterMessage(ref Message m)
        {
            switch ((Win32Message)m.Msg)
            {
                case Win32Message.WM_LBUTTONDOWN:
                    break;
                case Win32Message.WM_LBUTTONUP:
                    if (Visible) Visible = false;
                    break;
                default:
                    break;
            }
            return false;
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (TopLevelControl is { } mainWindow)
            {
                localTrackFromMainWindow();

                VisibleChanged += (sender, e) =>
                {
                    if (Visible)
                    {
                        _overlay.Show(mainWindow);
                        _overlayContent.Show(mainWindow);
                    }
                    else
                    {
                        _overlay.Hide();
                        _overlayContent.Hide();
                        InfoText = string.Empty;
                        if (IsHandleCreated)
                        {
                            BeginInvoke(() => TopLevelControl?.BringToFront());
                        }
                    }
                };

                mainWindow.Move += (sender, e) =>
                    localTrackFromMainWindow();

                mainWindow.SizeChanged += (sender, e) =>
                {
                    localTrackFromMainWindow();
                };

                void localTrackFromMainWindow()
                {
                    _overlay.Size = mainWindow.ClientSize;
                    _overlayContent.Size = new Size(mainWindow.Width - 50, 400);
                    TopLevelControl.CenterChildInClientRectangle(_overlay);
                    _overlay.CenterChildInClientRectangle(_overlayContent);
                }
                ;
            }
        }

        private readonly Form _overlay;
        private readonly Form _overlayContent;

        private TableLayoutPanel tableLayoutPanelOverlayMock;
        private InfoLayoutPanel gridInfo;
        private CheckBox checkBoxDSA;
        private PictureBox iconInfo;
        private Label labelInfo;
        private Label labelVR;
        private Label label1;
        private Label label2;
    }
    static class InfoOverlayExtensions
    {

        public static void CenterChildInClientRectangle(this Control @this, Control child)
        {
            var parentClientRect = @this.ClientRectangle;
            var x = parentClientRect.Left + (parentClientRect.Width - child.Width) / 2;
            var y = parentClientRect.Top + (parentClientRect.Height - child.Height) / 2;

            var location = new Point(x, y);

            if (child is Form)
                child.Location = @this.PointToScreen(location);
            else
                child.Location = location;
        }
    }
}
