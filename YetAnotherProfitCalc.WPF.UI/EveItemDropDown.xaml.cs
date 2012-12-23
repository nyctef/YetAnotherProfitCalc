using System;
using System.Collections.Generic;
using System.ComponentModel;
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
	partial class EveItemDropDown : UserControl, INotifyPropertyChanged
	{
        private EveItemDropDownModel m_ViewModel;
		public EveItemDropDown()
        {
            InitializeComponent();
            m_ViewModel = new EveItemDropDownModel();
            ComboBox.DataContext = ViewModel;
            ComboBox.DropDownOpened += DeselectText;
            m_ViewModel.PropertyChanged += ViewModelPropertyChanged;
		}

        /// <summary>
        /// This is a hack to cope with the fact that, when we start getting completions, we open the dropdown, 
        /// which causes the current text in the combobox to be selected for some reason
        /// 
        /// TODO: should probably actually figure out the root cause of this
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DeselectText(object sender, EventArgs e)
        {
            var textBox = TextBoxPart;
            textBox.SelectionLength = 0;
            textBox.SelectionStart = textBox.Text.Length;
        }

        EveItemDropDownModel ViewModel
        {
            get { return m_ViewModel; }
        }

        TextBox TextBoxPart
        {
            get { return (TextBox)ComboBox.Template.FindName("PART_EditableTextBox", ComboBox); }
        }

        public string SelectedName
        {
            get
            {
                return m_ViewModel != null ? m_ViewModel.SelectedName : null;
            }
        }

        public TypeID SelectedID
        {
            get
            {
                return m_ViewModel != null ? m_ViewModel.SelectedID : null;
            }
        }

        private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName) {
                case "SelectedName":
                case "SelectedID":
                    NotifyPropertyChangedBase.FirePropertyChanged(args.PropertyName, PropertyChanged, this);
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
