using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.IO;

namespace ape.EcgSolu.WorkUnit
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            initLogTrace();
        }

        //日志跟踪器初始化
        private void initLogTrace()
        {
            string logFile = string.Format("{0}{1}\\{2}_{3}.log", AppDomain.CurrentDomain.BaseDirectory, "Log", "EcgWorkbeanch", DateTime.Now.ToString("yyyyMM"));
            string logDir = Path.GetDirectoryName(logFile);
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);
            if (!File.Exists(logFile))
            {                
                FileStream fs = File.Create(logFile);
                fs.Close();
            }
            TextWriterTraceListener textListener = new TextWriterTraceListener(logFile);
            Global.LogTrace.Listeners.Remove("Default");
            Global.LogTrace.Listeners.Add(textListener);
        }

        //输出错误日志
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Trace.WriteLine(string.Format("{0} : Exception:{1}",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),e.Exception.Message));
        }
    }
}
