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
using ape.EcgSolu.Model;
using ape.EcgSolu.BLL;

namespace ape.EcgSolu.WorkUnit
{
    /// <summary>
    /// NewExam.xaml 的交互逻辑
    /// </summary>
    public partial class NewExamWindow : Window
    {
        public Ecg EcgEntity = new Ecg();

        public NewExamWindow()
        {
            InitializeComponent();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            this.EcgEntity.Id = Guid.NewGuid();
            if (this.RadioMale.IsChecked==true)
            {
                this.EcgEntity.Sex=DataReference.Sex.Male;
            }
            if (this.RadioFemale.IsChecked==true)
            {
                this.EcgEntity.Sex = DataReference.Sex.Female;
            }
            if (this.RadioFemale.IsChecked == false && this.RadioMale.IsChecked == false)
            {
                this.EcgEntity.Sex = DataReference.Sex.Unknow;
            }
            this.EcgEntity.Status = DataReference.Status.WaitSampling;
            this.EcgEntity.CpuIdentifier = BuildCpuIpNo.GetCpuIndetifier();
            this.DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EcgEntity.PatientName = "未命名";
            this.DataContext = EcgEntity;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
