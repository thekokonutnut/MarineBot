using System;

namespace MarineBot.Helpers
{
    internal static class NumbersHelper
    {
        static public int GetRandom(int min, int max)
        {
            return (new Random().Next(min, max));
        }
    }
}