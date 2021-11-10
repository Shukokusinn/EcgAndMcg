//---------------------------------------------------------------
//诊断界面
//---------------------------------------------------------------
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
using System.Drawing;
using ape.EcgSolu.Model;
using ape.EcgSolu.BLL;
using ape.EcgSolu.WorkUnit.Draw;
using System.Printing;
using System.Configuration;
using ape.EcgSolu.WorkUnit.Controls;
using Microsoft.Win32;
using System.IO;
using System.Windows.Xps.Packaging;
using System.Windows.Xps;
using System.Windows.Markup;
using ape.EcgSystem.Analysis;
using ape.EcgSystem.Analysis.Model;

namespace ape.EcgSolu.WorkUnit.Diagnosis
{
    /// <summary>
    /// DiagWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DiagWindow : Window
    {
        private int dataCount;  //不直接使用Ecg.DataCount而定义一个变量，是用于处理Ecg.DataCount值错误，与Ecg.Data的长度不一致的逻辑
        private Ecg ecgEntity;
        private WriteableBitmap wbmp;
        private Bitmap bmp;
        private DiagDraw drawer;
        private EcgBLL ecgBLL = new EcgBLL();
        private EcgRuler ecgRuler;
        private System.Windows.Point offset;

        public DiagWindow(Guid id)
        {
            InitializeComponent();           
            this.ecgEntity = ecgBLL.GetById(id);
            this.dataCount = this.ecgEntity.DataCount;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.ScrollViewInfo.DataContext = this.ecgEntity;
            this.loadEcgInfo(this.ecgEntity);
            wbmp = new WriteableBitmap((int)this.CanvasWave.ActualWidth, (int)this.CanvasWave.ActualHeight, 96, 96, PixelFormats.Bgr24, null);
            this.ScrollBarWave.ViewportSize = this.GetViewportSize(wbmp.PixelWidth, ecgEntity.SamplingRate);
            this.ScrollBarWave.Maximum = this.ecgEntity.DataCount-this.ScrollBarWave.ViewportSize;  
            this.ImageWave.Source=wbmp;
            wbmp.Lock();
            bmp = new Bitmap(wbmp.PixelWidth, wbmp.PixelHeight, wbmp.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format24bppRgb, wbmp.BackBuffer);
            this.drawer = new DiagDraw(bmp,ecgEntity.Data,ecgEntity.uVpb,ecgEntity.SamplingRate);
            this.drawer.ScrollTo(0);
            wbmp.AddDirtyRect(new Int32Rect(0,0,wbmp.PixelWidth,wbmp.PixelHeight));
            wbmp.Unlock();
            loadDiagWord();
            this.loadDiagValue(this.ecgEntity);
        }

        /// <summary>
        /// 加载信息到界面控件
        /// </summary>
        /// <param name="ecgEntity"></param>
        private void loadEcgInfo(Ecg ecgEntity)
        {
            this.TextBlockGender.Text = Helper.GenderConvert(ecgEntity.Sex);           
        }

        /// <summary>
        /// 滚动绘图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollBarWave_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            wbmp.Lock();
            int position = (int)this.ScrollBarWave.Value;
            this.drawer.ScrollTo(position);
            wbmp.AddDirtyRect(new Int32Rect(50, 0, wbmp.PixelWidth / 2 - 50, wbmp.PixelHeight));
            wbmp.AddDirtyRect(new Int32Rect(wbmp.PixelWidth / 2 + 50, 0, wbmp.PixelWidth / 2 - 50, wbmp.PixelHeight));
            wbmp.AddDirtyRect(new Int32Rect(wbmp.PixelWidth / 2, wbmp.PixelHeight-50, 50, 50));
            wbmp.Unlock();
        }

        //计算scrollbar滚动条的滚动范围值
        private float GetViewportSize(int width,int samplingRate)
        {
            int clientWidth = width / 2-50;
            float xSetp = 25 * 5f / samplingRate;
            float size = clientWidth / xSetp;
            return size;
        }

        /// <summary>
        /// 保存诊断
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            this.ButtonSave.IsEnabled = false;
            this.ecgEntity.Status = DataReference.Status.Done;
            this.ecgEntity.DiagResult = this.TextBoxDiagResult.Text;
            int hr,rr,rv1,rv5,sv1,sv5;
            int.TryParse(this.textBoxHeartRate.Text, out hr);
            int.TryParse(this.textBoxRR.Text, out rr);
            int.TryParse(this.textBoxRV1.Text, out rv1);
            int.TryParse(this.textBoxRV5.Text, out rv5);
            int.TryParse(this.textBoxSV1.Text, out sv1);
            int.TryParse(this.textBoxSV5.Text, out sv5);
            this.ecgEntity.HeartRate = hr;
            this.ecgEntity.RR = rr;
            this.ecgEntity.RV1 = rv1;
            this.ecgEntity.RV5 = rv5;
            this.ecgEntity.SV1 = sv1;
            this.ecgEntity.SV5 = sv5;
            this.ecgEntity.DataStart = (int)this.ScrollBarWave.Value;
            ecgBLL.UpdateResult(this.ecgEntity);
            this.ButtonSave.IsEnabled = true;
            this.textBlockInfo.Text = "保存诊断成功，姓名：" + this.ecgEntity.PatientName;
        }

        /// <summary>
        /// 加载诊断术语
        /// </summary>
        private void loadDiagWord()
        {
            List<DiagWord> allWordList = new DiagWordBLL().GetDiagWord();
            var cateList = allWordList.Select(w => w.Category).Distinct();
            foreach (var cate in cateList)
            {
                Expander expander = new Expander { Header = cate };
                var wordList = allWordList.Where(w => w.Category == cate);
                StackPanel stackPanelWord = new StackPanel { Orientation = Orientation.Vertical};
                foreach (var word in wordList)
                {
                    
                    TextBlock textBlockWord = new TextBlock { Text = word.Word };                    
                    Border border = new Border { BorderThickness = new Thickness(1),Padding=new Thickness(20,3,3,3) };
                    border.Child = textBlockWord;
                    border.MouseEnter += new MouseEventHandler(textBlockWord_MouseEnter);
                    border.MouseLeave += new MouseEventHandler(textBlockWord_MouseLeave);
                    border.MouseLeftButtonDown += new MouseButtonEventHandler(textBlockWord_MouseLeftButtonDown);
                    stackPanelWord.Children.Add(border);
                }
                expander.Content = stackPanelWord;                
                this.StackPanelDiagWord.Children.Add(expander);
            }
        }

        private void textBlockWord_MouseEnter(object sender, MouseEventArgs e)
        {
            Border border = sender as Border;
            border.Background = System.Windows.Media.Brushes.ForestGreen;
            border.BorderBrush = System.Windows.Media.Brushes.Black;            
        }

        private void textBlockWord_MouseLeave(object sender, MouseEventArgs e)
        {
            Border border = sender as Border;
            TextBlock textBlock = border.Child as TextBlock;
            border.BorderBrush = System.Windows.Media.Brushes.Transparent;
            border.Background = System.Windows.Media.Brushes.Transparent;
        }

        private void textBlockWord_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            TextBlock textBlock = border.Child as TextBlock;
            if (!this.TextBoxDiagResult.Text.Contains(textBlock.Text))
            {
                this.TextBoxDiagResult.Text += textBlock.Text + "\r\n";
            }
        }

        /// <summary>
        /// 打开预览报告窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonPreview_Click(object sender, RoutedEventArgs e)
        {
            System.Drawing.Color color5mm = System.Drawing.Color.FromArgb(int.Parse(ConfigurationManager.AppSettings["Grid5mmColor"]));
            System.Drawing.Color color1mm = System.Drawing.Color.FromArgb(int.Parse(ConfigurationManager.AppSettings["Grid1mmColor"]));            
            System.Windows.Media.Color grid5mmColor = System.Windows.Media.Color.FromRgb(color5mm.R,color5mm.G,color5mm.B);
            System.Windows.Media.Color grid1mmColor = System.Windows.Media.Color.FromRgb(color1mm.R, color1mm.G, color1mm.B);
            double grid5mmThickness = double.Parse(ConfigurationManager.AppSettings["Grid5mmThickness"]);
            double grid1mmThickness = double.Parse(ConfigurationManager.AppSettings["Grid1mmThickness"]);
            double waveThickness = double.Parse(ConfigurationManager.AppSettings["WaveThickness"]);
            bool grid5mmPoint = bool.Parse(ConfigurationManager.AppSettings["Grid5mmPoint"]);
            bool grid1mmPoint = bool.Parse(ConfigurationManager.AppSettings["Grid1mmPoint"]);
            PreviewWindow previewWindow = new PreviewWindow(this.ecgEntity, grid5mmColor, grid1mmColor, grid5mmThickness, grid1mmThickness, waveThickness, grid5mmPoint, grid1mmPoint);
            previewWindow.Owner = this;
            previewWindow.ShowInTaskbar = false;
            previewWindow.ShowDialog();
        }

        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog dlg = new PrintDialog();
            if (dlg.ShowDialog() == true)
            {
                Visual report = this.buildReportVisual();          
                //dlg.PrintTicket.PageOrientation = PageOrientation.Landscape;
                //dlg.PrintTicket.PageMediaSize = new PageMediaSize(dlg.PrintableAreaWidth, dlg.PrintableAreaHeight);
                dlg.PrintVisual(report, string.Empty);
            }
        }

        //创建报告visual
        private FrameworkElement buildReportVisual()
        {           
            System.Drawing.Color color5mm = System.Drawing.Color.FromArgb(int.Parse(ConfigurationManager.AppSettings["Grid5mmColor"]));
            System.Drawing.Color color1mm = System.Drawing.Color.FromArgb(int.Parse(ConfigurationManager.AppSettings["Grid1mmColor"]));
            System.Windows.Media.Color grid5mmColor = System.Windows.Media.Color.FromRgb(color5mm.R, color5mm.G, color5mm.B);
            System.Windows.Media.Color grid1mmColor = System.Windows.Media.Color.FromRgb(color1mm.R, color1mm.G, color1mm.B);
            double grid5mmThickness = double.Parse(ConfigurationManager.AppSettings["Grid5mmThickness"]);
            double grid1mmThickness = double.Parse(ConfigurationManager.AppSettings["Grid1mmThickness"]);
            double waveThickness = double.Parse(ConfigurationManager.AppSettings["WaveThickness"]);
            bool grid5mmPoint = bool.Parse(ConfigurationManager.AppSettings["Grid5mmPoint"]);
            bool grid1mmPoint = bool.Parse(ConfigurationManager.AppSettings["Grid1mmPoint"]);
            FrameworkElement report = ReportVisualFactory.BuildVisual(this.ecgEntity, grid5mmColor, grid1mmColor, grid5mmThickness, grid1mmThickness, waveThickness, grid5mmPoint, grid1mmPoint);
            return report;
        }

        /// <summary>
        /// 测量尺，鼠标按下，开始拖放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanvasWave_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is EcgRuler)
            {
                this.ecgRuler = (EcgRuler)e.Source;
                offset = Mouse.GetPosition(this.ecgRuler);
            }
        }

        /// <summary>
        /// 测量尺，鼠标弹起，放弃拖放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanvasWave_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.ecgRuler = null;           
        }

        /// <summary>
        /// 测量尺，移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanvasWave_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.ecgRuler != null)
            {
                PositionCard();
            }
        }

        /// <summary>
        /// 移动标尺
        /// </summary>
        private void PositionCard()
        {
            System.Windows.Point mousePos = Mouse.GetPosition(this.CanvasWave);
            Canvas.SetLeft(this.ecgRuler, mousePos.X - offset.X);
            Canvas.SetTop(this.ecgRuler, mousePos.Y - offset.Y);           
        }

        /// <summary>
        /// 切换显示和隐藏测量尺
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBoxMeasure_Click(object sender, RoutedEventArgs e)
        {
            if (this.checkBoxMeasure.IsChecked==true)
            {
                ecgRulerMeasure.Visibility = Visibility.Visible;
            }
            else
            {
                ecgRulerMeasure.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 进入叠加图调整界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonAdjust_Click(object sender, RoutedEventArgs e)
        {
            AjustWindow ajustWindow = new AjustWindow(this.ecgEntity);
            ajustWindow.Owner = this;
            ajustWindow.ShowInTaskbar = false;
            ajustWindow.ShowDialog();
        }

        /// <summary>
        /// 弹出增益选择下拉菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonGain_Click(object sender, RoutedEventArgs e)
        {
            this.popupGain.IsOpen = !this.popupGain.IsOpen;
            this.listBoxGain.SelectionChanged -= this.listBoxGain_SelectionChanged;
            this.listBoxGain.SelectedIndex = -1;
            this.listBoxGain.SelectionChanged += this.listBoxGain_SelectionChanged;
        }

        /// <summary>
        /// 选择增益调整
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxGain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BitmapImage bi;
            bool update = true;
            int gain = 10;
            switch (this.listBoxGain.SelectedIndex)
            {
                case 0:
                    bi = new BitmapImage(new Uri("/EcgWorkbeanch;component/Images/Gain1_40.png", UriKind.Relative));
                    this.imageGain.Source = bi;
                    gain = 5;
                    break;
                case 1:
                    bi = new BitmapImage(new Uri("/EcgWorkbeanch;component/Images/Gain2_40.png", UriKind.Relative));
                    this.imageGain.Source = bi;
                    gain = 10;
                    break;
                case 2:
                    bi = new BitmapImage(new Uri("/EcgWorkbeanch;component/Images/Gain3_40.png", UriKind.Relative));
                    this.imageGain.Source = bi;
                    gain = 20;
                    break;
                default:
                    update = false;
                    break;
            }
            this.popupGain.IsOpen = false;
            if (update)
            {                
                this.drawer.Gain = gain;
                wbmp.Lock();
                int position = (int)this.ScrollBarWave.Value;
                this.drawer.Refresh();
                this.drawer.ScrollTo(position);
                wbmp.AddDirtyRect(new Int32Rect(0, 0, wbmp.PixelWidth, wbmp.PixelHeight));               
                wbmp.Unlock();
            }
        }

        private void buttonReportOutput_Click(object sender, RoutedEventArgs e)
        {
            this.popupReportOutput.IsOpen = !this.popupReportOutput.IsOpen;            
        }

        /// <summary>
        /// 输出报告成xps文档
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stackPanelReportXps_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "XPS Document files|*.xps";
            if (sfd.ShowDialog() == true)
            {
                FrameworkElement report = this.buildReportVisual();
                FixedPage fixedpage = new FixedPage();
                int reportType;
                int.TryParse(ConfigurationManager.AppSettings["ReportTemplate"], out reportType);                
                switch (reportType)
                {
                    case 1:
                         fixedpage.Width = 1122.5;
                         fixedpage.Height = 793.7;
                        break;
                    case 2:
                         fixedpage.Width = 793.7;
                         fixedpage.Height = 1122.5;
                        break;
                    default:
                        fixedpage.Width = 1122.5;
                        fixedpage.Height = 793.7;
                        break;
                }               
                fixedpage.Children.Add(report);
                PageContent pc = new PageContent();
                ((IAddChild)pc).AddChild(fixedpage);
                FixedDocument fixedDoc = new FixedDocument();
                fixedDoc.Pages.Add(pc);
                XpsDocument xpsDoc = new XpsDocument(sfd.FileName, FileAccess.Write);
                XpsDocumentWriter xpsDocWriter = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
                xpsDocWriter.Write(fixedDoc);
                xpsDoc.Close();
            }
        }

        /// <summary>
        /// 输出报告成png图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stackPanelReportImg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "png file|*.png";
            if (sfd.ShowDialog() == true)
            {
                double mmPerInch = 25.4;
                Visual report = this.buildReportVisual();
                
                int dpi = 96;
                RenderTargetBitmap rtb;
                int reportType;
                int.TryParse(ConfigurationManager.AppSettings["ReportTemplate"], out reportType);
                switch (reportType)
                {
                    case 1:
                        rtb = new RenderTargetBitmap((int)(297 / mmPerInch * dpi), (int)(210 / mmPerInch * dpi), dpi, dpi, PixelFormats.Default);
                        break;
                    case 2:
                        rtb = new RenderTargetBitmap((int)(210 / mmPerInch * dpi), (int)(297 / mmPerInch * dpi), dpi, dpi, PixelFormats.Default);
                        break;
                    default:
                        rtb = new RenderTargetBitmap((int)(297 / mmPerInch * dpi), (int)(210 / mmPerInch * dpi), dpi, dpi, PixelFormats.Default);
                        break;
                }
                rtb.Render(report);
                using (FileStream fs = new FileStream(sfd.FileName, FileMode.Create))
                {
                    BmpBitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(rtb));
                    enc.Save(fs);
                }
            }
        }

        //分析计算，获取特征参数--或者从数据库读取特征参数
        private void loadDiagValue(Ecg ecgEntity)
        {
            ecgEntity.HeartRate = 0;
            if (ecgEntity.HeartRate == 0)
            {
                EcgFeatureValue efv = EcgAnalysis.EcgAnalysis12(ecgEntity.Data, ecgEntity.SamplingRate, ecgEntity.uVpb);
                this.textBoxHeartRate.Text = efv.HeartRate.ToString();
                this.textBoxRR.Text = efv.RR.ToString();
                this.textBoxRV1.Text = efv.RV1.ToString("F2");
                this.textBoxSV1.Text = efv.SV1.ToString("F2");
                this.textBoxRV5.Text = efv.RV5.ToString("F2");
                this.textBoxSV5.Text = efv.SV5.ToString("F2");
                this.textBoxRV5SV1.Text = (efv.RV5 + efv.SV1).ToString("F2");
                this.TextBoxRV1SV5.Text = (efv.RV1 + efv.SV5).ToString("F2");
                this.TextBoxQRS.Text = 0.ToString();
                this.textBoxPR.Text = 0.ToString();
                this.ecgEntity.TempDataCount = efv.TemplateDataCount;
                this.ecgEntity.TempData = efv.TemplateData;
                this.ecgEntity.OnOff = efv.OnOff;
            }
            else
            {
                this.textBoxHeartRate.Text = ecgEntity.HeartRate.ToString();
                this.textBoxRR.Text = ecgEntity.RR.ToString();
                this.textBoxRV1.Text = ecgEntity.RV1.ToString();
                this.textBoxSV1.Text = ecgEntity.SV1.ToString();
                this.textBoxRV5.Text = ecgEntity.SV5.ToString();
                this.textBoxSV5.Text = ecgEntity.SV5.ToString();
                this.TextBoxRV1SV5.Text = (ecgEntity.RV1 + ecgEntity.SV5).ToString();
                this.textBoxRV5SV1.Text = (ecgEntity.RV5 + ecgEntity.SV1).ToString();
                this.TextBoxQRS.Text = ecgEntity.QRS.ToString();
                this.textBoxPR.Text = ecgEntity.PR.ToString();
            }
        }
    }
}
