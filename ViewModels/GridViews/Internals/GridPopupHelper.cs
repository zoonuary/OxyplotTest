using DevExpress.Mvvm;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using OxyTest.Composition;
using OxyTest.Models.Graph;
using OxyTest.ViewModels.GridVIews.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace OxyTest.ViewModels.GridViews.Internals
{
    public class GridPopupHelper
    {
        GraphCore GraphCore { get; }

        public GridPopupHelper(GraphCore graphCore)
        {
            GraphCore = graphCore;

        }
        public IEnumerable<object> CreateGridControlPopupItems(GraphModel selectedItem)
        {
            BarButtonItem addPlotButton;
            BarButtonItem deletePlotButton;
            BarSubItem detailButton;

            if (selectedItem != null)
            {
                addPlotButton = CreateBarbuttonItem("Add Plot...", new DelegateCommand(() => GraphCore.DialogService.ShowSignalAddDialog((model) => GraphCore.GraphData.AddGraph(model))));
                deletePlotButton = CreateBarbuttonItem("delete Plot", new DelegateCommand(() => GraphCore.GraphData.RemoveGraph(selectedItem)));
                detailButton = CreateDetailSubItem("Details", selectedItem);
            }
            else
            {
                addPlotButton = CreateBarbuttonItem("Add Plot...", new DelegateCommand(() => GraphCore.DialogService.ShowSignalAddDialog((model) => GraphCore.GraphData.AddGraph(model))));
                deletePlotButton = CreateBarbuttonItem("delete Plot", null);
                detailButton = CreateDetailSubItem("Details", null);
            }


            yield return addPlotButton;
            yield return deletePlotButton;
            yield return new BarItemLinkSeparator();
            yield return detailButton;
            yield return new BarItemLinkSeparator();
        }

        public BarButtonItem CreateColumnChooserButtonItem(Popup popup)
        {
            if (popup.IsOpen)
            {
                return new BarButtonItem
                {
                    Content = "Close ColumnChooser",
                    Command = new DelegateCommand(() => popup.IsOpen = false),
                    IsEnabled = true
                };
            }
            return new BarButtonItem
            {
                Content = "Open ColumChooser...",
                Command = new DelegateCommand(() => popup.IsOpen = true),
                IsEnabled = true
            };
        }


        private BarButtonItem CreateBarbuttonItem(object content, ICommand command)
        {
            if (command != null)
            {
                return new BarButtonItem
                {
                    Content = content,
                    Command = command,
                    IsEnabled = true
                };
            }
            return new BarButtonItem
            {
                Content = content,
                IsEnabled = false
            };
        }

        private BarSubItem CreateDetailSubItem(object content, GraphModel model)
        {
            if (model == null)
            {
                return new BarSubItem
                {
                    Content = content,
                    IsEnabled = false
                };
            }

            var detailButton = new BarSubItem
            {
                Content = content
            };
            detailButton.Items.Clear();
            AddDetailSubItem(detailButton, "Sginal Name", model.SignalDataModel.Name);
            AddDetailSubItem(detailButton, "ID", model.SignalDataModel.ID.ToString());
            AddDetailSubItem(detailButton, "Factor", model.SignalDataModel.Factor.ToString());
            AddDetailSubItem(detailButton, "Offset", model.SignalDataModel.Offset.ToString());
            AddDetailSubItem(detailButton, "Length", model.SignalDataModel.Length.ToString());
            AddDetailSubItem(detailButton, "StartBit", model.SignalDataModel.StartBit.ToString());
            AddDetailSubItem(detailButton, "Unsigned", model.SignalDataModel.IsUnsigned.ToString());
            return detailButton;
        }

        private void AddDetailSubItem(BarSubItem parent, string label, string text)
        {
            parent.Items.Add(new BarStaticItem
            {
                Content = $"{label} : {text}",
                Margin = new System.Windows.Thickness(0, 0, 5, 0)
            });
        }


        public Popup CreateColumnChooserPopup(GridControl target, GridViewModel viewmodel)
        {
            var columnCheckbox = new CheckBox
            {
                Content = "columns",
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(8, 0, 0, 0),
            };

            columnCheckbox.SetBinding(CheckBox.IsCheckedProperty, new Binding(nameof(viewmodel.IsAllColumnsVisible))
            {
                Source = viewmodel,
                Mode = BindingMode.TwoWay
            });

            var separator = new Separator
            {
                Margin = new Thickness(0, 4, 0, 4)
            };

            var itemsControl = new ItemsControl();
            itemsControl.SetBinding(ItemsControl.ItemsSourceProperty, new Binding
            {
                Source = viewmodel.ColumnSettings
            });

            var template = new DataTemplate(typeof(ColumnSetting));
            var checkBoxFactory = new FrameworkElementFactory(typeof(CheckBox));
            checkBoxFactory.SetBinding(CheckBox.ContentProperty, new Binding(nameof(ColumnSetting.FieldName)));
            checkBoxFactory.SetBinding(CheckBox.IsCheckedProperty, new Binding(nameof(ColumnSetting.IsVisible)));
            checkBoxFactory.SetValue(CheckBox.PaddingProperty, new Thickness(8, 0, 0, 4));
            template.VisualTree = checkBoxFactory;
            itemsControl.ItemTemplate = template;

            var closeButton = new Button
            {
                Content = "[Close]",
                HorizontalAlignment = HorizontalAlignment.Right,
                Padding = new Thickness(2),
                Background = Brushes.Transparent,
                Cursor = Cursors.Hand,
                Foreground = Brushes.Gray,
                BorderThickness = new Thickness(0)
            };
            
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            var border = new Border
            {
                Background = Brushes.White,
                BorderBrush = Brushes.Gray,
                BorderThickness = new System.Windows.Thickness(1),
                Padding = new System.Windows.Thickness(5),
                Child = stackPanel
            };

            var popup = new Popup
            {
                Child = border,
                PlacementTarget = target,
                Placement = PlacementMode.Right,
                StaysOpen = true,
                AllowsTransparency = true
            };

            stackPanel.Children.Add(columnCheckbox);
            stackPanel.Children.Add(separator);
            stackPanel.Children.Add(itemsControl);
            stackPanel.Children.Add(closeButton);

            closeButton.Click += (s, e) => { popup.IsOpen = !popup.IsOpen; };
            return popup;
        }
    }
}
