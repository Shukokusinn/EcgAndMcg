//-----------------------------------------------------------------
//QRS侦测，QRSDet函数
//-----------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ape.EcgSystem.Analysis
{
    public class QRSDetect
    {
        //变量定义
        public int SAMPLE_RATE = 200;
        double MS_PER_SAMPLE;       //每一个采样点所占用的时间长度，单位ms
        int MS10;
        int MS25;
        int MS30;
        int MS80;
        int MS95;
        int MS100;
        int MS125;
        int MS150;
        int MS160;
        int MS175;
        int MS195;
        int MS200;
        int MS220;
        int MS250;
        int MS300;
        int MS360;
        int MS450;
        int MS1000;         
        int MS1500;
        int DERIV_LENGTH;   //MS10
        int LPBUFFER_LGTH;  //2*MS25
        int HPBUFFER_LGTH;  //MS125
        int WINDOW_WIDTH;   //MS80
        int FILTER_DELAY;
        int DER_DELAY;
        int PRE_BLANK;
        Filter filter;

        //构造函数，初始化
        public QRSDetect(int sampleRate)
        {
            this.SAMPLE_RATE = sampleRate;
            MS_PER_SAMPLE = 1000d / SAMPLE_RATE;
            MS10 = (int)(10 / MS_PER_SAMPLE + 0.5);
            MS25 = (int)(25 / MS_PER_SAMPLE + 0.5);
            MS30 = (int)(30 / MS_PER_SAMPLE + 0.5);
            MS80 = (int)(80 / MS_PER_SAMPLE + 0.5);
            MS95 = (int)(95 / MS_PER_SAMPLE + 0.5);
            MS100 = (int)(100 / MS_PER_SAMPLE + 0.5);
            MS125 = (int)(125 / MS_PER_SAMPLE + 0.5);
            MS150 = (int)(150 / MS_PER_SAMPLE + 0.5);
            MS160 = (int)(160 / MS_PER_SAMPLE + 0.5);
            MS175 = (int)(175 / MS_PER_SAMPLE + 0.5);
            MS195 = (int)(195 / MS_PER_SAMPLE + 0.5);
            MS200 = (int)(200 / MS_PER_SAMPLE + 0.5);
            MS220 = (int)(220 / MS_PER_SAMPLE + 0.5);
            MS250 = (int)(250 / MS_PER_SAMPLE + 0.5);
            MS300 = (int)(300 / MS_PER_SAMPLE + 0.5);
            MS360 = (int)(360 / MS_PER_SAMPLE + 0.5);
            MS450 = (int)(450 / MS_PER_SAMPLE + 0.5);
            MS1000 = SAMPLE_RATE;
            MS1500= (int)(1500 / MS_PER_SAMPLE);
            DERIV_LENGTH = MS10;
            LPBUFFER_LGTH = (int)(2 * MS25);
            HPBUFFER_LGTH = MS125;
            WINDOW_WIDTH = MS80;
            PRE_BLANK = MS200;
            FILTER_DELAY = (int)(((double)DERIV_LENGTH / 2) + ((double)LPBUFFER_LGTH / 2 - 1) + (((double)HPBUFFER_LGTH - 1) / 2) + PRE_BLANK);
            DER_DELAY = WINDOW_WIDTH + FILTER_DELAY + MS100;
            sbloc = MS1500;
            sbcount = MS1500;
            DDBuffer = new int[DER_DELAY];
            this.filter = new Filter(LPBUFFER_LGTH, HPBUFFER_LGTH, DERIV_LENGTH, WINDOW_WIDTH);       
        }

        //变量定义
        int det_thresh=0, qpkcnt = 0;        //侦测阙值，qrs波峰数量
        int[] qrsbuf = new int[8];
        int[] noise = new int[8];
        int[] rrbuf = new int[8];
        int[] rsetBuff = new int[8];
        int rsetCount = 0;
        int nmedian, qmedian, rrmedian;
        int count = 0, sbpeak = 9, sbloc, sbcount;
        int maxder, lastmax;
        int initBlank, initMax;
        int preBlankCnt, tempPeak;

        //定义变量
        double TH = 0.475;
        int DDPtr;
        int[] DDBuffer;
        int Dly = 0;

        public int QRSDet(int datum, int init)
        {
            int fdatum, QrsDelay = 0;
            int newPeak, aPeak;
            //初次运行，把所有buffer初始化为0
            if (init==1)
            {
                for (int i = 0; i < 8; i++)
                {
                    noise[i] = 0;
                    rrbuf[i] = MS1000;
                }
                qpkcnt = maxder = lastmax = count = sbpeak = 0;
                initBlank = initMax = preBlankCnt = DDPtr = 0;
                sbcount = MS1500;
                this.filter.QRSFilter(0, 1);                
                this.Peak(0, 1);
            }
            fdatum = this.filter.QRSFilter(datum, 0);
            aPeak = Peak(fdatum, 0);
            // Hold any peak that is detected for 200 ms
            // in case a bigger one comes along.  There
            // can only be one QRS complex in any 200 ms window.
            newPeak = 0;
            // If there has been no peak for 200 ms,save this one and start counting.
            if (aPeak!=0 && preBlankCnt==0)
            {
                tempPeak = aPeak;
                preBlankCnt = PRE_BLANK;        //PRE_BLANK:MS200
            }
            else if (aPeak == 0 && preBlankCnt != 0)    //预测 
            {
                if (--preBlankCnt == 0)
                    newPeak = tempPeak;
            }
            else if (aPeak != 0)
            {
                if (aPeak > tempPeak)
                {
                    tempPeak = aPeak;
                    preBlankCnt = PRE_BLANK;        //MS200;
                }
                else if (--preBlankCnt == 0)
                    newPeak = tempPeak;
            }
            DDBuffer[DDPtr] = this.filter.deriv1(datum, 0);
            if (++DDPtr == DER_DELAY)
                DDPtr = 0;
            //Initialize the qrs peak buffer with the first eight local maximum peaks detected
            if (qpkcnt < 8)
            {
                #region MyRegion
                ++count;
                if (newPeak > 0)
                    count = WINDOW_WIDTH;
                if (++initBlank == MS1000)
                {
                    initBlank = 0;
                    qrsbuf[qpkcnt] = initMax;
                    initMax = 0;
                    ++qpkcnt;
                    if (qpkcnt == 8)
                    {
                        qmedian = this.median(qrsbuf, 8);//----------------求中值
                        nmedian = 0;
                        rrmedian = MS1000;
                        sbcount = MS1500 + MS150;
                        det_thresh = thresh(qmedian, nmedian);//-----------------------------------------处理
                    }
                }
                if (newPeak > initMax)
                    initMax = newPeak; 
                #endregion
            }
            else
            {
                #region MyRegion
                ++count;
                if (newPeak > 0)
                {
                    /* Check for maximum derivative and matching minima and maxima for T-wave and baseline shift rejection.  Only consider this
			        peak if it doesn't seem to be a base line shift. */
                    if (this.BLSCheck(DDBuffer, DDPtr, ref maxder) == 0)
                    {
                        //Classify the beat as a QRS complex,if the peak is larger than the detection threshold.
                        if (newPeak > det_thresh)
                        {
                            arrayLeftShift(qrsbuf, qrsbuf, 7);
                            qrsbuf[0] = newPeak;
                            qmedian = median(qrsbuf, 8);
                            det_thresh = thresh(qmedian, nmedian);
                            arrayLeftShift(rrbuf, rrbuf, 7);
                            rrbuf[0] = count - WINDOW_WIDTH;
                            rrmedian = median(rrbuf, 8);
                            sbcount = rrmedian + (rrmedian >> 1) + WINDOW_WIDTH;
                            count = WINDOW_WIDTH;
                            sbpeak = 0;
                            lastmax = maxder;
                            maxder = 0;
                            QrsDelay = WINDOW_WIDTH + FILTER_DELAY;
                            initBlank = initMax = rsetCount = 0;
                        }
                        else
                        {
                            arrayLeftShift(noise, noise, 7);
                            noise[0] = newPeak;
                            nmedian = median(noise, 8);
                            det_thresh = thresh(qmedian, nmedian);
                            if ((newPeak > sbpeak) && ((count - WINDOW_WIDTH) >= MS360))
                            {
                                sbpeak = newPeak;
                                sbloc = count - WINDOW_WIDTH;
                            }
                        }
                    }                
                }
                if ((count > sbcount) && (sbpeak > (det_thresh >> 1)))
                {
                    arrayLeftShift(qrsbuf, qrsbuf, 7);
                    qrsbuf[0] = sbpeak;
                    qmedian = median(qrsbuf, 8);
                    det_thresh = thresh(qmedian, nmedian);
                    arrayLeftShift(rrbuf, rrbuf, 7);
                    rrbuf[0] = sbloc;
                    rrmedian = median(rrbuf, 8);
                    sbcount = rrmedian + (rrmedian >> 1) + WINDOW_WIDTH;
                    QrsDelay = count - sbloc;
                    count = count - sbloc;
                    QrsDelay += FILTER_DELAY;
                    sbpeak = 0;
                    lastmax = maxder;
                    maxder = 0;
                    initBlank = initMax = rsetCount = 0;
                }
                #endregion
            }
            // In the background estimate threshold to replace adaptive threshold
            // if eight seconds elapses without a QRS detection.
            if (qpkcnt == 8)
            {
                #region MyRegion
                if (++initBlank == MS1000)
                {
                    initBlank = 0;
                    rsetBuff[rsetCount] = initMax;
                    initMax = 0;
                    ++rsetCount;
                    // Reset threshold if it has been 8 seconds without a detection.
                    if (rsetCount == 8)
                    {
                        for (int i = 0; i < 8; ++i)
                        {
                            qrsbuf[i] = rsetBuff[i];
                            noise[i] = 0;
                        }
                        qmedian = median(rsetBuff, 8);
                        nmedian = 0;
                        rrmedian = MS1000;
                        sbcount = MS1500 + MS150;
                        det_thresh = thresh(qmedian, nmedian);
                        initBlank = initMax = rsetCount = 0;
                        sbpeak = 0;
                    }
                }
                if (newPeak > initMax)
                    initMax = newPeak; 
                #endregion
            }
            return QrsDelay;
        }

        //计算峰值
        static int max = 0, timeSinceMax = 0, lastDatum = 0;
        private int Peak(int datum, int init)
        {
            int pk = 0;
            if (init == 1)
                max = timeSinceMax = 0;
            if (timeSinceMax > 0)
                ++timeSinceMax;
            if ((datum > lastDatum) && (datum > max))
            {
                max = datum;
                if (max > 2)
                    timeSinceMax = 1;
            }
            else if (datum < (max >> 1))
            {
                pk = max;
                max = 0;
                timeSinceMax = 0;
                Dly = 0;
            }
            else if (timeSinceMax > MS95)
            {
                pk = max;
                max = 0;
                timeSinceMax = 0;
                Dly = 3;
            }
            lastDatum = datum;
            return pk;
        }

        //median returns the median of an array of integers.计算中值
        private int median(int[] array, int datum)
        {
            int j,temp;
            int[] sort=new int[20];
            for (int i = 0; i < datum; ++i)
                sort[i] = array[i];
            for (int i = 0; i < datum; ++i)
            {
                temp = sort[i];
                for (j = 0; (temp < sort[j]) && (j < i); ++j);
                for (int k = i - 1; k >= j; --k)
                    sort[k + 1] = sort[k];
                sort[j] = temp;
            }
            return sort[datum]>>1;
        }

        //thresh() calculates the detection threshold from the qrs median and noise median estimates.
        //thresh函数从qrs中值和噪声估算中值来计算出界限值
        private int thresh(int qmedian, int nmedian)
        {
            int thrsh, dmed;
            double temp;
            dmed = qmedian - nmedian;
            temp = dmed;
            temp = TH * temp;
            dmed = (int)temp;
            thrsh = nmedian + dmed;
            return thrsh;
        }
        
	    //BLSCheck() reviews data to see if a baseline shift has occurred.
	    //This is done by looking for both positive and negative slopes of
	    //roughly the same magnitude in a 220 ms window.
        //通过检查正向和220ms范围内的数据的负向的斜率，来检查基线是否偏移
        private int BLSCheck(int[] dBuf, int dbPtr, ref int maxder)
        {
            //int max, min, maxt, mint, t, x;
            //max = min = 0;
            return 0;
            //for (t = 0; t < MS220; ++t)
            //{
            //}
        }

        //数组左移一个单元
        private void arrayLeftShift(int[] srcBuf, int[] destBuf, int length)
        {
            int[] tempBuf = new int[srcBuf.Length];
            for (int i = 0; i < srcBuf.Length - 1; i++)
            {
                tempBuf[i] = srcBuf[i + 1];
            }
            destBuf = tempBuf;
        }
    }
}
