using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace YetAnotherProfitCalc.WPF.UI
{
    class ItemDescriptionModel : DependencyObject
    {
        public static readonly DependencyProperty ItemIDProperty = DependencyProperty.Register("ItemID", typeof(TypeID), typeof(ItemDescriptionModel));

        public static readonly DependencyProperty ItemNameProperty = DependencyProperty.Register("ItemName", typeof(string), typeof(ItemDescriptionModel));

        public static readonly DependencyProperty ItemDescriptionProperty = DependencyProperty.Register("ItemDescription", typeof(string), typeof(ItemDescriptionModel));

        public static readonly DependencyProperty AttributesProperty = DependencyProperty.Register("Attributes", typeof(ObservableCollection<AttributeValue>), typeof(ItemDescriptionModel));
        
        public TypeID ItemID 
        {
            get { return (TypeID)GetValue(ItemIDProperty); } 
            set 
            {
                if (ItemID != value && !DesignerProperties.GetIsInDesignMode(this))
                {
                    SetValue(ItemIDProperty, value);
                    ItemName = CommonQueries.GetTypeName(value);
                    ItemDescription = CommonQueries.GetTypeDescription(value);
                    Attributes = new ObservableCollection<AttributeValue>(CommonQueries.GetAttributesForType(value));
                }
            } 
        }

        public string ItemName 
        {
            get { return (string)GetValue(ItemNameProperty); }
            set { SetValue(ItemNameProperty, value); }
        }

        public string ItemDescription
        {
            get { return (string)GetValue(ItemDescriptionProperty); }
            set { SetValue(ItemDescriptionProperty, value); }
        }

        public ObservableCollection<AttributeValue> Attributes
        {
            get { return (ObservableCollection<AttributeValue>)GetValue(AttributesProperty); }
            set { SetValue(AttributesProperty, value); }
        }
    }
}
