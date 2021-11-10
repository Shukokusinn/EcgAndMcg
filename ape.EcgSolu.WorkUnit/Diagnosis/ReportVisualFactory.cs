//----------------------------------------------------------------
//打印报告的绘图
//----------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ape.EcgSolu.Model;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Shapes;
using System.Configuration;

namespace ape.EcgSolu.WorkUnit.Diagnosis
{
    class ReportVisualFactory
    {
        public static FrameworkElement BuildVisual(Ecg ecgEntity, Color grid5mmColor, Color grid1mmColor, double grid5mmThickness,
            double grid1mmThickness, double waveThickness, bool grid5mmPoint, bool grid1mmPoint)
        {
            int reportType;
            int.TryParse(ConfigurationManager.AppSettings["ReportTemplate"], out reportType);
            FrameworkElement reportVisual = null;
            switch (reportType)
            {
                case 1:
                    reportVisual = new ReportVisual1(ecgEntity, grid5mmColor, grid1mmColor, grid5mmThickness, grid1mmThickness, waveThickness, grid5mmPoint, grid1mmPoint);
                    break;
                case 2:
                    reportVisual = new ReportVisual2(ecgEntity, grid5mmColor, grid1mmColor, grid5mmThickness, grid1mmThickness, waveThickness, grid5mmPoint, grid1mmPoint);
                    break;
                default:
                    reportVisual = new ReportVisual1(ecgEntity, grid5mmColor, grid1mmColor, grid5mmThickness, grid1mmThickness, waveThickness, grid5mmPoint, grid1mmPoint);
                    break;
            }
            return reportVisual;
        }
    }
        
}
