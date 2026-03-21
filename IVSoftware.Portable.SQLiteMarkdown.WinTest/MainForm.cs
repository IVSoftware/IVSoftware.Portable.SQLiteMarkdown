using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.MSTest;
using IVSoftware.Portable.SQLiteMarkdown.WinTest.OP;
using SQLite;
using IVSoftware.Portable;
using IVSoftware.WinForms;
using System.Diagnostics;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.SQLiteMarkdown.Util;
using System.Collections.Specialized;

namespace IVSoftware.Portable.SQLiteMarkdown.WinTest
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            QFSUT = new QFSUT();
            QFSUT.ItemPropertyChanged += (sender, e) =>
            {
            };
            vcView.SelectionMode = SelectionMode.Single;
            vcView.CanMultiselect = () =>
                vcView.SelectionMode != SelectionMode.None &&
                ModifierKeys == Keys.Control;
            vcView.ItemsSource = QFSUT;
            vcView.DataTemplate = new CollectionView.CollectionViewDataTemplate<SelectableQFViewCard>();
            if (vcView.ItemsSource is IObservableQueryFilterSource qfs)
            {
                textInputText.TextChanged += (sender, e) =>
                {
                    qfs.InputText = textInputText.Text;
                };
                textInputText.KeyDown += (sender, e) =>
                {
                    switch (e.KeyData)
                    {
                        case Keys.Escape:
                            e.SuppressKeyPress = true;
                            break;
                        case Keys.Return:
                            e.SuppressKeyPress = true;
                            qfs.Commit();
                            break;
                        case Keys.Back:
                            if (textInputText.TextLength == 0)
                            {
                                qfs.Clear(all: true);
                            }
                            break;
                    }
                };
                qfs.MemoryDatabase = new SQLiteConnection(":memory:"); 
                qfs.MemoryDatabase.PopulateDemoDatabase<SelectableQFModel>(includeLive: true);
                qfs.PropertyChanged += (sender, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(qfs.InputText):
                            // This is a two-way binding accomodation.
                            if(textInputText.Text != qfs.InputText)
                            {
                                textInputText.Text = qfs.InputText;
                            }
                            break;
                        case nameof(IObservableQueryFilterSource.Busy):
                            textInputText.Cursor = 
                                qfs.Busy
                                ? Cursors.WaitCursor
                                : Cursors.Default;
                            break;
                        case nameof(IObservableQueryFilterSource.SearchEntryState):
                            switch (qfs.SearchEntryState)
                            {
                                case SearchEntryState.Cleared:
                                case SearchEntryState.QueryEmpty:
                                    labelSearchIcon.ForeColor = ColorTranslator.FromHtml("444444");
                                    break;
                                case SearchEntryState.QueryENB:
                                    labelSearchIcon.ForeColor = Color.Salmon;
                                    break;
                                case SearchEntryState.QueryEN:
                                    labelSearchIcon.ForeColor = Color.ForestGreen;
                                    break;
                                default:
                                    /* G T K - N O O P */
                                    break;
                            }
                            break;
                        case nameof(IObservableQueryFilterSource.FilteringState):
                            switch (qfs.FilteringState)
                            {
                                case FilteringState.Armed:
                                    labelSearchIcon.ForeColor = Color.ForestGreen;
                                    break;
                                case FilteringState.Active:
                                    labelSearchIcon.ForeColor = Color.Salmon;
                                    break;
                                default:
                                    /* G T K - N O O P */
                                    break;
                            }
                            break;
                        case nameof(IObservableQueryFilterSource.IsFiltering):
                            textInputText.PlaceholderText = qfs.Placeholder;
                            labelSearchIcon.Text = 
                                qfs.IsFiltering
                                ? GlyphProvider.IconBasics.Filter.ToGlyph()
                                : GlyphProvider.IconBasics.Search.ToGlyph();
                            break;
                    }
                };
            }
            buttonClear.Click += (sender, e) =>
            {
                if (vcView.ItemsSource is IObservableQueryFilterSource qfs)
                {
                    qfs.Clear();
                    switch (qfs.FilteringState)
                    {
                        case FilteringState.Ineligible:
                            ActiveControl = null;
                            break;
                        case FilteringState.Armed:
                            break;
                        case FilteringState.Active:
                            break;
                        default:
                            throw new NotImplementedException($"Bad case: {qfs.FilteringState}");
                    }
                }
            };
            Disposed += (sender, e) =>
            {
                if (vcView.ItemsSource is IObservableQueryFilterSource qfs)
                {
                    qfs.MemoryDatabase?.Dispose();
                }
            };
            tsmiQuery.Click += (sender, e) =>
            {
                BeginInvoke(() => 
                { 
                    if (vcView.ItemsSource is IObservableQueryFilterSource qfs)
                    {
                        QFSUT.FilteringStateForTest = FilteringState.Ineligible;
                        tsmiFilter.Checked = false;
                    }
                });
            };
            tsmiFilter.Click += (sender, e) =>
            {
                BeginInvoke(() =>
                {
                    QFSUT.FilteringStateForTest = FilteringState.Active;
                    tsmiQuery.Checked = false;
                });
            };
            tsmiEvaluate.Click += (sender, e) =>
            {
                BeginInvoke(() =>
                {
                    MessageBox.Show(QFSUT.ParseSqlMarkdown());
                });
            };
            tsmiCombo.SelectedIndexChanged += (sender, e) =>
            {
                contextMenuQueryFilter.Close();
                BeginInvoke(() =>
                {
                    if (tsmiCombo.SelectedItem?.ToString() is { } expr)
                    {
                        MessageBox.Show(expr.ParseSqlMarkdown<SelectableQFModel>());
                    }
                });
            };
            ExtensionsOR.StackPrompt += (sender, e) =>
            {
                BeginInvoke(() =>
                {
                    MessageBox.Show(sender?.ToString());
                });
            };
            tsmiPromptEachStep.CheckedChanged += (sender, e) =>
            {
                BeginInvoke(() =>
                {
                    ExtensionsOR.PromptEachStep = tsmiPromptEachStep.Checked;
                });
            };

            labelSearchIcon.Click += (sender, e) => 
            {
                // The idea hear is to leave filtering mode but
                // use the existing text as a new query.
                var tmp = QFSUT.InputText;
                QFSUT.Clear(all: true);
                QFSUT.InputText = tmp;
                QFSUT.Commit();
            };
            _ = InitAsync();
        }
        async Task InitAsync()
        {
            await GlyphProvider.BoostCache();

            // Retrieve the FontFamily from the PrivateFontCollection
            // (implemented inside the WinForms-specific NuGet package).
            if (GlyphProvider.Providers[typeof(GlyphProvider.IconBasics)] is GlyphProvider provider &&
                provider.GetFontFamily() is FontFamily fontFamily)
            {
                IconBasics11 = new Font(fontFamily, 11F);
                labelSearchIcon.UseCompatibleTextRendering = true;
                labelSearchIcon.Font = IconBasics11;
                labelSearchIcon.Margin = new ();
                labelSearchIcon.Padding = new Padding(2, 5, -1, 0);
            }
            else
            {   /* G T K */
            }
            labelSearchIcon.Text = GlyphProvider.IconBasics.Search.ToGlyph();


#if DEBUG
            string actual;
            Debug.Assert(DateTime.Now.Date == new DateTime(2026, 3, 21).Date, "Don't forget DEBUG");
            if(Controls[nameof(InfoOverlay)] is Control control) control.Visible = false;
            QFSUT.InputText = "green";
            QFSUT.Commit();
            { }
            var mdc = QFSUT.Model.To<MarkdownContext>();
            actual = mdc.StateReport();
            actual = mdc.SerializeTopology();
            { }
            foreach (var item in QFSUT)
            {

            }


            //QFSUT.InputText += " apple";
            //await (QFSUT);
            //actual = mdc.StateReport();
            //actual = mdc.SerializeTopology();
            //{ }
#endif
        }

        /// <summary>
        /// QSF Under Test for ad hoc states and expr evals.
        /// </summary>
        private QFSUT QFSUT { get; }

        public static Font? IconBasics11 { get; private set; }
    }
    class QFSUT : ObservableQueryFilterSource<SelectableQFModel> 
    {
        public QFSUT()
        {
            Debug.Assert(DateTime.Now.Date == new DateTime(2026, 3, 21).Date, "Don't forget disabled");
            QueryFilterConfig = QueryFilterConfig.Query;
        }
        public new FilteringState FilteringStateForTest
        {
            get => FilteringState;
            set
            {
                FilteringState = value;
            }
        }
        protected override void OnCanonicalSupersetChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCanonicalSupersetChanged(e);
            OnCollectionChanged(e);
        }
    }
}
