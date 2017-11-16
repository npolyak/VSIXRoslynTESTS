using NP.Paradigms.Attrs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPropertyExpansionProject
{
    public class MyTestPropExpansionsClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #region DESIRED_FORMAT
        //int _myInt = 0;
        //public int MyInt
        //{
        //    get => _myInt;

        //    set
        //    {
        //        if (_myInt.ObjEquals(value))
        //            return;

        //        _myInt = value;

        //        OnPropertyChanged(nameof(MyInt));
        //    }
        //}
        #endregion DESIRED_FORMAT

        [PostNotifiableProperty]
        public int MyInt { get; set; }
    }
}
