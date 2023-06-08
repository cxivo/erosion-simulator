using System;
namespace ErosionSimulator
{
    public struct Vector2D
    {
        public static readonly Vector2D ZERO = new Vector2D(0, 0);

        public double x, y;
        public Vector2D(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public bool IsZero()
        {
            return x == 0 && y == 0;
        }

        public double Magnitude()
        {
            return Math.Sqrt(x * x + y * y);
        }

        public double SquareMagnitude()
        {
            return x * x + y * y;
        }

        public Vector2D Normalized()
        {
            double m = Magnitude();
            if (m > 0)
            {
                return new Vector2D(x / m, y / m);
            }
            return new Vector2D(0d, 0d);
        }

        public Vector2D NormalizedOrSmaller()
        {
            double m = Magnitude();
            if (m <= 1)
            {
                return this;
            }
            else
            {
                return new Vector2D(x / m, y / m);
            }
        }

        public static Vector2D operator -(Vector2D a)
        {
            return new Vector2D(-a.x, -a.y);
        }

        public static Vector2D operator +(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.x + b.x, a.y + b.y);
        }

        public static Vector2D operator -(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.x - b.x, a.y - b.y);
        }

        public static Vector2D operator *(double a, Vector2D b)
        {
            return new Vector2D(a * b.x, a * b.y);
        }

        public static Vector2D operator *(Vector2D a, double b)
        {
            return new Vector2D(a.x * b, a.y * b);
        }

        public Vector2D InterpolateLinear(Vector2D a, Vector2D b, double weight)
        {
            return a + weight * (b - a);
        }
    }
}