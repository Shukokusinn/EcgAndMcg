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
using System.Windows.Shapes;
using ape.EcgSolu.Model;
using System.Windows.Markup;
using System.Configuration;

namespace ape.EcgSolu.WorkUnit.Diagnosis
{
    /// <summary>
    /// PreviewWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PreviewWindow : Window
    {
        public PreviewWindow()
        {
            InitializeComponent();           
        }

        public PreviewWindow(Ecg ecgEntity, Color grid5mmColor, Color grid1mmColor, double grid5mmThickness,
            double grid1mmThickness, double waveThickness, bool grid5mmPoint, bool grid1mmPoint)
        {
            InitializeComponent();
            int reportType;
            int.TryParse(ConfigurationManager.AppSettings["ReportTemplate"], out reportType);
           
            switch (reportType)
            {
                case 1:
                    loadReport1(ecgEntity, grid5mmColor, grid1mmColor, grid5mmThickness, grid1mmThickness, waveThickness, grid5mmPoint, grid1mmPoint);
                    break;
                case 2:
                    loadReport2(ecgEntity, grid5mmColor, grid1mmColor, grid5mmThickness, grid1mmThickness, waveThickness, grid5mmPoint, grid1mmPoint);
                    break;
                default:
                    loadReport1(ecgEntity, grid5mmColor, grid1mmColor, grid5mmThickness, grid1mmThickness, waveThickness, grid5mmPoint, grid1mmPoint);
                    break;
            }            
        }

        private void loadReport1(Ecg ecgEntity, Color grid5mmColor, Color grid1mmColor, double grid5mmThickness,
            double grid1mmThickness, double waveThickness, bool grid5mmPoint, bool grid1mmPoint)
        {
            FrameworkElement report = ReportVisualFactory.BuildVisual(ecgEntity, grid5mmColor, grid1mmColor, grid5mmThickness, grid1mmThickness, waveThickness, grid5mmPoint, grid1mmPoint);
            FixedPage fixedpage = new FixedPage();
            fixedpage.Width = 1122.5;
            fixedpage.Height = 793.7;
            fixedpage.Children.Add(report);
            PageContent pc = new PageContent();
            ((IAddChild)pc).AddChild(fixedpage);
            FixedDocument fixedDoc = new FixedDocument();
            fixedDoc.Pages.Add(pc);
            documentViewReport.Document = fixedDoc;
        }

        private void loadReport2(Ecg ecgEntity, Color grid5mmColor, Color grid1mmColor, double grid5mmThickness,
            double grid1mmThickness, double waveThickness, bool grid5mmPoint, bool grid1mmPoint)
        {
            FrameworkElement report = ReportVisualFactory.BuildVisual(ecgEntity, grid5mmColor, grid1mmColor, grid5mmThickness, grid1mmThickness, waveThickness, grid5mmPoint, grid1mmPoint);
            FixedPage fixedpage = new FixedPage();
            fixedpage.Height = 1122.5;
            fixedpage.Width = 793.7;
            fixedpage.Children.Add(report);
            PageContent pc = new PageContent();
            ((IAddChild)pc).AddChild(fixedpage);
            FixedDocument fixedDoc = new FixedDocument();
            fixedDoc.Pages.Add(pc);
            documentViewReport.Document = fixedDoc;
        }
    }
}
