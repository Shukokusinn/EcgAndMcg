//-----------------------------------------------------------------
//采样显示界面,采样处理
//-----------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ape.EcgSolu.BLL;
using ape.EcgSolu.IDevice;
using ape.EcgSolu.Model;
using ape.EcgSolu.WorkUnit.Draw;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
namespace ape.EcgSolu.WorkUnit
{
    /// <summary>
    /// SamplingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SamplingWindow : Window
    {
        private Thread DataProcess;
        Stopwatch sw = new Stopwatch();
        Bitmap gdiBmp;
        WriteableBitmap wbmp;
        Drawing12 drawer12;
        Device samplingDevice;
        SamplingDrawArg samplingDrawArg;        //采样绘图参数，不同设备类型，参数不一样
        MemoryStream dataStream;
        Ecg ecgEntity;
        RectangleF[] refreshRegion;
        Int32Rect refreshRect;
        int currentDuration = 0;                   //当前采样时长/s
        //int currentPreDuration = 0;                 //超前采样时间长度
        int dataCount = 0;                  //多少组数
        short[] data = new short[12];
        int currentPoint = 0;               //第几组点绘一次图
        bool saveData = false;              //保存数据的开关
        bool StopBtnClicked = false;        // 停止按钮按下
        bool isSampling = false;            //是否在采样中       
        int preSamplingDuration = 0;
        Queue<short[]> preQueue = new Queue<short[]>();
        int preQueueCapacity = 0;           //预采样容纳数据长度
        HighpassFilter[] highFilter = new HighpassFilter[12];
        LowpassFilter[] lowFilter = new LowpassFilter[12];
        NotchFilter[] notchFilter = new NotchFilter[12];
        bool NotchFilterChecked;
        bool HighpassFilterChecked;
        bool LowpassFilterChecked;
        int SamplingDuration;
        int readCount;
        int ReadIdx, DataIdx;
        public SamplingWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始化，初始化绘图类
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.preSamplingDuration = int.Parse(ConfigurationManager.AppSettings["PreSamplingDuration"]);
            this.textBlockMaxDuration.Text = ConfigurationManager.AppSettings["SamplingDuration"];
            this.samplingDevice = this.initDevice();
            //this.samplingDevice.DataRecived += new EventHandler(samplingDevice_DataRecived);
            this.preQueueCapacity = this.samplingDevice.SampleRate * this.preSamplingDuration;
            this.wbmp = new WriteableBitmap((int)(this.CanvasWave.ActualWidth), (int)(this.CanvasWave.ActualHeight), 96, 96, PixelFormats.Bgr24, null);
            this.wbmp.Lock();
            this.ImageWave.Source = wbmp;
            this.gdiBmp = new Bitmap((int)wbmp.Width, (int)wbmp.Height, wbmp.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format24bppRgb, wbmp.BackBuffer);
            this.drawer12 = new Drawing12(gdiBmp, 5);
            this.drawer12.uVpb = this.samplingDevice.uVpb;
            this.refreshRect = new Int32Rect(0, 0, gdiBmp.Width, gdiBmp.Height);
            this.wbmp.AddDirtyRect(refreshRect);
            this.wbmp.Unlock();
            //this.ButtonStop.IsEnabled = false;
            //this.ButtonDiag.IsEnabled = false;
            //this.ButtonSave.IsEnabled = false;
        }

        /// <summary>
        /// 创建新检查，开始采样
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            this.drawer12.WaveColor = System.Drawing.Color.FromArgb(int.Parse(ConfigurationManager.AppSettings["SamplingPreviewColor"]));
            NewExamWindow newExam = new NewExamWindow();
            newExam.Owner = this;
            newExam.ShowInTaskbar = false;
            if (newExam.ShowDialog() == true)
            {
                this.ecgEntity = newExam.EcgEntity;
                this.ecgEntity.SamplingRate = this.samplingDevice.SampleRate;
                this.initFilter(this.ecgEntity.SamplingRate);
                this.ecgEntity.uVpb = this.samplingDevice.uVpb;
                newExam.Close();
                this.drawer12.Frequency = this.samplingDrawArg.DrawFrequencey;
                this.drawer12.WaveColor = System.Drawing.Color.FromArgb(int.Parse(ConfigurationManager.AppSettings["SamplingPreviewColor"]));
                this.dataStream = new MemoryStream();
                this.samplingDevice.Start();
                this.isSampling = true;
                this.ButtonStart.IsEnabled = false;
                //this.ButtonStart.Background = System.Windows.Media.Brushes.Blue;
                NotchFilterChecked = bool.Parse(ConfigurationManager.AppSettings["NotchFilterChecked"]);
                HighpassFilterChecked = bool.Parse(ConfigurationManager.AppSettings["HighpassFilterChecked"]);
                LowpassFilterChecked = bool.Parse(ConfigurationManager.AppSettings["LowpassFilterChecked"]);
                SamplingDuration = int.Parse(ConfigurationManager.AppSettings["SamplingDuration"]);
                DataProcess = new Thread(DataProcessThread);
                DataProcess.Name = "xxxxxxxxxxxxxxx";
                DataProcess.Start();
                //DataProcessThread();
            }
        }

        /// <summary>
        /// 修改数据保存标识
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            this.currentDuration = 0;
            this.dataCount = 0;
            this.drawer12.WaveColor = System.Drawing.Color.FromArgb(int.Parse(ConfigurationManager.AppSettings["SamplingSaveColor"]));
            this.saveData = true;
            sw.Start();

        }

        /// <summary>
        /// 接收数据，绘图，保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void samplingDevice_DataRecived(object sender, EventArgs e)
        {
            #region
            //测试计时的测试代码
            //this.dataCount += 100;
            //if (dataCount % 500 == 0)
            //{
            //    this.duration = dataCount / 500;
            //    this.Dispatcher.Invoke(new Action(updateUI));
            //}  
            #endregion
            Queue<short[]> internalBuffer;
//            Thread.Sleep(3000);
            internalBuffer = samplingDevice.Read();
            readCount = internalBuffer.Count;
            for (ReadIdx = 0; ReadIdx < readCount; ReadIdx++)
            {
                data = internalBuffer.Dequeue();
                //滤波
                for (DataIdx = 0; DataIdx < 12; DataIdx++)
                {
                    if (NotchFilterChecked == true)
                        data[DataIdx] = (short)notchFilter[DataIdx].Filter(data[DataIdx]);
                    if (HighpassFilterChecked == true)
                        data[DataIdx] = (short)highFilter[DataIdx].Filter(data[DataIdx]);
                    if (LowpassFilterChecked == true)
                        data[DataIdx] = (short)lowFilter[DataIdx].Filter(data[DataIdx]);
                }
                //保存数据
                if (this.saveData && this.isSampling != false)
                {
                    this.dataCount++;
                    this.writeDataStream(dataStream, data);
                    //foreach (short unit in data)
                    //{
                    //    dataStream.Write(BitConverter.GetBytes(unit), 0, 2);
                    //}

                    if (dataCount % this.samplingDevice.SampleRate == 0)
                    {
                        //this.currentDuration = dataCount / this.samplingDevice.SampleRate + this.preSamplingDuration;
                        this.currentDuration = dataCount / this.samplingDevice.SampleRate + this.preSamplingDuration;
                        this.Dispatcher.Invoke(new Action(updateUI));
                        if (this.currentDuration >= SamplingDuration || StopBtnClicked == true)
                        {
                            sw.Stop();
                            this.stopSampling();
                            StopBtnClicked = false;
                            this.isSampling = false;
                            this.currentDuration = (int)sw.ElapsedMilliseconds;
                            this.Dispatcher.Invoke(new Action(updateUI));
                            return;
                        }
                    }
                }
                else
                {
                    if (this.preQueue != null)
                        this.writeToPreQueue(this.preQueue, this.preQueueCapacity, data);
                }
                if (this.currentPoint >= this.samplingDrawArg.PointInterval)//注意修改，几个点画一次
                {
                    this.ImageWave.Dispatcher.Invoke(new Action<short[]>(updateDrawing), data);
                    this.currentPoint = 0;
                }
                this.currentPoint++;
            }
            if (StopBtnClicked == true)
            {
                this.stopSampling();
                StopBtnClicked = false;
                this.isSampling = false;
            }
        }
        //        private void samplingDevice_DataRecived(object sender, EventArgs e)
        public void DataProcessThread()
        {
            #region
            //测试计时的测试代码
            //this.dataCount += 100;
            //if (dataCount % 500 == 0)
            //{
            //    this.duration = dataCount / 500;
            //    this.Dispatcher.Invoke(new Action(updateUI));
            //}  
            #endregion
            Queue<short[]> internalBuffer;
            internalBuffer = samplingDevice.Read();
            for (; ; )
            {
               // Thread.Sleep(3000); 
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
                lock (samplingDevice.LockObj)
                {
                    readCount = internalBuffer.Count;
                }
                for (ReadIdx = 0; ReadIdx < readCount; ReadIdx++)
                {
                    lock (samplingDevice.LockObj)
                    {

                        data = internalBuffer.Dequeue();
                    }
                    if (data  == null)
                    {
                        Thread.Sleep(10);
                        continue;
                    }
                    //滤波
                    for (DataIdx = 0; DataIdx < 12; DataIdx++)
                    {
                        // System.Diagnostics.Trace.WriteLine("data[" + DataIdx.ToString() + "] = " + data[DataIdx].ToString());

                        if (NotchFilterChecked == true)
                            data[DataIdx] = (short)notchFilter[DataIdx].Filter(data[DataIdx]);
                        if (HighpassFilterChecked == true)
                            data[DataIdx] = (short)highFilter[DataIdx].Filter(data[DataIdx]);
                        if (LowpassFilterChecked == true)
                            data[DataIdx] = (short)lowFilter[DataIdx].Filter(data[DataIdx]);
                    }
                    //保存数据
                    if (this.saveData && this.isSampling != false)
                    {
                        this.dataCount++;
                        this.writeDataStream(dataStream, data);
                        //foreach (short unit in data)
                        //{
                        //    dataStream.Write(BitConverter.GetBytes(unit), 0, 2);
                        //}

                        if (dataCount % this.samplingDevice.SampleRate == 0)
                        {
                            //this.currentDuration = dataCount / this.samplingDevice.SampleRate + this.preSamplingDuration;
                            this.currentDuration = dataCount / this.samplingDevice.SampleRate + this.preSamplingDuration;
                            this.Dispatcher.Invoke(new Action(updateUI));
                            if (this.currentDuration >= SamplingDuration || StopBtnClicked == true)
                            {
                                sw.Stop();
                                if (StopBtnClicked == false)
                                    this.Dispatcher.Invoke(new Action(stopSampling));
                                StopBtnClicked = false;
                                this.isSampling = false;
                                this.currentDuration = (int)sw.ElapsedMilliseconds;
                                this.Dispatcher.Invoke(new Action(updateUI));
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (this.preQueue != null)
                            this.writeToPreQueue(this.preQueue, this.preQueueCapacity, data);
                    }
                    if (this.currentPoint >= this.samplingDrawArg.PointInterval)//注意修改，几个点画一次
                    {
                        this.Dispatcher.BeginInvoke(new Action<short[]>(updateDrawing), data);
                        this.currentPoint = 0;
                    }
                    this.currentPoint++;
                    if (StopBtnClicked == true)
                    {
                        StopBtnClicked = false;
                        this.isSampling = false;
                        break;
                    }
                }
            }
        }
        //把超前采样数据写入Queue中缓存起来
        private void writeToPreQueue(Queue<short[]> preQueue, int preQueueCapacity, short[] data)
        {
            if (preQueueCapacity == 0)
                return;
            if (preQueue.Count >= preQueueCapacity)
                preQueue.Dequeue();
            preQueue.Enqueue(data);
        }

        /// <summary>
        /// 异步更新UI
        /// </summary>
        private void updateUI()
        {
            this.TextDuration.Text = currentDuration.ToString();
        }

        /// <summary>
        /// 跨线程更新UI
        /// </summary>
        private void updateDrawing(short[] data)
        {
            wbmp.Lock();
            drawer12.DrawWave(data, out refreshRegion);
            //foreach (RectangleF rect in refreshRegion)
            //{
            //    wbmp.AddDirtyRect(new Int32Rect((int)rect.X,(int)rect.Y,(int)rect.Width,(int)rect.Height));
            //}
            wbmp.AddDirtyRect(refreshRect);
            wbmp.Unlock();
        }

        /// <summary>
        /// 把short[]数组写入stream流
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="data"></param>
        private void writeDataStream(Stream stream, short[] data)
        {
            foreach (short unit in data)
            {
                stream.Write(BitConverter.GetBytes(unit), 0, 2);
            }
        }

        /// <summary>
        /// 停止采样
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            if (this.isSampling)
            {
                stopSampling();
                StopBtnClicked = true;
            }
        }

        /// <summary>
        /// 跳转到诊断界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonDiag_Click(object sender, RoutedEventArgs e)
        {
            if (this.ecgEntity != null)
            {
                MainWindow.Id = this.ecgEntity.Id;
                this.DialogResult = true;
            }
            else
            {

            }
        }

        /// <summary>
        /// 保存数据的内存流，数据存储为short[,]数组
        /// </summary>
        /// <param name="dataStream"></param>
        /// <returns></returns>
        private short[,] StreamToShort(MemoryStream dataStream)
        {
            if (dataStream == null || dataStream.CanRead == false)
            {
                return null;
            }
            dataStream.Position = 0;
            byte[] buffer = new byte[24];
            int dataCount = (int)(dataStream.Length / 24);
            short[,] data = new short[12, dataCount];
            for (int i = 0; i < dataCount; i++)
            {
                dataStream.Read(buffer, 0, 24);
                for (int j = 0; j < 12; j++)
                {
                    data[j, i] = BitConverter.ToInt16(buffer, 2 * j);
                }
            }
            return data;
        }

        /// <summary>
        /// 当Image控件大小变化时，同步修改内存源图像的大小
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageWave_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //wbmp = new WriteableBitmap(Convert.ToInt32(this.Width), Convert.ToInt32(this.Height), 96, 96, PixelFormats.Rgb24, null);
            //this.ImageWave.Source = wbmp;
            //gdiBmp = new Bitmap((int)wbmp.Width, (int)wbmp.Height, wbmp.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format24bppRgb, wbmp.BackBuffer);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.isSampling)
            {
                e.Cancel = true;
                return;
            }
            if (this.drawer12 != null)
            {
                this.drawer12.Dispose();
            }
            if (this.gdiBmp != null)
            {
                this.gdiBmp.Dispose();
            }
        }

        /// <summary>
        /// 停止采样，保存数据
        /// </summary>
        private void stopSampling()
        {
            this.samplingDevice.Stop();
            if (this.dataStream != null)
            {
                MemoryStream combinedDataStream = new MemoryStream();
                this.combineData(combinedDataStream, dataStream, this.preQueue);
                this.ecgEntity.Data = StreamToShort(combinedDataStream);
                this.ecgEntity.Duration = this.currentDuration;
                this.ecgEntity.Status = DataReference.Status.WaitDiag;
                this.dataStream.Close();
                combinedDataStream.Close();
                if (this.ecgEntity.Data != null && this.ecgEntity.Data.Length > 0)
                {
                    this.ecgEntity.DataCount = this.ecgEntity.Data.Length / 10;
                    this.ecgEntity.SamplingDate = DateTime.Now;
                    //this.ecgEntity.TempDataCount = 500;
                    //this.ecgEntity.TempData = buildTempTest(this.ecgEntity.Data);       //正式版本需要改正，目前只是虚拟出模板数据用于测试
                    EcgBLL ecgBLL = new EcgBLL();
                    ecgBLL.InsertEcg(this.ecgEntity);
                }
            }
            this.isSampling = false;
        }

        //把超前采样的数据与正常纪录的数据组合到一起
        private void combineData(Stream combinedStream, MemoryStream dataStream, Queue<short[]> preData)
        {
            int preDataCount = preData.Count;
            for (int i = 0; i < preDataCount; i++)
            {
                short[] data = preData.Dequeue();
                this.writeDataStream(combinedStream, data);
            }
            byte[] buffer = dataStream.ToArray();
            combinedStream.Write(buffer, 0, buffer.Length);
        }

        //测试用的，虚拟一个模板数据
        private short[,] buildTempTest(short[,] data)
        {
            if (data.GetLongLength(1) >= 500)
            {
                short[,] tempData = new short[12, 500];
                for (int i = 0; i < 500; i++)
                {
                    for (int j = 0; j < 12; j++)
                    {
                        tempData[j, i] = data[j, i];
                    }
                }
                return tempData;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 采样设备初始化
        /// </summary>
        /// <returns></returns>
        private Device initDevice()
        {
            Device samplingDevice;
            if (ConfigurationManager.AppSettings["DeviceType"] == DeviceList.DeviceTcj12)
            {
                samplingDevice = new DeviceTcj12(ConfigurationManager.AppSettings["DeviceCOM"]);     //12导采集盒
                this.samplingDrawArg = new SamplingDrawArg { DrawFrequencey = 100, PointInterval = 20 };
            }
            else
            {
                samplingDevice = new DeviceDemo();
                this.samplingDrawArg = new SamplingDrawArg { DrawFrequencey = 100, PointInterval = 5 };
            }
            return samplingDevice;
        }

        /// <summary>
        /// 滤波器的初始化
        /// </summary>       
        /// <param name="sampleRate">采样率</param>
        private void initFilter(int sampleRate)
        {
            double highpassFreq = double.Parse(ConfigurationManager.AppSettings["HighpassFilter"]);
            double lowpassFreq = double.Parse(ConfigurationManager.AppSettings["LowpassFilter"]);
            double notchFreq = 50d, notchArg = 40d;
            for (int i = 0; i < 12; i++)
            {
                this.notchFilter[i] = new NotchFilter(notchFreq, sampleRate, notchArg);
                this.highFilter[i] = new HighpassFilter(highpassFreq, sampleRate);
                this.lowFilter[i] = new LowpassFilter(lowpassFreq, sampleRate);
            }
        }
    }

    /// <summary>
    /// 采样绘图参数
    /// </summary>
    public class SamplingDrawArg
    {
        public int DrawFrequencey { get; set; }      //绘图频率，比如500Hz，每5个点画一个点，那么绘图频率就是100Hz
        public int PointInterval { get; set; }      //每多少个点绘一次
    }
}
