using ErosionSimulator.Geology;
using System;

namespace ErosionSimulator.Simulators
{
    public class WaterMovement : Simulator
    {
        private const double INITIAL_RAIN = 0.5d;
        private const double INITIAL_SEDIMENT = 0d;
        private const double OUTGOING_WATER_MULTIPLIER = 0.25d;
        //private const double FRICTION = 0.3d;
        private const double DELTA_T = 1d;
        private const double WATER_TERMINAL_VELOCITY = 10d;
        private const double CELL_SIZE = 10d;
        private const double GRAVITY = 9.81d;
        private const double DRAG_CONSTANT = -0.05d;
        private const double DRAG_LINEAR = 0.9d;
        private const double DRAG_QUADRATIC = -0.05d;
        private const double WATER_DENSITY = 1000d;  // kilograms per meter cubed

        public int SizeX { get; }
        public int SizeY { get; }
        private double[,] terrain;
        private WaterColumn[,] water;
        private System.Random random;
        private int sinceLastRain = 0;
        private int sinceLastDiffusion = 0;

        public WaterMovement(int sizeX, int sizeY, TerrainProvider terrainProvider)
        {
            this.SizeX = sizeX;
            this.SizeY = sizeY;
            this.terrain = new double[SizeX, SizeY];
            this.water = new WaterColumn[SizeX, SizeY];
            this.random = new System.Random();

            // initialize terrain data
            for (int i = 0; i < SizeX; i++)
            {
                for (int j = 0; j < SizeY; j++)
                {
                    this.terrain[i, j] = terrainProvider.GetHeightAt(i, j);
                    this.water[i, j] = new WaterColumn(INITIAL_RAIN, INITIAL_SEDIMENT);
                }
            }
        }

        public void Rain()
        {
            for (int i = 0; i < SizeX; i++)
            {
                for (int j = 0; j < SizeY; j++)
                {
                    water[i, j].Height += INITIAL_RAIN;
                }
            }
        }

        public override void Step()
        {
            WaterColumn[,] water2 = new WaterColumn[SizeX, SizeY];

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    if (water[x, y].Height > 0d)
                    {
                        double heightHere = terrain[x, y] + water[x, y].Height;
                        double pressureHere = water[x, y].Height * GRAVITY * WATER_DENSITY;
                        double totalOutput = 0d;
                        int[] deltaX = { 1, 0, -1, 0 };
                        int[] deltaY = { 0, 1, 0, -1 };
                        double[] deltaHeight = { 0d, 0d, 0d, 0d };
                        double[] pressure = { 0d, 0d, 0d, 0d };
                        double[] plannedOutput = { 0d, 0d, 0d, 0d };  // [0, 1] fraction of the amount of water to be given to the correspnding neighbor

                        // make sure that more water than is present here doesn't exit the cell
                        double scaleAmountBy = Math.Abs(water[x, y].Velocity.x) + Math.Abs(water[x, y].Velocity.y);
                        if (scaleAmountBy == 0d)
                        {
                            scaleAmountBy = 1d;  // we will not use it, so random value
                        }

                        //double lowX = 0d, highX = 0d, lowY = 0d, highY = 0d;

                        // calculate the height differences with neighbors
                        for (int i = 0; i < 4; i++)
                        {
                            if (isInBounds(x + deltaX[i], y + deltaY[i]))
                            {
                                deltaHeight[i] = heightHere - (terrain[x + deltaX[i], y + deltaY[i]] + water[x + deltaX[i], y + deltaY[i]].Height);                               

                                // static pressure
                                pressure[i] = deltaHeight[i] * GRAVITY * WATER_DENSITY;
                                if (pressure[i] < 0d)
                                {
                                    pressure[i] = 0d;
                                }

                                plannedOutput[i] = (pressure[i] / pressureHere) * OUTGOING_WATER_MULTIPLIER;                                

                                // calculate at what velocity is the water travelling towards the neighboring cell (0 if travelling away from it)
                                // if we use only 4 neighbors, we can add velocities like this, since at most one will be non-zero
                                double speedInThisDirection = Math.Max(deltaX[i] * water[x, y].Velocity.x, 0d)
                                    + Math.Max(deltaY[i] * water[x, y].Velocity.y, 0d);

                                // dynamic pressure (cell sizes cancel out)
                                //dynamicPressure[i] = water[x + deltaX[i], y + deltaY[i]].Height * WATER_DENSITY * speedInThisDirection / DELTA_T;
                                
                                // kinetic energy??

                                // how much of the water is acutally able to reach the next cell
                                double waterAbleToGoOut = (0.5d * speedInThisDirection * speedInThisDirection / GRAVITY) + terrain[x, y] - terrain[x + deltaX[i], y + deltaY[i]];
                                if (waterAbleToGoOut < 0d)
                                {
                                    waterAbleToGoOut = 0d;
                                }
                                if (waterAbleToGoOut > water[x, y].Height / scaleAmountBy)
                                {
                                    waterAbleToGoOut = water[x, y].Height / scaleAmountBy;
                                }

                                plannedOutput[i] += Math.Min(speedInThisDirection * DELTA_T / CELL_SIZE, 1d) * waterAbleToGoOut / water[x, y].Height;

                                totalOutput += plannedOutput[i];
                            }
                        }

                        //double dynamicPressure = water[x, y].Height * WATER_DENSITY * water[x, y].Velocity.Magnitude() / DELTA_T;

                        // we decide what amount of water gets distributed to level the water out and to be moved according to the velocity vector
                        // by just comparing the pressures arising from the velocity and the static pressures
                        // likely not physically correct, hopefully good enough

                        // nah lol
                        // clamping it is
                        double multiplier = 1d;
                        double remainingWaterMultiplier = 1d - totalOutput;
                        if (totalOutput > 1)
                        {
                            multiplier = 1 / totalOutput;
                            remainingWaterMultiplier = 0d;
                        }


                        for (int i = 0; i < 4; i++)
                        {
                            if (isInBounds(x + deltaX[i], y + deltaY[i]))
                            {
                                plannedOutput[i] *= multiplier;

                                // the height difference the water will undergo is hard to get - just approximate it as the water level difference between the two columns
                                water2[x + deltaX[i], y + deltaY[i]] += plannedOutput[i] * Accelerated(water[x, y], new Vector3D(deltaX[i], deltaY[i], -deltaHeight[i]), -deltaHeight[i]);
                            }
                        }

                        water2[x, y] += remainingWaterMultiplier * water[x, y];                       
                    } 
                    else
                    {
                        water2[x, y] += water[x, y];
                    }   
                }
            }
            water = water2;
        }

        // terminal velocity of a drop of water is around 10m/s
        // I have no idea how the water is supposed to be slowed down
        // constantly? linearly? quadratically? I've seen all of these used somewhere
        private WaterColumn Accelerated(WaterColumn column, Vector3D direction, double height)
        {           
            // adds some velocity in the direction the water moved
            // acceleration in the direction of travel a = g*sin(angle)
            Vector3D velocity = column.Velocity + (GRAVITY * height / (Math.Sqrt(CELL_SIZE * CELL_SIZE + height * height))) * DELTA_T * direction.Normalized();

            // drag + friction + whatever, just slow the water down
            double speedOld = velocity.Magnitude();
            double speedNew = DRAG_QUADRATIC * speedOld * speedOld + DRAG_LINEAR * speedOld + DRAG_CONSTANT;

            // negative or terminal velocity check
            speedNew = Math.Min(WATER_TERMINAL_VELOCITY, Math.Max(speedNew, 0d));
            if (speedOld != 0)
            {
                velocity *= speedNew / speedOld;
            }

            return new WaterColumn(column.Height, column.Sediment, velocity);
        }

        private bool isInBounds(int x, int y)
        {
            return x >= 0 && x < SizeX && y >= 0 && y < SizeY;
        }

        public override double GetHeightAt(double x, double y)
        {
            int lowX = (int)Math.Floor(x);
            int highX = (int)Math.Ceiling(x);
            int lowY = (int)Math.Floor(y);
            int highY = (int)Math.Ceiling(y);

            // interpolate
            return InterpolateLinear(
                InterpolateLinear(terrain[lowX, lowY], terrain[lowX, highY], y - lowY),
                InterpolateLinear(terrain[highX, lowY], terrain[highX, highY], y - lowY),
                x - lowX);
        }

        public override double GetWaterHeightAt(double x, double y)
        {
            int lowX = (int)Math.Floor(x);
            int highX = (int)Math.Ceiling(x);
            int lowY = (int)Math.Floor(y);
            int highY = (int)Math.Ceiling(y);

            // interpolate
            double result = InterpolateLinear(
                InterpolateLinear(water[lowX, lowY].Height, water[lowX, highY].Height, y - lowY),
                InterpolateLinear(water[highX, lowY].Height, water[highX, highY].Height, y - lowY),
                x - lowX);

            // if there is no water, prevent Z-fighting with the terrain
            if (result == 0d)
            {
                result = -0.01d;
            }

            return GetHeightAt(x, y) + result;
        }
    }
}
