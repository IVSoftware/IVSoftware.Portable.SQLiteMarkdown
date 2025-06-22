using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
