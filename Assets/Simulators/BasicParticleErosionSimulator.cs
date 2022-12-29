using System;
using UnityEngine;

class Drop
{
    public double velocity, water, sediment;
    public Vector2 position, direction;
    public Drop(Vector2 position, double water)
    {
        this.position = position;
        this.water = water;
        velocity = 0d;
        sediment = 0d;
        direction = new Vector2(0d, 0d);
    }
}

public class BasicParticleErosionSimulator : SimpleTerrainProvider
{
    private const double INERTIA = 0.1d;
    private const double MIN_SLOPE = 0.01d;
    private const double CAPACITY_MULTIPLIER = 1d;
    private const double DEPOSIT_FRACTION = 0.1d;
    private const double EROSION_FRACTION = 0.75d;
    private const double GRAVITY = 1d;
    private const int EROSION_RADIUS = 2;
    private const int DROP_ITERATIONS = 16;

    public int SizeX { get; }
    public int SizeY { get; }
    //private ISimpleTerrainProvider terrainProvider;
    private double[,] terrain;
    private System.Random random;

    public BasicParticleErosionSimulator(int sizeX, int sizeY, SimpleTerrainProvider terrainProvider)
    {
        this.SizeX = sizeX;
        this.SizeY = sizeY;
        //this.terrainProvider = terrainProvider;
        this.terrain = new double[SizeX, SizeY];
        this.random = new System.Random();

        for (int i = 0; i < SizeX; i++)
        {
            for (int j = 0; j < SizeY; j++)
            {
                this.terrain[i, j] = terrainProvider.getHeightAt(i, j);
            }
        }
    }

    // source: https://www.firespark.de/resources/downloads/implementation%20of%20a%20methode%20for%20hydraulic%20erosion.pdf
    public void simulateStep()
    {
        double dropSize = 1d;
        Drop drop = new Drop(new Vector2(random.NextDouble() * SizeX, random.NextDouble() * SizeY), dropSize);

        for (int i = 0; i < DROP_ITERATIONS; i++)
        {
            // drop escaped the map
            if (drop.position.x < EROSION_RADIUS + 1 || drop.position.y < EROSION_RADIUS + 1 || drop.position.x >= SizeX - EROSION_RADIUS - 1 || drop.position.y >= SizeY - EROSION_RADIUS - 1)
            {
                break;
            }
        
            // find the new direction
            Vector2 directionNew = drop.direction * INERTIA - getGradient(drop.position) * (1 - INERTIA);
           
            if (directionNew.isZero())
            {
                // pick a random direction
                double angle = 2 * Math.PI * random.NextDouble();
                directionNew = new Vector2(Math.Cos(angle), Math.Sin(angle));
            }
            //directionNew.normalize();

            double m = directionNew.magnitude();
            directionNew.x /= m;
            directionNew.y /= m;

            // height difference
            double heightDifference = getHeightAt(drop.position.x + directionNew.x, drop.position.y + directionNew.y) - getHeightAt(drop.position.x, drop.position.y);

            if (heightDifference >= 0)
            {
                // drop went up -> fill up the hole behind it & deposit sediment
                double toDeposit = Math.Min(heightDifference, drop.sediment);
                if (toDeposit < 0) Debug.Log("Zaporny po A");
                depositSediment(drop.position, toDeposit);
                drop.sediment -= toDeposit;
            } else
            {
                // drop went downhill -> erode
                double capacity = Math.Max(-heightDifference, MIN_SLOPE) * drop.velocity * drop.water * CAPACITY_MULTIPLIER;
                if (capacity < 0) Debug.Log("kapacita");

                // if drop has more sediment than the calculated capacity, it deposits a percentage of the surplus
                if (drop.sediment > capacity)
                {
                    double toDeposit = (drop.sediment - capacity) * DEPOSIT_FRACTION;
                    if (toDeposit < 0) Debug.Log("Zaporny po B");
                    depositSediment(drop.position, toDeposit);
                    drop.sediment -= toDeposit;
                }

                // erode amount equal to a percentage of drop's capacity, but no more than the height difference
                double toErode = Math.Min(Math.Max(capacity - drop.sediment, 0) * EROSION_FRACTION, -heightDifference);
                if (toErode < 0) Debug.Log("Zaporny po C");

                erodeSediment(drop.position, toErode);
                drop.sediment += toErode;              
            }

            // evaporate some water
            drop.water -= dropSize / DROP_ITERATIONS;

            // update speed 
            drop.velocity = Math.Sqrt(Math.Abs(drop.velocity * drop.velocity + heightDifference * GRAVITY));


            // update drop's position
            drop.direction = directionNew;
            drop.position += directionNew;
        }
    }

    private Vector2 getGradient(Vector2 position)
    {
        int lowX = (int)Math.Floor(position.x);
        int highX = (int)Math.Ceiling(position.x);
        int lowY = (int)Math.Floor(position.y);
        int highY = (int)Math.Ceiling(position.y);
        double xModulo = position.x - lowX;
        double yModulo = position.y - lowY;

        return new Vector2(interpolateLinear(terrain[highX, lowY] - terrain[lowX, lowY], terrain[highX, highY] - terrain[lowX, highY], yModulo),
            interpolateLinear(terrain[lowX, highY] - terrain[lowX, lowY], terrain[highX, highY] - terrain[highX, lowY], xModulo));
    }

    private void depositSediment(Vector2 position, double sediment)
    {
        int lowX = (int)Math.Floor(position.x);
        int highX = (int)Math.Ceiling(position.x);
        int lowY = (int)Math.Floor(position.y);
        int highY = (int)Math.Ceiling(position.y);
        double xModulo = position.x - lowX;
        double yModulo = position.y - lowY;

        // distribute the sediment
        terrain[lowX, lowY] += sediment * (1 - xModulo) * (1 - yModulo);
        terrain[lowX, highY] += sediment * (1 - xModulo) * yModulo;
        terrain[highX, lowY] += sediment * xModulo * (1 - yModulo);
        terrain[highX, highY] += sediment * xModulo * yModulo;
    }

    private void erodeSediment(Vector2 position, double sediment)
    {
        double[,] weights = new double[2 * EROSION_RADIUS + 1, 2 * EROSION_RADIUS + 1];
        double sum = 0d;

        // calculate weights
        for (int x = 0; x < 2 * EROSION_RADIUS + 1; x++)
        {
            for (int y = 0; y < 2 * EROSION_RADIUS + 1; y++)
            {
                int cellX = (int)Math.Floor(position.x) - EROSION_RADIUS + x;
                int cellY = (int)Math.Floor(position.y) - EROSION_RADIUS + y;
                double xDifference = cellX - position.x;
                double yDifference = cellY - position.y;

                weights[x, y] = Math.Max(0, EROSION_RADIUS - Math.Sqrt(xDifference * xDifference + yDifference * yDifference));
                sum += weights[x, y];
            }
        }

        // distribute sediment
        for (int x = 0; x < 2 * EROSION_RADIUS + 1; x++)
        {
            for (int y = 0; y < 2 * EROSION_RADIUS + 1; y++)
            {
                int cellX = (int)Math.Floor(position.x) - EROSION_RADIUS + x;
                int cellY = (int)Math.Floor(position.y) - EROSION_RADIUS + y;

                terrain[cellX, cellY] -= sediment * weights[x, y] / sum;
            }
        }
    }

    public override double getHeightAt(double x, double y)
    {
        int lowX = (int)Math.Floor(x);
        int highX = (int)Math.Ceiling(x);
        int lowY = (int)Math.Floor(y);
        int highY = (int)Math.Ceiling(y);

        // interpolate
        return interpolateLinear(
            interpolateLinear(terrain[lowX, lowY], terrain[lowX, highY], y - lowY),
            interpolateLinear(terrain[highX, lowY], terrain[highX, highY], y - lowY),
            x - lowX);
    }
}
