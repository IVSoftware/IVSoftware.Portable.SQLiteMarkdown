using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
using IVSoftware.Portable.SQLiteMarkdown.MSTest;
using IVSoftware.Portable.SQLiteMarkdown.WinTest.OP;
using Newtonsoft.Json;
using SQLite;

namespace IVSoftware.Portable.SQLiteMarkdown.WinTest
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            QFSUT = new ObservableQueryFilterSource<SelectableQFModel>();
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
                        case nameof(IObservableQueryFilterSource.FilteringState):
                            switch (qfs.FilteringState)
                            {
                                case FilteringState.Ineligible:
                                    labelSearchIcon.ForeColor = ForeColor;
                                    break;
                                case FilteringState.Armed:
                                    labelSearchIcon.ForeColor = Color.Salmon;
                                    break;
                                case FilteringState.Active:
                                    labelSearchIcon.ForeColor = Color.ForestGreen;
                                    break;
                                default:
                                    throw new NotImplementedException($"Bad case: {qfs.FilteringState}");
                            }
                            break;
                        case nameof(IObservableQueryFilterSource.IsFiltering):
                            textInputText.PlaceholderText = qfs.Placeholder;
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
        }

        /// <summary>
        /// QSF Under Test for ad hoc states and expr evals.
        /// </summary>
        private ObservableQueryFilterSource<SelectableQFModel> QFSUT { get; } 
    }
}
