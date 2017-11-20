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

namespace IntellisenseUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public CompletionViewModel TheVM { get; } =
            new CompletionViewModel();

        public MainWindow()
        {
            var testCompletions = new CompletionsTestCollectionVM();

            foreach(CompletionVM completionVM in testCompletions)
            {
                TheVM.TheCompletions.Add(completionVM);
            }

            TheVM.TheCollectionView.Refresh();

            this.DataContext = TheVM;

            InitializeComponent();
        }

        public void ScrollIntoView()
        {
            TheCompletionsList.ScrollIntoView(TheCompletionsList.SelectedItem);

            TheCompletionsList.Focus();
        }
    }
}
