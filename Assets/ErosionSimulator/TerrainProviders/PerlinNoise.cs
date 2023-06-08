using System;

namespace ErosionSimulator
{
    public class PerlinNoise : TerrainProvider
    {
        private int seed;

        public PerlinNoise(int seed)
        {
            this.seed = seed;
        }

        public override double GetHeightAt(double x, double y)
        {
            int lowX = (int)Math.Floor(x);
            int highX = (int)Math.Ceiling(x);
            int lowY = (int)Math.Floor(y);
            int highY = (int)Math.Ceiling(y);

            // the angle will always be an integer, but it's good enough
            // the seed is added last, otherwise it would just act like an offset
            double angleLowLow = KnuthHash(seed + KnuthHash(lowX + KnuthHash(lowY)));
            double angleLowHigh = KnuthHash(seed + KnuthHash(lowX + KnuthHash(highY)));
            double angleHighLow = KnuthHash(seed + KnuthHash(highX + KnuthHash(lowY)));
            double angleHighHigh = KnuthHash(seed + KnuthHash(highX + KnuthHash(highY)));

            Vector2D thisVector = new Vector2D(x, y);

            // get dot products
            double lowlow = DotProduct(new Vector2D(Math.Cos(angleLowLow), Math.Sin(angleLowLow)), new Vector2D(lowX - x, lowY - y));
            double lowhigh = DotProduct(new Vector2D(Math.Cos(angleLowHigh), Math.Sin(angleLowHigh)), new Vector2D(lowX - x, highY - y));
            double highlow = DotProduct(new Vector2D(Math.Cos(angleHighLow), Math.Sin(angleHighLow)), new Vector2D(highX - x, lowY - y));
            double highhigh = DotProduct(new Vector2D(Math.Cos(angleHighHigh), Math.Sin(angleHighHigh)), new Vector2D(highX - x, highY - y));

            // interpolate
            return InterpolateSmooth(
                InterpolateSmooth(lowlow, lowhigh, y - lowY),
                InterpolateSmooth(highlow, highhigh, y - lowY), x - lowX);
        }

        private double DotProduct(Vector2D a, Vector2D b)
        {
            return (a.x * b.x) + (a.y * b.y);
        }

    }
}