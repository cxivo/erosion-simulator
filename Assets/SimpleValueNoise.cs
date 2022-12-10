using System;

public class SimpleValueNoise : ITerrain
{
    public int SizeX { get; }
    public int SizeY { get; }
    public double Scale { get; }

    private double[,] heights;

    public SimpleValueNoise(int sizeX, int sizeY, double scale)
    {
        this.SizeX = (int)Math.Ceiling(sizeX * scale);
        this.SizeY = (int)Math.Ceiling(sizeY * scale);
        this.Scale = scale;
        Random random = new Random();

        heights = new double[this.SizeX + 1, this.SizeY + 1];
        for (int i = 0; i <= this.SizeX; i++)
        {
            for (int j = 0; j <= this.SizeY; j++)
            {
                heights[i, j] = random.NextDouble();
            }
        }
    }

    public double getHeightAt(double x, double y)
    {
        x /= Scale;
        y /= Scale;

        int lowX = (int)Math.Floor(x);
        int highX = (int)Math.Ceiling(x);
        int lowY = (int)Math.Floor(y);
        int highY = (int)Math.Ceiling(y);

        // interpolate
        return interpolate(
            interpolate(heights[lowX, lowY], heights[lowX, highY], y - lowY), 
            interpolate(heights[highX, lowY], heights[highX, highY], y - lowY),
            x - lowX);
    }

    // Smoothstep
    private double interpolate(double a, double b, double weight)
    {
        return a + (3 * weight * weight - 2 * weight * weight * weight) * (b - a);
    }
}
