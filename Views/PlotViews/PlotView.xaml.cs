using DevExpress.Mvvm;
using OxyTest.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace OxyTest.Views
{
    /// <summary>
    /// Interaction logic for PlotView.xaml
    /// </summary>
    public partial class PlotView : UserControl
    {
        public PlotView()
        {
            InitializeComponent();

            DataContextChanged += (s, e) =>
            {
                if(DataContext is PlotViewModel viewmodel)
                {
                    plotview.AddHandler(UIElement.MouseMoveEvent, new MouseEventHandler((s, e) =>
                    {
                        var position = e.GetPosition(plotview);
                        viewmodel.PlotControllerBuilder.OnMeasureCursorMove(position.X);
                    }), true);
                }
            };
        }

        private void plotview_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(DataContext is PlotViewModel viewmodel)
            {
                viewmodel.PlotControllerBuilder.SetViewPortInfo(viewmodel.XAxis);
            }
        }
    }
}
