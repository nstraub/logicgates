namespace ElectricityButtonsPush.Harmony.Gates
{
    internal interface Gate
    {
        bool Evaluate(bool p, bool q);
    }
}