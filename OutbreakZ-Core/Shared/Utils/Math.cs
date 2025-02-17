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
    }    
}