using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ape.EcgSolu.Model;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Media.Imaging;
using ape.EcgSolu.WorkUnit.Utility;

namespace ape.EcgSolu.WorkUnit.Diagnosis
{
    abstract class ReportVisualBase:FrameworkElement
    {
        protected VisualCollection _children;
        protected double unitPerMm = 96 / 25.39999;       //1mm=多少wpf单位       

        public ReportVisualBase(Ecg ecgEntity, Color grid5mmColor, Color grid1mmColor, double grid5mmThickness,
            double grid1mmThickness, double waveThickness, bool grid5mmPoint, bool grid1mmPoint)
        {
            _children = new VisualCollection(this);
            _children.Add(initVisual(ecgEntity, grid5mmColor, grid1mmColor, grid5mmThickness, grid1mmThickness, waveThickness, grid5mmPoint, grid1mmPoint));
        }

         /// <summary>
        /// 绘制报告，初始化
        /// </summary>
        /// <returns></returns>
        protected abstract DrawingVisual initVisual(Ecg ecgEntity, Color grid5mmColor, Color grid1mmColor, double grid5mmThickness,
            double grid1mmThickness, double waveThickness, bool grid5mmPoint, bool grid1mmPoint);

        //构造生成QRCode的字符串
        protected string buildTextInfo(Ecg ecgEntity)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("姓名:" + ecgEntity.PatientName + ";");
            sb.Append("性别:" + Helper.GenderConvert(ecgEntity.Sex) + ";");
            sb.Append("Identifier:" + ecgEntity.CpuIdentifier + ";");
            sb.Append("门诊号:" + ecgEntity.OutPatientNo + ";");
            sb.Append("住院号:" + ecgEntity.InPatientNo + ";");
            sb.Append("医保号:" + ecgEntity.InsuranceNo + ";");
            sb.Append("时间：" + ecgEntity.SamplingDate.ToString() + ";");
            return sb.ToString();
        }

        /// <summary>
        /// 采样值转换成绘图单位值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="uVpb"></param>
        /// <param name="unitPerMm"></param>
        /// <returns></returns>
        protected double ValueToUnit(short value, double uVpb, double unitPerMm)
        {
            return value * uVpb / 1000 * 10 * unitPerMm;
        }

        /// <summary>
        /// 性别转换映射
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected string GenderConvert(int value)
        {
            string gender = string.Empty;
            switch (value)
            {
                case -1:
                    gender = "未知";
                    break;
                case 0:
                    gender = "女";
                    break;
                case 1:
                    gender = "男";
                    break;
            }
            return gender;
        }

        // Provide a required override for the VisualChildrenCount property.
        protected override int VisualChildrenCount
        {
            get { return _children.Count; }
        }

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _children[index];
        }       
    }
}
