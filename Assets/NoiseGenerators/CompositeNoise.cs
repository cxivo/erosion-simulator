using System;

public class CompositeNoise : SimpleTerrainProvider
{
    public double Scale { get; }
    private readonly int levels;
    private readonly SimpleTerrainProvider[] noises;

    public CompositeNoise(double scale, params SimpleTerrainProvider[] noises)
    {
        this.Scale = scale;
        this.noises = noises;
        this.levels = noises.Length;
    }

    public override double GetHeightAt(double x, double y)
    {
        double value = 0d;
        double power = 1d;
        for (int i = 0; i < levels; i++)
        {
            value += noises[i].GetHeightAt(Scale * (x + 0.5*power) / power, Scale * (y + 0.5*power) / power) * power;
            power *= 0.5d;
        }
        return value;
    }
}
