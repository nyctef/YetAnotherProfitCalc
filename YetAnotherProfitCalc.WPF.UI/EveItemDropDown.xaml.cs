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
            ComboBox.DropDownClosed += FinishOrCancelSelection;
            m_ViewModel.PropertyChanged += ViewModelPropertyChanged;
            ComboBox.KeyDown += ComboBox_KeyDown;
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

        /// <summary>
        /// This is some hacky code to ensure that the selected item and text etc are 
        /// in sync when we select something, or cancel out of the dropdown. 
        /// </summary>
        private void FinishOrCancelSelection(object ignored = null, EventArgs ignored2 = null)
        {
            m_ViewModel.DontPropagate = true;
            // If we've just selected something, then m_ViewModel will contain that, otherwise 
            // it will contain the previous thing we selected.
            ComboBox.SelectedItem = m_ViewModel.SelectedItem;
            ComboBox.Text = m_ViewModel.SelectedName;
            m_ViewModel.DontPropagate = false;

            try
            {
                TextBoxPart.CaretIndex = 0;
            }
            catch { }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                FinishOrCancelSelection();
                e.Handled = true;
            }
        }
    }
}
