using System;

public class SimpleValueNoise : SimpleTerrainProvider
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

    public override double getHeightAt(double x, double y)
    {
        x /= Scale;
        y /= Scale;

        int lowX = (int)Math.Floor(x);
        int highX = (int)Math.Ceiling(x);
        int lowY = (int)Math.Floor(y);
        int highY = (int)Math.Ceiling(y);

        // interpolate
        return interpolateSmooth(
            interpolateSmooth(heights[lowX, lowY], heights[lowX, highY], y - lowY), 
            interpolateSmooth(heights[highX, lowY], heights[highX, highY], y - lowY),
            x - lowX);
    }

    
}
