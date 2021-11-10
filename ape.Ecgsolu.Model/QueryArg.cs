//-----------------------------------------------------------------
//查询参数，数据实体
//-----------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ape.EcgSolu.Model
{
    public class QueryArg
    {
        public string PatientName { get; set; }
        public int? Gender { get; set; }
        public string ApplyNo { get; set; }
        public string InPatientNo { get; set; }
        public string OutPatientNo { get; set; }
        public string ApplyDept { get; set; }
        public string ApplyDocotor { get; set; }
        public string DiagDocotor { get; set; }
        public string DiagResult { get; set; }
        public bool DateCheck { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
    }
}
