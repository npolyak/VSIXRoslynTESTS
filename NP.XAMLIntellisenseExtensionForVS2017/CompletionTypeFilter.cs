using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NP.XAMLIntellisenseExtensionForVS2017
{
    public class CompletionTypeFilter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public string CompletionFilterType { get; }

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


        public CompletionTypeFilter(string completionFilterType, ImageSource icon)
        {
            CompletionFilterType = completionFilterType;
            TheIcon = icon.Clone();
        }
    }
}
