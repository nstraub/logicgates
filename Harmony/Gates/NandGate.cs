namespace ElectricityButtonsPush.Harmony.Gates
{
    internal class NandGate : Gate
    { 
        public bool Evaluate(bool abovePi, bool belowPi)
        {
            return !(abovePi && belowPi);
        }
    }
}
