//------------------------------------------------------------------
//xml伪序列化帮助类
//------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using ape.EcgSolu.Model;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ape.EcgSolu.BLL
{
    public class CustomSerializer
    {
        /// <summary>
        /// 序列化输出xml文件
        /// </summary>
        /// <param name="ecgEntity"></param>
        public void SerializeXml(Ecg ecgEntity, string filename)
        {
            XElement Ecg = new XElement("Ecg");
            XElement CpuIdentifier = new XElement("CpuIdentifier") { Value = objectToString(ecgEntity.CpuIdentifier) };
            XElement Id = new XElement("Id") { Value = ecgEntity.Id.ToString() };
            XElement PatientName = new XElement("PatientName") { Value = objectToString(ecgEntity.PatientName) };
            XElement Birthday = new XElement("Birthday") { Value = objectToString(ecgEntity.Birthday.ToString()) };
            XElement DataSource = new XElement("DataSource") { Value = objectToString(ecgEntity.DataSource) };
            XElement Status = new XElement("Status") { Value = objectToString(ecgEntity.Status) };
            XElement Sex = new XElement("Sex") { Value = objectToString(ecgEntity.Sex) };
            XElement Age = new XElement("Age") { Value = objectToString(ecgEntity.Age) };
            XElement ClinicDiag = new XElement("ClinicDiag") { Value = objectToString(ecgEntity.ClinicDiag) };
            XElement DiagResult = new XElement("DiagResult") { Value = objectToString(ecgEntity.DiagResult) };
            XElement uVpb = new XElement("uVpb") { Value = objectToString(ecgEntity.uVpb) };
            XElement Duration = new XElement("Duration") { Value = ecgEntity.Duration.ToString() };
            XElement DiagDoctor = new XElement("DiagDoctor") { Value = objectToString(ecgEntity.DiagDoctor) };
            XElement ApplyNo = new XElement("ApplyNo") { Value = objectToString(ecgEntity.ApplyNo) };
            XElement ApplyDept = new XElement("ApplyDept") { Value = objectToString(ecgEntity.ApplyDept) };
            XElement ApplyDate = new XElement("ApplyDate") { Value = objectToString(ecgEntity.ApplyDate) };
            XElement ApplyDoctor = new XElement("ApplyDoctor") { Value = objectToString(ecgEntity.ApplyDoctor) };
            XElement SamplingRate = new XElement("SamplingRate") { Value = objectToString(ecgEntity.SamplingRate) };
            XElement SamplingDate = new XElement("SamplingDate") { Value = objectToString(ecgEntity.SamplingDate) };
            XElement OutPatientNo = new XElement("OutPatientNo") { Value = objectToString(ecgEntity.OutPatientNo) };
            XElement InPatientNo = new XElement("InPatientNo") { Value = objectToString(ecgEntity.InPatientNo) };
            XElement PatientId = new XElement("PatientId") { Value = objectToString(ecgEntity.PatientId) };
            XElement PatientTelephone = new XElement("PatientTelephone") { Value = objectToString(ecgEntity.PatientTelephone) };
            XElement EcgType = new XElement("EcgType") { Value = objectToString(ecgEntity.EcgType) };
            XElement LeadCount = new XElement("LeadCount") { Value = ecgEntity.LeadCount.ToString() };
            XElement LeadTitle = new XElement("LeadTitle") { Value = ecgEntity.LeadTitle == null ? string.Empty : string.Join(",", ecgEntity.LeadTitle) };
            XElement HighpassFilter = new XElement("HighpassFilter") { Value = objectToString(ecgEntity.HighpassFilter) };
            XElement InsuranceNo = new XElement("InsuranceNo") { Value = objectToString(ecgEntity.InsuranceNo) };
            XElement LowpassFilter = new XElement("LowpassFilter") { Value = objectToString(ecgEntity.LowpassFilter) };
            XElement NotchFilter = new XElement("NotchFilter") { Value = objectToString(ecgEntity.NotchFilter) };
            XElement Data = new XElement("Data") { Value = objectToBase64(ecgEntity.Data) };
            XElement DataCount = new XElement("DataCount") { Value = ecgEntity.DataCount.ToString() };
            XElement DataStart = new XElement("DataStart") { Value = ecgEntity.DataStart.ToString() };
            XElement TempDataCount = new XElement("TempDataCount") { Value = ecgEntity.TempDataCount.ToString() };
            XElement TempData = new XElement("TempData") { Value = objectToBase64(ecgEntity.TempData) };
            XElement Printed = new XElement("Printed") { Value = ecgEntity.Printed.ToString() };
            XElement OnOff = new XElement("OnOff") { Value = objectToBase64(ecgEntity.OnOff) };
            XElement RR = new XElement("RR") { Value = ecgEntity.RR.ToString() };
            XElement Pd = new XElement("Pd") { Value = ecgEntity.Pd.ToString() };
            XElement PR = new XElement("PR") { Value = ecgEntity.PR.ToString() };
            XElement QRS = new XElement("QRS") { Value = ecgEntity.QRS.ToString() };
            XElement QT = new XElement("QT") { Value = ecgEntity.QT.ToString() };
            XElement QTc = new XElement("QTc") { Value = ecgEntity.QTc.ToString() };
            XElement QTd = new XElement("QTd") { Value = ecgEntity.QTd.ToString() };
            XElement QTMax = new XElement("QTMax") { Value = ecgEntity.QTMax.ToString() };
            XElement QTMin = new XElement("QTMin") { Value = ecgEntity.QTMin.ToString() };
            XElement Axis_P = new XElement("Axis_P") { Value = ecgEntity.Axis_P.ToString() };
            XElement Axis_T = new XElement("Axis_T") { Value = ecgEntity.Axis_T.ToString() };
            XElement Axis_QRS = new XElement("Axis_QRS") { Value = ecgEntity.Axis_QRS.ToString() };
            XElement RV5 = new XElement("RV5") { Value = ecgEntity.RV5.ToString() };
            XElement RV6 = new XElement("RV6") { Value = ecgEntity.RV6.ToString() };
            XElement SV1 = new XElement("SV1") { Value = ecgEntity.SV1.ToString() };
            XElement SV2 = new XElement("SV2") { Value = ecgEntity.SV2.ToString() };
            XElement RV1 = new XElement("RV1") { Value = ecgEntity.RV1.ToString() };
            XElement SV5 = new XElement("SV5") { Value = ecgEntity.SV5.ToString() };
            XElement ExamItem = new XElement("ExamItem") { Value = objectToString(ecgEntity.ExamItem) };
            XElement DiagDept = new XElement("DiagDept") { Value = objectToString(ecgEntity.DiagDept) };
            XElement DiagDate = new XElement("DiagDate") { Value = objectToString(ecgEntity.DiagDate) };
            XElement ModifyDate = new XElement("ModifyDate") { Value = objectToString(ecgEntity.ModifyDate) };
            XElement HeartRate = new XElement("HeartRate") { Value = ecgEntity.HeartRate.ToString() };
            XElement IsDeleted = new XElement("IsDeleted") { Value = ecgEntity.IsDeleted.ToString() };

            Ecg.Add(CpuIdentifier);
            Ecg.Add(Id);
            Ecg.Add(PatientName);
            Ecg.Add(Birthday);
            Ecg.Add(DataSource);
            Ecg.Add(Status);
            Ecg.Add(Sex);
            Ecg.Add(Age);
            Ecg.Add(ClinicDiag);
            Ecg.Add(DiagResult);
            Ecg.Add(uVpb);
            Ecg.Add(Duration);
            Ecg.Add(DiagDoctor);
            Ecg.Add(ApplyNo);
            Ecg.Add(ApplyDept);
            Ecg.Add(ApplyDate);
            Ecg.Add(ApplyDoctor);
            Ecg.Add(SamplingRate);
            Ecg.Add(SamplingDate);
            Ecg.Add(OutPatientNo);
            Ecg.Add(InPatientNo);
            Ecg.Add(PatientId);
            Ecg.Add(PatientTelephone);
            Ecg.Add(EcgType);
            Ecg.Add(LeadCount);
            Ecg.Add(LeadTitle);
            Ecg.Add(HighpassFilter);
            Ecg.Add(InsuranceNo);
            Ecg.Add(LowpassFilter);
            Ecg.Add(NotchFilter);
            Ecg.Add(Data);
            Ecg.Add(DataCount);
            Ecg.Add(DataStart);
            Ecg.Add(TempDataCount);
            Ecg.Add(TempData);
            Ecg.Add(Printed);
            Ecg.Add(OnOff);
            Ecg.Add(RR);
            Ecg.Add(Pd);
            Ecg.Add(PR);
            Ecg.Add(QRS);
            Ecg.Add(QT);
            Ecg.Add(QTc);
            Ecg.Add(QTd);
            Ecg.Add(QTMax);
            Ecg.Add(QTMin);
            Ecg.Add(Axis_P);
            Ecg.Add(Axis_T);
            Ecg.Add(Axis_QRS);
            Ecg.Add(RV5);
            Ecg.Add(RV6);
            Ecg.Add(SV1);
            Ecg.Add(SV2);
            Ecg.Add(RV1);
            Ecg.Add(SV5);
            Ecg.Add(ExamItem);
            Ecg.Add(DiagDept);
            Ecg.Add(DiagDate);
            Ecg.Add(ModifyDate);
            Ecg.Add(HeartRate);
            Ecg.Add(IsDeleted);
            using (StreamWriter sw = new StreamWriter(filename,false, Encoding.UTF8))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                sw.Write(Ecg.ToString());
            }
        }

        public Ecg DeserializeXml(string filename)
        {
            Ecg ecgEntity = new Ecg();

            //using (StreamReader sw = new StreamReader(filename, Encoding.UTF8, false))
            //{
            //    sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            //    sw.Write(Ecg.ToString());
            //}
            byte[] wkdata ;
        Stream stream = File.OpenRead(filename);  
        XDocument document = XDocument.Load(stream);  
        stream.Dispose();
        //IEnumerable<XElement> de =
    //from element in document.Descendants("Ecg")
    //select element;
    //    foreach (XElement element in de)
        try
        {
            foreach (XElement element in document.Descendants("Ecg"))
            {
                ecgEntity.CpuIdentifier = element.Element("CpuIdentifier").Value;
                ecgEntity.Id = Guid.Parse(element.Element("Id").Value);
                ecgEntity.PatientName = element.Element("PatientName").Value; 
                if (element.Element("Birthday").Value == "" ) 
                    ecgEntity.Birthday = null;
                else
                    ecgEntity.Birthday = Convert.ToDateTime(element.Element("Birthday").ToString());
                ecgEntity.DataSource = element.Element("DataSource").Value;
                ecgEntity.Status = int.Parse(element.Element("Status").Value);
                ecgEntity.Sex = int.Parse(element.Element("Sex").Value);
                ecgEntity.Age = int.Parse(element.Element("Age").Value);
                ecgEntity.ClinicDiag = element.Element("ClinicDiag").Value;
                ecgEntity.DiagResult = element.Element("DiagResult").Value;
                ecgEntity.uVpb = double.Parse(element.Element("uVpb").Value);
                ecgEntity.Duration = int.Parse(element.Element("Duration").Value);
                ecgEntity.DiagDoctor = element.Element("DiagDoctor").Value;
                ecgEntity.ApplyNo = element.Element("ApplyNo").Value;
                ecgEntity.ApplyDept = element.Element("ApplyDept").Value;
                if (element.Element("ApplyDate").Value == "")
                    ecgEntity.ApplyDate = null;
                else
                    ecgEntity.ApplyDate = Convert.ToDateTime(element.Element("ApplyDate").Value.ToString());
                ecgEntity.ApplyDoctor = element.Element("ApplyDoctor").Value;
                ecgEntity.SamplingRate = int.Parse(element.Element("SamplingRate").Value);
                if (element.Element("SamplingDate").Value == "")
                    ecgEntity.SamplingDate = null;
                else
                    ecgEntity.SamplingDate = Convert.ToDateTime(element.Element("SamplingDate").Value);
                ecgEntity.OutPatientNo = element.Element("OutPatientNo").Value;
                ecgEntity.InPatientNo = element.Element("InPatientNo").Value;
                ecgEntity.PatientId = element.Element("PatientId").Value;
                ecgEntity.PatientTelephone = element.Element("PatientTelephone").Value;
                ecgEntity.EcgType = int.Parse(element.Element("EcgType").Value);
                ecgEntity.LeadCount = int.Parse(element.Element("LeadCount").Value);
                ecgEntity.LeadTitle = element.Element("LeadTitle").Value.Split(',');
                ecgEntity.HighpassFilter = element.Element("HighpassFilter").Value;
                ecgEntity.InsuranceNo = element.Element("InsuranceNo").Value;
                ecgEntity.LowpassFilter = element.Element("LowpassFilter").Value;
                ecgEntity.NotchFilter = element.Element("NotchFilter").Value;
                ecgEntity.DataCount = int.Parse(element.Element("DataCount").Value);
                wkdata = Convert.FromBase64String(element.Element("Data").Value);
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                ms.Write(wkdata, 0, wkdata.Length);
                ms.Seek(0, SeekOrigin.Begin);
                object cc = bf.Deserialize(ms);

                ecgEntity.Data =(short[,]) cc;
                ms.Close();
                ms.Dispose();
                
                ecgEntity.DataStart = int.Parse(element.Element("DataStart").Value);
                ecgEntity.TempDataCount = int.Parse(element.Element("TempDataCount").Value);
                if (ecgEntity.TempDataCount != 0)
                {
                    wkdata = Convert.FromBase64String(element.Element("TempData").Value);
                    MemoryStream ms1 = new MemoryStream();
                    ms1.Write(wkdata, 0, wkdata.Length);
                    ms1.Seek(0, SeekOrigin.Begin);
                    object cc1 = bf.Deserialize(ms1);

                    ecgEntity.TempData = (short[,])cc1;
                    ms1.Close();
                    ms1.Dispose();
                }
                ecgEntity.Printed = int.Parse(element.Element("Printed").Value);
                wkdata = Convert.FromBase64String(element.Element("OnOff").Value);
                if (wkdata.Length != 0)
                {
                    MemoryStream ms2 = new MemoryStream();
                    ms2.Write(wkdata, 0, wkdata.Length);
                    ms2.Seek(0, SeekOrigin.Begin);
                    object cc2 = bf.Deserialize(ms2);

                    ecgEntity.OnOff = (int[,])cc2;
                    ms2.Close();
                    ms2.Dispose();
                }
                // ecgEntity.OnOff =  ;
                ecgEntity.RR = int.Parse(element.Element("RR").Value);
                ecgEntity.Pd = int.Parse(element.Element("Pd").Value);
                ecgEntity.PR = int.Parse(element.Element("PR").Value);
                ecgEntity.QRS = int.Parse(element.Element("QRS").Value);
                ecgEntity.QT = int.Parse(element.Element("QT").Value);
                ecgEntity.QTc = int.Parse(element.Element("QTc").Value);
                ecgEntity.QTd = int.Parse(element.Element("QTd").Value);
                ecgEntity.QTMax = int.Parse(element.Element("QTMax").Value);
                ecgEntity.QTMin = int.Parse(element.Element("QTMin").Value);
                ecgEntity.Axis_P = int.Parse(element.Element("Axis_P").Value);
                ecgEntity.Axis_T = int.Parse(element.Element("Axis_T").Value);
                ecgEntity.Axis_QRS = int.Parse(element.Element("Axis_QRS").Value);
                ecgEntity.RV5 = int.Parse(element.Element("RV5").Value);
                ecgEntity.RV6 = int.Parse(element.Element("RV6").Value);
                ecgEntity.SV1 = int.Parse(element.Element("SV1").Value);
                ecgEntity.SV2 = int.Parse(element.Element("SV2").Value);
                ecgEntity.RV1 = int.Parse(element.Element("RV1").Value);
                ecgEntity.SV5 = int.Parse(element.Element("SV5").Value);
                ecgEntity.ExamItem = element.Element("ExamItem").Value;
                ecgEntity.DiagDept = element.Element("DiagDept").Value;
                if (element.Element("DiagDate").Value == "")
                    ecgEntity.DiagDate = null;
                else
                    ecgEntity.DiagDate = Convert.ToDateTime(element.Element("DiagDate").Value);
                if (element.Element("ModifyDate").Value == "")
                    ecgEntity.ModifyDate = null;
                else
                    ecgEntity.ModifyDate = Convert.ToDateTime(element.Element("ModifyDate").Value);
                ecgEntity.HeartRate = int.Parse(element.Element("HeartRate").Value);
                ecgEntity.IsDeleted = bool.Parse(element.Element("IsDeleted").Value);
                EcgBLL ecgBLL = new EcgBLL();
               // ecgBLL.DeleteByid(ecgEntity.Id);
                ecgBLL.InsertEcg(ecgEntity);

            }
        }
        catch (Exception es)
        {
            int ss = 0;// MessageBox.Show("其他异常" + es.Message);      //捕获其他异常
        }

            return null;
        }

        /// <summary>
        /// object转成string，用于xml序列化
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string objectToString(object value)
        {
            if (value == null)
                return string.Empty;
            else
                return value.ToString();
        }

        /// <summary>
        /// 把对象用base64字符串表示
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string objectToBase64(object value)
        {
            if (value == null)
                return string.Empty;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms=new MemoryStream();
            bf.Serialize(ms, value);
            
            byte[] tempBuffer = ms.ToArray();
            
            ms.Close();

            return Convert.ToBase64String(tempBuffer, Base64FormattingOptions.InsertLineBreaks);
        }
    }
}
