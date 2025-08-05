using OxyTest.Composition;
using OxyTest.Models.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.ViewModels.PlotViews.Internals
{
    public class LayoutManager
    {
        private GraphCore GraphCore { get; }

        public LayoutManager(GraphCore graphCore)
        {
            GraphCore = graphCore;
        }

        public void SetLayout(ePAGE_TYPE pageType, GraphModel CurrentItem)
        {
            switch (pageType)
            {
                case ePAGE_TYPE.SINGLE_Y:
                    SetSingleYLayout(CurrentItem);
                    break;
                case ePAGE_TYPE.MULTIPLE_Y:
                    SetMultiYLayOut(CurrentItem);
                    break;
                case ePAGE_TYPE.SEPARATE_Y:
                    SetSepaYLayout(CurrentItem);
                    break;
            }
        }

        private void SetSingleYLayout(GraphModel CurrentItem)
        {
            if (CurrentItem != null
                && CurrentItem.GraphRenderModel != null)
            {
                double maxY;
                double minY;
                if(CurrentItem.GraphRenderModel.ValueType == eVALUE_TYPE.RAW)
                {
                    maxY = CurrentItem.GraphRenderModel.RawMax;
                    minY = CurrentItem.GraphRenderModel.RawMin;
                }
                else
                {
                    maxY = CurrentItem.GraphRenderModel.physicalMax;
                    minY = CurrentItem.GraphRenderModel.physicalMin;
                }

                foreach (var graphModel in GraphCore.GraphData.Graphs)
                {
                    var renderModel = graphModel.GraphRenderModel;
                    ResetYAxis(renderModel);
                    renderModel.FitYAxis();
                    if (renderModel.YAxis.Tag == CurrentItem.Tag) { renderModel.YAxis.IsAxisVisible = true; }
                    else { renderModel.YAxis.IsAxisVisible = false; }
                        
                    renderModel.YAxis.Zoom(minY, maxY);
                }
            }
        }

        private void SetMultiYLayOut(GraphModel CurrentItem)
        {
            if (CurrentItem != null)
            {
                int idx = 1;
                foreach (var graphModel in GraphCore.GraphData.Graphs)
                {
                    var renderModel = graphModel.GraphRenderModel;
                    ResetYAxis(renderModel);
                    if (renderModel.YAxis.Tag == CurrentItem.Tag) renderModel.YAxis.PositionTier = 0;
                    else
                    {
                        renderModel.YAxis.PositionTier = idx++;
                        renderModel.SetGridLineVisible(false);
                    }
                }
            }
        }

        private void SetSepaYLayout(GraphModel CurrentItem)
        {
            if (CurrentItem != null)
            {
                var graphs = GraphCore.GraphData.Graphs.Where(x => x.GraphRenderModel.Visible == true).ToList();
                int cnt = graphs.Count();
                int reverseIdx = cnt;
                for (int i = 0; i < cnt; i++)
                {
                    var renderModel = graphs[--reverseIdx].GraphRenderModel;
                    ResetYAxis(renderModel);
                    renderModel.YAxis.StartPosition = (double)i / cnt;
                    renderModel.YAxis.EndPosition = (double)(i + 1) / cnt;
                }
            }
        }

        private void ResetYAxis(GraphRenderModel renderModel)
        {
            renderModel.YAxis.IsAxisVisible = renderModel.Visible;
            renderModel.YAxis.PositionTier = 0;
            renderModel.YAxis.StartPosition = 0;
            renderModel.YAxis.EndPosition = 1;
            renderModel.SetGridLineVisible(GraphCore.GraphData.GridLineVisible);
            
        }
    }
}
