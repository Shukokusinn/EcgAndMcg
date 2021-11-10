//----------------------------------------------------------------
//采样绘图,12导联，6×2模式
//----------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;

namespace ape.EcgSolu.WorkUnit.Draw
{
    public class Drawing12:IDisposable
    {
        public int PixelPerMM = 5;          //x pixel/mm
        public int Frequency = 100;         //绘图频率,画图上认为的采样频率（跟真实采样频率不一致，比如采样频率500，每5个点画一次，那么绘图认为的采样频率就是100），大概意思就是每秒画多少个点
        public int Gain = 10;               //增益，10mm/mv
        public double uVpb = 0.899999976158142;
      
        Bitmap rendedImage;
        int xStartL = 0;
        int xStartR = 0;
        int xStartB = 0;
        float xLeft,xRight,xBottom;
        int[] yBase = new int[13];
        int yBottom;
        Graphics dc;
        Pen backGrid1mm = new Pen(Color.FromArgb(255, 223, 224), 1);
        Pen backGrid5mm = new Pen(Color.FromArgb(240, 143, 143), 1);
        Pen wavePen = new Pen(Color.FromArgb(0x33, 0x33, 0x33), 1);
        Font textFont = new Font("微软雅黑",12,GraphicsUnit.Pixel);
        short[] yPrev = new short[13];
        float xStep;
        

        public Color WaveColor
        {
            set
            {
                wavePen.Dispose();
                wavePen = new Pen(value, 1);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="resolution">x pixel/1mm</param>        
        public Drawing12(Bitmap bmp, int resolution)
        {
            this.rendedImage = bmp;
            this.PixelPerMM = resolution;           
            xStep = 25f * PixelPerMM / Frequency;
            xStartL = 10 * PixelPerMM;
            xStartR = bmp.Width / 2 + 10 * PixelPerMM;
            xStartB = 10 * PixelPerMM;

            int height = bmp.Height;
            float perHeight = (height - PixelPerMM * 20) / 12f;
            for (int i = 0; i < 12; i++)
            {
                if (i < 6)
                {
                    yBase[i] = (int)(PixelPerMM * 10 + perHeight * (i * 2 ))+10;
                }
                else
                {
                    yBase[i] = yBase[i - 6];
                }
            }
            yBase[1] = yBase[2];
            yBase[2] = yBase[4];
            yBase[3] = (int)(PixelPerMM * 10 + perHeight * (6 * 2))+10;
            yBottom = yBase[3] - (int)perHeight;
            dc = Graphics.FromImage(bmp);
            dc.SmoothingMode = SmoothingMode.AntiAlias;
            this.DrawBackGrid(dc, bmp.Width, bmp.Height);
            this.DrawLeadName();
            xLeft = xStartL;
            xBottom = xStartB;
            xRight = xStartR;
        }

        /// <summary>
        /// 更新绘图
        /// </summary>
        /// <param name="data"></param>
        /// <param name="refreshRegion"></param>
        public void DrawWave(short[] data,out RectangleF[] refreshRegion)
        {
            //for (int i = 0; i < 6; i++)
            //{
            //    dc.DrawLine(wavePen, xLeft, yBase[i] - valToPixel(yPrev[i], uVpb), xLeft + xStep, yBase[i] - valToPixel(data[i],uVpb));
            //    yPrev[i] = data[i];                
            //}
            //dc.DrawLine(wavePen, xBottom, yBase[3] - valToPixel(yPrev[3], uVpb), xBottom + xStep, yBase[3] - valToPixel(data[9], uVpb));
            //yPrev[12] = data[1];
            xLeft+=xStep;
            for (int i = 6; i < 10; i++)
            {
                dc.DrawLine(wavePen, xBottom, yBase[i-6] - valToPixel(yPrev[i], uVpb), xBottom + xStep, yBase[i-6] - valToPixel(data[i],uVpb));
                yPrev[i] = data[i];
            }
            //xRight+=xStep;
            //if (xLeft+xStep >= rendedImage.Width / 2)
            //{
            //    xLeft = xStartL;
            //    xRight = xStartR;
            //}
            xBottom += xStep; ;
            if (xBottom + xStep >= rendedImage.Width)
            {
                xBottom = xStartB;
            }
            refreshRegion = null;
            this.Refresh(xLeft + 1, xRight + 1, xBottom + 1, out refreshRegion);
            //Debug.WriteLine("count:" + refreshRegion.Count());
            //foreach (RectangleF rect in refreshRegion)
            //{
            //    Debug.WriteLine("xBottom[0]:" + xBottom + "  " + rect.X + "  " + rect.Y + "  " + rect.Width + "  " + rect.Height);
            //}

        }

        /// <summary>
        /// 采样值绘制多少像素
        /// </summary>
        /// <param name="value"></param>
        /// <param name="uVpb"></param>
        /// <returns></returns>
        float valToPixel(short value,double uVpb)
        {
            return (float)(value * uVpb / 1000 * PixelPerMM * 10);
        }

        private void Refresh(float xLeft,float xRight,float xBottom, out RectangleF[] refreshRegion)
        {
            int width = 10 * PixelPerMM;
            //if (xLeft + width > rendedImage.Width / 2)
            //{
            //    //RectangleF leftRect1 = new RectangleF(xLeft, 0, rendedImage.Width / 2 - xLeft, rendedImage.Height);
            //    //RectangleF leftRect2 = new RectangleF(xStartL, 0, width - (rendedImage.Width / 2 - xLeft), rendedImage.Height);
            //    //RectangleF rightRect1 = new RectangleF(xRight, 0, rendedImage.Width - xRight, rendedImage.Height);
            //    //RectangleF rightRect2 = new RectangleF(xStartR, 0, width - (rendedImage.Width - xRight), rendedImage.Height);
            //    RectangleF leftRect1 = new RectangleF(xLeft, 0, rendedImage.Width / 2 - xLeft, yBottom);
            //    RectangleF leftRect2 = new RectangleF(xStartL, 0, width - (rendedImage.Width / 2 - xLeft), yBottom);
            //    RectangleF rightRect1 = new RectangleF(xRight, 0, rendedImage.Width - xRight, yBottom);
            //    RectangleF rightRect2 = new RectangleF(xStartR, 0, width - (rendedImage.Width - xRight), yBottom);
            //    Region clip = new Region(leftRect1);
            //    clip.Union(leftRect2);
            //    clip.Union(rightRect1);
            //    clip.Union(rightRect2);
            //    if (xBottom + width > rendedImage.Width)
            //    {
            //        RectangleF BottomRect1 = new RectangleF(xBottom, yBottom, rendedImage.Width - xBottom, rendedImage.Height - yBottom);
            //        RectangleF BottomRect2 = new RectangleF(xStartB, yBottom, width - (rendedImage.Width - xBottom), rendedImage.Height - yBottom);
            //        clip.Union(BottomRect1);
            //        clip.Union(BottomRect2);
            //        refreshRegion = new RectangleF[6] { leftRect1, leftRect2, rightRect1, rightRect2, BottomRect1, BottomRect2 };
            //    }
            //    else
            //    {
            //        RectangleF BottomRect = new RectangleF(xBottom, yBottom, width , rendedImage.Height - yBottom);
            //        clip.Union(BottomRect);
            //        refreshRegion = new RectangleF[5] { leftRect1, leftRect2, rightRect1, rightRect2, BottomRect };
            //    }
            //    dc.Clip = clip;
            //   // refreshRegion = new RectangleF[4] { leftRect1, leftRect2, rightRect1, rightRect2 };
            //}
            //else
            {
                //RectangleF leftRect = new RectangleF(xLeft, 0, width, rendedImage.Height);
                //RectangleF rightRect = new RectangleF(xRight, 0, width, rendedImage.Height);
                RectangleF leftRect = new RectangleF(xLeft, 0, width, 1);
                RectangleF rightRect = new RectangleF(xRight, 0, width, 1);
                Region clip = new Region(leftRect);
                clip.Union(rightRect);
                if (xBottom + width > rendedImage.Width)
                {
                    RectangleF BottomRect1 = new RectangleF(xBottom, 0, rendedImage.Width - xBottom, rendedImage.Height);
                    RectangleF BottomRect2 = new RectangleF(xStartB, 0, width - (rendedImage.Width - xBottom), rendedImage.Height);
                    clip.Union(BottomRect1);
                    clip.Union(BottomRect2);
                    refreshRegion = new RectangleF[4] { leftRect, rightRect, BottomRect1, BottomRect2 };
                }
                else
                {
                    RectangleF BottomRect = new RectangleF(xBottom, 0, width, rendedImage.Height);
                    clip.Union(BottomRect);
                    refreshRegion = new RectangleF[3] { leftRect, rightRect, BottomRect };
                }

                dc.Clip = clip;
                //refreshRegion = new RectangleF[2] { leftRect, rightRect };
            }
            this.DrawBackGrid(dc, rendedImage.Width, rendedImage.Height);
            dc.ResetClip();
        }

        /// <summary>
        /// invalidate
        /// </summary>
        private void Refresh()
        {
            Rectangle leftRect = new Rectangle(xStartL, 0, rendedImage.Width / 2 - xStartL, rendedImage.Height);
            Rectangle rightRect = new Rectangle(xStartR, 0, rendedImage.Width - xStartR, rendedImage.Height);
            Region clip = new Region(leftRect);
            clip.Union(rightRect);
            dc.Clip = clip;
            this.DrawBackGrid(dc, rendedImage.Width, rendedImage.Height);
        }

        private void DrawBackGrid(Graphics dc, int width, int height)
        {
            dc.Clear(Color.White);            
            for (int row = 0; row < height; row+=PixelPerMM)
            {
                if (row % 25 != 0)
                {
                    dc.DrawLine(backGrid1mm, 0, row, width, row);
                }              
            }
            for (int col = 0; col < width; col+=PixelPerMM)
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

        /// <summary>
        /// 绘制导联名与定标
        /// </summary>
        private void DrawLeadName()
        {
            string[] leadNameL = new string[] { "I", "II", "III", "aVR", "aVL", "aVF" };
            string[] leadNameR = new string[] { "V1", "V2", "V3", "V4", "V5", "V6" };
            for (int i = 0; i < 4; i++)
            {
               // this.DrawLeadNameUnit(dc, textFont, wavePen, leadNameL[i], 0, yBase[i], PixelPerMM);
                this.DrawLeadNameUnit(dc, textFont, wavePen, leadNameR[i], 0, yBase[i], PixelPerMM);
            }
           // this.DrawLeadNameUnit(dc, textFont, wavePen, leadNameL[1], 0, yBase[12], PixelPerMM);
        }

        /// <summary>
        /// 绘制单一导联的导联名与定标
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="textFont"></param>
        /// <param name="wavePen"></param>
        /// <param name="leadName"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="pixelPerMM"></param>
        private void DrawLeadNameUnit(Graphics dc, Font textFont, Pen wavePen, string leadName, int x, int y, int pixelPerMM)
        {
            PointF[] normLinePoints = new PointF[] 
            {
                new PointF(x+5*pixelPerMM,y),
                new PointF(x+6.5f*pixelPerMM,y),
                new PointF(x+6.5f*pixelPerMM,y-10*pixelPerMM),
                new PointF(x+8.5f*pixelPerMM,y-10*pixelPerMM),
                new PointF(x+8.5f*pixelPerMM,y),
                new PointF(x+10*pixelPerMM,y)
            };
            dc.DrawLines(wavePen, normLinePoints);
//            dc.DrawString(leadName, textFont, Brushes.Black, x, y - 15 * pixelPerMM);
            dc.DrawString(leadName, textFont, Brushes.Black, x, y - 10 * pixelPerMM);
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
