using System;

public class SimplePerlinNoise : SimpleTerrainProvider
{
    public int SizeX { get; }
    public int SizeY { get; }
    private Vector2[,] vectors;
    private Random random;
    private double scale;

    public SimplePerlinNoise(int sizeX, int sizeY, double scale)
    {
        this.SizeX = (int)Math.Ceiling(sizeX * scale);
        this.SizeY = (int)Math.Ceiling(sizeY * scale);
        this.vectors = new Vector2[this.SizeX + 1, this.SizeY + 1];
        this.random = new Random();
        this.scale = scale;

        for (int i = 0; i <= this.SizeX; i++)
        {
            for (int j = 0; j <= this.SizeY; j++)
            {
                // random unit vector
                double angle = 2 * Math.PI * random.NextDouble();
                vectors[i, j] = new Vector2(Math.Cos(angle), Math.Sin(angle));
            }
        }
    }

    public override double getHeightAt(double x, double y)
    {
        x /= scale;
        y /= scale;

        int lowX = (int) Math.Floor(x);
        int highX = (int) Math.Ceiling(x);
        int lowY = (int) Math.Floor(y);
        int highY = (int) Math.Ceiling(y);

        Vector2 thisVector = new Vector2(x, y);

        // get dot products
        double lowlow = dotProduct(vectors[lowX, lowY], new Vector2(x - lowX, y - lowY));
        double lowhigh = dotProduct(vectors[lowX, highY], new Vector2(x - lowX, y - highY));
        double highlow = dotProduct(vectors[highX, lowY], new Vector2(x - highX, y - lowY));
        double highhigh = dotProduct(vectors[highX, highY], new Vector2(x - highX, y - highY));

        // interpolate
        return interpolateSmooth(
            interpolateSmooth(lowlow, lowhigh, y - lowY), 
            interpolateSmooth(highlow, highhigh, y - lowY), x - lowX);
    }

    private double dotProduct(Vector2 a, Vector2 b)
    {
        return (a.x * b.x) + (a.y * b.y);
    }
}
