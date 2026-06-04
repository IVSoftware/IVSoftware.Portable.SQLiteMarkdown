using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IVSoftware.Portable.SQLiteMarkdown.WinTest.OP
{
    public partial class InfoContentForm : Form
    {
        const int MAX_CONTENT_WIDTH = 500;
        const int MIN_CONTENT_WIDTH = 320;
        const int OWNER_MARGIN = 34;
        const int MIN_BODY_HEIGHT = 96;
        const int TITLE_HEIGHT = 44;
        const int DIVIDER_HEIGHT = 1;
        const int FOOTER_HEIGHT = 40;

        public InfoContentForm()
        {
            InitializeComponent();
        }

        public string CaptionText
        {
            get => labelWelcome.Text;
            set => labelWelcome.Text = value;
        }

        public string MessageText
        {
            get => labelInfo.Text;
            set => labelInfo.Text = value;
        }

        public Size GetPreferredOverlaySize(Size ownerClientSize)
        {
            var width = Math.Clamp(ownerClientSize.Width - OWNER_MARGIN, MIN_CONTENT_WIDTH, MAX_CONTENT_WIDTH);
            var bodyWidth = Math.Max(
                100,
                width -
                (int)gridInfo.ColumnStyles[0].Width -
                (int)gridInfo.ColumnStyles[1].Width -
                (int)gridInfo.ColumnStyles[3].Width -
                labelInfo.Margin.Horizontal);

            var proposed = new Size(bodyWidth, int.MaxValue);
            var flags =
                TextFormatFlags.WordBreak |
                TextFormatFlags.TextBoxControl |
                TextFormatFlags.NoPrefix;
            var messageSize = TextRenderer.MeasureText(MessageText, labelInfo.Font, proposed, flags);

            var iconHeight = iconInfo.Height + iconInfo.Margin.Vertical;
            var bodyHeight = Math.Max(MIN_BODY_HEIGHT, Math.Max(messageSize.Height + labelInfo.Margin.Vertical, iconHeight));
            var footerHeight =
                labelDSA.Visible || checkBoxDSA.Visible
                ? FOOTER_HEIGHT
                : 0;
            var dividerHeight = labelHR.Visible ? DIVIDER_HEIGHT : 0;
            var desiredHeight = TITLE_HEIGHT + bodyHeight + dividerHeight + footerHeight;
            var minHeight = TITLE_HEIGHT + MIN_BODY_HEIGHT + dividerHeight + footerHeight;
            var maxHeight = Math.Max(minHeight, ownerClientSize.Height - OWNER_MARGIN);

            if (desiredHeight > maxHeight)
            {
                bodyHeight = Math.Max(MIN_BODY_HEIGHT, maxHeight - TITLE_HEIGHT - dividerHeight - footerHeight);
                desiredHeight = TITLE_HEIGHT + bodyHeight + dividerHeight + footerHeight;
            }

            gridInfo.RowStyles[0].Height = TITLE_HEIGHT;
            gridInfo.RowStyles[1].Height = bodyHeight;
            gridInfo.RowStyles[2].Height = dividerHeight;
            gridInfo.RowStyles[3].Height = footerHeight;

            return new Size(width, desiredHeight);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            using var path = new GraphicsPath();
            int radius = 30;
            var rect = ClientRectangle;
            if (rect.Width > 0 && rect.Height > 0)
            {
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();
                Region = new Region(path);
            }
        }
    }
}
