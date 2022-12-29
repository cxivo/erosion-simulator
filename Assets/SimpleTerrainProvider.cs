public abstract class SimpleTerrainProvider
{
    public abstract double getHeightAt(double x, double y);
    public double interpolateSmooth(double a, double b, double weight)
    {
        return a + (3 * weight * weight - 2 * weight * weight * weight) * (b - a);
    }

    public double interpolateLinear(double a, double b, double weight)
    {
        return a + weight * (b - a);
    }
}
