using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YetAnotherProfitCalc.WPF.UI
{
    class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void FirePropertyChanged(string propName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propName));
        }

        protected void UpdateProperty<T>(string propName, ref T property, T newValue)
        {
            var comparer = EqualityComparer<T>.Default;
            if (!comparer.Equals(property, newValue))
            {
                property = newValue;
                FirePropertyChanged(propName);
            }
        }
    }
}
