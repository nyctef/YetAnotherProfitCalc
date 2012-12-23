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
            FirePropertyChanged(propName, PropertyChanged, this);
        }

        static public void FirePropertyChanged(string propName, PropertyChangedEventHandler handler, object sender)
        {
            if (handler != null) handler(sender, new PropertyChangedEventArgs(propName));
        }

        protected void UpdateProperty<T>(string propName, ref T property, T newValue)
        {
            UpdateProperty(propName, ref property, newValue, PropertyChanged, this);
        }

        static public void UpdateProperty<T>(string propName, ref T property, T newValue, PropertyChangedEventHandler handler, object sender)
        {
            var comparer = EqualityComparer<T>.Default;
            if (!comparer.Equals(property, newValue))
            {
                property = newValue;
                FirePropertyChanged(propName, handler, sender);
            }
        }
    }
}
