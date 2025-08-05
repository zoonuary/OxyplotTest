using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace OxyTest.ViewModels.PlotViews.Internals
{
    public class PlotPopupHelper
    {

        private ObservableCollection<CursorData> ItemsSource { get; set; } = new ObservableCollection<CursorData>();
        private Popup CursorInfoPopup { get; } 

        public PlotPopupHelper()
        {
            CursorInfoPopup = CreateCursorInfoPopup(ItemsSource);
        }

        private Popup CreateCursorInfoPopup(ObservableCollection<CursorData> items)
        {
            var itemsControl = new ItemsControl();
            itemsControl.SetBinding(ItemsControl.ItemsSourceProperty, new Binding
            {
                Source = items
            });

            var template = new DataTemplate(typeof(CursorData));
            //setgrid
            var gridFactory = new FrameworkElementFactory(typeof(Grid));
            gridFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(2));
            gridFactory.SetValue(Grid.IsSharedSizeScopeProperty, true);

            //setcolumns
            var col1 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col1.SetValue(ColumnDefinition.WidthProperty, new GridLength(0, GridUnitType.Auto));
            col1.SetValue(ColumnDefinition.SharedSizeGroupProperty, "KeyGroup");

            var colSpace = new FrameworkElementFactory(typeof(ColumnDefinition));
            colSpace.SetValue(ColumnDefinition.WidthProperty, new GridLength(10)); // 구분 여백

            var col2 = new FrameworkElementFactory(typeof(ColumnDefinition));
            col2.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
            gridFactory.AppendChild(col1); 
            gridFactory.AppendChild(colSpace);
            gridFactory.AppendChild(col2);

            // name textblock
            var nameFactory = new FrameworkElementFactory(typeof(TextBlock));
            nameFactory.SetBinding(TextBlock.TextProperty, new Binding(nameof(CursorData.Name)));
            nameFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            nameFactory.SetValue(TextBlock.ForegroundProperty, new Binding(nameof(CursorData.BrushColor)));
            nameFactory.SetValue(Grid.ColumnProperty, 0);
            gridFactory.AppendChild(nameFactory);

            //value textblock
            var valueFactory = new FrameworkElementFactory(typeof(TextBlock));
            valueFactory.SetBinding(TextBlock.TextProperty, new Binding(nameof(CursorData.Value)));
            valueFactory.SetValue(Grid.ColumnProperty, 2);
            gridFactory.AppendChild(valueFactory);

            template.VisualTree = gridFactory;
            itemsControl.ItemTemplate = template;

            var border = new Border
            {
                Background = Brushes.White,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5),
                Child = itemsControl
            };

            var popup = new Popup
            {
                Child = border,
                Placement = PlacementMode.Absolute,
                AllowsTransparency = true,
                StaysOpen = false,
                IsOpen = false
            };

            return popup;
        }

        public void OpenPopup(Point position)
        {
            CursorInfoPopup.VerticalOffset = position.Y + 10;
            CursorInfoPopup.HorizontalOffset = position.X + 10;
            CursorInfoPopup.IsOpen = true;
        }

        public void ClosePopup()
        {
            CursorInfoPopup.IsOpen = false;
        }

        public void SetPopupData(List<CursorData> items)
        {
            ItemsSource.Clear();
            foreach(CursorData data in items)
            {
                ItemsSource.Add(data);
            }
        }

    }
}
