using IVSoftware.Portable.SQLiteMarkdown.Collections;
using IVSoftware.Portable.SQLiteMarkdown.Common;
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
                qfs.MemoryDatabase = CreateDemoDatabase<SelectableQFModel>();
                qfs.PropertyChanged += (sender, e) =>
                {
                    switch (e.PropertyName)
                    {
                        case nameof(qfs.InputText):
                            textInputText.Text = qfs.InputText;
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

        private SQLiteConnection CreateDemoDatabase<T>() where T : new()
        {
            var imdb = new SQLiteConnection(":memory:");
            imdb.CreateTable<T>();

            var list = new List<T>();

            void Add(string description, string tags, bool isChecked, List<string> keywords = null)
            {
                var instance = new T();
                typeof(T).GetProperty("Description")?.SetValue(instance, description);
                typeof(T).GetProperty("Tags")?.SetValue(instance, tags);
                typeof(T).GetProperty("IsChecked")?.SetValue(instance, isChecked);
                if (keywords != null)
                {
                    var json = JsonConvert.SerializeObject(keywords);
                    typeof(T).GetProperty("Keywords")?.SetValue(instance, json);
                }
                list.Add(instance);
            }
            // Unit tested
            Add("Brown Dog", "[canine] [color]", false, new() { "loyal", "friend", "furry" });
            Add("Green Apple", "[fruit] [color]", false, new() { "tart", "snack", "healthy" });
            Add("Yellow Banana", "[fruit] [color]", false);
            Add("Blue Bird", "[bird] [color]", false, new() { "sky", "feathered", "song" });
            Add("Red Cherry", "[fruit] [color]", false, new() { "sweet", "summer", "dessert" });
            Add("Black Cat", "[animal] [color]", false);
            Add("Orange Fox", "[animal] [color]", false);
            Add("White Rabbit", "[animal] [color]", false, new() { "bunny", "soft", "jump" });
            Add("Purple Grape", "[fruit] [color]", false);
            Add("Gray Wolf", "[animal] [color]", false, new() { "pack", "howl", "wild" });
            Add("Pink Flamingo", "[bird] [color]", false);
            Add("Golden Lion", "[animal] [color]", false);
            Add("Brown Bear", "[animal] [color]", false, new() { "strong", "wild", "forest" });
            Add("Green Pear", "[fruit] [color]", false);
            Add("Red Strawberry", "[fruit] [color]", false);
            Add("Black Panther", "[animal] [color]", false, new() { "stealthy", "feline", "night" });
            Add("Yellow Lemon", "[fruit] [color]", false);
            Add("White Swan", "[bird] [color]", false);
            Add("Purple Plum", "[fruit] [color]", false);
            Add("Blue Whale", "[marine-mammal] [ocean]", false, new() { "ocean", "mammal", "giant" });
            Add("Elephant", "[animal]", false, new() { "trunk", "herd", "safari" });
            Add("Pineapple", "[fruit]", false);
            Add("Shark", "[fish]", false);
            Add("Owl", "[bird]", false);
            Add("Giraffe", "[animal]", false);
            Add("Coconut", "[fruit]", false);
            Add("Kangaroo", "[animal]", false, new() { "bounce", "outback", "marsupial" });
            Add("Dragonfruit", "[fruit]", false);
            Add("Turtle", "[animal]", false);
            Add("Mango", "[fruit]", false);
            Add("Should NOT match an expression with an \"animal\" tag.", "[not animal]", false);
            // Live-demo specific.
            Add("Appetizer Plate", "[dish]", false, new() { "starter", "appealing", "snack" });
            Add("Errata", "[notes]", false, new() { "crunchy", "green", "appended" });
            Add("Happy Camper", "[phrase]", false, new() { "joyful", "camp", "approach-west" });
            Add("Great example - Markdown Demo", "[app] [portable]", false, new() { "digital", "mobile", "software" });
            Add("Application Form", "[document]", false, new() { "paperwork", "apply" });
            Add("App Store", "[app]", false, new() { "digital", "mobile", "software" });


            imdb.InsertAll(list);
            return imdb;
        }
    }
}
