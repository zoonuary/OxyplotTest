using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OxyTest.ViewModels.GridViews.Internals
{
    public class SelectedItemsBehavior : Behavior<GridControl>
    {
        public static readonly DependencyProperty SelectedItemsProperty =
         DependencyProperty.Register(
             nameof(SelectedItems),
             typeof(IList),
             typeof(SelectedItemsBehavior),
             new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public IList SelectedItems
        {
            get => (IList)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += SelectedItems_CollectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= SelectedItems_CollectionChanged;
        }

        private void SelectedItems_CollectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            if (SelectedItems == null)
                return;

            SelectedItems.Clear();
            foreach (var item in AssociatedObject.SelectedItems)
            {
                SelectedItems.Add(item);
            }
        }
    }
}
