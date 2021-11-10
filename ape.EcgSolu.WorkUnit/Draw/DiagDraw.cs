//-----------------------------------------------------------
//诊断视图的辅助绘图
//-----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ape.EcgSolu.WorkUnit.Draw
{
    public class DiagDraw : IDisposable
    {
        int PixelPerMM = 5;
        int[] yBase = new int[13];
        int yBottom;
        Graphics dc;
        int width, height;
        Pen backGrid1mm = new Pen(Color.FromArgb(255, 223, 224), 1);
        Pen backGrid5mm = new Pen(Color.FromArgb(240, 143, 143), 1);
        Pen wavePen = new Pen(Color.FromArgb(0x33, 0x33, 0x33), 1);
        Font textFont = new Font("微软雅黑", 12, GraphicsUnit.Pixel);
        double uVpb;
        int sampleRate;             //采样频率
        PointF[,] dataPoint;
        short[,] data;              //缓存下来的原始数据
        const int PaperSpeed = 25;  //mm/s
        float xStep;                //pixel per point       
        public int Gain = 10;       //10mm/mV,增益

        public DiagDraw(Bitmap bmp, short[,] data, double uVpb, int sampleRate)
        {
            dc = Graphics.FromImage(bmp);
            dc.SmoothingMode = SmoothingMode.AntiAlias;
            this.uVpb = uVpb;
            this.sampleRate = sampleRate;
            this.width = bmp.Width;
            this.height = bmp.Height;
            this.DrawBackGrid(dc, backGrid1mm, backGrid5mm, width, height);
            xStep = PaperSpeed * (float)PixelPerMM / sampleRate;
            float perHeight = (bmp.Height - PixelPerMM * 20) / 12f;
            this.data = data;
            //for (int i = 0; i < 12; i++)
            //{
            //    if (i < 6)
            //    {
            //        yBase[i] = (int)(PixelPerMM * 10 + perHeight * (i * 2 + 1));
            //    }
            //    else
            //    {
            //        yBase[i] = yBase[i - 6];
            //    }
            //}
            for (int i = 0; i < 12; i++)
            {
                if (i < 6)
                {
                    yBase[i] = (int)(PixelPerMM * 10 + perHeight * (i * 2)) + 10;
                }
                else
                {
                    yBase[i] = yBase[i - 6];
                }
            }
            yBase[12] = (int)(PixelPerMM * 10 + perHeight * (6 * 2)) + 10;
            yBottom = yBase[12] - (int)perHeight;
            this.DrawLeadName(dc, textFont, bmp.Width, bmp.Height, yBase, this.Gain);
            this.dataPoint = this.loadDataPoints(data, uVpb, sampleRate);
        }

        /// <summary>
        /// 加载数据到点数组，用于绘图,按10mm/mV的增益计算的，之后可以根据增益比例进行二次调节
        /// </summary>
        /// <param name="data"></param>
        /// <param name="uVpb"></param>
        /// <param name="samplingRate"></param>
        /// <returns></returns>
        private PointF[,] loadDataPoints(short[,] data, double uVpb, int samplingRate,int gain=10)
        {
            int leadCount = data.GetLength(0);
            int dataCount = data.GetLength(1);
            PointF[,] dataPoint = new PointF[leadCount+1, dataCount];
            for (int i = 0; i < leadCount; i++)
            {
                for (int j = 0; j < dataCount; j++)
                {
                    if (i < leadCount)
                    {
                        dataPoint[i, j] = new PointF(0 + j * xStep, yBase[i] - DrawerBase.ValToPixel(data[i, j], uVpb, PixelPerMM, gain));
                    }
                    //else if (i >= 6)
                    //{
                    //    dataPoint[i, j] = new PointF(width / 2 + j * xStep, yBase[i] - DrawerBase.ValToPixel(data[i, j], uVpb, PixelPerMM));
                    //}
                }
            }
            for (int j = 0; j < dataCount; j++)
            {
                dataPoint[leadCount, j].X = dataPoint[1, j].X;
                dataPoint[leadCount, j].Y = dataPoint[1, j].Y+yBase[12]-yBase[1];
            }
            return dataPoint;
        }

        /// <summary>
        /// 修改增益时，刷新绘图面
        /// </summary>
        public void Refresh()
        {
            this.dc.ResetClip();
            this.dc.Clear(Color.White);
            this.DrawBackGrid(dc, backGrid1mm, backGrid5mm, width, height);
            this.DrawLeadName(dc, textFont, width, height, yBase, this.Gain);
            this.dataPoint = this.loadDataPoints(this.data, this.uVpb, this.sampleRate, this.Gain);
        }

        public void initDraw()
        {
            for (int i = 0; i < 12; i++)
            {
                this.DrawWaveUnit(dc, 0, i);
            }
        }

        private void DrawWaveColumn1()
        {
            for (int i = 0; i < 6; i++)
            {
               // this.DrawWaveUnit(dc, 0, i);
            }
        }

        private void DrawWaveColumn2()
        {
            for (int i = 6; i < 12; i++)
            {
                this.DrawWaveUnit(dc, 0, i);
            }
            this.DrawWaveUnit(dc, 0, 12);
        }
        private void DrawWaveColumn3()
        {
            this.DrawWaveUnit(dc, 0, 12);
        }

        /// <summary>
        /// 刷新绘图，绘制某一个位置的图
        /// </summary>
        /// <param name="position"></param>
        public void ScrollTo(int position)
        {
            int leadNameWidth = 50;     //绘制导联标题的宽度：10mm=50pixel
            Rectangle leftRect = new Rectangle(leadNameWidth, 0, width / 2 - leadNameWidth, yBottom);
            Rectangle rightRect = new Rectangle(width / 2 + leadNameWidth, 0, width / 2 - leadNameWidth, yBottom);
            Rectangle BottomRect = new Rectangle(leadNameWidth, yBottom, width - leadNameWidth, height-yBottom);
            Region backRegion = new Region(leftRect);
            backRegion.Union(rightRect);
            backRegion.Union(BottomRect);
            this.dc.Clip = backRegion;
            this.DrawBackGrid(dc, backGrid1mm, backGrid5mm, width, height);
            this.dc.SetClip(leftRect);
            float offset = position * xStep;
            GraphicsState transState = this.dc.Save();
            this.dc.TranslateTransform(-offset + leadNameWidth, 0);
            this.DrawWaveColumn1();
            this.dc.Restore(transState);
            this.dc.SetClip(rightRect);
            transState = this.dc.Save();
            this.dc.TranslateTransform(-offset + width / 2 + leadNameWidth, 0);
            this.DrawWaveColumn2();
            this.dc.Restore(transState);

            this.dc.SetClip(BottomRect);
            transState = this.dc.Save();
            this.dc.TranslateTransform(-offset + leadNameWidth, 0);
            this.DrawWaveColumn3();
            this.dc.Restore(transState);
        }

        private void DrawWaveUnit(Graphics dc, int position, int leadOrder)
        {
            PointF[] leadPoints = new PointF[dataPoint.GetLength(1)];
            for (int j = 0; j < dataPoint.GetLength(1); j++)
            {
                leadPoints[j] = dataPoint[leadOrder, j];
            }
            dc.DrawLines(wavePen, leadPoints);
        }

        private void DrawBackGrid(Graphics dc, Pen backGrid1mm, Pen backGrid5mm, int width, int height)
        {
            dc.Clear(Color.White);
            for (int row = 0; row < height; row += PixelPerMM)
            {
                if (row % 25 != 0)
                {
                    dc.DrawLine(backGrid1mm, 0, row, width, row);
                }
            }
            for (int col = 0; col < width; col += PixelPerMM)
            {
                if (col % 25 != 0)
                {
                    dc.DrawLine(backGrid1mm, col, 0, col, height);
                }
            }
            for (int row = 0; row < height; row += PixelPerMM * 5)
            {
                dc.DrawLine(backGrid5mm, 0, row, width, row);
            }
            for (int col = 0; col < width; col += PixelPerMM * 5)
            {
                dc.DrawLine(backGrid5mm, col, 0, col, height);
            }
        }

        private void DrawLeadName(Graphics dc, Font textFont, int width, int height, int[] yBase, int gain = 10)
        {
            string[] leadNameL = new string[] { "I", "II", "III", "aVR", "aVL", "aVF" };
            string[] leadNameR = new string[] { "X1", "V2", "V3", "V4", "V5", "V6" };
            for (int i = 0; i < 6; i++)
            {
           //     this.DrawLeadNameUnit(dc, textFont, wavePen, leadNameL[i], 0, yBase[i], PixelPerMM, gain);
                this.DrawLeadNameUnit(dc, textFont, wavePen, leadNameR[i], width / 2, yBase[i], PixelPerMM, gain);
            }
            this.DrawLeadNameUnit(dc, textFont, wavePen, leadNameL[1], 0, yBase[12], PixelPerMM);

        }

        /// <summary>
        /// 绘制单一导联的导联名和定标
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="textFont"></param>
        /// <param name="wavePen"></param>
        /// <param name="leadName"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="pixelPerMM"></param>
        private void DrawLeadNameUnit(Graphics dc, Font textFont, Pen wavePen, string leadName, int x, int y, int pixelPerMM, int gain = 10)
        {
            PointF[] normLinePoints = new PointF[] 
            {
                new PointF(x+5*pixelPerMM,y),
                new PointF(x+6.5f*pixelPerMM,y),
                new PointF(x+6.5f*pixelPerMM,y-gain * pixelPerMM),
                new PointF(x+8.5f*pixelPerMM,y-gain * pixelPerMM),
                new PointF(x+8.5f*pixelPerMM,y),
                new PointF(x+10*pixelPerMM,y)
            };
            dc.DrawLines(wavePen, normLinePoints);
            dc.DrawString(leadName, textFont, Brushes.Black, x, y - gain * pixelPerMM);
        }

        public void Dispose()
        {
            if (dc != null)
            {
                dc.Dispose();
            }
            if (backGrid1mm != null)
            {
                backGrid1mm.Dispose();
            }
            if (backGrid5mm != null)
            {
                backGrid5mm.Dispose();
            }
            if (wavePen != null)
            {
                wavePen.Dispose();
            }
            if (textFont != null)
            {
                textFont.Dispose();
            }
        }
    }
}
