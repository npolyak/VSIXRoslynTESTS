using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NP.XAMLIntellisenseExtensionForVS2017
{
    public class CompletionFilter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public string CompletionFilterKind { get; private set; }

        public ImageSource TheIcon { get; }


        #region IsOn Property
        private bool _isOn;
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


        public CompletionFilter(string completionFilterKind, ImageSource icon)
        {
            CompletionFilterKind = completionFilterKind;
            TheIcon = icon.Clone();
        }
    }
}
