using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageQueueApp.Common.Utilities
{
    public static class IdGenerator
    {
        private static readonly Random _random = new Random();

        // Generate 4 uppercase alphabets
        public static string GenerateRandomAlphabets(int length = 4)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[_random.Next(chars.Length)]);
            }
            return sb.ToString();
        }

        // Generate numeric string with given length
        public static string GenerateRandomDigits(int length)
        {
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(_random.Next(10));
            }
            return sb.ToString();
        }
    }
}
