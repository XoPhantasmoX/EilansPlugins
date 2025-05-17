using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EilansPlugins
{
    public enum EaseType { None, Linear }

    class Ease
    {
        public static double GetEase(EaseType easeType, double x) => x;
    }
}
