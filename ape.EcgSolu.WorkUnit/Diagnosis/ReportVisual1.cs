//---------------------------------------------------------------
//报告模板1，6×2的A4横向格式
//---------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ape.EcgSolu.Model;
using System.Globalization;
using System.Windows.Media.Imaging;
using ape.EcgSolu.WorkUnit.Utility;

namespace ape.EcgSolu.WorkUnit.Diagnosis
{
    class ReportVisual1:ReportVisualBase
    {
        public ReportVisual1(Ecg ecgEntity,Color grid5mmColor,Color grid1mmColor,double grid5mmThickness,
            double grid1mmThickness,double waveThickness,bool grid5mmPoint,bool grid1mmPoint):base(ecgEntity,grid5mmColor,grid1mmColor,grid5mmThickness,
            grid1mmThickness,waveThickness,grid5mmPoint,grid1mmPoint)
        {
           
        }

        /// <summary>
        /// 绘制报告，初始化
        /// </summary>
        /// <returns></returns>
        protected override DrawingVisual initVisual(Ecg ecgEntity,Color grid5mmColor,Color grid1mmColor,double grid5mmThickness,
            double grid1mmThickness,double waveThickness,bool grid5mmPoint,bool grid1mmPoint)
        {
            string[] leadNames = new string[] { "I", "II", "III", "aVR", "aVL", "aVF", "V1", "V2", "V3", "V4", "V5", "V6" };
            int leadCount = 12;
            Rect paperRect = new Rect(0, 0, 297 * unitPerMm, 210 * unitPerMm);
            Rect waveRect = new Rect(10 * unitPerMm, 30 * unitPerMm, 277 * unitPerMm, 180 * unitPerMm);
            Rect infoRect = new Rect(10 * unitPerMm, 10 * unitPerMm, 277 * unitPerMm, 190 * unitPerMm);
            double[] yBase = this.calYBase(waveRect, leadCount);            
            DrawingVisual dv = new DrawingVisual();            
            waveThickness = this.unitPerMm * waveThickness;
            grid1mmThickness = this.unitPerMm * grid1mmThickness;
            grid5mmThickness = this.unitPerMm * grid5mmThickness;
            using (DrawingContext dc = dv.RenderOpen())
            {
                //this.drawPaperRange(dc);
                dc.DrawRectangle(Brushes.White, (Pen)null, paperRect);
                GuidelineSet guidelineSet = new GuidelineSet();
                guidelineSet.GuidelinesX.Add(0.2);
                guidelineSet.GuidelinesY.Add(0.2);
                dc.PushGuidelineSet(guidelineSet);                
                this.drawGrid1mm(dc,grid1mmColor,grid1mmThickness,grid1mmPoint);
                this.drawGrid5mm(dc,grid5mmColor,grid5mmThickness,grid5mmPoint);
                this.drawLeadName(dc, waveRect, yBase,leadNames);
                this.drawWave(dc, waveRect, yBase, ecgEntity.Data, ecgEntity.uVpb, ecgEntity.SamplingRate, ecgEntity.DataStart,waveThickness);
                this.drawInfoText(dc, infoRect, ecgEntity);
            }           
            return dv;
        }

        /// <summary>
        /// 绘制页面边框，发布时不执行
        /// </summary>
        /// <param name="dc"></param>
        private void drawPaperRange(DrawingContext dc)
        {
            Pen redPen = new Pen(Brushes.Red, 1);
            Rect paperRange = new Rect(0, 0, 297 * unitPerMm, 210 * unitPerMm);
            dc.DrawRectangle((Brush)null, redPen, paperRange);
        }

        /// <summary>
        /// 绘制1mm网格
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="grid1mmColor"></param>
        /// <param name="grid1mmThickness"></param>
        /// <param name="grid1mmPoint"></param>
        private void drawGrid1mm(DrawingContext dc,Color grid1mmColor,double grid1mmThickness,bool grid1mmPoint)
        {
            int colCount = 0, rowCount = 0;
            SolidColorBrush brush = new SolidColorBrush(grid1mmColor);
            double thickness = grid1mmThickness;
            Pen linePen = new Pen(brush, thickness);
            Rect waveRect = new Rect(10 * unitPerMm, 30 * unitPerMm, 277 * unitPerMm, 175 * unitPerMm);
            if (grid1mmPoint)
            {
                #region 用点绘制
                PathGeometry geometryGrid = new PathGeometry();
                for (double col = 10 * unitPerMm; col <= 287 * unitPerMm; col += unitPerMm)
                {
                    rowCount = 0;
                    if (colCount % 5 != 0)
                    {
                        for (double row = 30 * unitPerMm; row <= 195 * unitPerMm; row += unitPerMm)
                        {
                            if (rowCount % 5 != 0)
                            {
                                geometryGrid.AddGeometry(new RectangleGeometry(new Rect(col, row, thickness, thickness)));
                            }
                            rowCount++;
                        }
                    }
                    colCount++;
                }
                dc.DrawGeometry(brush, linePen, geometryGrid); 
                #endregion
            }
            else
            {
                #region 用直线绘制
                for (double col = 10 * unitPerMm; col <= 287 * unitPerMm; col += unitPerMm)
                {
                    if (colCount % 5 != 0)
                    {
                        dc.DrawLine(linePen, new Point(col, waveRect.Top), new Point(col, waveRect.Bottom));
                    }
                    colCount++;
                }
                for (double row = 30 * unitPerMm; row <= 205 * unitPerMm; row += unitPerMm)
                {
                    if (rowCount % 5 != 0)
                    {
                        dc.DrawLine(linePen, new Point(waveRect.Left, row), new Point(waveRect.Right, row));
                    }
                    rowCount++;
                } 
                #endregion
            }
            dc.DrawRectangle((Brush)null, linePen, waveRect);
        }

        /// <summary>
        /// 绘制5mm网格
        /// </summary>
        /// <param name="dc"></param>
        private void drawGrid5mm(DrawingContext dc,Color grid5mmColor,double grid5mmThickness,bool grid5mmPoint)
        {
            int colCount = 0, rowCount = 0;
            SolidColorBrush brush = new SolidColorBrush(grid5mmColor);
            double thickness = grid5mmThickness;
            Pen linePen = new Pen(brush, thickness);
            Rect waveRect = new Rect(10 * unitPerMm, 30 * unitPerMm, 277 * unitPerMm, 175 * unitPerMm);
            if (grid5mmPoint)
            {
                linePen.DashStyle = new DashStyle(new double[] { thickness,this.unitPerMm },0 );
            }
            for (double col = 10 * unitPerMm; col <= 287 * unitPerMm; col += unitPerMm)
            {
                if (colCount % 5 == 0)
                {
                    dc.DrawLine(linePen, new Point(col, waveRect.Top), new Point(col, waveRect.Bottom));
                }
                colCount++;
            }
            for (double row = 30 * unitPerMm; row <= 205 * unitPerMm; row += unitPerMm)
            {
                if (rowCount % 5 == 0)
                {
                    dc.DrawLine(linePen, new Point(waveRect.Left, row), new Point(waveRect.Right, row));
                }
                rowCount++;
            }
            dc.DrawRectangle((Brush)null, linePen, waveRect);
        }

        /// <summary>
        /// 绘制导联名
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="clientRect"></param>
        /// <param name="leadNames"></param>
        private void drawLeadName(DrawingContext dc,Rect clientRect ,double[] yBase, string[] leadNames)
        {
            double col1, col2, col3;
            if (leadNames.Length > 12)
            {
                double xInterval = clientRect.Width / 3;
                col1 = 0;
                col2 = xInterval;
                col3 = xInterval * 2;
            }
            else
            {
                double xInterval = clientRect.Width / 2;
                col1 = 0;
                col2 = xInterval;
                col3 = 0;
            }
            for (int i = 0; i < leadNames.Length; i++)
            {
                double x;
                if (i < 6)
                {
                    x = col1 + clientRect.Left;
                }
                else if (i < 12)
                {
                    x = col2 + clientRect.Left;
                }
                else
                {
                    x = col3 + clientRect.Left;
                }
                this.drawCalibration(dc, x, yBase[i]+clientRect.Top, leadNames[i]);
            }
            this.drawCalibration(dc, col1 + clientRect.Left, yBase[12] + clientRect.Top, leadNames[1]);
        }

        /// <summary>
        /// 绘制单一导联的导联名与定标
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="leadName"></param>
        private void drawCalibration(DrawingContext dc, double x, double y, string leadName)
        {
            Pen linePen = new Pen(Brushes.Black, 1);
            dc.DrawLine(linePen, new Point(x + 5 * unitPerMm, y), new Point(x + 6.5 * unitPerMm, y));
            dc.DrawLine(linePen, new Point(x + 6.5 * unitPerMm, y), new Point(x + 6.5 * unitPerMm, y - 10 * unitPerMm));
            dc.DrawLine(linePen, new Point(x + 6.5 * unitPerMm, y - 10 * unitPerMm), new Point(x + 8.5 * unitPerMm, y - 10 * unitPerMm));
            dc.DrawLine(linePen, new Point(x + 8.5 * unitPerMm, y - 10 * unitPerMm), new Point(x + 8.5 * unitPerMm, y));
            dc.DrawLine(linePen, new Point(x + 8.5 * unitPerMm, y), new Point(x + 10 * unitPerMm, y));
            FormattedText formattedText = new FormattedText(leadName, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Verdana"), 10, Brushes.Black);
            dc.DrawText(formattedText, new Point(x + 2*unitPerMm, y - 10*unitPerMm-formattedText.Height));
        }

        /// <summary>
        /// 绘制波形
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="clietRect"></param>
        /// <param name="data"></param>
        /// <param name="uVpb"></param>
        private void drawWave(DrawingContext dc, Rect clientRect,double[] yBase, short[,] data, double uVpb,int sampleRate,int dataStart,double waveThickness)
        {
            int leadCount = yBase.Length-1;
            double col1, col2, col3;
            double xInterval;
            if (leadCount > 12)
            {
                xInterval = clientRect.Width / 3;
                col1 = 0;
                col2 = xInterval;
                col3 = xInterval * 2;
            }
            else
            {
                xInterval = clientRect.Width / 2;
                col1 = 0;
                col2 = xInterval;
                col3 = 0;
            }            
            for (int i = 0; i < leadCount; i++)
            {
                double x;
                if (i < 6)
                {
                    x = col1 + clientRect.Left + 10 * unitPerMm;
                }
                else if (i < 12)
                {
                    x = col2 + clientRect.Left + 10 * unitPerMm;
                }
                else
                {
                    x = col3 + clientRect.Left + 10 * unitPerMm;
                }
                //this.drawLeadWave(dc, clientRect, x, yBase[i]+clientRect.Top, data, uVpb, sampleRate, dataStart, i,xInterval-10*unitPerMm);
                this.drawLeadWaveGeometry(dc, clientRect, x, yBase[i] + clientRect.Top, data, uVpb, sampleRate, dataStart, i, xInterval - 10 * unitPerMm, waveThickness);
            }
            this.drawLeadWaveGeometry(dc, clientRect, col1 + clientRect.Left + 10 * unitPerMm, yBase[12] + clientRect.Top, data, uVpb, sampleRate, dataStart, 1, xInterval*2 - 10 * unitPerMm, waveThickness);
        }

        /// <summary>
        /// 绘制单一导联
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="clientRect"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="data"></param>
        /// <param name="uVpb"></param>
        /// <param name="sampleRate"></param>
        /// <param name="dataStart"></param>
        /// <param name="leadIndex"></param>
        private void drawLeadWave(DrawingContext dc, Rect clientRect, double x, double y, short[,] data, double uVpb, int sampleRate, int dataStart, int leadIndex,double drawRegionWidth)
        {
            int leadCount = data.GetLength(0);
            int dataCount = data.GetLength(1);
            double xSetp = 25 * unitPerMm / sampleRate;
            Point prevPoint = new Point(x, y);
            Point nextPoint;
            Pen linePen=new Pen(Brushes.Black,1);
            int drawDataCount = Math.Min((int)(drawRegionWidth / xSetp), dataCount - dataStart);
            for (int i = 0; i < drawDataCount; i++)
            {
                double xc = x + i * xSetp;
                double yc = y - ValueToUnit(data[leadIndex, dataStart + i], uVpb, unitPerMm);
                nextPoint=new Point(xc,yc);
                dc.DrawLine(linePen, prevPoint, nextPoint);
                prevPoint = nextPoint;
            }
        }

        /// <summary>
        /// 绘制单一导联，使用StreamGeometry来绘制
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="clientRect"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="data"></param>
        /// <param name="uVpb"></param>
        /// <param name="sampleRate"></param>
        /// <param name="dataStart"></param>
        /// <param name="leadIndex"></param>
        /// <param name="drawRegionWidth"></param>
        private void drawLeadWaveGeometry(DrawingContext dc, Rect clientRect, double x, double y, short[,] data, double uVpb, int sampleRate, int dataStart, int leadIndex, double drawRegionWidth, double waveThickness)
        {
            int leadCount = data.GetLength(0);
            int dataCount = data.GetLength(1);
            double xSetp = 25 * unitPerMm / sampleRate;
            Point prevPoint = new Point(x, y);
            Point nextPoint;
            Pen linePen = new Pen(Brushes.Black, waveThickness);
            int drawDataCount = Math.Min((int)(drawRegionWidth / xSetp), dataCount - dataStart);          
            List<Point> pointList=new List<Point>();
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(prevPoint, false, false);
                for (int i = 0; i < drawDataCount; i++)
                {
                    double xc = x + i * xSetp;
                    double yc = y - ValueToUnit(data[leadIndex, dataStart + i], uVpb, unitPerMm);
                    nextPoint = new Point(xc, yc);
                    pointList.Add(nextPoint);                  
                }
                ctx.PolyLineTo(pointList, true, false);
            }
            dc.DrawGeometry(null,linePen,geometry);                 
        }

        /// <summary>
        /// 计算Y轴的参考坐标
        /// </summary>
        /// <param name="clientRect"></param>
        /// <param name="leadCount"></param>
        /// <returns></returns>
        private double[] calYBase(Rect clientRect, int leadCount)
        {
            double[] yBase = new double[leadCount+1];
            double yInterval = (clientRect.Height /*- 5 * unitPerMm*/) / 7;
            for (int i = 0; i <= leadCount; i++)
            {
                if (i < 6)
                {
                    yBase[i] =/* 5 * unitPerMm*/ + yInterval * i + yInterval / 2;
                }
                else if (i >= 6)
                {
                    yBase[i] = /*5 * unitPerMm*/ + yInterval * (i - 6) + yInterval / 2;
                }
                else if (i >= 12)
                {
                    yBase[i] = /*5 * unitPerMm*/ +yInterval * (i - 6) + yInterval / 2;
                }
            }
            return yBase;
        }

        /// <summary>
        /// 绘制信息文本
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="clientRect"></param>
        /// <param name="ecgEntity"></param>
        private void drawInfoText(DrawingContext dc, Rect clientRect, Ecg ecgEntity,string reportTitle="")
        {
            double xCol2 = clientRect.X + 190;
            double xCol3 = clientRect.X + 360;
            double xCol4 = clientRect.X + 510;
            FormattedText formattedTextPatName = new FormattedText("姓名  \t: "+ecgEntity.PatientName, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight, new Typeface("微软雅黑"), 11, Brushes.Black);
            FormattedText formattedTextAge = new FormattedText("年龄  \t: "+ecgEntity.Age.ToString(), CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight, new Typeface("微软雅黑"), 11, Brushes.Black);
            FormattedText formattedTextGender = new FormattedText("性别  \t: "+GenderConvert(ecgEntity.Sex), CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight, new Typeface("微软雅黑"), 11, Brushes.Black);
            FormattedText formattedTextDiagDoctor = new FormattedText("诊断医生: " + ecgEntity.DiagDoctor, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight, new Typeface("微软雅黑"), 11, Brushes.Black);
            FormattedText formattedTextExamNo = new FormattedText("检查号  \t: " + ecgEntity.ApplyNo, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight, new Typeface("微软雅黑"), 11, Brushes.Black);
            FormattedText formattedTextOutPatientNo = new FormattedText("门诊号\t: " + ecgEntity.OutPatientNo, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight, new Typeface("微软雅黑"), 11, Brushes.Black);
            FormattedText formattedTextInPatientNo = new FormattedText("住院号\t: " + ecgEntity.InPatientNo, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight, new Typeface("微软雅黑"), 11, Brushes.Black);
            FormattedText formattedTextHR = new FormattedText("HR        : " + ecgEntity.HeartRate, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight, new Typeface("微软雅黑"), 11, Brushes.Black);
            FormattedText formattedTextPR = new FormattedText("PR        : " + ecgEntity.PR, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight, new Typeface("微软雅黑"), 11, Brushes.Black);
            FormattedText formattedTextQRS = new FormattedText("QRS       : " + ecgEntity.QRS, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight, new Typeface("微软雅黑"), 10, Brushes.Black);
            FormattedText formattedTextQtQtc = new FormattedText("Qt/Qtc    : " + ecgEntity.QT+"/"+ecgEntity.QTc, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight, new Typeface("微软雅黑"), 10, Brushes.Black);
            FormattedText formattedTextPQRST = new FormattedText("P/QRS/T   : " + ecgEntity.Axis_P+"/"+ecgEntity.Axis_QRS+"/"+ecgEntity.Axis_T, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight, new Typeface("微软雅黑"), 10, Brushes.Black);
            FormattedText formattedTextRV5 = new FormattedText("RV5       : " + ecgEntity.RV5, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight, new Typeface("微软雅黑"), 10, Brushes.Black);
            FormattedText formattedTextSV1 = new FormattedText("SV1       : " + ecgEntity.SV1, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight, new Typeface("微软雅黑"), 10, Brushes.Black);
            FormattedText formattedTextResultTitle = new FormattedText("诊断", CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight, new Typeface("微软雅黑"), 10, Brushes.Black);
            FormattedText formattedTextDiagResult = new FormattedText(ecgEntity.DiagResult==null?string.Empty:ecgEntity.DiagResult, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight, new Typeface("微软雅黑"), 10, Brushes.Black);
            FormattedText formattedTextReportTitle = new FormattedText(reportTitle, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight, new Typeface("微软雅黑"), 10, Brushes.Black);
            FormattedText fromattedTextSamplingDate = new FormattedText("检查时间 : " + ecgEntity.SamplingDate, CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight, new Typeface("微软雅黑"), 10, Brushes.Black);
            formattedTextDiagResult.MaxTextWidth = 450;
            Point pointPatientName = new Point(clientRect.X, clientRect.Y);
            Point pointAge = new Point(clientRect.X, clientRect.Y + formattedTextPatName.Height);
            Point pointGender = new Point(clientRect.X, pointAge.Y + formattedTextAge.Height);
            Point pointDiagDoctor = new Point(clientRect.X, pointGender.Y + formattedTextGender.Height);
            Point pointExamNo = new Point(clientRect.X, pointDiagDoctor.Y + formattedTextDiagDoctor.Height);
            Point pointOutPatientNo = new Point(xCol2, clientRect.Y);
            Point pointInPatientNo = new Point(xCol2, pointOutPatientNo.Y + formattedTextOutPatientNo.Height);
            Point pointHR=new Point(xCol2,pointInPatientNo.Y+formattedTextInPatientNo.Height);
            Point pointPR = new Point(xCol2, pointHR.Y + formattedTextHR.Height);
            Point pointQRS = new Point(xCol2, pointPR.Y + formattedTextPR.Height);
            Point pointQtQtc = new Point(xCol3, clientRect.Y);
            Point pointPQRST = new Point(xCol3, pointQtQtc.Y + formattedTextQtQtc.Height);
            Point pointRV5 = new Point(xCol3, pointPQRST.Y + formattedTextPQRST.Height);
            Point pointSV1 = new Point(xCol3, pointRV5.Y + formattedTextPQRST.Height);
            Point pointResultTitle = new Point(xCol4, clientRect.Y);
            Point pointDiagResult = new Point(xCol4, pointResultTitle.Y + formattedTextResultTitle.Height);
            Point pointReportTitle = new Point(clientRect.X, clientRect.Y - formattedTextReportTitle.Height+3);
           // Point pointSamplingDate = new Point(xCol3, clientRect.Bottom - fromattedTextSamplingDate.Height+3);
            Point pointSamplingDate = new Point(xCol3, pointExamNo.Y );
            dc.DrawText(formattedTextPatName, pointPatientName);
            dc.DrawText(formattedTextAge, pointAge);
            dc.DrawText(formattedTextGender, pointGender);
            dc.DrawText(formattedTextDiagDoctor, pointDiagDoctor);
            dc.DrawText(formattedTextExamNo, pointExamNo);
            dc.DrawText(formattedTextOutPatientNo, pointOutPatientNo);
            dc.DrawText(formattedTextInPatientNo, pointInPatientNo);
            dc.DrawText(formattedTextHR, pointHR);
            dc.DrawText(formattedTextPR, pointPR);
            dc.DrawText(formattedTextQRS, pointQRS);
            dc.DrawText(formattedTextQtQtc, pointQtQtc);
            dc.DrawText(formattedTextPQRST, pointPQRST);
            dc.DrawText(formattedTextRV5, pointRV5);
            dc.DrawText(formattedTextSV1, pointSV1);
            dc.DrawText(formattedTextResultTitle, pointResultTitle);
            dc.DrawText(formattedTextDiagResult, pointDiagResult);
            dc.DrawText(fromattedTextSamplingDate, pointSamplingDate);            
            System.Drawing.Bitmap image = ape.EcgSolu.BLL.Utility.QRCode.CreateQRCode(buildTextInfo(ecgEntity), 80);
            BitmapSource bmpSource = Imaging.CreateBitmapSourceFromBitmap(image);
            dc.DrawImage(bmpSource, new Rect(clientRect.X+975,clientRect.Y,70,70));
        }
    }
}
