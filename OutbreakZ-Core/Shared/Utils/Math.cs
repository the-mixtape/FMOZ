using System;

namespace OutbreakZCore.Shared.Utils
{
    public class Math
    {
        public static int RandomInt(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value > max)
            {
                return max;
            }

            if (value < min)
            {
                return min;
            }
            
            return value;
        }
    }    
}