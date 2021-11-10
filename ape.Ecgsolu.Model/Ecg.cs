using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ape.EcgSolu.Model
{    
    public class Ecg
    {
        public string CpuIdentifier{get;set;}       //天津要求的一个标识符：Cpu+ip+time
        public Guid Id { get; set; }                //guid，唯一标识符
        public string PatientName { get; set; }
        public DateTime? Birthday { get; set; }
        public string DataSource { get; set; }
        public int Status { get; set; }
        public int Sex { get; set; }    
        public int Age { get; set; }
        public string ClinicDiag { get; set; }      //临床诊断
        public string DiagResult { get; set; }
        public double uVpb { get; set; }
        public int Duration { get; set; }
        public string DiagDoctor { get; set; }     
        public string ApplyNo { get; set; }
        public string ApplyDept { get; set; }
        public DateTime? ApplyDate { get; set; }        
        public string ApplyDoctor { get; set; }
        public int SamplingRate { get; set; }
        public DateTime? SamplingDate { get; set; }
        public string OutPatientNo { get; set; }
        public string InPatientNo { get; set; }
        public string PatientId { get; set; }
        public string PatientTelephone { get; set; }
        public int EcgType { get; set; }
        public int LeadCount { get; set; }
        public string[] LeadTitle { get; set; }
        public string HighpassFilter { get; set; }
        public string InsuranceNo { get; set; }         //医疗保险号
        public string LowpassFilter { get; set; }
        public string NotchFilter { get; set; }
        public short[,] Data { get; set; }
        public int DataCount { get; set; }
        public int DataStart { get; set; }      //数据开始点，用于确认报告打印从哪开始
        public int TempDataCount { get; set; }     
        public short[,] TempData { get; set; }
        public int Printed { get; set; }
        public int[,] OnOff { get; set; }
        public int RR { get; set; }
        public int Pd { get; set; }
        public int PR { get; set; }
        public int QRS { get; set; }
        public int QT { get; set; }
        public int QTc { get; set; }
        public int QTd { get; set; }
        public int QTMax { get; set; }
        public int QTMin { get; set; }
        public int Axis_P { get; set; }
        public int Axis_T { get; set; }
        public int Axis_QRS { get; set; }
        public double RV5 { get; set; }
        public double RV6 { get; set; }
        public double SV1 { get; set; }
        public double SV2 { get; set; }
        public double RV1 { get; set; }
        public double SV5 { get; set; }
        public string ExamItem { get; set; }        //检查项目名称
        public string DiagDept { get; set; }
        public DateTime? DiagDate { get; set; }         //初次诊断日期
        public DateTime? ModifyDate { get; set; }       //更新日期
        public int HeartRate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
