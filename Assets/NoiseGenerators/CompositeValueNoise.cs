using System;

public class CompositeValueNoise : SimpleTerrainProvider
{
    public int SizeX { get; }
    public int SizeY { get; }
    public double Scale { get; }

    private int levels;
    private SimpleValueNoise[] noises;

    public CompositeValueNoise(int sizeX, int sizeY, double scale, int levels)
    {
        this.SizeX = sizeX;
        this.SizeY = sizeY;
        this.Scale = scale;
        this.levels = levels;

        noises = new SimpleValueNoise[levels];
        for (int i = 0; i < levels; i++)
        {
            noises[i] = new SimpleValueNoise(sizeX, sizeY, scale * Math.Pow(2, -i));
        }
    }

    public override double getHeightAt(double x, double y)
    {
        double value = 0d;
        for (int i = 0; i < levels; i++)
        {
            value += noises[i].getHeightAt(x, y) * Math.Pow(2, -i);
        }
        return value;
    }
}
