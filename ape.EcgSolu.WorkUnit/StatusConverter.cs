//----------------------------------------------------------------------
//数据类型转换器，用于UI的binding
//----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using ape.EcgSolu.Model;

namespace ape.EcgSolu.WorkUnit
{
    class StatusBackConverter:IValueConverter
    {
        SolidColorBrush waitSamplingBrush = new SolidColorBrush(Color.FromArgb(255,255,0,0));
        SolidColorBrush waitDiagBrush = new SolidColorBrush(Color.FromArgb(255,255,169,0));
        SolidColorBrush diagDoneBrush = new SolidColorBrush(Color.FromArgb(255,0,167,83));

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int status = (int)value;
            switch (status)
            {
                case DataReference.Status.WaitSampling:
                    return waitSamplingBrush;
                case DataReference.Status.WaitDiag:
                    return waitDiagBrush;
                case DataReference.Status.Done:
                    return diagDoneBrush;
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class StatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int status = (int)value;
            switch (status)
            {
                case DataReference.Status.WaitSampling:
                    return "未采集";
                case DataReference.Status.WaitDiag:
                    return "待诊";
                case DataReference.Status.Done:
                    return "完成";
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
