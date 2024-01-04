using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Dawnbreaker_DKP.Web.Utilities;

namespace Dawnbreaker_DKP.Utility
{
    public static class RNG
    {
        private static readonly RandomNumberGenerator rng = new RNGCryptoServiceProvider();

        /// <summary>
        /// Returns a double in the range [0, 1)
        /// </summary>
        /// <returns></returns>
        public static double NextDouble()
        {
            // Step 1: fill an array with 8 random bytes
            var bytes = new byte[8];
            rng.GetBytes(bytes);
            // Step 2: bit-shift 11 and 53 based on double's mantissa bits
            var ul = BitConverter.ToUInt64(bytes, 0) / (1 << 11);
            return ul / (double)(1UL << 53);
        }

        public static double NextGaussian(double mu = 0, double sigma = 1)
        {
            var u1 = NextDouble();
            var u2 = NextDouble();

            var rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            var rand_normal = mu + sigma * rand_std_normal;

            return rand_normal;
        }

        public static double Next(double minimum, double maximum)
        {
            return NextDouble() * (maximum - minimum) + minimum;
        }

        public static int Next(int minimum, int maximum)
        {
            return Convert.ToInt32(NextDouble() * (maximum - minimum) + minimum);
        }

        public static bool NextBoolean()
        {
            return NextDouble() >= 0.5;
        }

        public static T Select<T>(T[] items)
        {
            var item = Next(0, items.Length-1);
            return items[item];
        }

        public static T SelectWeighted<T>(WeightedItem<T>[] items)
        {
            var total = items.Sum(x => x.Weight);
            var selected = NextDouble() * total;
            foreach (var item in items)
            {
                if (selected < item.Weight) return item.Item;
                selected -= item.Weight;
            }
            return items.Last().Item;
        }

        public static string RandomIdentifier(int numBytes)
        {
            var bytes = new byte[numBytes];
            rng.GetBytes(bytes);

            var identifier = Convert.ToBase64String(bytes, 0, bytes.Length, Base64FormattingOptions.None);
            return identifier;
        }

        public static void Shuffle<T>(List<T> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var r = RNG.Next(0, items.Count-1);
                T current = items[i];
                items[i] = items[r];
                items[r] = current;
            }
        }
    }
}
