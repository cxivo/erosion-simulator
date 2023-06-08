namespace ErosionSimulator.Geology
{
    public struct WaterColumn
    {
        public double Height { get; set; }
        public double PreviousHeight { get; set; }
        public Vector3D Velocity { get; set; }
        public double Sediment { get; set; }

        public WaterColumn(double height, double sediment)
        {
            this.Height = height;
            this.Sediment = sediment;
            this.Velocity = Vector3D.ZERO;
            this.PreviousHeight = 0d;
        }

        public WaterColumn(double height, double sediment, Vector3D velocity)
        {
            this.Height = height;
            this.Sediment = sediment;
            this.Velocity = velocity;
            this.PreviousHeight = 0d;
        }

        // mix two water columns together, mix velocities
        public static WaterColumn operator + (WaterColumn left, WaterColumn right)
        {
            double ratio = 0d;
            if (left.Height + right.Height > 0d) {
                ratio = right.Height / (left.Height + right.Height);
            }
            return new WaterColumn(left.Height + right.Height, left.Sediment + right.Sediment, left.Velocity + ratio * (right.Velocity - left.Velocity));
        }

        // this operation can be thought of as splitting the original left column into two: right and the result, conserving mass and energy
        public static WaterColumn operator -(WaterColumn left, WaterColumn right)
        {
            double ratio = 0d;
            if (left.Height + right.Height > 0d)
            {
                ratio = right.Height / (left.Height + right.Height);
            }
            return new WaterColumn(left.Height - right.Height, left.Sediment - right.Sediment, left.Velocity + ratio * (-right.Velocity - left.Velocity));
        }

        public static WaterColumn operator * (double scalar, WaterColumn water)
        {
            return new WaterColumn(scalar * water.Height, scalar * water.Sediment, water.Velocity);
        }

        public static WaterColumn operator * (WaterColumn water, double scalar)
        {
            return new WaterColumn(scalar * water.Height, scalar * water.Sediment, water.Velocity);
        }
    }
}
