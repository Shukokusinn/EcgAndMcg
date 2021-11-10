//-------------------------------------------------------------------
//涂工12导采集盒，有线usb转com口
//-------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
namespace ape.EcgSolu.IDevice
{
    public class DeviceTcj12 : Device
    {
        SerialPort devicePort;
        byte[] seBuffer = new byte[1024 * 100];         //缓冲区，所有串口接收过来的数据都扔进缓冲区
        int bufferReadPoint = 0;
        int bufferWritePoint = 0;
        bool dataStatus = false;
        int byteCount = 0;                  //每组数据内（24字节）的游标
        int groupCount = 0;                 //每100组数据的第多少组的游标
        byte[] dataGroupBuffer = new byte[24];
        byte[] DataTag = { 0x54, 0x4a, 0x4f, 0x52, 0x4b, 0x49, 0x4e, 0x47 };
        byte[] markstr =new byte[24];
        int imark;
        int idx;
        short[] originalData = new short[8];
        short[] outputGrpData = new short[12];
        public DeviceTcj12(string comName, int baudRate = 460800)
        {
            this.devicePort = new SerialPort(comName, baudRate) { ReceivedBytesThreshold = 2048 };
            this.devicePort.DataReceived += new SerialDataReceivedEventHandler(devicePort_DataReceived);
        }

        /// <summary>
        /// 串口接收数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void devicePort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                byte[] buffer = new byte[devicePort.ReadBufferSize];
                int byteRead = devicePort.Read(buffer, 0, buffer.Length);
                this.writeToBuffer(buffer, seBuffer, this.bufferReadPoint, ref this.bufferWritePoint, byteRead);
                this.readBuffer(this.seBuffer, ref this.bufferReadPoint, this.bufferWritePoint, ref this.dataStatus);
            }
            catch (System.InvalidOperationException cc)
            {
                System.Diagnostics.Trace.WriteLine(cc.Message);
            }
        }

        /// <summary>
        /// 把串口读取的数据缓存到数据校验处理缓存里
        /// </summary>
        /// <param name="sourceBuffer"></param>
        /// <param name="destBuffer"></param>
        /// <param name="readPoint"></param>
        /// <param name="writePoint"></param>
        /// <param name="copyLength"></param>
        private void writeToBuffer(byte[] sourceBuffer, byte[] destBuffer, int readPoint, ref int writePoint, int copyLength)
        {
            for (idx = 0; idx < copyLength; idx++)
            {
                destBuffer[writePoint] = sourceBuffer[idx];
                writePoint++;
                if (writePoint >= destBuffer.Length)
                    writePoint = 0;
                if (writePoint == readPoint)
                    return;
            }
        }

        /// <summary>
        /// 分析数据校验处理缓存，输出数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="readPoint"></param>
        /// <param name="writePoint"></param>
        /// <param name="dataStatus">true：当前是数据，false：要查找标识码“TJORKING"</param>
        /// <param name="markstr"></param>
        private void readBuffer(byte[] buffer, ref int readPoint, int writePoint, ref bool dataStatus)
        {
            while (readPoint != writePoint)
            {
                if (dataStatus)
                {
                    dataGroupBuffer[this.byteCount] = buffer[readPoint];
                    readPoint++;
                    if (readPoint == buffer.Length)
                        readPoint = 0;
                    this.byteCount++;
                    if (this.byteCount >= 12)
                    {
                        for (idx = 0; idx < 4; idx++)
                        {
                            originalData[idx] = (short)((dataGroupBuffer[idx * 3] + 0x80) << 16 | dataGroupBuffer[idx * 3 + 1] << 8 | dataGroupBuffer[idx * 3 + 2]);

                        }
                        short[] dataGroup = this.convert8To12(originalData);
                        PutData(dataGroup);
                        this.byteCount = 0;
                        this.groupCount++;
                        if (this.groupCount >= 100)
                        {
                            if (buffer[readPoint] != 0x54 && readPoint != writePoint)
                                System.Diagnostics.Trace.WriteLine("originalData = " + readPoint.ToString());
                            //Global
                            dataStatus = false;
                            this.groupCount = 0;
                        }
                    }
                }
                else//寻找标识符
                {
                    int seek = readPoint;
                    idx = 0;
                    if ((seek + 8) < writePoint)
                    {
                        for (idx = 0; idx < 8; idx++)
                        {
                            if (DataTag[idx] != buffer[seek])  //DataTag = { 0x54, 0x4a, 0x4f, 0x52, 0x4b, 0x49, 0x4e, 0x47 };
                            {
                                break;
                            }
                              
                            seek++;
                            if (seek == buffer.Length)
                            {
                                seek = 0;
                            }
                        }

                        if (idx==8)
                        {
                            for(imark=0;imark<24;imark++)
                                markstr[imark] = buffer[seek-8+imark];
                            imark = 0;
                        }
                    }

                    if (idx == 8)
                    {
                        readPoint = seek;
                        dataStatus = true;
//                        System.Diagnostics.Trace.WriteLine("readPoint = " + readPoint.ToString());
                    }
                    else
                    {
                        readPoint++;
                        if (readPoint == buffer.Length)
                            readPoint = 0;
                    }
                }
            }
            //if (internalBuffer.Count >= 200)
            //{
            //    this.OnDataRecived();
            //}
        }

        /// <summary>
        /// 寻找标识码“TJORKING”时，验证是否是标识码
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="readPoint"></param>
        /// <param name="writePoint"></param>
        /// <returns></returns>
        private bool isDataTag(byte[] buffer, ref int readPoint, int writePoint)
        {
            string dataTag = "TJORKING";
            int seek = readPoint;
            byte[] charBuffer = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                if (seek == writePoint)
                {
                    return false;
                }
                charBuffer[i] = buffer[seek];
                seek++;
                if (seek == buffer.Length)
                {
                    seek = 0;
                }
            }
            if (Encoding.ASCII.GetString(charBuffer) == dataTag)
            {
                readPoint = seek;
                //                System.Diagnostics.Trace.WriteLine("readPoint = " + readPoint.ToString());
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool isDataTagnew(byte[] buffer, ref int readPoint, int writePoint)
        {
            int seek = readPoint;
            if ((seek + 8) >= writePoint)
            {
                return false;
            }
            for (int i = 0; i < 8; i++)
            {
                if (DataTag[i] != buffer[seek])
                    return false;
                seek++;
                if (seek == buffer.Length)
                {
                    seek = 0;
                }
            }
            readPoint = seek;
            return true;
        }

        /// <summary>
        /// 分析一组数，一个采样点
        /// </summary>
        /// <param name="buffer">24字节，8×3原始数据</param>
        /// <returns>一组，12个short数据</returns>
        //private short[] analysisData(byte[] buffer)
        //{
        //    short[] originalData = new short[8];
        //    for (int i = 0; i < 8; i++)
        //    {
        //        byte[] tempBuffer=new byte[3]{buffer[i*3],buffer[i*3+1],buffer[i*3+2]};
        //       // originalData[i] = dataConvert(tempBuffer);
        //        originalData[i] = (short)((buffer[i * 3] + 0x80) << 16 | buffer[i* 3 + 1] << 8 | buffer[i * 3 + 2]);
        //    }
        //    short[] outputData = this.convert8To12(originalData);
        //    return outputData;
        //}
        private void analysisData(byte[] buffer)
        {
            for (idx = 0; idx < 8; idx++)
            {
                originalData[idx] = (short)((buffer[idx * 3] + 0x80) << 16 | buffer[idx * 3 + 1] << 8 | buffer[idx * 3 + 2]);
                if (originalData[idx] != 0)
                    System.Diagnostics.Trace.WriteLine("originalData = " + originalData[idx].ToString());
            }
            this.convert8To12(originalData);
        }

        /// <summary>
        /// 三个字节，bigendian排序， 转换成一个short值
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private short dataConvert(byte[] buffer)
        {
            if (buffer.Length != 3)
            {
                throw new Exception("dataConvert()，输入字节数组长度不等于3");
            }
            byte[] temp = new byte[4];
            temp[0] = buffer[2];
            temp[1] = buffer[1];
            temp[2] = (byte)(buffer[0] + 0x80);
            temp[3] = 0;
            return (short)(BitConverter.ToInt32(temp, 0));
        }

        /// <summary>
        /// 8转换成12导
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        /// 
        private short[] convert8To12(short[] inputData)
        {
            short[] outputGrpData = new short[12];
            outputGrpData[0] = 0;
            outputGrpData[1] = 0;
            outputGrpData[2] = 0;
            outputGrpData[3] = 0;
            outputGrpData[4] = 0;
            outputGrpData[5] = 0;
            int refValue = (inputData[7] + inputData[0]) / 3;
            outputGrpData[6] = (short)(inputData[0]);
            outputGrpData[7] = (short)(inputData[1]);
            outputGrpData[8] = (short)(inputData[2]);
            outputGrpData[9] = (short)(inputData[3]);
            outputGrpData[10] = 0;
            outputGrpData[11] = 0;
            ///// for  debug
            //outputGrpData[0] = 0;
            //outputGrpData[1] = 100;
            //outputGrpData[2] = 200;
            //outputGrpData[3] = 300;
            //outputGrpData[4] = 400;
            //outputGrpData[5] = 500;
            //outputGrpData[6] = 600;
            //outputGrpData[7] = 700;
            //outputGrpData[8] = 800;
            //outputGrpData[9] = 900;
            //outputGrpData[10] = 1000;
            //outputGrpData[11] = 1100;
            ///// for  debug
            return outputGrpData;
        }

        //private short[] convert8To12(short[] inputData)
        //{
        //    if (inputData.Length != 8)
        //    {
        //        throw new Exception("convert8To12()出错了，输入数据导联不等于8");
        //    }
        //    short[] outputGrpData = new short[12];
        //    outputData[0] = (short)(inputData[0] - inputData[7]);
        //    outputData[1] = (short)-inputData[7];
        //    outputData[2] = (short)(-inputData[0]);
        //    outputData[3] = (short)(-(outputData[0] + outputData[1]) / 2);
        //    outputData[4] = (short)(outputData[0] - outputData[1] / 2);
        //    outputData[5] = (short)(outputData[1] - outputData[0] / 2);
        //    int refValue = (inputData[7] + inputData[0]) / 3;
        //    outputData[6] = (short)(inputData[1] - refValue);
        //    outputData[7] = (short)(inputData[2] - refValue);
        //    outputData[8] = (short)(inputData[3] - refValue);
        //    outputData[9] = (short)(inputData[4] - refValue);
        //    outputData[10] = (short)(inputData[5] - refValue);
        //    outputData[11] = (short)(inputData[6] - refValue);
        //    return outputData;
        //}
        //private short[] convert8To12(short[] inputData)
        //{
        //    if (inputData.Length != 8)
        //    {
        //        throw new Exception("convert8To12()出错了，输入数据导联不等于8");
        //    }
        //    short[] outputData = new short[12];
        //    outputData[0] = (short)(inputData[7] - inputData[0]);
        //    outputData[1] = inputData[7];
        //    outputData[2] = inputData[0];
        //    outputData[3] = (short)(-inputData[7] + inputData[0] / 2);
        //    outputData[4] = (short)(inputData[7] / 2 - inputData[0]);
        //    outputData[5] = (short)((inputData[7] + inputData[0]) / 2);
        //    int refValue=(inputData[7] + inputData[0]) / 3;
        //    outputData[6] = (short)(inputData[1] - refValue);
        //    outputData[7] = (short)(inputData[2] - refValue);
        //    outputData[8] = (short)(inputData[3] - refValue);
        //    outputData[9] = (short)(inputData[4] - refValue);
        //    outputData[10] = (short)(inputData[5] - refValue);
        //    outputData[11] = (short)(inputData[6] - refValue);
        //    return outputData;
        //}

        /// <summary>
        /// 采样频率
        /// </summary>
        public override int SampleRate
        {
            get { return 2000; }
        }

        /// <summary>
        /// uVpb，每位等于多少微伏
        /// </summary>
        public override double uVpb
        {
            get { return 0.09933711282; }       //0xFFFFFF=833.3mv*2
        }

        /// <summary>
        /// 开始采样
        /// </summary>
        public override void Start()
        {
            this.devicePort.Open();
        }

        /// <summary>
        /// 停止采样
        /// </summary>
        public override void Stop()
        {
            this.devicePort.Close();
        }
    }
}
