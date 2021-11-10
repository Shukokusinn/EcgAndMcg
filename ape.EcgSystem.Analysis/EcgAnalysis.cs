//---------------------------------------------------------------
//心电图特征参数分析
//---------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ape.EcgSystem.Analysis.Model;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ape.EcgSystem.Analysis
{
    public class EcgAnalysis
    {
        [DllImport("HeatBeat.dll",EntryPoint="Dectect_Init")]
        static extern void Dectect_Init(int init, int value, int fs);

        [DllImport("HeatBeat.dll")]
        static extern int fnHeatBeat(int sample);

        //分析12导联心电图数据
        public static EcgFeatureValue EcgAnalysis12(short[,] ecgData, int sampleRate, double uVpb)
        {
            //try
            //{
                int dataCount = ecgData.GetUpperBound(1) + 1;
                if (dataCount < sampleRate * 10)
                    throw new Exception("分析的心电图数据长度不够10秒");
                int[] leadData = new int[dataCount];
                int templateDataCount = 0;
                EcgFeatureValue efValue = new EcgFeatureValue();
                //通过II导联计算
                for (int i = 0; i < dataCount; i++)
                {
                    leadData[i] = ecgData[1, i];
                }
                List<int> qrsPoint;
                EcgAnalysis.EcgLeadAnalysis(leadData, sampleRate, out qrsPoint);

                int[,] onOff = new int[13, 6];
                templateDataCount = calTemplateDataCount(qrsPoint);
                efValue.TemplateDataCount = templateDataCount;
                efValue.TemplateData = getTemplateData12(qrsPoint, ecgData, templateDataCount, ref onOff);
                int rr, heartRate;
                calRR(qrsPoint, sampleRate, out rr, out heartRate);
                efValue.HeartRate = heartRate;
                efValue.RR = rr;
                efValue.RV1 = getRValue(qrsPoint, ecgData, 6, templateDataCount) * uVpb / 1000; //单位mV
                efValue.SV1 = getSValue(qrsPoint, ecgData, 6, templateDataCount) * uVpb / 1000;
                efValue.RV5 = getRValue(qrsPoint, ecgData, 10, templateDataCount) * uVpb / 1000;//单位mV，故除以1000
                efValue.SV5 = getSValue(qrsPoint, ecgData, 10, templateDataCount) * uVpb / 1000;
                efValue.OnOff = onOff;               
                return efValue;
            //}
            //catch (Exception e)
            //{
            //    Trace.Write(string.Format("{0} : {1}", DateTime.Now.ToString("yyyy-MM-dd"), e.Message));
            //    return new EcgFeatureValue();
            //}
        }

        private static void EcgLeadAnalysis2(int[] ecgData, int sampleRate, out List<int> qrsPoint)
        {
            qrsPoint = new List<int>();
            Dectect_Init(0,1,300);
            int qrsDeley = 0;
            for (int i = 0; i < ecgData.Length; i++)
            {
                if(i%3==0)
                {
                    qrsDeley = fnHeatBeat(ecgData[i]);
                    if (qrsDeley != 0)
                        qrsPoint.Add(i - qrsDeley);
                }
            }
        }

        //计算单一导联的QRS波的侦测
        private static void EcgLeadAnalysis(int[] ecgData, int sampleRate, out List<int> qrsPoint)
        {
            QRSDetect qrsDetect = new QRSDetect(sampleRate);            
            qrsPoint = new List<int>();
            for (int i = 0; i < ecgData.Length; i++)
            {
                int qrsDelay=0;
                if (i == 0)                
                    qrsDelay = qrsDetect.QRSDet(ecgData[i], 1);                
                else                
                    qrsDelay = qrsDetect.QRSDet(ecgData[i], 0);                               
                if ( qrsDelay!= 0)
                    qrsPoint.Add(i-qrsDelay);
            }
        }      

        //计算rr间期和hr心律
        private static void calRR(List<int> qrsPoint, int sampleRate, out int rr, out int heartRate)
        {
            long sum = 0;
            for (int i = 1; i < qrsPoint.Count; i++)
            {
                sum += (qrsPoint[i] - qrsPoint[i - 1]);
            }           
            int rrPoint = (int)(sum / (qrsPoint.Count-1));
            rr = (int)((double)rrPoint / sampleRate * 1000);
            heartRate = 60 * 1000 / rr;
        }

        //计算一个心博模板的数据长度，多少个数据
        private static int calTemplateDataCount(List<int> qrsPoint)
        {
            long sum = 0;
            for (int i = 1; i < qrsPoint.Count; i++)
            {
                sum += (qrsPoint[i] - qrsPoint[i - 1]);
            }
            int templateDataCount = (int)(sum / (qrsPoint.Count - 1));
            return templateDataCount;
        }

        //获取一个导联的模板的数据
        private static short[] getTemplateData(List<int> qrsPoint,short[,] ecgData, int leadNum,int templateDataCount,ref int[] onOff)
        {
            int qrsNum = 3;     //取第几个qrs波作为模板
            int qrsPeak = qrsPoint[qrsNum-1];
            int startPoint = qrsPeak - templateDataCount / 2;
            onOff[2] = templateDataCount / 2;
            onOff[0] = 1;
            onOff[1] = (onOff[2] + onOff[0]) / 2;
            onOff[5] = templateDataCount;
            onOff[3] = (onOff[5] + onOff[2]) / 2;
            onOff[4] = (onOff[5] + onOff[3]) / 2;
            //int endPoint = templateDataCount % 2 == 0 ? qrsPeak + templateDataCount / 2 : qrsPeak + templateDataCount / 2 + 1;
            short[] templateData = new short[templateDataCount];
            for (int i = 0; i < templateDataCount; i++)
            {
                templateData[i] = ecgData[leadNum, startPoint + i];
            }
            return templateData;
        }

        //获取12导联的模板数据
        private static short[,] getTemplateData12(List<int> qrsPoint, short[,] ecgData, int templateDataCount,ref int[,] onOff)
        {
            short[,] templateData = new short[12, templateDataCount];
            int[] onOffUnit=new int[6];
            for (int i = 0; i < 12; i++)
            {
                short[] templateDataUnit = getTemplateData(qrsPoint, ecgData, i, templateDataCount,ref onOffUnit);
                for (int j = 0; j < templateDataCount; j++)
                    templateData[i, j] = templateDataUnit[j];
                for (int j = 0; j < 6; j++)
                {
                    if (i == 11)
                        onOff[12, j] = onOffUnit[j];
                    else
                        onOff[i, j] = onOffUnit[j];
                }
            }
            
            return templateData;
        }

        //获取一个导联的R波的平均值
        //leadNum:第几导联
        private static int getRValue(List<int> qrsPoint, short[,] ecgData, int leadNum,int templateDataCount)
        {
            if (qrsPoint.Count == 0)
                return 0;
            List<int> rList = new List<int>();
            int rSum = 0;
            foreach (int qrsPeak in qrsPoint)
            {
                rSum += getRValueUnit(qrsPeak, ecgData, leadNum, templateDataCount);
            }
            return rSum / qrsPoint.Count;
        }

        //获取一个qrs波群附近的R点（正向最大值）
        private static int getRValueUnit(int qrsPeak, short[,] ecgData, int leadNum, int templateDataCount)
        {
            int dataCount=ecgData.GetUpperBound(1);
            int maxValue=0;
            if (qrsPeak - templateDataCount / 2 >= 0)
            {
                if (qrsPeak + templateDataCount / 2 >= dataCount)
                {
                    for(int i=qrsPeak-templateDataCount/2;i<dataCount;i++)
                        maxValue=ecgData[leadNum,i]>maxValue?ecgData[leadNum,i]:maxValue;
                }
                else
                {
                    for (int i = qrsPeak - templateDataCount / 2; i < qrsPeak+templateDataCount/2; i++)
                        maxValue = ecgData[leadNum, i] > maxValue ? ecgData[leadNum, i] : maxValue;
                }
            }
            else
            {
                for(int i=0;i<qrsPeak+templateDataCount/2;i++)
                    maxValue = ecgData[leadNum, i] > maxValue ? ecgData[leadNum, i] : maxValue;
            }
            return maxValue;
        }

        //获取一个qrs波群附近的S点（最小值）
        private static int getSValueUnit(int qrsPeak, short[,] ecgData, int leadNum, int templateDataCount)
        {
            int dataCount = ecgData.GetUpperBound(1);
            int minValue = 0;
            if (qrsPeak - templateDataCount / 2 >= 0)
            {
                if (qrsPeak + templateDataCount / 2 >= dataCount)
                {
                    for (int i = qrsPeak - templateDataCount / 2; i < dataCount; i++)
                        minValue = ecgData[leadNum, i] < minValue ? ecgData[leadNum, i] : minValue;
                }
                else
                {
                    for (int i = qrsPeak - templateDataCount / 2; i < qrsPeak + templateDataCount / 2; i++)
                        minValue = ecgData[leadNum, i] < minValue ? ecgData[leadNum, i] : minValue;
                }
            }
            else
            {
                for (int i = 0; i < qrsPeak + templateDataCount / 2; i++)
                    minValue = ecgData[leadNum, i] < minValue ? ecgData[leadNum, i] : minValue;
            }
            return minValue;
        }

        //获取一个导联的S波的平均值
        //leadNum：第几导联
        private static int getSValue(List<int> qrsPoint, short[,] ecgData, int leadNum,int templateDataCount)
        {
            if (qrsPoint.Count == 0)
                return 0;
            List<int> rList = new List<int>();
            int rSum = 0;
            foreach (int qrsPeak in qrsPoint)
            {
                rSum += getSValueUnit(qrsPeak, ecgData, leadNum, templateDataCount);
            }
            return rSum / qrsPoint.Count;
        }
    }
}
