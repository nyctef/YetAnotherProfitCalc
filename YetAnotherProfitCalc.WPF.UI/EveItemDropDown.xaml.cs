using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        /// <summary>
        /// True when the model is changing the UI, which means we want to ignore events coming from
        /// the UI complaining about how it's getting changed and change is scary
        /// </summary>
        private bool m_InModelUpdate;

        public string SelectedName
        {
            get { return (string)GetValue(SelectedNameProperty); }
            protected set { SetValue(SelectedNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedNameProperty =
            DependencyProperty.Register("SelectedName", typeof(string), typeof(EveItemDropDown));

        public TypeID SelectedTypeID
        {
            get { return (TypeID)GetValue(SelectedTypeIDProperty); }
            protected set { SetValue(SelectedTypeIDProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedTypeID.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedTypeIDProperty =
            DependencyProperty.Register("SelectedTypeID", typeof(TypeID), typeof(EveItemDropDown));

        private EveItemDropDownCompletionsModel m_ViewModel;

		public EveItemDropDown()
        {
            InitializeComponent();
            m_ViewModel = new EveItemDropDownCompletionsModel();
            DataContext = ViewModel;
            PART_TextBox.TextChanged += PART_TextBox_TextChanged;
            ViewModel.CompletionsChanged += ViewModel_CompletionsChanged;
            PART_CompletionList.KeyDown += PART_CompletionList_KeyDown;
            PART_TextBox.PreviewKeyDown += PART_TextBox_PreviewKeyDown;
            //LostFocus += EveItemDropDown_LostFocus;

            // this forces the item to be rendered - TextBoxPart will be null otherwise
            //Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
		}

        void PART_TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                // the completion list cares about these events 
                case Key.Enter:
                case Key.Escape:
                case Key.Up:
                case Key.PageUp:
                case Key.Down:
                case Key.PageDown:
                    PART_CompletionList.Focus();
                    PART_CompletionList.RaiseEvent(e);
                    break;
            }
        }

        void PART_CompletionList_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    SelectItem((EveItem)PART_CompletionList.SelectedItem);
                    IsDropDownOpen = false;
                    e.Handled = true;
                    break;

                case Key.Escape:
                    CancelSelection();
                    e.Handled = true;
                    break;

                case Key.Up:
                case Key.PageUp:
                case Key.Down:
                case Key.PageDown:
                    break;

                default:
                    // if we're typing, we probably want to go back to the textbox
                    PART_TextBox.Focus();
                    PART_TextBox.RaiseEvent(e);
                    break;
            }
        }

        void EveItemDropDown_LostFocus(object sender, RoutedEventArgs e)
        {
            CancelSelection();
        }

        void ViewModel_CompletionsChanged(object sender, EventArgs e)
        {
            IsDropDownOpen = ViewModel.Items.Any();
        }

        void PART_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!m_InModelUpdate)
            {
                ViewModel.Input = PART_TextBox.Text;
            }
        }

        void DeselectText(object sender, EventArgs e)
        {
            var textBox = PART_TextBox;
            textBox.SelectionLength = 0;
            textBox.SelectionStart = textBox.Text.Length;
        }

        EveItemDropDownCompletionsModel ViewModel
        {
            get { return m_ViewModel; }
        }

        protected bool IsDropDownOpen
        {
            get { return PART_CompletionPopup.IsOpen; }
            set { PART_CompletionPopup.IsOpen = value; }
        }

        private void SelectItem(EveItem item)
        {
            m_InModelUpdate = true;

            SelectedName = item.Name;
            SelectedTypeID = item.TypeID;
            PART_TextBox.Text = item.Name;

            m_InModelUpdate = false;
        }

        private void CancelSelection()
        {
            IsDropDownOpen = false;

            m_InModelUpdate = true;
            PART_TextBox.Text = SelectedName;
            m_InModelUpdate = false;
        }
    }
}
