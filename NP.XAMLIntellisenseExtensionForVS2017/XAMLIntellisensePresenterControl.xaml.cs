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

namespace NP.XAMLIntellisenseExtensionForVS2017
{
    /// <summary>
    /// Interaction logic for MyIntellisensePresenterUI.xaml
    /// </summary>
    public partial class XAMLIntellisensePresenterControl :
        UserControl, 
        IPopupIntellisensePresenter, 
        IIntellisenseCommandTarget
    {
        #region IPopupIntellisensePresenter_Properties_Implementation
#pragma warning disable 0067
        public event EventHandler SurfaceElementChanged;
        public event EventHandler PresentationSpanChanged;
        public event EventHandler<ValueChangedEventArgs<PopupStyles>> PopupStylesChanged;
#pragma warning restore 0067

        public IIntellisenseSession Session => CompletionSession;

        public UIElement SurfaceElement => this;


        public ITrackingSpan PresentationSpan
        {
            get
            {
                SnapshotSpan span = 
                    this.CompletionSession
                            .SelectedCompletionSet
                            .ApplicableTo
                            .GetSpan
                            (
                                this.CompletionSession.TextView.TextSnapshot
                            );

                NormalizedSnapshotSpanCollection spans = 
                    this.CompletionSession
                            .TextView
                            .BufferGraph
                            .MapUpToBuffer
                            (
                                span, 
                                this.CompletionSession
                                        .SelectedCompletionSet
                                        .ApplicableTo
                                        .TrackingMode, 
                                this.CompletionSession.TextView.TextBuffer);
                if (spans.Count <= 0)
                {
                    throw new InvalidOperationException
                    (
                        @"Completion Session Applicable-To Span is invalid.  
It doesn't map to a span in the session's text view."
                    );
                }
                SnapshotSpan span2 = spans[0];

                return 
                    this.CompletionSession
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
        #endregion IPopupIntellisensePresenter_Properties_Implementation

        // Completion Session
        public ICompletionSession CompletionSession { get; }

        // collection view that facilitates filtering
        // of the completions collection
        public ICollectionView TheCompletionsCollectionView { get; }

        // View Model collection of the completion filters
        public ObservableCollection<CompletionTypeFilter> 
            TheCompletionTypeFilters { get; }

        // The text being typed by the user. 
        // It is used for filtering the completion result set. 
        public string UserText =>
            CompletionSession
                .SelectedCompletionSet
                ?.ApplicableTo
                    .GetText
                    (
                        CompletionSession.SelectedCompletionSet
                                            .ApplicableTo
                                            .TextBuffer
                                            .CurrentSnapshot)?.ToLower();

        public XAMLIntellisensePresenterControl(ICompletionSession completionSession)
        {
            CompletionSession = completionSession;

            CompletionSession.Dismissed +=
                _completionSession_Dismissed;

            // get all completions 
            // this set does not change throughout the session
            IEnumerable<Completion>
                allCompletions = 
                    completionSession.SelectedCompletionSet
                                     .Completions
                                     .ToList();

            // create the ICollectionView object
            // in order to facilitate the filtering of the
            // completions
            TheCompletionsCollectionView =
                CollectionViewSource.GetDefaultView(allCompletions);

            // create a single filter for each IconAutomationText
            TheCompletionTypeFilters = new ObservableCollection<CompletionTypeFilter>
            (
                allCompletions
                .GroupBy(compl => compl.IconAutomationText)
                .Where(groupItem => groupItem.Key != "9") // completion tag (gets automatically added)
                .Select
                (
                    groupItem => 
                        new CompletionTypeFilter
                        (
                            groupItem.Key, 
                            groupItem.First()
                                     .IconSource
                                     .Clone()))
            );

            // there is no reason to have a single filter
            // changing it will show or hide 
            // all the completions
            if (TheCompletionTypeFilters.Count == 1)
            {
                TheCompletionTypeFilters.Clear();
            }

            // set the filtering delegate
            // to DoFiltering method that filters
            // a completion by user text and by and by 
            // the completion type filters which are in 
            // 'On' state. 
            TheCompletionsCollectionView.Filter = DoFiltering;

            // set the completion status to 
            // the current completion every time 
            // the Current item changes with the 
            // collection view
            TheCompletionsCollectionView.CurrentChanged +=
                TheCompletionsCollectionView_CurrentChanged;

            // force refreshing of the view
            // each time any filter's state changes
            foreach (CompletionTypeFilter filter in TheCompletionTypeFilters)
            {
                filter.PropertyChanged += Filter_PropertyChanged;
            }

            InitializeComponent();

            SelectItemBasedOnTextFiltering();

            // when user text changes,
            // re-filter and (possibly)
            // choose another Completion item for
            // the CompletionStatus of the session
            CompletionSession.TextView.TextBuffer.Changed +=
                TextBuffer_Changed;

            // select the Completion item corresponding
            // to the clicked ListViewItem
            this.AddHandler
            (
                ResendEventBehavior.CustomEvent, 
                (RoutedEventHandler) TheCompletionsListView_MouseDown
            );

            // commit the session
            TheCompletionsListView.MouseDoubleClick += 
                TheCompletionsListView_MouseDoubleClick;
        }

        // set the SelectionStatus to the 
        // current item of the CollectionView
        private void TheCompletionsCollectionView_CurrentChanged
        (
            object sender, 
            EventArgs e
        )
        {
            object selectedItem =
                TheCompletionsCollectionView.CurrentItem;

            if (selectedItem != null)
            {
                if (CompletionSession.SelectedCompletionSet != null)
                {
                    try
                    {
                        // sometimes it throws an unclear exception
                        // so placed it within try/catch block
                        CompletionSession
                            .SelectedCompletionSet
                            .SelectionStatus =
                                new CompletionSelectionStatus
                                (
                                    selectedItem as Completion, 
                                    true, 
                                    true
                                );
                    }
                    catch
                    {
                    }
                }
            }
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
            CompletionSession?.Commit();
        }

        // force refiltering of the CollectionView
        private void Filter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TheCompletionsCollectionView.Refresh();
        }

        // filter a completion first by UserText and 
        // then by completion filters
        bool DoFiltering(object obj)
        {
            Completion completion = (Completion) obj;

            string userText = UserText;

            // filter by user text (if text does not match return 'false')
            if ((userText != null) && (completion.DisplayText?.ToLower()?.Contains(userText) != true))
                return false;

            IEnumerable<CompletionTypeFilter> completionFiltersThatAreOn =
                TheCompletionTypeFilters.Where(filt => filt.IsOn).ToList();

            // filter by completion filters that are in 'On' state
            return
                (completionFiltersThatAreOn.Count() == 0) ||  
                completionFiltersThatAreOn.FirstOrDefault(filt => filt.CompletionFilterType == completion.IconAutomationText) != null;
        }

        private void _completionSession_Dismissed(object sender, EventArgs e)
        {
            CompletionSession.TextView.TextBuffer.Changed -= TextBuffer_Changed;
            CompletionSession.Dismissed -= _completionSession_Dismissed;
        }

        void SelectItem(Completion completionItem)
        {
            TheCompletionsCollectionView.MoveCurrentTo(completionItem);
        }

        void SelectItemBasedOnTextFiltering()
        {
            string userText = UserText;

            bool foundCompletion = false;
            // if we find completion that starts with the text
            // we choose it. 
            if (!string.IsNullOrEmpty(userText))
            {
                foreach (Completion completion in TheCompletionsCollectionView)
                {
                    if (completion.DisplayText?.ToLower().StartsWith(userText) == true)
                    {
                        SelectItem(completion);
                        foundCompletion = true;
                        break;
                    }
                }
            }

            // if the match by text was not found
            // we move the current item to the first of
            // items within the filtered collection
            if (!foundCompletion)
            {
                TheCompletionsCollectionView.MoveCurrentToFirst();
            }

            // we force the ListView to scroll to the 
            // current item
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

        private void TextBuffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            // refresh the filter
            TheCompletionsCollectionView.Refresh();

            // choose the CompletionStatus based on the new filtering
            SelectItemBasedOnTextFiltering();
        }

        public bool ExecuteKeyboardCommand(IntellisenseKeyboardCommand command)
        {
            switch (command)
            {
                case IntellisenseKeyboardCommand.Up:
                    MoveCurrentByIdx(-1);
                    return true;
                case IntellisenseKeyboardCommand.PageUp:
                    MoveCurrentByIdx(-10);
                    return true;
                case IntellisenseKeyboardCommand.Down:
                    MoveCurrentByIdx(1);
                    return true;
                case IntellisenseKeyboardCommand.PageDown:
                    MoveCurrentByIdx(10);
                    return true;
                case IntellisenseKeyboardCommand.Escape:
                    this.CompletionSession.Dismiss();
                    return true;
                default:
                    return false;
            }
        }

        private void MoveCurrentByIdx(int relativeIndex)
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
