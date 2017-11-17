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

namespace MyIntellisenseTest1
{
    /// <summary>
    /// Interaction logic for MyIntellisensePresenterUI.xaml
    /// </summary>
    public partial class MyIntellisensePresenterUI : UserControl, IPopupIntellisensePresenter
    {
        ICompletionSession _completionSession;

        public event EventHandler SurfaceElementChanged;
        public event EventHandler PresentationSpanChanged;
        public event EventHandler<ValueChangedEventArgs<PopupStyles>> PopupStylesChanged;

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


        public MyIntellisensePresenterUI(ICompletionSession completionSession)
        {
            _completionSession = completionSession;
            InitializeComponent();
        }
    }
}
