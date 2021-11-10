using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ape.EcgSolu.WorkUnit.Draw
{
    public class DrawerBase
    {
        public static float ValToPixel(short value, double uVpb,int PixelPerMM,int gain=10)
        {
            return (float)(value * uVpb / 1000 * PixelPerMM * gain);
        }
    }
}
