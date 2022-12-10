using System;

public class CompositePerlinNoise : ITerrain
{
    public int SizeX { get; }
    public int SizeY { get; }
    public double Scale { get; }

    private int levels;
    private SimplePerlinNoise[] noises;

    public CompositePerlinNoise(int sizeX, int sizeY, double scale, int levels)
    {
        this.SizeX = sizeX;
        this.SizeY = sizeY;
        this.Scale = scale;
        this.levels = levels;

        noises = new SimplePerlinNoise[levels];
        for (int i = 0; i < levels; i++)
        {
            noises[i] = new SimplePerlinNoise(sizeX, sizeY, scale * Math.Pow(2, -i));
        }
    }

    public double getHeightAt(double x, double y)
    {
        double value = 0d;
        for (int i = 0; i < levels; i++)
        {
            value += noises[i].getHeightAt(x, y) * Math.Pow(2, -i);
        }
        return value;
    }
}
