using ErosionSimulator.Geology;
using System;

namespace ErosionSimulator.Simulators
{
    public class FourthPipeModelSimulator : Simulator
    {
        private const double INITIAL_RAIN = 0d;  // meters
        private const double INITIAL_SEDIMENT = 0d;  // meters
        private const double RAIN_HEIGHT = 0.2d;  // meters
        private const double EVAPORATION_HEIGHT = 100d;
        private const double FRICTION = 0d; // 0.8
        private const double INERTIA = 0.75d;
        private const double DELTA_T = 0.1d;  // seconds
        private const double CELL_SIZE = 1d;  // meters
        private const double GRAVITY = 9.81d;  // meters per second squared
        private const double WATER_DENSITY = 1000d;  // kilograms per meter cubed
        private const double WATER_TERMINAL_VELOCITY = 10d;  // meters per second
        private const double SEDIMENT_CAPACITY_CONSTANT = 0.1d;
        private const double COLLISION_EROSION_CONSTANT = 0.5d;
        private const double MAXIMUM_REGOLITH_THICKNESS = 0.05d;
        private const double DEPOSIT_FRACTION = 0.25d;
        private const double EROSION_FRACTION = 0.5d;
        private const int EVAPORATION_FREQUENCY = 300;
        private const int DIFFUSION_FREQUENCY = 1;
        private const double TALUS_SLOPE = 1.5d;
        private const double SLIPPAGE_AMOUNT = 0.1d;
        private int[] deltaX = { 1, 0, -1, 0 };
        private int[] deltaY = { 0, 1, 0, -1 };

        public int SizeX { get; }
        public int SizeY { get; }
        private double[,] terrain;
        private WaterColumn[,] water;
        private double[,,] outFlow;  // m^3/s

        private System.Random random;
        private int sinceLastEvaporation = 0;
        private int sinceLastDiffusion = 0;
        private double evaporatedAmount = 0d;
        private double originalHeight;

        public FourthPipeModelSimulator(int sizeX, int sizeY, TerrainProvider terrainProvider)
        {
            this.SizeX = sizeX;
            this.SizeY = sizeY;
            this.terrain = new double[SizeX, SizeY];
            this.water = new WaterColumn[SizeX, SizeY];
            this.outFlow = new double[SizeX, SizeY, 4];
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

            originalHeight = terrain[4, SizeY / 2];
        }

        public void Evaporate()
        {
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    if (water[x, y].Height >= EVAPORATION_HEIGHT)
                    {
                        water[x, y].Height -= EVAPORATION_HEIGHT;
                        evaporatedAmount += EVAPORATION_HEIGHT;
                    }
                    else
                    {
                        evaporatedAmount += water[x, y].Height;
                        water[x, y].Height = 0d;
                        terrain[x, y] += water[x, y].Sediment;
                        water[x, y].Sediment = 0d;
                    }
                }
            }
        }

        public void Rain()
        {
            for (int i = 0; i < SizeX; i++)
            {
                for (int j = 0; j < SizeY; j++)
                {
                    water[i, j].Height += RAIN_HEIGHT;
                }
            }
        }

        private void Diffuse()
        {
            WaterColumn[,] water2 = new WaterColumn[SizeX, SizeY];

            // clone 
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    water2[x, y] = water[x, y];
                }
            }

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    // apply blur to the amount of sediment in the water column
                    double originalSediment = water[x, y].Sediment;
                    for (int i = 0; i < 4; i++)
                    {
                        if (isInBounds(x + deltaX[i], y + deltaY[i]))
                        {
                            water2[x + deltaX[i], y + deltaY[i]].Sediment += 0.2d * originalSediment;
                            water2[x, y].Sediment -= 0.2d * originalSediment;
                        }
                    }

                    //water2[x, y].Sediment += water[x, y].Sediment;
                }
            }

            this.water = water2;
        }

        private void Slippage()
        {
            for (int x = 1; x < SizeX; x++)
            {
                for (int y = 1; y < SizeY; y++)
                {
                    double slopeX = terrain[x - 1, y] - terrain[x, y];
                    if (Math.Abs(slopeX) > TALUS_SLOPE * CELL_SIZE)
                    {
                        double slipAmount = DELTA_T * (Math.Abs(slopeX) - TALUS_SLOPE * CELL_SIZE);
                        terrain[x - 1, y] -= Math.Sign(slopeX) * slipAmount;
                        terrain[x, y] += Math.Sign(slopeX) * slipAmount;
                    }

                    double slopeY = terrain[x, y - 1] - terrain[x, y];
                    if (Math.Abs(slopeY) > TALUS_SLOPE * CELL_SIZE)
                    {
                        double slipAmount = DELTA_T * (Math.Abs(slopeY) - TALUS_SLOPE * CELL_SIZE);
                        terrain[x, y - 1] -= Math.Sign(slopeY) * slipAmount;
                        terrain[x, y] += Math.Sign(slopeY) * slipAmount;
                    }
                }
            }
        }

        private void ManageRiver()
        {
            // water source
            for (int y = -80; y < 80; y++)
            {
                water[4, y + SizeY / 2].Height = originalHeight - terrain[4, y + SizeY / 2] + 4d;
                water[4, y + SizeY / 2].Sediment = 0.001d;
            }

            // water drain
            for (int y = 0; y < SizeY; y++)
            {
                water[SizeX - 1, y].Height = 0d;
                water[SizeX - 1, y].Sediment = 0d;
            }
        }

        private void WaterCycle()
        {
            if (sinceLastEvaporation >= EVAPORATION_FREQUENCY)
            {
                Evaporate();
                Rain();
                /*
                if (evaporatedAmount >= SizeX * SizeY * RAIN_HEIGHT)
                {
                    Rain();
                    evaporatedAmount -= SizeX * SizeY * RAIN_HEIGHT;
                }
                */
                sinceLastEvaporation = 1;
            }
            else
            {
                sinceLastEvaporation++;
            }

            if (sinceLastDiffusion >= DIFFUSION_FREQUENCY)
            {
                //Diffuse();
                Slippage();
                sinceLastDiffusion = 1;
            }
            else
            {
                sinceLastDiffusion++;
            }
        }

        public override void Step()
        {
            //WaterCycle();
            ManageRiver();
            //Diffuse();

            WaterColumn[,] water2 = new WaterColumn[SizeX, SizeY];

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    if (water[x, y].Height > 0d)
                    {                        
                        // velocity is the average of the inflows and outflows                        
                        Vector3D flowVelocity = new Vector3D(
                            0.5d * ((x > 1 ? (outFlow[x - 1, y, 0] - outFlow[x, y, 2]) : 0d) + (x < SizeX - 1 ? (outFlow[x, y, 0] - outFlow[x + 1, y, 2]) : 0d)),
                            0.5d * ((y > 1 ? (outFlow[x, y - 1, 1] - outFlow[x, y, 3]) : 0d) + (y < SizeY - 1 ? (outFlow[x, y, 1] - outFlow[x, y + 1, 3]) : 0d)),
                            0d);


                        // FORCE-BASED EROSION
                        // scale by the water height
                        double averageHeight = 0.5d * (water[x, y].Height + water[x, y].PreviousHeight);
                        if (averageHeight < 0.2d)
                        {
                            averageHeight = 0.2d;
                        }
                        Vector3D velocity = flowVelocity / (CELL_SIZE * averageHeight);                        

                        // how much sediment this water column should be able to carry
                        //double sedimentTransportCapacity = 0.5d * (flowVelocity.Magnitude() + previousFlowVelocity[x, y].Magnitude()) 
                        //    * SEDIMENT_CAPACITY_CONSTANT * SteepnessAt(x, y) / CELL_SIZE;
                        double sedimentTransportCapacity = velocity.Magnitude() * SEDIMENT_CAPACITY_CONSTANT * SteepnessAt(x, y);

                        if (water[x, y].Sediment <= sedimentTransportCapacity)
                        {
                            // erode
                            // how much sediment to erode
                            double sedimentHeight = (sedimentTransportCapacity - water[x, y].Sediment) * EROSION_FRACTION;

                            // erode from terrain
                            terrain[x, y] -= sedimentHeight;

                            // add a water column made entirely out of sediment to the existing column
                            // this way, the height of terrain + water will remain constant + the water gets slowed down a bit
                            //water[x, y] += new WaterColumn(sedimentHeight, sedimentHeight);
                            water[x, y].Sediment += sedimentHeight;
                        }
                        else
                        {
                            // deposit
                            // how much sediment to deposit
                            double sedimentHeight = (water[x, y].Sediment - sedimentTransportCapacity) * DEPOSIT_FRACTION;

                            // add to the terrain
                            terrain[x, y] += sedimentHeight;

                            // take sediment from the water column
                            //water[x, y] -= new WaterColumn(sedimentHeight, sedimentHeight, -water[x, y].Velocity);
                            water[x, y].Sediment -= sedimentHeight;
                        }



                        // COLLISION-BASED EROSION
                        Vector3D normalizedFlowVelocity = flowVelocity.Normalized();
                        double neighborHeight = GetHeightAt(x - normalizedFlowVelocity.x, y - normalizedFlowVelocity.y);
                        if (!double.IsNegativeInfinity(neighborHeight))
                        {
                            normalizedFlowVelocity.z = (terrain[x, y] - neighborHeight) * 0.2d;
                            normalizedFlowVelocity = normalizedFlowVelocity.Normalized();

                            water[x, y].Velocity = Vector3D.InterpolateLinear(normalizedFlowVelocity, water[x, y].Velocity, INERTIA).Normalized();

                            for (int i = 0; i < 4; i++)
                            {
                                if (isInBounds(x + deltaX[i], y + deltaY[i]))
                                {                              
                                    Vector3D terrainNormal = new Vector3D(
                                        deltaX[i] * (terrain[x, y] - terrain[x + deltaX[i], y + deltaY[i]]) / CELL_SIZE,
                                        deltaY[i] * (terrain[x, y] - terrain[x + deltaX[i], y + deltaY[i]]) / CELL_SIZE,
                                        1d).Normalized();

                                    // scalar product
                                    double force = Math.Max(0d, -(terrainNormal * water[x, y].Velocity));
                                    if (force > 0d)
                                    {
                                        double erodeableHeight = Math.Min(force * CELL_SIZE, water[x, y].Height);

                                        double flow = Math.Max(0d, water[x, y].Velocity.x * deltaX[i] + water[x, y].Velocity.y * deltaY[i]) * erodeableHeight / water[x, y].Height;

                                        double sedimentHeight = Math.Min((flow / (CELL_SIZE * CELL_SIZE)) * force * DELTA_T * COLLISION_EROSION_CONSTANT, erodeableHeight);

                                        // erode from terrain
                                        terrain[x + deltaX[i], y + deltaY[i]] -= sedimentHeight;

                                        // add a water column made entirely out of sediment to the existing column
                                        // this way, the height of terrain + water will remain constant + the water gets slowed down a bit
                                        //water[x, y] += new WaterColumn(sedimentHeight, sedimentHeight);
                                        water[x, y].Sediment += sedimentHeight;
                                    }
                                }
                            }
                        }


                        // DISSUOLUTION-BASED EROSION
                        double regolithDepth = Math.Min(Math.Max(0d, 0.025d * (water2[x, y].Height)), MAXIMUM_REGOLITH_THICKNESS);
                        //water[x, y] += new WaterColumn(regolithDepth * DELTA_T, regolithDepth * DELTA_T);


                        for (int i = 0; i < 4; i++)
                        {
                            if (isInBounds(x + deltaX[i], y + deltaY[i]))
                            {
                                // height difference between this and the neighboring column
                                double deltaHeight = terrain[x, y] - terrain[x + deltaX[i], y + deltaY[i]];

                                // move some regolith to even out the bottom
                                double regolithMovement = Math.Min(0.05d * Math.Max(0d, DELTA_T * GRAVITY * deltaHeight), regolithDepth);

                                terrain[x, y] -= Math.Abs(regolithMovement) * DELTA_T;
                                terrain[x + deltaX[i], y + deltaY[i]] += Math.Abs(regolithMovement) * DELTA_T;

                            }
                        }


                        // MOVE WATER
                        double heightHere = terrain[x, y] + water[x, y].Height;
                        double outFlowSum = 0d;
                        double[] acceleration = { 0d, 0d, 0d, 0d };
                        double frictionHeight = 0d;

                        // calculate the height differences with neighbors and flow
                        for (int i = 0; i < 4; i++)
                        {
                            if (isInBounds(x + deltaX[i], y + deltaY[i]))
                            {
                                // height difference between this and the neighboring column
                                double deltaHeight = heightHere - (terrain[x + deltaX[i], y + deltaY[i]] + water[x + deltaX[i], y + deltaY[i]].Height);
                                frictionHeight += 0.25d * Math.Min(Math.Max(0d, deltaHeight), water[x, y].Height);

                                // acceleration towards the next cell
                                acceleration[i] = GRAVITY * deltaHeight / CELL_SIZE;

                                // output flow from this cell in m^3/s
                                outFlow[x, y, i] = Math.Max(0d, outFlow[x, y, i] + (DELTA_T * acceleration[i] * CELL_SIZE * CELL_SIZE));

                                // total output flow from this cell
                                outFlowSum += DELTA_T * outFlow[x, y, i];
                            }
                        }

                        // slow down water near the edges
                        /*
                        double friction = FRICTION * (frictionHeight / water[x, y].Height);

                        outFlowSum *= (1d - friction);
                        for (int i = 0; i < 4; i++)
                        {
                            if (isInBounds(x + deltaX[i], y + deltaY[i]))
                            {
                                outFlow[x, y, i] *= (1d - friction);
                            }
                        }
                        */

                        outFlowSum *= 0.99d;
                        for (int i = 0; i < 4; i++)
                        {
                            if (isInBounds(x + deltaX[i], y + deltaY[i]))
                            {
                                outFlow[x, y, i] *= 0.99d;
                            }
                        }

                        // scale the flow, so more water than already is in the cell doesn't exit
                        double scaling = 1d;
                        if (outFlowSum > CELL_SIZE * CELL_SIZE * water[x, y].Height)
                        {
                            scaling = CELL_SIZE * CELL_SIZE * water[x, y].Height / outFlowSum;
                            outFlowSum = CELL_SIZE * CELL_SIZE * water[x, y].Height;
                        }

                        // calculate how much water to add to neighbors
                        for (int i = 0; i < 4; i++)
                        {
                            if (isInBounds(x + deltaX[i], y + deltaY[i]))
                            {
                                // scale each flow
                                outFlow[x, y, i] *= scaling;

                                // add water to the neighboring cells
                                water2[x + deltaX[i], y + deltaY[i]] += (outFlow[x, y, i] * DELTA_T / (CELL_SIZE * CELL_SIZE * water[x, y].Height)) * water[x, y];
                            }
                        }

                        // how much water remains in this cell
                        water2[x, y] += (1 - (outFlowSum / (CELL_SIZE * CELL_SIZE * water[x, y].Height))) * water[x, y];




                        // at the end
                        water2[x, y].PreviousHeight = water[x, y].Height;
                    }
                    else
                    {
                        water2[x, y] += water[x, y];
                    }
                }
            }
            water = water2;
        }

        private bool isInBounds(int x, int y)
        {
            return x >= 0 && x < SizeX && y >= 0 && y < SizeY;
        }

        private double SteepnessAt(int x, int y)
        {
            double totalSteepness = 0d;
            double totalConvexness = 0d;
            double inBounds = 0d;

            for (int i = 0; i < 4; i++)
            {
                if (isInBounds(x + deltaX[i], y + deltaY[i]))
                {
                    double deltaHeight = terrain[x, y] - terrain[x + deltaX[i], y + deltaY[i]];
                    double sideLength = Math.Sqrt(CELL_SIZE * CELL_SIZE + deltaHeight * deltaHeight);
                    totalSteepness += Math.Abs(deltaHeight) / sideLength;
                    totalConvexness += deltaHeight / sideLength;
                    inBounds += 1d;
                }
            }

            return Math.Max(0d, totalSteepness + totalConvexness) / inBounds;
            //return totalSteepness / inBounds;
        }

        public override double GetHeightAt(double x, double y)
        {
            int lowX = (int)Math.Floor(x);
            int highX = (int)Math.Ceiling(x);
            int lowY = (int)Math.Floor(y);
            int highY = (int)Math.Ceiling(y);

            if (!isInBounds(lowX, lowY) || !isInBounds(highX, highY))
            {
                return double.NegativeInfinity;
            }

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

            // if there is no water here, return average of neighboring cells
            if (result == 0d)
            {
                double neighbors = 0d;
                bool hasWater = false;

                if (isInBounds(lowX - 1, lowY - 1))
                {
                    if (water[lowX - 1, lowY - 1].Height > 0d)
                    {
                        hasWater = true;
                        result += water[lowX - 1, lowY - 1].Height + terrain[lowX - 1, lowY - 1];
                        neighbors += 1d;
                    }                    
                }

                if (isInBounds(highX + 1, lowY - 1))
                {
                    if (water[highX + 1, lowY - 1].Height > 0d)
                    {
                        hasWater = true;
                        result += water[highX + 1, lowY - 1].Height + terrain[highX + 1, lowY - 1];
                        neighbors += 1d;
                    }                 
                }

                if (isInBounds(lowX - 1, highY + 1))
                {
                    if (water[lowX - 1, highY + 1].Height > 0d)
                    {
                        hasWater = true;
                        result += water[lowX - 1, highY + 1].Height + terrain[lowX - 1, highY + 1];
                        neighbors += 1d;
                    }                   
                }

                if (isInBounds(highX + 1, highY + 1))
                {
                    if (water[highX + 1, highY + 1].Height > 0d)
                    {
                        hasWater = true;
                        result += water[highX + 1, highY + 1].Height + terrain[highX + 1, highY + 1];
                        neighbors += 1d;
                    }                  
                }

                // do our neighbors have water?
                if (hasWater)
                {
                    return result / neighbors;
                }
                else
                {
                    // no, just prevent Z-fighting with the terrain
                    return GetHeightAt(x, y) - 0.01d;
                }
            }

            return GetHeightAt(x, y) + result;
        }
    }
}
