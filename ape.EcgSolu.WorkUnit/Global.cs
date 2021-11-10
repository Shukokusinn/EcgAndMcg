using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ape.EcgSolu.WorkUnit
{
    public class Global
    {
        public static TraceSource LogTrace = new TraceSource("EcgWorkbeanch");
    }

    class DeviceList
    {
        public const string DeviceDemo = "Demo";
        public const string DeviceTcj12 = "Tcj12";
    }
}
