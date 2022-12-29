using System;

public class CompositePerlinNoise : SimpleTerrainProvider
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

    public override double getHeightAt(double x, double y)
    {
        double value = 0d;
        double power = 1d;
        for (int i = 0; i < levels; i++)
        {
            value += noises[i].getHeightAt(x + 0.5*power, y + 0.5*power) * power;
            power *= 0.5d;
        }
        return value;
    }
}
