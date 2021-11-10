using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ape.EcgSolu.WorkUnit
{
    //高通滤波    
    class HighpassFilter
    {
        int fs;
        double fc;
        double a0, a1, a2, b1, b2;
        double x1, x2, y1, y2;
        double M_PI = Math.PI;

        public HighpassFilter(double Freq, int SampleRate)
        {
            if (Freq <= 0.01)
            {
                Freq = 0.01;
            }
            Init(Freq, SampleRate);
        }

        void Init(double Freq, int SampleRate)
        {
            fc = Freq;
            fs = SampleRate;
            double a, c, fsfc, tscp;
            fsfc = SampleRate / Freq;
            c = 0.5883003;
            tscp = fsfc / M_PI;
            a = 1.0 + c * fsfc + tscp * tscp;
            a0 = tscp * tscp / a;
            a1 = -2.0 * a0;
            a2 = a0;
            b1 = (2.0 - 2.0 * tscp * tscp) / a;
            b2 = (1.0 - c * fsfc + tscp * tscp) / a;
            x1 = x2 = y1 = y2 = 0;
        }

        void Init(double Freq)
        {
            Init(Freq, fs);
        }

        void Init()
        {
            Init(fc, fs);
        }

        void InitialValue(int xt)
        {
            x1 = x2 = y1 = y2 = xt;
        }

        void InitialValue(double xt)
        {
            x1 = x2 = y1 = y2 = xt;
        }

        public int Filter(int xt)
        {
            double yt;
            yt = a0 * xt + a1 * x1 + a2 * x2 - b1 * y1 - b2 * y2;
            if (fabs(yt) < 1.0e-100)
            {
                yt = 0;
            }
            x2 = x1;
            x1 = xt;
            y2 = y1;
            y1 = yt;
            return (int)(yt);
        }

        public double Filter(double xt)
        {
            double yt;
            yt = a0 * xt + a1 * x1 + a2 * x2 - b1 * y1 - b2 * y2;
            if (fabs(yt) < 1.0e-100)
            {
                yt = 0;
            }
            x2 = x1;
            x1 = xt;
            y2 = y1;
            y1 = yt;
            return yt;
        }

        double fabs(double yt)
        {
            return Math.Abs(yt);
        }
    }

    //低通滤波
    class LowpassFilter
    {
        double a1, a2, b0, b1, b2;
        double x1, x2, y1, y2;
        double fc;
        int fs;
        double M_PI = Math.PI;

        double tan(double value)
        {
            return Math.Tan(value);
        }
        double sqrt(double value)
        {
            return Math.Sqrt(value);
        }

        public LowpassFilter(double Freq, int SampleRate)
        {
            Init(Freq, SampleRate);
        }

        void Init(double Freq, int SampleRate)
        {
            fc = Freq;
            fs = SampleRate;
            double c = 1.0 / tan(M_PI * Freq / SampleRate);
            double n0 = 1, n1 = 2, n2 = 1;
            double cc = c * c, s2 = sqrt(2.0);
            double d0 = cc + s2 * c + 1;
            double d1 = -2 * (cc - 1);
            double d2 = cc - s2 * c + 1;
            b0 = n0 / d0; b1 = n1 / d0; b2 = n2 / d0; a1 = d1 / d0; a2 = d2 / d0;
            x1 = x2 = y1 = y2 = 0;
        }

        void Init(double Freq)
        {
            Init(Freq, fs);
        }

        void Init()
        {
            Init(fc, fs);
        }

        void InitialValue(int xt)
        {
            x1 = x2 = y1 = y2 = xt;
        }

        void InitialValue(double xt)
        {
            x1 = x2 = y1 = y2 = xt;
        }

        public int Filter(int xt)
        {
            double yt = b0 * xt + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;
            x2 = x1; x1 = xt; y2 = y1; y1 = yt;
            return (int)(yt);
        }

        public double Filter(double xt)
        {
            double yt = b0 * xt + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;
            x2 = x1; x1 = xt; y2 = y1; y1 = yt;
            return yt;
        }
    }

    //工频陷波
    class NotchFilter
    {
        double Q, fc;
        int fs;
        double a1, a2, b0, b1, b2;
        double x1, x2, y1, y2;
        double M_PI = Math.PI;

        double tan(double value)
        {
            return Math.Tan(value);
        }

        //q=40;
        public NotchFilter(double Freq, int SampleRate, double q)
        {
            Init(Freq, SampleRate, q);
        }

        void Init(double Freq, int SampleRate, double q)
        {
            Q = q; fc = Freq; fs = SampleRate;
            double c = 1.0 / tan(M_PI * Freq / SampleRate);
            double n0 = q * (c * c + 1);
            double n1 = -2 * q * (c * c - 1);
            double n2 = n0;
            double d0 = n0 + c;
            double d1 = n1;
            double d2 = n0 - c;
            b0 = n0 / d0; b1 = n1 / d0; b2 = n2 / d0; a1 = d1 / d0; a2 = d2 / d0;
            x1 = x2 = y1 = y2 = 0;
        }

        void Init(double Freq)
        {
            Init(Freq, fs, Q);
        }

        void Init()
        {
            Init(fc, fs, Q);
        }

        public int Filter(int xt)
        {
            double yt = b0 * xt + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;
            x2 = x1; x1 = xt; y2 = y1; y1 = yt;
            return (int)(yt);
        }

        public double Filter(double xt)
        {
            double yt = b0 * xt + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;
            x2 = x1; x1 = xt; y2 = y1; y1 = yt;
            return yt;
        }
    }
}
