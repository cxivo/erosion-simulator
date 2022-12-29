using System;

public class SimplePerlinNoise : SimpleTerrainProvider
{
    private int seed;

    public SimplePerlinNoise(int seed)
    {
        this.seed = seed;
    }

    public override double GetHeightAt(double x, double y)
    {
        int lowX = (int) Math.Floor(x);
        int highX = (int) Math.Ceiling(x);
        int lowY = (int) Math.Floor(y);
        int highY = (int) Math.Ceiling(y);

        // the angle will always be an integer, but it's good enough
        // the seed is added last, otherwise it would just act like an offset
        double angleLowLow = KnuthHash(seed + KnuthHash(lowX + KnuthHash(lowY)));
        double angleHighLow = KnuthHash(seed + KnuthHash(highX + KnuthHash(lowY)));
        double angleLowHigh = KnuthHash(seed + KnuthHash(lowX + KnuthHash(highY)));
        double angleHighHigh = KnuthHash(seed + KnuthHash(highX + KnuthHash(highY)));

        Vector2 thisVector = new Vector2(x, y);

        // get dot products
        double lowlow = DotProduct(new Vector2(Math.Cos(angleLowLow), Math.Sin(angleLowLow)), new Vector2(x - lowX, y - lowY));
        double lowhigh = DotProduct(new Vector2(Math.Cos(angleLowHigh), Math.Sin(angleLowHigh)), new Vector2(x - lowX, y - highY));
        double highlow = DotProduct(new Vector2(Math.Cos(angleHighLow), Math.Sin(angleHighLow)), new Vector2(x - highX, y - lowY));
        double highhigh = DotProduct(new Vector2(Math.Cos(angleHighHigh), Math.Sin(angleHighHigh)), new Vector2(x - highX, y - highY));

        // interpolate
        return InterpolateSmooth(
            InterpolateSmooth(lowlow, lowhigh, y - lowY), 
            InterpolateSmooth(highlow, highhigh, y - lowY), x - lowX);
    }

    private double DotProduct(Vector2 a, Vector2 b)
    {
        return (a.x * b.x) + (a.y * b.y);
    }
}
