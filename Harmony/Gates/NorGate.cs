namespace ElectricityButtonsPush.Harmony.Gates
{
    class NorGate : Gate
    {
        public bool Evaluate(bool p, bool q)
        {
            return !(p | q);
        }
    }
}
