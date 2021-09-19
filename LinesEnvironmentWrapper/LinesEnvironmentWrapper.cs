using RGiesecke.DllExport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LinesEnvironmentWrapper
{
    public class LinesEnvironmentWrapper
    {
        [DllExport("GetNumber", CallingConvention = CallingConvention.Cdecl)]
        public static int GetNumber() { return 22; }
    }
}
