using System;

namespace MarineBot.Helpers
{
    public static class NumbersHelper
    {
        static public int GetRandom(int min, int max)
        {
            return (new Random().Next(min, max));
        }
    }
}