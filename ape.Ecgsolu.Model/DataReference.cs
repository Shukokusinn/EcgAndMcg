using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ape.EcgSolu.Model
{
    public static class DataReference
    {
        public static class Sex
        {
            public const int Unknow = 2;
            public const int Male = 1;
            public const int Female = 0;
            public const int All = -1;
        }

        public static class Status
        {
            public const int WaitSampling = 1;
            public const int WaitDiag = 2;
            public const int Diaging = 3;
            public const int WaitCheck = 4;
            public const int Done = 5;
        }
    }
}
