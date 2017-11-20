using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntellisenseUI
{
    public class CompletionVM
    {
        public string Text { get; set; }

        public string Kind { get; set; }

        public CompletionVM()
        {

        }

        public CompletionVM(string text, string kind)
        {
            Text = text;
            Kind = kind;
        }
    }

    public class CompletionsTestCollectionVM : 
        ObservableCollection<CompletionVM>
    {
        void Add(string text, string kind)
        {
            this.Add(new CompletionVM(text, kind));
        }

        public CompletionsTestCollectionVM()
        {
            Add("comp1_1", "1");
            Add("comp1_2", "1");
            Add("comp2_1", "2");
            Add("comp2_2", "2");
            Add("comp2_333", "2");
            Add("comp2_4", "2");
            Add("comp3_1", "3");
            Add("comp3_2", "3");
            Add("comp3_333", "3");
        }
    }
}
