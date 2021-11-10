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

namespace ape.EcgSolu.WorkUnit.Diagnosis
{
    /// <summary>
    /// AjustWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AjustWindow : Window
    {
        AjustVisual ajustVisual;
        Ecg ecgEntity;
        //double pStart, pEnd, qrsStart, qrsEnd, tStart, tEnd;       
        bool[] pointMoveable = new bool[6] { false, false, false, false, false, false };

        public AjustWindow(Ecg ecgEntity)
        {
            InitializeComponent();
            this.ecgEntity = ecgEntity;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ajustVisual = new AjustVisual(this.ecgEntity,(int)this.canvasWave.ActualWidth, (int)this.canvasWave.ActualHeight);
            this.canvasWave.Children.Add(ajustVisual);
            this.radioButtonLeadAll.IsChecked = true;
        }

        private void init()
        {

        }

        //显示LeadI的叠加图
        private void radioButtonLeadI_Click(object sender, RoutedEventArgs e)
        {
            bool[] leadToggle = new bool[12] { true, false, false, false, false, false, false, false, false, false, false, false };
            ajustVisual.DrawWave(leadToggle);
        }

        private void radioButtonLeadII_Click(object sender, RoutedEventArgs e)
        {
            bool[] leadToggle = new bool[12] { false, true, false, false, false, false, false, false, false, false, false, false };
            ajustVisual.DrawWave(leadToggle);
        }

        private void radioButtonLeadIII_Click(object sender, RoutedEventArgs e)
        {
            bool[] leadToggle = new bool[12] { false, false, true, false, false, false, false, false, false, false, false, false };
            ajustVisual.DrawWave(leadToggle);
        }

        private void radioButtonLeadaVR_Click(object sender, RoutedEventArgs e)
        {
            bool[] leadToggle = new bool[12] { false, false, false, true, false, false, false, false, false, false, false, false };
            ajustVisual.DrawWave(leadToggle);
        }

        private void radioButtonLeadaVL_Click(object sender, RoutedEventArgs e)
        {
            bool[] leadToggle = new bool[12] { false, false, false, false, true, false, false, false, false, false, false, false };
            ajustVisual.DrawWave(leadToggle);
        }

        private void radioButtonLeadaVF_Click(object sender, RoutedEventArgs e)
        {
            bool[] leadToggle = new bool[12] { false, false, false, false, false, true, false, false, false, false, false, false };
            ajustVisual.DrawWave(leadToggle);
        }

        private void radioButtonLeadV1_Click(object sender, RoutedEventArgs e)
        {
            bool[] leadToggle = new bool[12] { false, false, false, false, false, false, true, false, false, false, false, false };
            ajustVisual.DrawWave(leadToggle);
        }

        private void radioButtonLeadV2_Click(object sender, RoutedEventArgs e)
        {
            bool[] leadToggle = new bool[12] { false, false, false, false, false, false, false, true, false, false, false, false };
            ajustVisual.DrawWave(leadToggle);
        }

        private void radioButtonLeadV3_Click(object sender, RoutedEventArgs e)
        {
            bool[] leadToggle = new bool[12] { false, false, false, false, false, false, false, false, true, false, false, false };
            ajustVisual.DrawWave(leadToggle);
        }

        private void radioButtonLeadV4_Click(object sender, RoutedEventArgs e)
        {
            bool[] leadToggle = new bool[12] { false, false, false, false, false, false, false, false, false, true, false, false };
            ajustVisual.DrawWave(leadToggle);
        }

        private void radioButtonLeadV5_Click(object sender, RoutedEventArgs e)
        {
            bool[] leadToggle = new bool[12] { false, false, false, false, false, false, false, false, false, false, true, false };
            ajustVisual.DrawWave(leadToggle);
        }

        private void radioButtonLeadV6_Click(object sender, RoutedEventArgs e)
        {
            bool[] leadToggle = new bool[12] { false, false, false, false, false, false, false, false, false, false, false, true };
            ajustVisual.DrawWave(leadToggle);
        }

        private void radioButtonLeadAll_Click(object sender, RoutedEventArgs e)
        {
            bool[] leadToggle = new bool[12] { true, true, true, true, true, true, true, true, true, true, true, true };
            ajustVisual.DrawWave(leadToggle);
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            
            this.Close();
        }

        /// <summary>
        /// 鼠标拖动处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void canvasWave_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point mousePos = Mouse.GetPosition(this.canvasWave);
            if (this.pointMoveable[0])
            {
                if (mousePos.X<=60 || mousePos.X >= this.stackPanelPEnd.TranslatePoint(new Point(0, 0), this.canvasWave).X - 10)
                    return;
                Canvas.SetLeft(this.stackPanelPStart, mousePos.X);
            }
            if (this.pointMoveable[1])
            {
                if (mousePos.X <= this.stackPanelPStart.TranslatePoint(new Point(0,0),this.canvasWave).X+10 || mousePos.X >= this.stackPanelQStart.TranslatePoint(new Point(0, 0), this.canvasWave).X - 10)
                    return;
                Canvas.SetLeft(this.stackPanelPEnd, mousePos.X);
            }
            if (this.pointMoveable[2])
            {
                if (mousePos.X <= this.stackPanelPEnd.TranslatePoint(new Point(0, 0), this.canvasWave).X + 10 || mousePos.X >= this.stackPanelSEnd.TranslatePoint(new Point(0, 0), this.canvasWave).X - 10)
                    return;
                Canvas.SetLeft(this.stackPanelQStart, mousePos.X);
            }
            if (this.pointMoveable[3])
            {
                if (mousePos.X <= this.stackPanelQStart.TranslatePoint(new Point(0, 0), this.canvasWave).X + 10 || mousePos.X >= this.stackPanelTStart.TranslatePoint(new Point(0, 0), this.canvasWave).X - 10)
                    return;
                Canvas.SetLeft(this.stackPanelSEnd, mousePos.X);
            }
            if (this.pointMoveable[4])
            {
                if (mousePos.X <= this.stackPanelSEnd.TranslatePoint(new Point(0, 0), this.canvasWave).X + 10 || mousePos.X >= this.stackPanelTEnd.TranslatePoint(new Point(0, 0), this.canvasWave).X - 10)
                    return;
                Canvas.SetLeft(this.stackPanelTStart, mousePos.X);
            }
            if (this.pointMoveable[5])
            {
                if (mousePos.X <= this.stackPanelTStart.TranslatePoint(new Point(0, 0), this.canvasWave).X + 10 || mousePos.X >= this.canvasWave.ActualWidth - 10)
                    return;
                Canvas.SetLeft(this.stackPanelTEnd, mousePos.X);
            }
        }

        private void clearPointMoveable(bool[] pointMoveable)
        {
            for (int i = 0; i < pointMoveable.Length; i++)
            {
                pointMoveable[i] = false;
            }
        }

        //private void canvasWave_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.Source is StackPanel)
        //    {
        //        this.Cursor = Cursors.Hand;
        //        this.pointMoveable[0] = true;
        //    }
        //}

        private void canvasWave_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
            this.clearPointMoveable(this.pointMoveable);
        }

        //P波开始调整线
        private void stackPanelPStart_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = Cursors.Hand;
            this.pointMoveable[0] = true;
        }

        //P波结束调整线
        private void stackPanelPEnd_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = Cursors.Hand;
            this.pointMoveable[1] = true;
        }

        //T波结束调整线
        private void stackPanelTEnd_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = Cursors.Hand;
            this.pointMoveable[5] = true;
        }

        //T波开始调整线
        private void stackPanelTStart_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = Cursors.Hand;
            this.pointMoveable[4] = true;
        }

        //S波结束调整线
        private void stackPanelSEnd_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = Cursors.Hand;
            this.pointMoveable[3] = true;
        }

        //Q波开始调整线
        private void stackPanelQStart_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = Cursors.Hand;
            this.pointMoveable[2] = true;
        }
    }
}
