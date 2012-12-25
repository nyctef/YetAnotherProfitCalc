﻿using System;
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
    /// Interaction logic for ItemDescriptionPanel.xaml
    /// </summary>
    public partial class ItemDescriptionPanel : UserControl
    {
        private readonly ItemDescriptionModel m_Model; 

        public static DependencyProperty ItemIDProperty = ItemDescriptionModel.ItemIDProperty.AddOwner(typeof(ItemDescriptionPanel), new FrameworkPropertyMetadata(ItemDescriptionPanel.ItemIDPropertyChangedCallback));
            
        public ItemDescriptionPanel()
        {
            InitializeComponent();
            m_Model = new ItemDescriptionModel();
            DataContext = m_Model;
        }

        public TypeID ItemID
        {
            get { return ((ItemDescriptionModel)DataContext).ItemID; }
            set { ((ItemDescriptionModel)DataContext).ItemID = value; }
        }

        private static void ItemIDPropertyChangedCallback(DependencyObject controlInstance, DependencyPropertyChangedEventArgs args)
        {
            ((ItemDescriptionPanel)controlInstance).m_Model.ItemID = (TypeID)args.NewValue;
        }  

    }
}