//-------------------------------------------------------
//预处理滤波
//-------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ape.EcgSystem.Analysis
{
    public class Filter
    {
        int y1 = 0, y2 = 0;
        int yHp;
        int[] dataLpFilt;
        int ptrLp = 0;
        int LpBuffer_Lgth;
        int HpBuffer_Lgth;
        int[] dataHpFilt;
        int ptrHp = 0;
        int derivLength;
        int derI = 0;
        int[] derBuff1;
        int derI2 = 0;
        int[] derBuff2;
        int sum = 0;
        int windowWidth;
        int[] dataWin;
        int ptrWin = 0;

        //初始化一些参数
        public Filter(int LpBuffer_Lgth,int HpBuffer_Lgth,int derivLength,int windowWidth)
        {
            this.dataLpFilt=new int[LpBuffer_Lgth];
            this.LpBuffer_Lgth = LpBuffer_Lgth;
            this.HpBuffer_Lgth = HpBuffer_Lgth;
            this.dataHpFilt = new int[HpBuffer_Lgth];
            this.derivLength = derivLength;
            this.derBuff1 = new int[derivLength];
            this.derBuff2 = new int[derivLength];
            this.windowWidth = windowWidth;
            this.dataWin = new int[windowWidth];
        }

        //预处理滤波
        public int QRSFilter(int datum, int init)
        {
            int fdatum;
            if (init == 1)
            {
                //初始化各滤波器
                this.hpfilt(0, 1);
                this.lpfilt(0, 1);
                this.mvwint(0, 1);
                this.deriv1(0, 1);
                this.deriv2(0, 1);
            }
            fdatum = this.lpfilt(datum, 0);
            fdatum = this.hpfilt(fdatum, 0);
            fdatum = this.deriv2(fdatum, 0);
            fdatum = Math.Abs(fdatum);
            fdatum = mvwint(fdatum, 0);
            return fdatum;
        }

        //lpfilt() implements the digital filter represented by the difference
        //equation:
        //y[n] = 2*y[n-1] - y[n-2] + x[n] - 2*x[t-24 ms] + x[t-48 ms]
        //Note that the filter delay is (LPBUFFER_LGTH/2)-1
        //低通滤波
        int lpfilt(int datum, int init)
        {
            int y0;
            int output, halfPtr;
            if (init == 1)
            {
                for (ptrLp = 0; ptrLp < LpBuffer_Lgth; ++ptrLp)
                    this.dataLpFilt[ptrLp] = 0;
                y1 = y2 = 0;
                this.ptrLp = 0;
            }
            halfPtr = ptrLp - (this.LpBuffer_Lgth / 2);
            if (halfPtr < 0)
                halfPtr += this.LpBuffer_Lgth;
            y0 = (y1 <<1)-y2+datum-(this.dataLpFilt[halfPtr]<<1)+this.dataLpFilt[ptrLp];
            y2 = y1;
            y1 = y0;
            output = y0 / ((LpBuffer_Lgth * LpBuffer_Lgth) / 4);
            this.dataLpFilt[ptrLp] = datum;
            if (++this.ptrLp == this.LpBuffer_Lgth)
                this.ptrLp = 0;
            return output;
        }

        //高通滤波器
        //hpfilt() implements the high pass filter represented by the following difference equation:
        //y[n] = y[n-1] + x[n] - x[n-128 ms]
        //z[n] = x[n-64 ms] - y[n] ;
        //Filter delay is (HPBUFFER_LGTH-1)/2
        int hpfilt(int datum, int init)
        {
            int z, halfPtr;
            if (init == 1)
            {
                for (this.ptrHp = 0; this.ptrHp < this.HpBuffer_Lgth; ++this.ptrHp)
                    this.dataHpFilt[this.ptrHp] = 0;
                this.ptrHp = 0;
                this.yHp = 0;
            }
            this.yHp += datum - this.dataHpFilt[this.ptrHp];
            halfPtr = this.ptrHp - (this.HpBuffer_Lgth / 2);
            if (halfPtr < 0)
                halfPtr += this.HpBuffer_Lgth;
            z = this.dataHpFilt[halfPtr] - (yHp / this.HpBuffer_Lgth);
            this.dataHpFilt[this.ptrHp] = datum;
            if (++this.ptrHp >= this.HpBuffer_Lgth)
                this.ptrHp = 0;
            return z;
        }

        //求导函数1
        public int deriv1(int x, int init)
        {
            int y;
            if (init != 0)
            {
                for (this.derI = 0; this.derI < this.derivLength; ++this.derI)
                    this.derBuff1[this.derI] = 0;
                this.derI = 0;
                return 0;
            }
            y = x - this.derBuff1[this.derI];
            this.derBuff1[this.derI] = x;
            if (++this.derI == this.derivLength)
                this.derI = 0;
            return y;
        }

        //求导函数2
        public int deriv2(int x, int init)
        {
            int y;
            if (init != 0)
            {
                for (this.derI2 = 0; this.derI2 < this.derivLength; ++this.derI2)
                    this.derBuff2[this.derI2] = 0;
                this.derI2 = 0;
                return 0;
            }
            y = x - this.derBuff2[this.derI2];
            if (++this.derI2 == this.derivLength)
                this.derI2 = 0;
            return y;
        }

        //mvwint() implements a moving window integrator.  Actually, mvwint() averages the signal values over the last WINDOW_WIDTH samples.
        //加窗
        int mvwint(int datum, int init)
        {
            int output;
            if (init == 1)
            {
                for (this.ptrWin = 0; this.ptrWin < this.windowWidth; ++this.ptrWin)
                    this.dataWin[this.ptrWin] = 0;
                this.sum = 0;
                this.ptrWin = 0;
            }
            this.sum += datum;
            this.sum -= this.dataWin[this.ptrWin];
            this.dataWin[this.ptrWin] = datum;
            if (++this.ptrWin == this.windowWidth)
                this.ptrWin = 0;
            if ((this.sum / this.windowWidth) > 32000)
                output = 32000;
            else
                output = this.sum / this.windowWidth;
            return output;
        }

    }
}
