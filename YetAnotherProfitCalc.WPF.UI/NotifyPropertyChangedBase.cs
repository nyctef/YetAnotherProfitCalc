using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YetAnotherProfitCalc.WPF.UI
{
    /// <summary>
    /// Provides useful methods for implementing INotifyPropertyChanged. Classes can either 
    /// inherit from this one, or use the static methods if they have to inherit from something else.
    /// </summary>
    class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        private int m_DontPropagateCounter;

        /// <summary>
        /// This is a property we can set if we want to temporarily disable events propagating 
        /// through a model (for example, to prevent infinite loops)
        /// </summary>
        /// <remarks>This uses a counter in the backend so that this property can be reentrant
        /// TODO: better name</remarks>
        public bool DontPropagate
        {
            get { return m_DontPropagateCounter > 0; }
            set 
            { 
                if (value) Interlocked.Increment(ref m_DontPropagateCounter);
                else Interlocked.Decrement(ref m_DontPropagateCounter); 
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void FirePropertyChanged(string propName)
        {
            if (!DontPropagate) FirePropertyChanged(propName, PropertyChanged, this);
        }

        protected void UpdateProperty<T>(string propName, ref T property, T newValue)
        {
            if (!DontPropagate) UpdateProperty(propName, ref property, newValue, PropertyChanged, this);
        }

        static public void FirePropertyChanged(string propName, PropertyChangedEventHandler handler, object sender)
        {
            if (handler != null) handler(sender, new PropertyChangedEventArgs(propName));
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
