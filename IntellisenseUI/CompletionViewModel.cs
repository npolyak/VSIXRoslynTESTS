using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace IntellisenseUI
{
    public class CompletionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public ObservableCollection<CompletionVM> TheCompletions { get; } =
            new ObservableCollection<CompletionVM>();

        public ICollectionView TheCollectionView { get; private set; }

        #region TheStringToComplete Property
        private string _stringToComplete;
        public string TheStringToComplete
        {
            get
            {
                return this._stringToComplete;
            }
            set
            {
                if (this._stringToComplete == value)
                {
                    return;
                }

                this._stringToComplete = value;
                this.OnPropertyChanged(nameof(TheStringToComplete));

                Refresh();
            }
        }

        private void Refresh()
        {
            TheCollectionView.Refresh();

            if (!string.IsNullOrEmpty(TheStringToComplete))
            {
                foreach (CompletionVM completion in TheCollectionView)
                {
                    if (completion?.Text.ToLower().StartsWith(TheStringToComplete.ToLower()) == true)
                    {
                        TheCollectionView.MoveCurrentTo(completion);
                        return;
                    }
                }
            }

            TheCollectionView.MoveCurrentToFirst();

        }
        #endregion TheStringToComplete Property


        public ObservableCollection<KindFilter> TheFilters { get; } = 
            new ObservableCollection<KindFilter>();


        public CompletionViewModel()
        {

            TheCollectionView = CollectionViewSource.GetDefaultView(TheCompletions);

            TheCompletions.CollectionChanged += TheCompletions_CollectionChanged;

            TheFilters.CollectionChanged += TheFilters_CollectionChanged;

            TheCollectionView.Filter =
                (obj) =>
                {
                    CompletionVM completion = obj as CompletionVM;

                    return
                        (TheFilters.Where(filt => filt.IsOn).Count() == 0) ||
                        TheFilters.FirstOrDefault(filt => filt.IsOn && completion.Kind == filt.TheCompletionKind) != null;
                };

            TheCollectionView.CollectionChanged += TheCollectionView_CollectionChanged;
        }

        private void TheCollectionView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            
        }

        private void TheFilters_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UnsetFilters(e.OldItems);

            SetFilters(e.NewItems);
        }


        void UnsetFilters(IEnumerable filters)
        {
            if (filters == null)
                return;

            foreach (KindFilter filter in filters)
            {
                filter.PropertyChanged -= Filter_PropertyChanged;
            }
        }


        void SetFilters(IEnumerable filters)
        {
            if (filters == null)
                return;

            foreach(KindFilter filter in filters)
            {
                filter.PropertyChanged += Filter_PropertyChanged;
            }

            TheCollectionView.Refresh();
        }

        private void Filter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(KindFilter.IsOn))
                return;

            TheCollectionView.Refresh();
        }

        void SetFilters()
        {
            var filters = TheFilters.ToList();

            filters.ForEach(filter => TheFilters.Remove(filter));

            filters =
                TheCompletions
                    .Select(comp => comp.Kind)
                    .Distinct()
                    .OrderBy((str) => str)
                    .Select(kindStr => new KindFilter(kindStr)).ToList();

            if (filters.Count == 1)
                return;

            filters.ForEach(filter => TheFilters.Add(filter));
        }



        private void TheCompletions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SetFilters();
        }
    }
}
