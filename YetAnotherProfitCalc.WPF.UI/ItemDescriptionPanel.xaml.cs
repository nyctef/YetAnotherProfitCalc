using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

namespace YetAnotherProfitCalc.WPF.UI
{
    [ValueConversion(typeof(TypeID), typeof(String))]
    internal class TypeIDConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return "<null>";
            Debug.Assert(targetType == typeof(String));
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) value = 0;
            Debug.Assert(targetType == typeof(TypeID));
            Debug.Assert(value.GetType() == typeof(String));
            return new TypeID(value.ToInt());
        }
    }

    /// <summary>
    /// Interaction logic for ItemDescriptionPanel.xaml
    /// </summary>
    public partial class ItemDescriptionPanel : UserControl
    {
        private readonly ItemDescriptionModel m_Model; 

        public static DependencyProperty TypeIDProperty = ItemDescriptionModel.TypeIDProperty.AddOwner(typeof(ItemDescriptionPanel), new FrameworkPropertyMetadata(ItemDescriptionPanel.TypeIDPropertyChangedCallback));
            
        public ItemDescriptionPanel()
        {
            InitializeComponent();
            m_Model = new ItemDescriptionModel();
            DataContext = m_Model;
        }

        public TypeID TypeID
        {
            get { return ((ItemDescriptionModel)DataContext).TypeID; }
            set { ((ItemDescriptionModel)DataContext).TypeID = value; }
        }

        private static void TypeIDPropertyChangedCallback(DependencyObject controlInstance, DependencyPropertyChangedEventArgs args)
        {
            ((ItemDescriptionPanel)controlInstance).m_Model.TypeID = (TypeID)args.NewValue;
        }  

    }
}
