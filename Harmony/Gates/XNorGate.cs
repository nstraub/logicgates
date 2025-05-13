using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectricityButtonsPush.Harmony.Gates
{
    internal class XNorGate: Gate
    {
        public bool Evaluate(bool p, bool q)
        {
            return !(p ^ q);
        }
    }
}
