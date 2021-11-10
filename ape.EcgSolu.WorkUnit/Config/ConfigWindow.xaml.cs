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
using System.IO.Ports;
using System.Configuration;
using System.Net;
using System.Runtime.InteropServices;
namespace ape.EcgSolu.WorkUnit
{
    class DeviceInfo
    {

        public const int DIGCF_PRESENT = (0x00000002);
        public const int MAX_DEV_LEN = 1000;
        public const int SPDRP_FRIENDLYNAME = (0x0000000C);
        // FriendlyName (R/W)
        public const int SPDRP_DEVICEDESC = (0x00000000);
        // DeviceDesc (R/W)

        [StructLayout(LayoutKind.Sequential)]
        public class SP_DEVINFO_DATA
        {
            public int cbSize;
            public Guid ClassGuid;
            public int DevInst;    // DEVINST handle
            public ulong Reserved;
        };

        [DllImport("setupapi.dll")]//
        public static extern Boolean
          SetupDiClassGuidsFromNameA(string ClassN, ref Guid guids,
            UInt32 ClassNameSize, ref UInt32 ReqSize);

        [DllImport("setupapi.dll")]
        public static extern IntPtr                //result HDEVINFO
          SetupDiGetClassDevsA(ref Guid ClassGuid, UInt32 Enumerator,
            IntPtr hwndParent, UInt32 Flags);

        [DllImport("setupapi.dll")]
        public static extern Boolean
          SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, UInt32 MemberIndex,
            SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll")]
        public static extern Boolean
          SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("setupapi.dll")]
        public static extern Boolean
          SetupDiGetDeviceRegistryPropertyA(IntPtr DeviceInfoSet,
          SP_DEVINFO_DATA DeviceInfoData, UInt32 Property,
          UInt32 PropertyRegDataType, StringBuilder PropertyBuffer,
          UInt32 PropertyBufferSize, IntPtr RequiredSize);



        public static int EnumerateDevices(UInt32 DeviceIndex,
                string ClassName,
                StringBuilder DeviceName)
        {
            UInt32 RequiredSize = 0;
            Guid guid = Guid.Empty;
            Guid[] guids = new Guid[1];
            IntPtr NewDeviceInfoSet;
            SP_DEVINFO_DATA DeviceInfoData = new SP_DEVINFO_DATA();


            bool res = SetupDiClassGuidsFromNameA(ClassName,
                       ref guids[0], RequiredSize,
                       ref RequiredSize);

            if (RequiredSize == 0)
            {
                //incorrect class name:
                DeviceName = new StringBuilder("");
                return -2;
            }

            if (!res)
            {
                guids = new Guid[RequiredSize];
                res = SetupDiClassGuidsFromNameA(ClassName, ref guids[0], RequiredSize,
                     ref RequiredSize);

                if (!res || RequiredSize == 0)
                {
                    //incorrect class name:
                    DeviceName = new StringBuilder("");
                    return -2;
                }
            }

            //get device info set for our device class
            NewDeviceInfoSet = SetupDiGetClassDevsA(ref guids[0], 0, IntPtr.Zero,
                        DIGCF_PRESENT);
            if (NewDeviceInfoSet.ToInt32() == -1)
                if (!res)
                {
                    //device information is unavailable:
                    DeviceName = new StringBuilder("");
                    return -3;
                }

            DeviceInfoData.cbSize = 28;
            //is devices exist for class
            DeviceInfoData.DevInst = 0;
            DeviceInfoData.ClassGuid = System.Guid.Empty;
            DeviceInfoData.Reserved = 0;

            res = SetupDiEnumDeviceInfo(NewDeviceInfoSet,
                   DeviceIndex, DeviceInfoData);
            if (!res)
            {
                //no such device:
                SetupDiDestroyDeviceInfoList(NewDeviceInfoSet);
                DeviceName = new StringBuilder("");
                return -1;
            }



            DeviceName.Capacity = MAX_DEV_LEN;
            if (!SetupDiGetDeviceRegistryPropertyA(NewDeviceInfoSet,
              DeviceInfoData,
            SPDRP_FRIENDLYNAME, 0, DeviceName, MAX_DEV_LEN, IntPtr.Zero))
            {
                res = SetupDiGetDeviceRegistryPropertyA(NewDeviceInfoSet,
                 DeviceInfoData, SPDRP_DEVICEDESC, 0, DeviceName, MAX_DEV_LEN,
                   IntPtr.Zero);
                if (!res)
                {
                    //incorrect device name:
                    SetupDiDestroyDeviceInfoList(NewDeviceInfoSet);
                    DeviceName = new StringBuilder("");
                    return -4;
                }
            }
            return 0;
        }

    //    [STAThread]
    }

    /// <summary>
    /// ConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigWindow : Window
    {

        ///// <summary>
        ///// 枚举win32 api
        ///// </summary>
        //public enum HardwareEnum
        //{
        //    // 硬件
        //    Win32_Processor, // CPU 处理器
        //    Win32_PhysicalMemory, // 物理内存条
        //    Win32_Keyboard, // 键盘
        //    Win32_PointingDevice, // 点输入设备，包括鼠标。
        //    Win32_FloppyDrive, // 软盘驱动器
        //    Win32_DiskDrive, // 硬盘驱动器
        //    Win32_CDROMDrive, // 光盘驱动器
        //    Win32_BaseBoard, // 主板
        //    Win32_BIOS, // BIOS 芯片
        //    Win32_ParallelPort, // 并口
        //    Win32_SerialPort, // 串口
        //    Win32_SerialPortConfiguration, // 串口配置
        //    Win32_SoundDevice, // 多媒体设置，一般指声卡。
        //    Win32_SystemSlot, // 主板插槽 (ISA & PCI & AGP)
        //    Win32_USBController, // USB 控制器
        //    Win32_NetworkAdapter, // 网络适配器
        //    Win32_NetworkAdapterConfiguration, // 网络适配器设置
        //    Win32_Printer, // 打印机
        //    Win32_PrinterConfiguration, // 打印机设置
        //    Win32_PrintJob, // 打印机任务
        //    Win32_TCPIPPrinterPort, // 打印机端口
        //    Win32_POTSModem, // MODEM
        //    Win32_POTSModemToSerialPort, // MODEM 端口
        //    Win32_DesktopMonitor, // 显示器
        //    Win32_DisplayConfiguration, // 显卡
        //    Win32_DisplayControllerConfiguration, // 显卡设置
        //    Win32_VideoController, // 显卡细节。
        //    Win32_VideoSettings, // 显卡支持的显示模式。

        //    // 操作系统
        //    Win32_TimeZone, // 时区
        //    Win32_SystemDriver, // 驱动程序
        //    Win32_DiskPartition, // 磁盘分区
        //    Win32_LogicalDisk, // 逻辑磁盘
        //    Win32_LogicalDiskToPartition, // 逻辑磁盘所在分区及始末位置。
        //    Win32_LogicalMemoryConfiguration, // 逻辑内存配置
        //    Win32_PageFile, // 系统页文件信息
        //    Win32_PageFileSetting, // 页文件设置
        //    Win32_BootConfiguration, // 系统启动配置
        //    Win32_ComputerSystem, // 计算机信息简要
        //    Win32_OperatingSystem, // 操作系统信息
        //    Win32_StartupCommand, // 系统自动启动程序
        //    Win32_Service, // 系统安装的服务
        //    Win32_Group, // 系统管理组
        //    Win32_GroupUser, // 系统组帐号
        //    Win32_UserAccount, // 用户帐号
        //    Win32_Process, // 系统进程
        //    Win32_Thread, // 系统线程
        //    Win32_Share, // 共享
        //    Win32_NetworkClient, // 已安装的网络客户端
        //    Win32_NetworkProtocol, // 已安装的网络协议
        //    Win32_PnPEntity,//all device
        //}
        ///// <summary>
        ///// WMI取硬件信息
        ///// </summary>
        ///// <param name="hardType"></param>
        ///// <param name="propKey"></param>
        ///// <returns></returns>
        //public static string[] MulGetHardwareInfo(HardwareEnum hardType, string propKey)
        //{

        //    List<string> strs = new List<string>();
        //    try
        //    {
        //        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from " + hardType))
        //        {
        //            var hardInfos = searcher.Get();
        //            foreach (var hardInfo in hardInfos)
        //            {
        //                if (hardInfo.Properties[propKey].Value.ToString().Contains("COM"))
        //                {
        //                    strs.Add(hardInfo.Properties[propKey].Value.ToString());
        //                }

        //            }
        //            searcher.Dispose();
        //        }
        //        return strs.ToArray();
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //    finally
        //    { strs = null; }
        //}

        public ConfigWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.loadDeviceConfig();
            this.loadSamplingConfig();
            this.loadReportConfig();
        }

        #region 设备设置
        private void loadDeviceConfig()
        {
            this.comboBoxDeviceType.Items.Add("Demo");
            this.comboBoxDeviceType.Items.Add("Tcj12");                         
            initComPort();
            string deviceType = ConfigurationManager.AppSettings["DeviceType"];
            string deviceCom = ConfigurationManager.AppSettings["DeviceCOM"];
            this.comboBoxDeviceType.SelectedItem = deviceType;
            this.comboBoxDeviceCom.SelectedItem = deviceCom;
        }

        private void initComPort()
        {
            StringBuilder devices = new StringBuilder("");
            UInt32 Index = 0;
            int result = 0;
            List<string> PortDesc = new List<string>();

            try
            {
                while (true)
                {
                    result = DeviceInfo.EnumerateDevices(Index, "Ports", devices);
                    Index++;
                    if (result == -1) break;
                    if (result == 0)
                        PortDesc.Add(devices.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            string delimStr = "Silicon Labs CP210x USB to UART Bridge";
            List<string> PortName = new List<string>();
            try
            {
                foreach (string str in PortDesc) {
                    if (str.Contains(delimStr) == true)
                    {
                        PortName.Add(str.Substring(delimStr.Length + 2, str.Length - delimStr.Length - 3));
                    }
                    else if (str.Contains("ELTIMA") == true)
                        PortName.Add(str.Substring(28, 4));//ELTIMA Virtual Serial Port ();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            string[] comPortList = PortName.ToArray();
            this.comboBoxDeviceCom.ItemsSource = comPortList;
        }

        private void buttonDeviceSave_Click(object sender, RoutedEventArgs e)
        {
            ConfigOp.SetAppSetting("DeviceType", this.comboBoxDeviceType.SelectedItem.ToString());
            if (this.comboBoxDeviceCom.SelectedIndex != -1)
            {
                ConfigOp.SetAppSetting("DeviceCOM", this.comboBoxDeviceCom.SelectedItem.ToString());
            }
            this.Close();
        }

        /// <summary>
        /// 测试设备连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonDeviceTest_Click(object sender, RoutedEventArgs e)
        {
            this.textBlockDeviceTestResult.Text=null;
            try
            {

                SerialPort serialPort = new SerialPort(comboBoxDeviceCom.SelectedValue.ToString());
                serialPort.Open();
                serialPort.Close();
                MessageBox.Show("设备连接成功");
            }
            catch(Exception ex)
            {
                MessageBox.Show("设备连接失败");
                this.textBlockDeviceTestResult.Text = "设备连接测试失败，错误信息："+ex.Message;
            }

        }

        private void buttonDeviceCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region 采样设置
        private void loadSamplingConfig()
        {
            string samplingDuration = ConfigurationManager.AppSettings["SamplingDuration"];
            this.comboBoxSamplingDuration.SelectedValue = int.Parse(samplingDuration);
            string presaveDuration = ConfigurationManager.AppSettings["PreSamplingDuration"];
            this.comboBoxSamplingDurationAhead.SelectedValue = int.Parse(presaveDuration);
            System.Drawing.Color samplingPreviewColor = System.Drawing.Color.FromArgb(int.Parse(ConfigurationManager.AppSettings["SamplingPreviewColor"]));
            this.borderSamplingPreviewColor.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(samplingPreviewColor.R,samplingPreviewColor.G,samplingPreviewColor.B));
            System.Drawing.Color samplingSaveColor=System.Drawing.Color.FromArgb(int.Parse(ConfigurationManager.AppSettings["SamplingSaveColor"]));
            this.borderSamplingSaveColor.Background=new SolidColorBrush(System.Windows.Media.Color.FromRgb(samplingSaveColor.R,samplingSaveColor.G,samplingSaveColor.B));
            this.checkBoxHighpassFilter.IsChecked = bool.Parse(ConfigurationManager.AppSettings["HighpassFilterChecked"]);
            this.checkBoxLowpassFilter.IsChecked = bool.Parse(ConfigurationManager.AppSettings["LowpassFilterChecked"]);
            this.checkBoxNotchFilter.IsChecked = bool.Parse(ConfigurationManager.AppSettings["NotchFilterChecked"]);
            this.comboBoxHighpassFilter.SelectedValue = double.Parse(ConfigurationManager.AppSettings["HighpassFilter"]);
            this.comboBoxLowpassFilter.SelectedValue = double.Parse(ConfigurationManager.AppSettings["LowpassFilter"]);            
        }

        /// <summary>
        /// 选择采样-预览时的颜色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SamplingPreviewColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.Color color = colorDialog.Color;
                System.Windows.Media.Color foreColor = System.Windows.Media.Color.FromRgb(color.R, color.G, color.B);
                this.borderSamplingPreviewColor.Background = new SolidColorBrush(foreColor);
            }
        }

        /// <summary>
        /// 选择采样-保存时的颜色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSamplingSaveColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.Color color = colorDialog.Color;
                System.Windows.Media.Color foreColor = System.Windows.Media.Color.FromRgb(color.R, color.G, color.B);
                this.borderSamplingSaveColor.Background = new SolidColorBrush(foreColor);
            }
        }

        /// <summary>
        /// 保存采样配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSamplingSave_Click(object sender, RoutedEventArgs e)
        {
            if (this.comboBoxSamplingDuration.SelectedIndex != -1)
                ConfigOp.SetAppSetting("SamplingDuration", this.comboBoxSamplingDuration.SelectedValue.ToString());
            System.Windows.Media.Color samplingPreviewColor = ((SolidColorBrush)this.borderSamplingPreviewColor.Background).Color;
            System.Windows.Media.Color samplingSaveColor = ((SolidColorBrush)this.borderSamplingSaveColor.Background).Color;
            System.Drawing.Color previewColor=System.Drawing.Color.FromArgb(samplingPreviewColor.R,samplingPreviewColor.G,samplingPreviewColor.B);
            System.Drawing.Color saveColor=System.Drawing.Color.FromArgb(samplingSaveColor.R,samplingSaveColor.G,samplingSaveColor.B);
            ConfigOp.SetAppSetting("SamplingPreviewColor", previewColor.ToArgb().ToString());
            ConfigOp.SetAppSetting("SamplingSaveColor", saveColor.ToArgb().ToString());
            ConfigOp.SetAppSetting("HighpassFilterChecked", this.checkBoxHighpassFilter.IsChecked.ToString());
            ConfigOp.SetAppSetting("LowpassFilterChecked", this.checkBoxLowpassFilter.IsChecked.ToString());
            ConfigOp.SetAppSetting("NotchFilterChecked", this.checkBoxNotchFilter.IsChecked.ToString());
            if (this.comboBoxHighpassFilter.SelectedIndex != -1)
                ConfigOp.SetAppSetting("HighpassFilter", this.comboBoxHighpassFilter.SelectedValue.ToString());
            if (this.comboBoxLowpassFilter.SelectedIndex != -1)
                ConfigOp.SetAppSetting("LowpassFilter", this.comboBoxLowpassFilter.SelectedValue.ToString());
            this.Close();
        }

        private void buttonSamplingCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region 打印报告的设置
        private void buttonReportCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 设置5mm网格线颜色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5mmColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDlg = new System.Windows.Forms.ColorDialog();
            if (colorDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.Color color = colorDlg.Color;
                SolidColorBrush solidColorBrush = new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
                this.rectangle5mmColor.Fill = solidColorBrush;
            }
        }

        private void button1mmColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDlg = new System.Windows.Forms.ColorDialog();
            if (colorDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.Color color = colorDlg.Color;
                SolidColorBrush solidColorBrush = new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
                this.rectangle1mmColor.Fill = solidColorBrush;

            }
        }

        /// <summary>
        /// 保存打印报告的参数设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonReportSave_Click(object sender, RoutedEventArgs e)
        {
            ConfigOp.SetAppSetting("ReportTitle", this.textBoxReportName.Text);
            SolidColorBrush solidBrush5mm=this.rectangle5mmColor.Fill as SolidColorBrush;
            System.Drawing.Color color5mm = System.Drawing.Color.FromArgb(solidBrush5mm.Color.R, solidBrush5mm.Color.G, solidBrush5mm.Color.B);
            ConfigOp.SetAppSetting("Grid5mmColor", color5mm.ToArgb().ToString());
            SolidColorBrush solidBrush1mm = this.rectangle1mmColor.Fill as SolidColorBrush;
            System.Drawing.Color color1mm = System.Drawing.Color.FromArgb(solidBrush1mm.Color.R, solidBrush1mm.Color.G, solidBrush1mm.Color.B);
            ConfigOp.SetAppSetting("Grid1mmColor", color1mm.ToArgb().ToString());
            ConfigOp.SetAppSetting("Grid5mmPoint", this.checkBox5mmPoint.IsChecked.ToString());
            ConfigOp.SetAppSetting("Grid1mmPoint", this.checkBox1mmPoint.IsChecked.ToString());
            if (this.comboBoxWaveWidth.SelectedIndex != -1)
            {
                ConfigOp.SetAppSetting("WaveThickness", this.comboBoxWaveWidth.SelectedValue.ToString());
            }
            if (this.comboBox5mmWidth.SelectedIndex != -1)
            {
                ConfigOp.SetAppSetting("Grid5mmThickness", this.comboBox5mmWidth.SelectedValue.ToString());
            }
            if (this.comboBox1mmWidth.SelectedIndex != -1)
            {
                ConfigOp.SetAppSetting("Grid1mmThickness", this.comboBox1mmWidth.SelectedValue.ToString());
            }
            ConfigOp.SetAppSetting("ReportTemplate", (this.comboBoxReportType.SelectedIndex+1).ToString());          
            this.Close();
        }

        /// <summary>
        /// 加载打印报告配置到界面
        /// </summary>
        private void loadReportConfig()
        {
            this.textBoxReportName.Text = ConfigurationManager.AppSettings["ReportTitle"];
            this.checkBox5mmPoint.IsChecked = Convert.ToBoolean(ConfigurationManager.AppSettings["Grid5mmPoint"]);
            this.checkBox1mmPoint.IsChecked = Convert.ToBoolean(ConfigurationManager.AppSettings["Grid1mmPoint"]);
            this.comboBoxWaveWidth.SelectedValue = double.Parse(ConfigurationManager.AppSettings["WaveThickness"]);
            this.comboBox5mmWidth.SelectedValue = double.Parse(ConfigurationManager.AppSettings["Grid5mmThickness"]);
            this.comboBox1mmWidth.SelectedValue = double.Parse(ConfigurationManager.AppSettings["Grid1mmThickness"]);
            System.Drawing.Color color5mm = System.Drawing.Color.FromArgb(int.Parse(ConfigurationManager.AppSettings["Grid5mmColor"]));
            this.rectangle5mmColor.Fill = new SolidColorBrush(Color.FromRgb(color5mm.R, color5mm.G, color5mm.B));
            System.Drawing.Color color1mm = System.Drawing.Color.FromArgb(int.Parse(ConfigurationManager.AppSettings["Grid1mmColor"]));
            this.rectangle1mmColor.Fill = new SolidColorBrush(Color.FromRgb(color1mm.R, color1mm.G, color1mm.B));
            int reportType;
            int.TryParse(ConfigurationManager.AppSettings["ReportTemplate"],out reportType);
            this.comboBoxReportType.SelectedIndex = reportType-1;
        }
        #endregion

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


        private void buttonTranSetCel_Click(object sender, RoutedEventArgs e)
        {            this.Close();

            this.Close();
        }

        private void tabSendConf_Loaded(object sender, RoutedEventArgs e)
        {
            int i;
            int SelIdx = 0;
            string CurLocalIP = ConfigurationManager.AppSettings["SendFromIPAddr"];
            string strHostName = Dns.GetHostName(); //得到本机的主机名
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName); //取得本机IPSystem.Net.IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(hostName);
            for (i = 0; i < ipEntry.AddressList.Count(); i++)
            {
                if (ipEntry.AddressList[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {

                    this.SendFromIPAddr.Items.Add(ipEntry.AddressList[i].ToString());
                    if (ipEntry.AddressList[i].Equals(CurLocalIP))
                        SelIdx = i;
                }
            }
            this.SendFromIPAddr.SelectedIndex = SelIdx;
            string strAddr = ipEntry.AddressList[0].ToString(); //假设本地主机为单网卡
            this.SendPortNo.Text = ConfigurationManager.AppSettings["SendPortNo"];
            this.SendToIPAddr.Text = ConfigurationManager.AppSettings["SendToIPAddr"];
        }

        private void tabRecvConf_Loaded(object sender, RoutedEventArgs e)
        {
            int i;
            int SelIdx=0;
            string CurServerIP = ConfigurationManager.AppSettings["RecvIPAddr"];
            string strHostName = Dns.GetHostName(); //得到本机的主机名
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName); //取得本机IPSystem.Net.IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(hostName);
            for (i = 0; i < ipEntry.AddressList.Count(); i++)
            {
                if (ipEntry.AddressList[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {

                    this.RecvIPAddr.Items.Add(ipEntry.AddressList[i].ToString());
                    if ( ipEntry.AddressList[i].ToString().Equals(CurServerIP))
                        SelIdx = this.RecvIPAddr.Items.Count - 1 ;
                }
            }
            this.RecvIPAddr.SelectedIndex = SelIdx;
            this.RecvPortNo.Text = ConfigurationManager.AppSettings["RecvPortNo"];
        }

        private void buttonRecvConfSet_Click(object sender, RoutedEventArgs e)
        {
            ConfigOp.SetAppSetting("RecvIPAddr", this.RecvIPAddr.Text.ToString());
            ConfigOp.SetAppSetting("RecvPortNo", this.RecvPortNo.Text);
            this.Close();
        }

        private void buttonSendConfSet_Click(object sender, RoutedEventArgs e)
        {
            ConfigOp.SetAppSetting("SendFromIPAddr", this.SendFromIPAddr.SelectedItem.ToString());
            ConfigOp.SetAppSetting("SendToIPAddr", this.SendToIPAddr.Text.ToString());
            ConfigOp.SetAppSetting("SendPortNo", this.SendPortNo.Text);
            this.Close();

        }
       
    }
}
