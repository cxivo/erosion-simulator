public abstract class SimpleTerrainProvider
{
    public abstract double GetHeightAt(double x, double y);

    public double InterpolateSmooth(double a, double b, double weight)
    {
        return a + (3 * weight * weight - 2 * weight * weight * weight) * (b - a);
    }

    public double InterpolateLinear(double a, double b, double weight)
    {
        return a + weight * (b - a);
    }
    protected int KnuthHash(int i)
    {
        return (int)(i * 2654435761 % int.MaxValue);
    }
}
