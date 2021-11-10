using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Net;
using System.Net.Sockets;

namespace ape.EcgSolu.BLL
{
    public class BuildCpuIpNo
    {
        static string cpuSerialNo = string.Empty;
        static string localIp = string.Empty;

        static BuildCpuIpNo()
        {
            cpuSerialNo = getCpuSerial();
            localIp = getLocalIp();
        }

        /// <summary>
        /// 获取CPU序列号
        /// </summary>
        /// <returns></returns>
        private static string getCpuSerial()
        {
            ManagementClass mc = new ManagementClass("Win32_Processor");
            ManagementObjectCollection moc = mc.GetInstances();
            string cpuInfo=string.Empty;
            foreach (ManagementObject mo in moc)
            {
                cpuInfo = mo.Properties["ProcessorId"].Value.ToString().ToLower();
                break;
            }
            return cpuInfo;
        }

        private static string getLocalIp()
        {
            string ipStr = "0.0.0.0";
            IPHostEntry ipe = Dns.GetHostEntry(Dns.GetHostName());           
            foreach (IPAddress ipaUnit in ipe.AddressList)
            {
                if (ipaUnit.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ipaUnit.ToString();                   
                }
            }
            return ipStr;
        }

        /// <summary>
        /// 获取CPU+IP+Time的标识符
        /// </summary>
        /// <returns></returns>
        public static string GetCpuIndetifier()
        {
            return string.Format("{0}{1}{2:yyyyMMddHHmmss}", cpuSerialNo, localIp, DateTime.Now);
        }
    }
}
