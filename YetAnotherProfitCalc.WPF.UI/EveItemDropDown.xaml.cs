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

namespace YetAnotherProfitCalc.WPF.UI
{
	/// <summary>
	/// Interaction logic for EveItemDropDown.xaml
	/// </summary>
	partial class EveItemDropDown : UserControl
	{
        private EveItemDropDownModel m_ViewModel;
		public EveItemDropDown()
        {
            InitializeComponent();
            m_ViewModel = new EveItemDropDownModel();
            ComboBox.DataContext = ViewModel;
		}

        EveItemDropDownModel ViewModel
        {
            get { return m_ViewModel; }
        }
    }
}
