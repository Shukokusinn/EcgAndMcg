//---------------------------------------------------------------
//硬件设备基类，所有的硬件设备，应该从这个基类继承下去，进行扩展
//---------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ape.EcgSolu.IDevice
{
    public abstract class Device
    {
        public object LockObj = new object();
        protected Queue<short[]> internalBuffer = new Queue<short[]>();

        public virtual int SampleRate       // 采集频率
        {
            get { return 500; }
        }
        public virtual double uVpb
        {
            get { return 0.899999976158142f; }
        }

        public event EventHandler DataRecived;          //数据绘图事件

        public Queue<short[]> Read()
        {
            return internalBuffer;
        }

        public void PutData(short[] Input)
        {
            lock (LockObj)
            {
                try
                {
                    internalBuffer.Enqueue(Input);
                }
                catch (Exception ex)
                {
                    return;
                }
            }
        }

        public abstract void Start();

        public abstract void Stop();

        public void OnDataRecived()
        {
            if (this.DataRecived != null)
            {
                this.DataRecived(this, new EventArgs());
            }
        }
    }
}
