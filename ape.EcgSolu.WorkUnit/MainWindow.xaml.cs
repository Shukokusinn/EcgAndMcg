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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ape.EcgSolu.Model;
using ape.EcgSolu.BLL;
using ape.EcgSolu.WorkUnit.Diagnosis;
using System.Xml.Serialization;
using System.IO;
using System.IO.Ports;
using System.Configuration;
using Microsoft.Win32;
using System.Runtime.Serialization.Json;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace ape.EcgSolu.WorkUnit
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private int pageIndex = 1;
        private int pageSize = 50;
        private int pageMax = 1;
        private EcgBLL ecgBLL = new EcgBLL();
        public static Guid Id;          //用于协调的检查id号     
        private Socket sSocket;         //定义Socket对象
        private Thread th;              //客户端连接服务器的线程
        private Thread ServerTh;        //服务器线程
        public NetworkStream cns;        //网络流
        private string strRecvIPAddr;
        private string strRecvPortNo;
        private delegate void SetTextCallback();         //用于操作主线程控件
        System.Threading.ManualResetEvent mEvent = new System.Threading.ManualResetEvent(true);  
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 进入采样UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSampling_Click(object sender, RoutedEventArgs e)
        {
            if (ConfigurationManager.AppSettings["DeviceType"] == DeviceList.DeviceTcj12)
            {
                try
                {

                    SerialPort serialPort = new SerialPort(ConfigurationManager.AppSettings["DeviceCOM"]);
                    serialPort.Open();
                    serialPort.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("没有找到指定的采集卡，请检查采集卡的连接及设置是否正确。", "抱歉，出错啦...");//, 0, 64);
                    return;
                    //                this.textBlockDeviceTestResult.Text = "设备连接测试失败，错误信息：" + ex.Message;
                }
            }
            SamplingWindow sampling = new SamplingWindow();
            sampling.Owner = this;
            sampling.ShowInTaskbar = false;
            if (sampling.ShowDialog() == true)
            {
                sampling.Close();
                DiagWindow diagWindow = new DiagWindow(Id);
                diagWindow.ShowDialog();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int i;
            this.WindowState = WindowState.Maximized;
            initialize();
            this.TextBoxPageIndex.Text = this.pageIndex.ToString();
            this.loadEcgList(this.pageIndex, this.pageSize);
            int maxRowsCount = ecgBLL.GetRowsCount();
            this.pageMax = (int)Math.Ceiling(maxRowsCount / (float)this.pageSize);
            this.TextBlockRowsCount.Text = maxRowsCount.ToString();
            this.TextBlockPageCount.Text = this.pageMax.ToString();
            string strHostName = Dns.GetHostName(); //得到本机的主机名
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName); //取得本机IPSystem.Net.IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(hostName);
            string CurRecvIPAddr = ConfigurationManager.AppSettings["RecvIPAddr"];
            strRecvIPAddr = "";
            for ( i = 0; i < ipEntry.AddressList.Count(); i++)
            {
                if (ipEntry.AddressList[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    if (strRecvIPAddr == "")
                        strRecvIPAddr = ipEntry.AddressList[i].ToString();
                    if (ipEntry.AddressList[i].Equals(CurRecvIPAddr))
                    {
                        strRecvIPAddr = CurRecvIPAddr;
                        break;
                    }
                }
            }
            if (strRecvIPAddr == "")
                strRecvIPAddr = "127.0.0.1";
            ConfigOp.SetAppSetting("RecvIPAddr", strRecvIPAddr);
            StartUpServer();
        }      

        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonExport_Click(object sender,RoutedEventArgs e)
        {
            if (this.DataGridEcgList.SelectedItems.Count > 0)
            {
                Ecg ecgEntity = this.DataGridEcgList.SelectedItems[0] as Ecg;
                if (ecgEntity == null) return;
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "XML files|*.xml";
                if (sfd.ShowDialog() == true)
                {
                    ecgEntity = ecgBLL.GetById(ecgEntity.Id);
                    new CustomSerializer().SerializeXml(ecgEntity, sfd.FileName);
                }
                this.textBlockMessage.Text = string.Format("导出成功，姓名：{0}", ecgEntity.PatientName);
            }
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.refreshEcgList();
            this.RecvInfo.Text = ""; 
        }

        /// <summary>
        /// 进入软件配置窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonConfig_Click(object sender, RoutedEventArgs e)
        {
            ConfigWindow configWindow = new ConfigWindow();
            configWindow.Owner = this;
            configWindow.ShowInTaskbar = false;
            configWindow.ShowDialog();
            if (!strRecvIPAddr.Equals(ConfigurationManager.AppSettings["RecvIPAddr"])||
                !strRecvPortNo.Equals(ConfigurationManager.AppSettings["RecvPortNo"])  )
            {
                this.sSocket.Close();
                StartUpServer(); 
            }
        }

        /// <summary>
        /// 诊断
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonDiag_Click(object sender, RoutedEventArgs e)
        {
            this.goDiag();
        }

        /// <summary>
        /// 初始化界面
        /// </summary>
        private void initialize()
        {
            List<GenderModel> genderList = new List<GenderModel>();
            genderList.Add(new GenderModel { Id = -1, Gender = "未知" });
            genderList.Add(new GenderModel { Id = 0, Gender = "女" });
            genderList.Add(new GenderModel { Id = 1, Gender = "男" });
            this.ComboBoxGender.ItemsSource = genderList;
        }
        class GenderModel
        {
            public int Id { get; set; }
            public string Gender { get; set; }
        }

        /// <summary>
        /// 双击打开诊断界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridEcgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.goDiag();
        }

        /// <summary>
        /// 跳转到诊断界面
        /// </summary>
        private void goDiag()
        {
            Ecg ecgEntity = this.DataGridEcgList.SelectedItem as Ecg;
            if (ecgEntity != null && ecgEntity.Duration >= 10)
            {
                DiagWindow diagWindow = new DiagWindow(ecgEntity.Id);
                diagWindow.Owner = this;
                diagWindow.ShowInTaskbar = false;
                try
                {
                    diagWindow.ShowDialog();
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.Message);
                }
            }
        }

        /// <summary>
        /// 刷新加载Ecg列表
        /// </summary>
        private void refreshEcgList()
        {
            this.loadEcgList(this.pageIndex, this.pageSize);
        }

        /// <summary>
        /// 加载列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        private void loadEcgList(int pageIndex, int pageSize)
        {
            List<Ecg> ecgList = new EcgBLL().GetPagedList(pageIndex, pageSize);
            this.DataGridEcgList.ItemsSource = ecgList;
        }

        /// <summary>
        /// 加载下一页列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (this.pageIndex < this.pageMax - 1)
            {
                this.pageIndex++;
                this.TextBoxPageIndex.Text = this.pageIndex.ToString();
                this.loadEcgList(this.pageIndex, this.pageSize);
            }
        }

        /// <summary>
        /// 加载上一页的列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (this.pageIndex > 1)
            {
                this.pageIndex--;
                this.TextBoxPageIndex.Text = this.pageIndex.ToString();
                this.loadEcgList(this.pageIndex, this.pageSize);
            }
        }

        /// <summary>
        /// 加载第一页的列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FirstPage_Click(object sender, RoutedEventArgs e)
        {
            this.pageIndex = 1;
            this.TextBoxPageIndex.Text = this.pageIndex.ToString();
            this.loadEcgList(this.pageIndex, this.pageSize);
        }

        /// <summary>
        /// 加载特定页的列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoPage_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(this.TextBoxPageIndex.Text, out this.pageIndex))
            {
                this.pageIndex = 1;
                this.TextBoxPageIndex.Text = "1";
            }
            if (this.pageIndex >= 1 && this.pageIndex <= this.pageMax)
            {
                this.TextBoxPageIndex.Text = this.pageIndex.ToString();
                this.loadEcgList(this.pageIndex, this.pageSize);
            }
        }

        /// <summary>
        /// 加载尾页列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LastPage_Click(object sender, RoutedEventArgs e)
        {            
            this.pageIndex = this.pageMax;
            this.TextBoxPageIndex.Text = this.pageIndex.ToString();
            this.loadEcgList(this.pageIndex, this.pageSize); 
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonQuery_Click(object sender, RoutedEventArgs e)
        {
            QueryArg queryArg = this.buildQueryArg();
            List<Ecg> ecgList = ecgBLL.GetPagedQueryList(1,this.pageSize ,queryArg);
            this.DataGridEcgList.ItemsSource = ecgList;
        }

        /// <summary>
        /// 构建查询对象
        /// </summary>
        /// <returns></returns>
        private QueryArg buildQueryArg()
        {
            QueryArg queryArg = new QueryArg();
            if (!string.IsNullOrWhiteSpace(this.TextBoxPatientName.Text))
            {
                queryArg.PatientName = this.TextBoxPatientName.Text.Trim();
            }
            if (this.ComboBoxGenderArg.SelectedIndex != -1)
            {
                queryArg.Gender = this.ComboBoxGenderArg.SelectedIndex;
            }
            if (!string.IsNullOrWhiteSpace(this.textBoxApplyNo.Text))
            {
                queryArg.ApplyNo = this.textBoxApplyNo.Text.Trim();
            }
            if (!string.IsNullOrWhiteSpace(this.textBoxInHospitalNo.Text))
            {
                queryArg.InPatientNo = this.textBoxInHospitalNo.Text.Trim();
            }
            if (!string.IsNullOrWhiteSpace(this.textBoxOutHospitalNo.Text))
            {
                queryArg.OutPatientNo = this.textBoxOutHospitalNo.Text.Trim();
            }
            if (!string.IsNullOrWhiteSpace(this.textBoxApplyDept.Text))
            {
                queryArg.ApplyDept = this.textBoxApplyDept.Text.Trim();
            }
            if (!string.IsNullOrWhiteSpace(this.textBoxApplyDoctor.Text))
            {
                queryArg.ApplyDocotor = this.textBoxApplyDoctor.Text.Trim();
            }
            if (!string.IsNullOrWhiteSpace(this.textBoxDiagDoctor.Text))
            {
                queryArg.DiagDocotor = this.textBoxDiagDoctor.Text.Trim();
            }
            if (!string.IsNullOrWhiteSpace(this.textBoxDiagResult.Text))
            {
                queryArg.DiagResult = this.textBoxDiagResult.Text.Trim();
            }
            if (this.checkBoxDateCheck.IsChecked == true && this.datePickerStart.SelectedDate!=null && this.datePickerEnd.SelectedDate!=null)
            {
                queryArg.DateCheck = true;
                if (this.datePickerStart.SelectedDate == null)
                    queryArg.DateStart = DateTime.MaxValue;
                else
                    queryArg.DateStart = (DateTime)this.datePickerStart.SelectedDate;
                if (this.datePickerEnd.SelectedDate == null)
                    queryArg.DateEnd = DateTime.MinValue;
                else
                    queryArg.DateEnd = (DateTime)this.datePickerEnd.SelectedDate;                
            }
            return queryArg;
        }

        /// <summary>
        /// 清空查询条件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClearArg_Click(object sender, RoutedEventArgs e)
        {
            this.TextBoxPatientName.Text = null;
            this.ComboBoxGenderArg.SelectedIndex = -1;
            this.textBoxApplyNo.Text = null;
            this.textBoxInHospitalNo.Text = null;
            this.textBoxOutHospitalNo.Text = null;
            this.textBoxApplyDept.Text = null;
            this.textBoxApplyDoctor.Text = null;
            this.textBoxDiagDoctor.Text = null;
            this.textBoxDiagResult.Text = null;
            this.checkBoxDateCheck.IsChecked = false;
            this.datePickerStart.SelectedDate = null;
            this.datePickerEnd.SelectedDate = null;
        }

        /// <summary>
        /// 删除检查记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataGridEcgList.SelectedItems.Count <= 0)
                return;
            string msg = "这些检查一旦被删除将不可恢复，你确定要删除它？";
            if (MessageBox.Show(this,msg,string.Empty,MessageBoxButton.OKCancel,MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                foreach (var item in this.DataGridEcgList.SelectedItems)
                {
                    Ecg ecgEntity = item as Ecg;
                    ecgBLL.DeleteByid(ecgEntity.Id);
                }
                this.refreshEcgList();
            }
        }

        //切换显示搜索面板
        private void checkBoxSearch_Click(object sender, RoutedEventArgs e)
        {
            if(this.checkBoxSearch.IsChecked==true)
                this.GridSearch.Visibility = Visibility.Visible;
            else
                this.GridSearch.Visibility = Visibility.Collapsed;           
        }
        // 上传
        private void buttonUpload_Click(object sender, RoutedEventArgs e)
        {
            StreamReader csr;         //流读取
            StreamWriter csw;         //流写入
            NetworkStream sns;        //网络流
            String ExecPath = System.Environment.CurrentDirectory;  
            if (this.DataGridEcgList.SelectedItems.Count == 0)            
            {
                MessageBox.Show("请选择上传对象");                  //捕获异常
                return;
            }
            Ecg ecgEntity = this.DataGridEcgList.SelectedItems[0] as Ecg;
            if (ecgEntity == null) 
                return;
            ecgEntity = ecgBLL.GetById(ecgEntity.Id);
            new CustomSerializer().SerializeXml(ecgEntity, ExecPath+"\\sss");

            Socket Soc;         //定义Socket对象
            try
            {
                Soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress serverIP = IPAddress.Parse(ConfigurationManager.AppSettings["SendToIPAddr"]);   //服务器IP
                Soc.Connect(serverIP, int.Parse(ConfigurationManager.AppSettings["SendPortNo"]));   //连接服务器，端口号用13
            }
            catch (Exception ex)
            {
                 MessageBox.Show(ex.Message);
                 return;
            }  
            try 
            {      
                cns = new NetworkStream(Soc);                     //实例化网络流
                csr = new StreamReader(ExecPath + "\\sss");                     //实例化流读取对象
                csw = new StreamWriter(cns);                     //实例化写入流对象
                csw.WriteLine(csr.ReadToEnd());                  //将textBox1.Text的数据写入流
                csw.Flush();                                     //清理缓冲区
                //lbInfo.Items.Add(sr.ReadLine());              //将从流中读取的数据写入lbInfo28
                csr.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);                  //捕获异常
            }
        }
        // 
        public void Communication()
        {
            NetworkStream sns;        //网络流
            StreamReader ssr;         //流读取
            StreamWriter ssw;         //流写入
            String ExecPath = System.Environment.CurrentDirectory;
            String wkStr;
            String clientIpAddr;
            Socket cSocket;          //单个客户端连接的Socket对象
            while (true)
            {
                try
                {
                    
                    cSocket = sSocket.Accept();                   //用cSocket来代表该客户端连接
                    clientIpAddr = cSocket.RemoteEndPoint.ToString();
                    clientIpAddr = clientIpAddr.Substring(0, clientIpAddr.IndexOf(':'));
                    if (cSocket.Connected)                  //测试是否连接成功
                    {
                        this.RecvInfo.Dispatcher.Invoke(new Action(delegate() { RecvInfo.Text = "正在接收数据..."; }));
                        sns = new NetworkStream(cSocket);  //建立网络流，便于数据的读取
                        ssr = new StreamReader(sns);         //实例化流读取对象
                        ssw = new StreamWriter(ExecPath + "\\RecvFrom" + clientIpAddr);         //实例化写入流对象
                        //FileStream fs = new FileStream(ExecPath+"\\ttt", FileMode.Create);

                        ////获得字节数组

                        //byte[] data = new UTF8Encoding().GetBytes(String);

                        ////开始写入

                        //fs.Write(data, 0, data.Length);

                        ////清空缓冲区、关闭流

                        //fs.Flush();

                        //fs.Close();         
                        //Thread.Sleep();
                        //ssw.Flush();
                        String sLine = "ss";
                        while (true)
                        {

                            sLine = ssr.ReadLine();

                            if (sLine != "")

                                ssw.WriteLine(sLine);
                            ssw.Flush();                           //清理缓冲区
                            if (sLine == "</Ecg>")
                                break;

                        }
                        ssw.Close();
                        ssr.Close();
                        //test();                               //从流中读取
                        //ssw.WriteLine("收到请求，允许连接"); //向流中写入数据
                        new CustomSerializer().DeserializeXml(ExecPath + "\\RecvFrom" + clientIpAddr);
                        this.RecvInfo.Dispatcher.Invoke(new Action(delegate() { RecvInfo.Text = "来自"+clientIpAddr+"数据接收完毕，请按刷新按钮确认。"; }));
                        File.Delete(ExecPath + "\\RecvFrom" + clientIpAddr);
                    }
                    else
                    {
                        MessageBox.Show("连接失败");
                    }
                }
                catch (SocketException ex)
                {
           //         MessageBox.Show(ex.Message);           //捕获Socket异常
                    break;
                }
                catch (Exception es)
                {
                    MessageBox.Show("其他异常" + es.Message);      //捕获其他异常
                }
            }
        }
        //以下代码的用法在第16章有关线程的用法时曾提到过，主要用于从当前线程操作主线程中的控件，这里就不在赘//述
        public void send()
        {
            //lbInfo.Items.Add(sr.ReadLine() + "\n");
        }
        public void test()
        {
            SetTextCallback stcb = new SetTextCallback(send);
  //          this.beINVOKE(stcb);
        }
        private void StartUpServer()
        {
            strRecvIPAddr = ConfigurationManager.AppSettings["RecvIPAddr"];

            strRecvPortNo = ConfigurationManager.AppSettings["RecvPortNo"];
            sSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress serverIP = IPAddress.Parse(strRecvIPAddr);   //服务器IP
            IPEndPoint Server = new IPEndPoint(serverIP, int.Parse(strRecvPortNo));
            sSocket.Bind(Server);
            sSocket.Listen(10); 
            try
            {
                th = new Thread(new ThreadStart(Communication));
                th.Start();
            }
            catch (Exception es)
            {
                MessageBox.Show("其他异常" + es.Message);      //捕获其他异常
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            sSocket.Close();
            //th.Abort();
            this.Close();

        }

//        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
//        {
//            th.Abort();
//            sSocket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
//            sSocket.Close();
//            this.Close();
////protected override void Dispose(bool disposing)
//// {
////     try
////     {
////         if (disposing && (components != null))
////         {
////             components.Dispose();
////             th.Abort();
//// //禁用当前Socket连接中的数据收发
////             s.Shutdown(System.Net.Sockets.SocketShutdown.Both);             
////             s.Close();
////         }
////         base.Dispose(disposing);
////     }
////     catch
////     {
////         return;
////     }
//// }
//// //接着为当前窗体的FormClosed事件添加如下代码。
//// this.Close();
//        }
    }
}
