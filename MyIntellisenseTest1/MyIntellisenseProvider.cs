using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.Collections.ObjectModel;

namespace MyIntellisenseTest1
{
    [Export(typeof(IIntellisensePresenterProvider))]
    [ContentType("XAML")]
    [ContentType("CSharp")]
    [Order(Before = "default")]
    [Name("My Intellisense Presenter")]
    public class MyIntellisenseProvider : IIntellisensePresenterProvider
    {
        public IIntellisensePresenter TryCreateIntellisensePresenter(IIntellisenseSession session)
        {
            ICompletionSession completionSession = session as ICompletionSession;

            ReadOnlyObservableCollection<CompletionSet> completionSets =
                completionSession.CompletionSets;

            IEnumerable<Completion>
                allCompletions = completionSets.SelectMany(completionSet => completionSet.Completions);

            return new MyIntellisensePresenterUI(completionSession);
        }
    }
}
