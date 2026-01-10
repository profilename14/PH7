#if UNITY_EDITOR
using System;

namespace GSPAWN
{
    public enum FlexiAxis
    {
        X = 0,
        Y = 1,
        Z = 2, 

        Longest = 3,
        Shortest = 4,

        [Obsolete]
        UIMixed
    }

    public static class FlexiAxisEx
    {
        public static FlexiAxis getNextAxis(this FlexiAxis flexiAxis)
        {
            if (flexiAxis == FlexiAxis.Shortest) return FlexiAxis.X;
            return (FlexiAxis)((int)flexiAxis + 1);
        }
    }
}
#endif