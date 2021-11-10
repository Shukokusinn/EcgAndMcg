using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Globalization;

namespace ape.EcgSolu.WorkUnit.Controls
{
    public class EcgRuler:FrameworkElement
    {
        private VisualCollection _children;
        private int pixelPerMm = 5;
        //private int mmPerMv = 10;       //x毫米/毫伏,增益
        private int paperSpeed = 25;    //25mm/s 纸速

        public EcgRuler()
        {
            _children = new VisualCollection(this);
            _children.Add(drawRuler());
            this.MouseRightButtonDown += new MouseButtonEventHandler(EcgRuler_MouseRightButtonDown);
        }

        private DrawingVisual drawRuler()
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                GuidelineSet guidelineSet = new GuidelineSet();
                guidelineSet.GuidelinesX.Add(0.5);
                guidelineSet.GuidelinesY.Add(0.5);
                dc.PushGuidelineSet(guidelineSet);
                Color backColor = Color.FromArgb(20, 25, 169, 123);
                Color borderColor=Color.FromArgb(255,0,0,0);                
                SolidColorBrush backBrush = new SolidColorBrush(backColor);
                SolidColorBrush borderBrush = new SolidColorBrush(borderColor);
                Pen borderPen = new Pen(borderBrush, 0.5);
                dc.DrawRectangle(backBrush, borderPen, new Rect(0, 0, 400, 200));
                Pen linePen = new Pen(Brushes.Black, 0.5);
                for (int x = 0; x < 400; x += pixelPerMm)
                {
                    if (x % (5 * pixelPerMm) != 0)
                    {
                        dc.DrawLine(linePen, new Point(x, 0), new Point(x, 10));
                    }
                    else
                    {
                        dc.DrawLine(linePen, new Point(x, 0), new Point(x, 20));
                    }
                }
                for (int y = 0; y < 200; y += pixelPerMm)
                {
                    if (y % (5 * pixelPerMm) != 0)
                    {
                        dc.DrawLine(linePen, new Point(0, y), new Point(10, y));
                    }
                    else
                    {
                        dc.DrawLine(linePen, new Point(0, y), new Point(20, y));
                    }
                }                
            }
            return drawingVisual;
        }

        /// <summary>
        /// 鼠标右键按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EcgRuler_MouseRightButtonDown(object sender, MouseEventArgs e)
        {
            System.Windows.Point pt = e.GetPosition((UIElement)sender);
            DrawingVisual drawingVisual = new DrawingVisual();
            double time = Math.Round(pt.X /(double)paperSpeed/5f, 1);
            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                GuidelineSet guidelineSet = new GuidelineSet();
                guidelineSet.GuidelinesX.Add(0.5);                
                dc.PushGuidelineSet(guidelineSet);
                FormattedText formattedTextTime = new FormattedText(string.Format("{0}s",time), CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight, new Typeface("微软雅黑"), 11, Brushes.Black);
                Pen linePen=new Pen(Brushes.Black,0.5);
                dc.DrawLine(linePen, new Point(pt.X, 0), new Point(pt.X, 200));
                dc.DrawText(formattedTextTime, pt);
            }
            _children.Add(drawingVisual);
        }

        private double pixelToUnit(double value)
        {
            return value;
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
