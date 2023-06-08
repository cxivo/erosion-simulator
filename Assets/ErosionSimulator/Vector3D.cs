using System;
namespace ErosionSimulator
{
    public struct Vector3D
    {
        public static readonly Vector3D ZERO = new Vector3D(0, 0, 0);
        public static readonly Vector3D PLUS_X = new Vector3D(1, 0, 0);
        public static readonly Vector3D MINUS_X = new Vector3D(-1, 0, 0);
        public static readonly Vector3D PLUS_Y = new Vector3D(0, 1, 0);
        public static readonly Vector3D MINUS_Y = new Vector3D(0, -1, 0);
        public static readonly Vector3D PLUS_Z = new Vector3D(0, 0, 1);
        public static readonly Vector3D MINUS_Z = new Vector3D(0, 0, -1);

        private static Random random = new Random();

        public double x, y, z;

        public Vector3D(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3D randomNonzero()
        {
            double x = random.NextDouble() * (random.Next(0, 2) * 2 - 1);
            double y = random.NextDouble() * (random.Next(0, 2) * 2 - 1);
            double z = random.NextDouble() * (random.Next(0, 2) * 2 - 1);
            return new Vector3D(x, y, z);
        }

        public static Vector3D randomNormalized()
        {
            return randomNonzero().Normalized();
        }

        public static explicit operator Vector3D(Vector2D other) => new Vector3D(other.x, other.y, 0);

        public bool IsZero()
        {
            return x == 0d && y == 0d && z == 0d;
        }

        public double Magnitude()
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }

        public double SquareMagnitude()
        {
            return x * x + y * y + z * z;
        }

        public Vector3D Normalized()
        {
            double m = Magnitude();
            if (m != 0d)
            {
                return new Vector3D(x / m, y / m, z / m);
            }
            return Vector3D.ZERO;
        }

        public Vector3D NormalizedOrSmaller()
        {
            double m = Magnitude();
            if (m <= 1)
            {
                return this;
            }
            else
            {
                return new Vector3D(x / m, y / m, z / m);
            }
        }

        public static Vector3D operator -(Vector3D a)
        {
            return new Vector3D(-a.x, -a.y, -a.z);
        }

        public static Vector3D operator +(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3D operator -(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3D operator *(double a, Vector3D b)
        {
            return new Vector3D(a * b.x, a * b.y, a * b.z);
        }

        public static Vector3D operator *(Vector3D b, double a)
        {
            return new Vector3D(a * b.x, a * b.y, a * b.z);
        }

        public static Vector3D operator /(Vector3D b, double a)
        {
            return b *= 1d / a;
        }

        public static double operator *(Vector3D a, Vector3D b)
        {
            return DotProduct(a, b);
        }

        public static double DotProduct(Vector3D a, Vector3D b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public static Vector3D CrossProduct(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        }

        public static Vector3D InterpolateLinear(Vector3D a, Vector3D b, double weight)
        {
            return a + weight * (b - a);
        }
    }
}