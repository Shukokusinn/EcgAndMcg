//-----------------------------------------------------------------
//调整特征参数的叠加图的绘制
//-----------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ape.EcgSolu.Model;

namespace ape.EcgSolu.WorkUnit.Diagnosis
{
    class AjustVisual:FrameworkElement
    {
        const int paperSpeed = 200;       //200mm/s
        const int gain = 20;              //20mm/mV
        Rect clientRect; 
        Visual[] waveVisuals = new Visual[12];
        Visual backGridVisual;
        

        private VisualCollection _children;
        //private double unitPerMm = 96 / 25.39999;       //1mm=多少wpf单位
        private int unitPerMm = 5;          //多少个wpf单位算1mm

        public AjustVisual(Ecg ecgEntity,int width,int height)
        {
            clientRect = new Rect(0, 0, width, height);
            this.backGridVisual = this.drawBackGrid(width, height);
            short[,] tempData = ecgEntity.TempData;
            int frequency = ecgEntity.SamplingRate;
            double uVpb = ecgEntity.uVpb;
            int leadCount = 12;
            _children = new VisualCollection(this);
            _children.Add(backGridVisual);
            drawWave(tempData,leadCount,frequency,uVpb);
        }

        /// <summary>
        /// 显示叠加图
        /// </summary>
        /// <param name="leadToggle">toggle为true的导联才显示</param>
        public void DrawWave(bool[] leadToggle)
        {
            foreach (Visual visual in waveVisuals)
            {
                this._children.Remove(visual);
            }
            for(int i=0;i<12;i++)
            {
                if (leadToggle[i])
                {
                    this._children.Add(waveVisuals[i]);
                }
            }
        }

        /// <summary>
        /// 绘制各个导联的叠加图
        /// </summary>
        private void drawWave(short[,] tempData,int leadCount,int frequency,double uVpb)
        {
            Color leadIColor = Color.FromRgb(0,0,0);
            Color leadIIColor = Color.FromRgb(155, 114, 239);
            Color leadIIIColor = Color.FromRgb(241, 158, 189);
            Color leadaVRColor = Color.FromRgb(164, 0, 0);
            Color leadaVLColor = Color.FromRgb(238, 87, 67);
            Color leadaVFColor = Color.FromRgb(206, 67, 0);
            Color leadV1Color = Color.FromRgb(254, 185, 0);
            Color leadV2Color = Color.FromRgb(255, 228, 1);
            Color leadV3Color = Color.FromRgb(137, 190, 55);
            Color leadV4Color = Color.FromRgb(99, 140, 11);
            Color leadV5Color = Color.FromRgb(73, 165, 236);
            Color leadV6Color = Color.FromRgb(119, 198, 255);
            Color[] leadColors = new Color[] { leadIColor, leadIColor, leadIColor, leadIColor, leadIColor, leadIColor, leadIColor, leadIColor, leadIColor, leadIColor, leadIColor, leadIColor };
            for (int i = 0; i < 12; i++)
            {
                this.waveVisuals[i] = this.drawWaveGeometry(this.clientRect, leadColors[i], this.getLeadTempData(tempData, i), frequency, paperSpeed, gain, uVpb);
                this._children.Add(this.waveVisuals[i]);
            }
        }

        /// <summary>
        /// 从二维数组里，取出某一导联的一维数组
        /// </summary>
        /// <param name="tempData"></param>
        /// <param name="leadIndex"></param>
        /// <returns></returns>
        private short[] getLeadTempData(short[,] tempData, int leadIndex)
        {
            if (tempData == null)
                return null;
            int dataCount = tempData.GetLength(1);
            short[] leadTempData = new short[dataCount];
            for (int i = 0; i < dataCount; i++)
            {
                leadTempData[i] = tempData[leadIndex, i];
            }
            return leadTempData;
        }

        //绘制背景
        private DrawingVisual drawBackGrid(int width, int height)
        {
            DrawingVisual dv = new DrawingVisual(); 
            using (DrawingContext dc = dv.RenderOpen())
            {
                GuidelineSet guidelineSet = new GuidelineSet();
                guidelineSet.GuidelinesX.Add(0.5);
                guidelineSet.GuidelinesY.Add(0.5);
                dc.PushGuidelineSet(guidelineSet);
                Rect clientRect=new Rect(0,0,width,height);
                Color grid1mmColor = Color.FromRgb(255, 223, 224);
                Color grid5mmColor = Color.FromRgb(240, 143, 143);
                double grid1mmThickness = 1;
                double grid5mmThickness = 1;
                this.drawGrid1mm(dc, clientRect, grid1mmColor, grid1mmThickness);
                this.drawGrid5mm(dc, clientRect, grid5mmColor, grid5mmThickness);
                this.drawRef(dc, clientRect);
            }
            return dv;
        }

        /// <summary>
        /// 绘制单一导联的叠加图
        /// </summary>
        /// <param name="clientRect"></param>
        /// <param name="data"></param>
        /// <param name="frequency"></param>
        /// <param name="paperSpeed"></param>
        /// <param name="gain"></param>
        /// <param name="uVpb"></param>
        /// <returns></returns>
        private DrawingVisual drawWaveGeometry(Rect clientRect,Color waveColor,short[] data,int frequency, int paperSpeed, int gain,double uVpb)
        {
            if (data == null)
                return null;
            int x=50;
            double y=clientRect.Height/2;
            double xStep = paperSpeed * (float)unitPerMm / frequency;
            DrawingVisual dv=new DrawingVisual();
            using(DrawingContext dc=dv.RenderOpen())
            {
                int drawDataCount = data.Length;
                Point prevPoint = new Point(x, y);
                Point nextPoint;
                SolidColorBrush waveBrush = new SolidColorBrush(waveColor);
                Pen linePen = new Pen(waveBrush, 1);
                List<Point> pointList = new List<Point>();
                StreamGeometry geometry = new StreamGeometry();
                using (StreamGeometryContext ctx = geometry.Open())
                {
                    ctx.BeginFigure(prevPoint, false, false);
                    for (int i = 0; i < drawDataCount; i++)
                    {
                        double xc = x + i * xStep;
                        double yc = y - valueToUnit(data[i], uVpb, unitPerMm,gain);
                        nextPoint = new Point(xc, yc);
                        pointList.Add(nextPoint);                  
                    }
                    ctx.PolyLineTo(pointList, true, false);
                }
                dc.DrawGeometry(null,linePen,geometry); 
            }
            return dv;
        }

        /// <summary>
        /// 绘制1mm网格
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="grid1mmColor"></param>
        /// <param name="grid1mmThickness"></param>
        /// <param name="grid1mmPoint"></param>
        private void drawGrid1mm(DrawingContext dc,Rect clientRect, Color grid1mmColor, double grid1mmThickness)
        {
            int colCount = 0, rowCount = 0;
            SolidColorBrush brush = new SolidColorBrush(grid1mmColor);
            double thickness = grid1mmThickness;
            Pen linePen = new Pen(brush, thickness);                  
            #region 用直线绘制
            for (int col = 0; col <= clientRect.Width; col += unitPerMm)
            {
                if (colCount % 5 != 0)
                {
                    dc.DrawLine(linePen, new Point(col, clientRect.Top), new Point(col, clientRect.Bottom));
                }
                colCount++;
            }
            for (int row = 0; row <= clientRect.Height; row += unitPerMm)
            {
                if (rowCount % 5 != 0)
                {
                    dc.DrawLine(linePen, new Point(clientRect.Left, row), new Point(clientRect.Right, row));
                }
                rowCount++;
            }
            #endregion            
            dc.DrawRectangle((Brush)null, linePen, clientRect);
        }

        /// <summary>
        /// 绘制5mm网格
        /// </summary>
        /// <param name="dc"></param>
        private void drawGrid5mm(DrawingContext dc,Rect clientRect, Color grid5mmColor, double grid5mmThickness)
        {
            int colCount = 0, rowCount = 0;
            SolidColorBrush brush = new SolidColorBrush(grid5mmColor);
            double thickness = grid5mmThickness;
            Pen linePen = new Pen(brush, thickness);
            for (int col = 0; col <= clientRect.Width; col += unitPerMm)
            {
                if (colCount % 5 == 0)
                {
                    dc.DrawLine(linePen, new Point(col, clientRect.Top), new Point(col, clientRect.Bottom));
                }
                colCount++;
            }
            for (double row = 0; row <= clientRect.Height; row += unitPerMm)
            {
                if (rowCount % 5 == 0)
                {
                    dc.DrawLine(linePen, new Point(clientRect.Left, row), new Point(clientRect.Right, row));
                }
                rowCount++;
            }
            dc.DrawRectangle((Brush)null, linePen, clientRect);
        }

        /// <summary>
        /// 绘制参考定标
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="clientRect"></param>
        private void drawRef(DrawingContext dc, Rect clientRect)
        {
            int x = 0;
            int y = (int)clientRect.Height / 2;
            Pen linePen = new Pen(Brushes.Black, 1);
            dc.DrawLine(linePen, new Point(x + 5 * unitPerMm, y), new Point(x + 6.5 * unitPerMm, y));
            dc.DrawLine(linePen, new Point(x + 6.5 * unitPerMm, y), new Point(x + 6.5 * unitPerMm, y - gain * unitPerMm));
            dc.DrawLine(linePen, new Point(x + 6.5 * unitPerMm, y - gain * unitPerMm), new Point(x + 8.5 * unitPerMm, y - gain * unitPerMm));
            dc.DrawLine(linePen, new Point(x + 8.5 * unitPerMm, y - gain * unitPerMm), new Point(x + 8.5 * unitPerMm, y));
            dc.DrawLine(linePen, new Point(x + 8.5 * unitPerMm, y), new Point(x + 10 * unitPerMm, y));            
        }

        //值转换为绘图单位的值
        private double valueToUnit(short value, double uVpb, int unitPerMm,int gain)
        {
            return value * uVpb / 1000 * gain * unitPerMm;
        }

        // Provide a required override for the VisualChildrenCount property.
        protected override int VisualChildrenCount
        {
            get { return _children.Count; }
        }

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _children[index];
        }
    }
}
