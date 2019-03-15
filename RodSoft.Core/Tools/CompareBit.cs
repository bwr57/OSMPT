namespace RodSoft.Core.Tools
{
    public class CompareBit
    {
        public static bool TestBit(int data, int bit)
        {
            return (data & bit) == bit;
        }

    }
}
