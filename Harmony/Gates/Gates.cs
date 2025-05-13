using System.Collections.Generic;

namespace ElectricityButtonsPush.Harmony.Gates
{
    internal static class Gates
    {
        private static readonly Dictionary<string, Gate> _gates = new Dictionary<string, Gate>();

        public static bool Evaluate(string name, bool p, bool q)
        {
            return GetGate(name).Evaluate(p, q);
        }

        private static Gate GetGate(string name)
        {
            if (!_gates.ContainsKey(name))
            {
                switch (name)
                {
                    case "ocbPushButton01White":
                        _gates[name] = new NandGate();
                        break;
                    case "ocbPushButton01Red":
                        _gates[name] = new NorGate();
                        break;
                    case "ocbPushButton01Green":
                        _gates[name] = new OrGate();
                        break;
                    case "ocbPushButton01Yellow":
                        _gates[name] = new AndGate();
                        break;
                    case "ocbPushButton01Blue":
                        _gates[name] = new XorGate();
                        break;
                    case "ocbPushButton01Purple":
                        _gates[name] = new XNorGate();
                        break;
                    default:
                        _gates[name] = new NandGate();
                        break;
                }
            }

            return _gates[name];
        }
    }
}
