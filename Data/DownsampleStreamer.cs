using OxyPlot;
using OxyTest.Models.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxyTest.Data
{
    /// <summary>
    /// downsampling 을 비 오름차순 ring buffer (array[]) 에서도 가능하도록 메서드를 추가한 클래스
    /// </summary>
    public class DownsampleStreamer
    {
        private double XMin, BinWidth; //viewport 시작, 버킷 폭
        private int Bins, BinIndex; //총 버킷수, 현재 버킷 index

        // M4 상태
        bool hasPoints;             // 현재 버킷에 점이 들어왔는가
        double _xf, _yf, _xl, _yl;     // first, last
        double _vmin, _vmax, _xmin, _xmax; // min/max 값과 그 위치

        double Bx0() => XMin + BinIndex * BinWidth;
        double Bx1() => Bx0() + BinWidth;

        public DownsampleStreamer() { }
        public DownsampleStreamer(double min, double max, int bins) => Reset(min, max, bins);

        public void Reset(double min, double max, int bins)
        {
            XMin = min;
            Bins = Math.Max(1, bins);
            var span = max - min;
            BinWidth = span > 0 ? span / Bins : 0;
            BinIndex = 0;
            hasPoints = false;
        }

        public void ProcessSegment(GraphDataPoint[] points, int start, int count, List<DataPoint> output)
        {
            if (BinWidth <= 0 || count <= 0 || BinIndex >= Bins) return;

            int end = start + count;
            for (int i = start; i < end && BinIndex < Bins; i++)
            {
                var x = points[i].X;
                var y = points[i].Y;

                if (x < Bx0()) continue;
                while (BinIndex < Bins && x >= Bx1())
                {
                    EmitIfAny(output);
                    NextBin();
                }
                if (BinIndex >= Bins) break;
                Ingest(x, y);
            }
        }

        private void EmitIfAny(List<DataPoint> output)
        {
            if (!hasPoints) return;

            output.Add(new DataPoint(_xf, _yf)); //add first

            //중복 제거
            bool addMin =
                !(_xmin == _xf && _vmin == _yf) &&
                !(_xmin == _xl && _vmin == _yl);
            bool addMax =
                !(_xmax == _xf && _vmax == _yf) &&
                !(_xmax == _xl && _vmax == _yl) &&
                !(_xmax == _xmin && _vmax == _vmin);

            //min max 추가(순서 확인해서)
            if (addMin && addMax)
            {
                if (_xmin <= _xmax)
                {
                    output.Add(new DataPoint(_xmin, _vmin));
                    output.Add(new DataPoint(_xmax, _vmax));
                }
                else
                {
                    output.Add(new DataPoint(_xmax, _vmax));
                    output.Add(new DataPoint(_xmin, _vmin));
                }
            }
            else if (addMin) output.Add(new DataPoint(_xmin, _vmin));
            else if (addMax) output.Add(new DataPoint(_xmax, _vmax));

            // last
            if (!(_xl == _xf && _yl == _yf))
                output.Add(new DataPoint(_xl, _yl));

            hasPoints = false;
        }

        private void Ingest(double x, double y)
        {
            if (!hasPoints)
            {
                hasPoints = true;
                _xf = _xl = x;
                _yf = _yl = y;
                _xmin = _xmax = x;
                _vmin = _vmax = y;
            }
            else
            {
                _xl = x;
                _yl = y;
                if (y < _vmin) { _vmin = y; _xmin = x; }
                if (y > _vmax) { _vmax = y; _xmax = x; }
            }

        }

        private void NextBin() { BinIndex++; hasPoints = false; }

        public void Finish(List<DataPoint> output) => EmitIfAny(output);
    }
}
