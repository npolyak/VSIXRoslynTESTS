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
using System.Windows;

namespace NP.XAMLIntellisenseExtensionForVS2017
{
    [Export(typeof(IIntellisensePresenterProvider))]
    [ContentType("XAML")]
    [Order(Before = "default")]
    [Name("XAML Intellisense Extension")]
    public class XAMLIntellisensePresenterProvider : 
        IIntellisensePresenterProvider
    {
        public IIntellisensePresenter 
            TryCreateIntellisensePresenter(IIntellisenseSession session)
        {
            ICompletionSession completionSession = session as ICompletionSession;

            CompletionSet completionSet = completionSession.SelectedCompletionSet;

            IEnumerable<Completion>
                allCompletions = completionSet?.Completions.ToList();

            if ( (allCompletions == null) || 
                 (allCompletions.Count() == 0) )
            {
                // ensures default behavior if there are no completions
                return null;
            }

            return new XAMLIntellisensePresenterControl(completionSession);
        }
    }
}
