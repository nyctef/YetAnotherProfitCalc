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
        public static readonly DependencyProperty TypeIDProperty = DependencyProperty.Register("TypeID", typeof(TypeID), typeof(ItemDescriptionModel));

        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(ItemDescriptionModel));

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(ItemDescriptionModel));

        public static readonly DependencyProperty AttributesProperty = DependencyProperty.Register("Attributes", typeof(ObservableCollection<AttributeValue>), typeof(ItemDescriptionModel));
        
        public TypeID TypeID 
        {
            get { return (TypeID)GetValue(TypeIDProperty); } 
            set 
            {
                if (TypeID != value && !DesignerProperties.GetIsInDesignMode(this))
                {
                    SetValue(TypeIDProperty, value);
                    ItemName = CommonQueries.GetTypeName(value);
                    ItemDescription = CommonQueries.GetTypeDescription(value);
                    Attributes = new ObservableCollection<AttributeValue>(CommonQueries.GetAttributesForType(value));
                }
            } 
        }

        public string ItemName 
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public string ItemDescription
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public ObservableCollection<AttributeValue> Attributes
        {
            get { return (ObservableCollection<AttributeValue>)GetValue(AttributesProperty); }
            set { SetValue(AttributesProperty, value); }
        }
    }
}
