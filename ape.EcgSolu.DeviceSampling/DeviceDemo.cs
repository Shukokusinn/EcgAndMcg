using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Timers;
using System.IO;
using System.Diagnostics;

namespace ape.EcgSolu.IDevice
{
    public class DeviceDemo:Device
    {
        object threadLock = new object();

        public override int SampleRate
        {
            get { return 500; }
        }
        public override double uVpb
        {
            get { return 0.899999976158142; }
        }
        
        private BackgroundWorker backWorker=new BackgroundWorker();
        private System.Timers.Timer timer=new System.Timers.Timer(200);
        short[,] dataBuffer;
        short[] output = new short[12];
        int nonius = 0;
      
        object lockObj = new object();//------读写同步
        Stopwatch watch = new Stopwatch();

        public DeviceDemo()
        {
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            dataBuffer = ReadData();          
        }

        /// <summary>
        /// 加载demo数据
        /// </summary>
        /// <returns></returns>
        short[,] ReadData()
        {
            short[,] data = new short[12, 1200];
            using(StreamReader sw=new StreamReader("ecg.csv"))
            {
                for(int i=0;i<12;i++)
                {
                    string line=sw.ReadLine();
                    string[] values=line.Split(',');                    
                    for (int j = 0; j < 1200; j++)
                    {
                        data[i, j] = short.Parse(values[j]);
                    }
                }
            }
            return data;
        }
       
        public override void Start()
        {
            //backWorker.RunWorkerAsync();
            timer.Start();
        }

        public override void Stop()
        {
            //backWorker.CancelAsync();
            timer.Stop();
        }

        private void backWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (backWorker.CancellationPending)
                {
                    return;
                }
                //Thread.Sleep(10);
                this.OnDataRecived();
            }
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //watch.Reset();
            //watch.Start();
            lock (threadLock)
            {
                for (int i = 0; i < 100; i++)
                {
                    short[] tempData = new short[12];
                    for (int j = 0; j < 12; j++)
                    {
                        tempData[j] = dataBuffer[j, nonius];
                    }
                    this.PutData (tempData);
                    nonius++;
                    if (nonius >= 1200)
                        nonius = 0;
                }
                this.OnDataRecived();
            }
            //watch.Stop();            
            //Debug.WriteLine("运行时间:" + watch.ElapsedMilliseconds);
        }
    }
}
