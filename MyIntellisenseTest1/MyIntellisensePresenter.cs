using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MyIntellisenseTest1
{
    public class MyIntellisensePresenter : Control, IIntellisensePresenter
    {
        ICompletionSession _completionSession;
        public IIntellisenseSession Session => _completionSession;

        public MyIntellisensePresenter(ICompletionSession completionSession)
        {
            _completionSession = completionSession;
        }
    }
}
