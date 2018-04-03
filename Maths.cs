using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DTest
{
    public class Angle
    {
        private double _actualValue = 0;
        private double Value
        {
            get { return _actualValue; }
            set
            {
                _actualValue = value;
                while (_actualValue < 0) _actualValue += 360;
                _actualValue %= 360;
            }
        }
        public Angle(double angle)
        {
            Value = angle;
        }


        public double GetRad()
        {
            return Value * (Math.PI / 180);
        }

        public static double GetRad(Angle angle)
        {
            return angle.GetRad();
        }

        public void SetRad(double value)
        {
            Value = value / (Math.PI / 180);
        }

        public static implicit operator Angle(double d)
        {
            return new Angle(d);
        }

        public static implicit operator double(Angle a)
        {
            return a.Value;
        }
    }

    public class Vector2
    {
        public double X;
        public double Y;
        public Vector2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double Dist(Vector2 vector)
        {
            return Vector2.Dist(this, vector);
        }

        public static double Dist(Vector2 vector1, Vector2 vector2)
        {
            double width = Math.Abs(vector1.X - vector2.X);
            double height = Math.Abs(vector1.Y - vector2.Y);
            return Math.Sqrt(width * width + height * height);
        }

        public bool Equals(Vector2 vector)
        {
            return ((X == vector.X) && (Y == vector.Y));
        }

        public override bool Equals(object vector)
        {
            if (vector.GetType() == typeof(Vector2))
                return Equals((Vector2)vector);
            return false;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public static bool operator == ( Vector2 vector, Vector2 vector2)
        {
            return vector2.Equals(vector);
        }

        public static bool operator != (Vector2 vector, Vector2 vector2)
        {
            return !vector2.Equals(vector);
        }
    }
}
