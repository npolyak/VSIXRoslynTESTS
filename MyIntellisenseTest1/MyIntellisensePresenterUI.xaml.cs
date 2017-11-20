using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;

namespace MyIntellisenseTest1
{
    /// <summary>
    /// Interaction logic for MyIntellisensePresenterUI.xaml
    /// </summary>
    public partial class MyIntellisensePresenterUI : UserControl, IPopupIntellisensePresenter, IIntellisenseCommandTarget
    {
        ICompletionSession _completionSession;

#pragma warning disable 0067
        public event EventHandler SurfaceElementChanged;
        public event EventHandler PresentationSpanChanged;
        public event EventHandler<ValueChangedEventArgs<PopupStyles>> PopupStylesChanged;
#pragma warning restore 0067

        public IIntellisenseSession Session => _completionSession;

        public UIElement SurfaceElement => this;


        public ITrackingSpan PresentationSpan
        {
            get
            {
                SnapshotSpan span = 
                    this._completionSession
                            .SelectedCompletionSet
                            .ApplicableTo
                            .GetSpan
                            (
                                this._completionSession.TextView.TextSnapshot
                            );

                NormalizedSnapshotSpanCollection spans = 
                    this._completionSession
                            .TextView
                            .BufferGraph
                            .MapUpToBuffer
                            (
                                span, 
                                this._completionSession.SelectedCompletionSet.ApplicableTo.TrackingMode, 
                                this._completionSession.TextView.TextBuffer);
                if (spans.Count <= 0)
                {
                    throw new InvalidOperationException("Completion Session Applicable-To Span is invalid.  It doesn't map to a span in the session's text view.");
                }
                SnapshotSpan span2 = spans[0];

                return 
                    this._completionSession
                            .TextView
                            .TextBuffer
                            .CurrentSnapshot
                            .CreateTrackingSpan(span2.Span, SpanTrackingMode.EdgeInclusive);
            }
        }

        public PopupStyles PopupStyles
        {
            get { return PopupStyles.PositionClosest; }
        }

        public string SpaceReservationManagerName => "completion";

        public ObservableCollection<Completion> AllCompletions { get; set; }

        public ICollectionView TheCompletionsCollectionView { get; }

        public ObservableCollection<CompletionFilter> TheCompletionFilters { get; }

        public MyIntellisensePresenterUI(ICompletionSession completionSession)
        {
            _completionSession = completionSession;

            _completionSession.TextView.TextBuffer.Changed += TextBuffer_Changed;
            _completionSession.Dismissed += _completionSession_Dismissed;

            ReadOnlyObservableCollection<CompletionSet> completionSets =
                completionSession.CompletionSets;

            IEnumerable<Completion>
                allCompletions = completionSets.SelectMany(completionSet => completionSet.Completions);

            this.AllCompletions = new ObservableCollection<Completion>(allCompletions);

            TheCompletionsCollectionView = CollectionViewSource.GetDefaultView(AllCompletions);

            TheCompletionFilters = new ObservableCollection<CompletionFilter>
            (
                allCompletions
                .GroupBy(compl => compl.IconAutomationText)
                .Where(groupItem => groupItem.Key != "9") // completion tag (gets automatically added)
                .Select(groupItem => new CompletionFilter(groupItem.Key, groupItem.First().IconSource.Clone()))
            );

            foreach(CompletionFilter filter in TheCompletionFilters)
            {
                filter.PropertyChanged += Filter_PropertyChanged;
            }

            TheCompletionsCollectionView.Filter = Filter;

            TheCompletionsCollectionView.CurrentChanged += TheCompletionsCollectionView_CurrentChanged;

            InitializeComponent();

            SelectItemBasedOnFiltering();

            this.AddHandler
            (
                ResendEventBehavior.CustomEvent, 
                (RoutedEventHandler) TheCompletionsListView_MouseDown
            );

            TheCompletionsListView.MouseDoubleClick += TheCompletionsListView_MouseDoubleClick;
        }

        private void TheCompletionsCollectionView_CurrentChanged(object sender, EventArgs e)
        {
            SetCompletionStatus();
        }

        private void TheCompletionsListView_MouseDown(object sender, RoutedEventArgs e)
        {
            ListViewItem listViewItem = e.OriginalSource as ListViewItem;

            if (listViewItem == null)
                return;

            Completion completionItem = listViewItem.DataContext as Completion;

            if (completionItem != null)
            {
                SelectItem(completionItem);
            }
        }

        private void TheCompletionsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _completionSession?.Commit();
        }

        public string UserText =>
            _completionSession
                .SelectedCompletionSet?.ApplicableTo
                                       .GetText
                                        (
                                            _completionSession.SelectedCompletionSet
                                                              .ApplicableTo
                                                              .TextBuffer
                                                               .CurrentSnapshot)?.ToLower();

        private void Filter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TheCompletionsCollectionView.Refresh();
        }

        bool Filter(object obj)
        {
            Completion completion = (Completion) obj;

            string userText = UserText;

            IEnumerable<CompletionFilter> completionFiltersThatAreOn =
                TheCompletionFilters.Where(filt => filt.IsOn).ToList();

            if ((userText != null) && (completion.DisplayText?.ToLower()?.Contains(userText) != true))
                return false;

            return
                (completionFiltersThatAreOn.Count() == 0) ||  
                completionFiltersThatAreOn.FirstOrDefault(filt => filt.CompletionFilterKind == completion.IconAutomationText) != null;
        }

        private void _completionSession_Dismissed(object sender, EventArgs e)
        {
            _completionSession.TextView.TextBuffer.Changed -= TextBuffer_Changed;
            _completionSession.Dismissed -= _completionSession_Dismissed;
        }

        void SelectItem(Completion completionItem)
        {
            TheCompletionsCollectionView.MoveCurrentTo(completionItem);
        }

        void SelectItemBasedOnFiltering()
        {
            string userText = UserText;

            bool moved = false;
            if (!string.IsNullOrEmpty(userText))
            {
                foreach (Completion completion in TheCompletionsCollectionView)
                {
                    if (completion.DisplayText?.ToLower().StartsWith(userText) == true)
                    {
                        TheCompletionsCollectionView.MoveCurrentTo(completion);
                        moved = true;
                        break;
                    }
                }
            }

            if (!moved)
            {
                TheCompletionsCollectionView.MoveCurrentToFirst();
            }

            ScrollAsync();
        }

        void ScrollAsync()
        {
            Dispatcher.BeginInvoke((Action)ScrollIntoView, DispatcherPriority.Send);
        }

        async void ScrollIntoView()
        {
            await Task.Delay(100);
            object selectedItem = TheCompletionsCollectionView.CurrentItem;

            if (selectedItem != null)
            {
                TheCompletionsListView.ScrollIntoView(selectedItem);
            }
        }

        void SetCompletionStatus()
        {
            object selectedItem = TheCompletionsCollectionView.CurrentItem;

            if (selectedItem != null)
            {
                if (_completionSession.SelectedCompletionSet != null)
                {
                    _completionSession.SelectedCompletionSet.SelectionStatus =
                         new CompletionSelectionStatus(selectedItem as Completion, true, true);
                }

            }
        }

        private void TextBuffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            TheCompletionsCollectionView.Refresh();

            SelectItemBasedOnFiltering();
        }

        public bool ExecuteKeyboardCommand(IntellisenseKeyboardCommand command)
        {
            switch (command)
            {
                case IntellisenseKeyboardCommand.Up:
                    MoveCompletionByIdx(-1);
                    return true;
                case IntellisenseKeyboardCommand.PageUp:
                    MoveCompletionByIdx(-10);
                    return true;
                case IntellisenseKeyboardCommand.Down:
                    MoveCompletionByIdx(1);
                    return true;
                case IntellisenseKeyboardCommand.PageDown:
                    MoveCompletionByIdx(10);
                    return true;
                case IntellisenseKeyboardCommand.Escape:
                    if (!this.Session.IsDismissed)
                        this.Session.Dismiss();
                    return true;
                default:
                    return false;
            }
        }

        private void MoveCompletionByIdx(int relativeIndex)
        {
            int newPosition = TheCompletionsCollectionView.CurrentPosition + relativeIndex;

            int totalNumberItems = 0;

            foreach (var obj in TheCompletionsCollectionView)
                totalNumberItems++;

            if (totalNumberItems == 0)
                return;

            if (newPosition >= totalNumberItems)
                newPosition = totalNumberItems - 1;

            if (newPosition < 0)
                newPosition = 0;

            TheCompletionsCollectionView.MoveCurrentToPosition(newPosition);

            ScrollAsync();
        }
    }
}
