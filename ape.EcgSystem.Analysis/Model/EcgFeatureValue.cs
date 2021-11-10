//----------------------------------------------------------------
//心电图特征参数的数据实体
//----------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ape.EcgSystem.Analysis.Model
{
    public class EcgFeatureValue
    {
        public int RR { get; set; }
        public int HeartRate { get; set; }
        public double RV1 { get; set; }
        public double SV1 { get; set; }
        public double RV5 { get; set; }
        public double SV5 { get; set; }
        public int TemplateDataCount { get; set; }
        public short[,] TemplateData { get; set; }
        public int[,] OnOff { get; set; }       //13*6
    }
}
