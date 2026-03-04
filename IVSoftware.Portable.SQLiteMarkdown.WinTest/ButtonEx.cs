using IVSoftware.Portable;
using IVSoftware.WinForms;

namespace IVSoftware.Portable.SQLiteMarkdown.WinTest
{
    public class ButtonEx : Button
    {
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            TabStop = false;
            SetStyle(ControlStyles.Selectable, false);
        }
    }
}
