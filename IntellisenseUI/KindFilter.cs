using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntellisenseUI
{
    public class KindFilter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public string TheCompletionKind { get; set; }

        public KindFilter()
        {
        }

        public KindFilter(string completionKind)
        {
            this.TheCompletionKind = completionKind;
        }


        #region IsOn Property
        private bool _isOn = false;
        public bool IsOn
        {
            get
            {
                return this._isOn;
            }
            set
            {
                if (this._isOn == value)
                {
                    return;
                }

                this._isOn = value;
                this.OnPropertyChanged(nameof(IsOn));
            }
        }
        #endregion IsOn Property
    }
}
