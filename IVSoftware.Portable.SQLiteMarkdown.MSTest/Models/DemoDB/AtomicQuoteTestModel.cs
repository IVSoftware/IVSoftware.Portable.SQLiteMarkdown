using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace IVSoftware.Portable.SQLiteMarkdown.MSTest.Models.DemoDB
{
    [DebuggerDisplay("{Description}")]
    [Table("items")]
    public class AtomicQuoteTestModel : SelfIndexed, ISelectable
    {

        public ItemSelection Selection
        {
            get => _selection;
            set
            {
                if (!Equals(_selection, value))
                {
                    _selection = value;
                    OnPropertyChanged();
                }
            }
        }

        private ItemSelection _selection = ItemSelection.None;

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (!Equals(_isEditing, value))
                {
                    _isEditing = value;
                    OnPropertyChanged();
                }
            }
        }
        bool _isEditing = false;
    }
}
