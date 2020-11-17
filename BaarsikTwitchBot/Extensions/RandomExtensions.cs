using System;

namespace BaarsikTwitchBot.Extensions
{
    public static class RandomExtensions
    {
        /// <summary>
        /// Generates normally distributed number using a Box-Muller transform
        /// </summary>
        /// <param name="random"></param>
        /// <param name="mean">Mean of the distribution</param>
        /// <param name="stdDev">Standard deviation</param>
        /// <returns></returns>
        public static double NextGaussian(this Random random, double mean = 0, double stdDev = 1)
        {
            var u1 = 1.0 - random.NextDouble();
            var u2 = 1.0 - random.NextDouble();
            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return mean + stdDev * randStdNormal;
        }
    }
}