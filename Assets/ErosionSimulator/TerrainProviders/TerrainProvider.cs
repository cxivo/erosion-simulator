using ErosionSimulator.Geology;

namespace ErosionSimulator
{
    public abstract class TerrainProvider
    {
        // child class must never raise an Exception, must return a value for every input
        public abstract double GetHeightAt(double x, double y);

        public double InterpolateSmooth(double a, double b, double weight)
        {
            return a + (3 * weight * weight - 2 * weight * weight * weight) * (b - a);
        }

        public double InterpolateLinear(double a, double b, double weight)
        {
            return a + weight * (b - a);
        }

        public Vector2D InterpolateLinear(Vector2D a, Vector2D b, double weight)
        {
            return a + weight * (b - a);
        }

        public Vector3D InterpolateLinear(Vector3D a, Vector3D b, double weight)
        {
            return a + weight * (b - a);
        }

        // hash function useful for noise generation (to transform coordinates into a random-looking number)
        protected int KnuthHash(int i)
        {
            return (int)(i * 2654435761 % (2L * int.MaxValue));
        }
    }
}