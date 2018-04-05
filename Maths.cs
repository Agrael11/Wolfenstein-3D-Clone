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

        public override string ToString()
        {
            return (Value).ToString();
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

    public class Rectangle
    {
        public double X;
        public double Y;
        public double Width;
        public double Height;

        public Rectangle(double X, double Y, double Width, double Height)
        {
            this.X = X;
            this.Y = Y;
            this.Width = Width;
            this.Height = Height;
        }

        public Rectangle(Vector2 Position, Vector2 Size)
        {
            X = Position.X;
            Y = Position.Y;
            Width = Size.X;
            Height = Size.Y;
        }

        public Rectangle(Vector2 Position, double Width, double Height)
        {
            X = Position.X;
            Y = Position.Y;
            this.Width = Width;
            this.Height = Height;
        }

        public Rectangle(double X, double Y, Vector2 Size)
        {
            this.X = X;
            this.Y = Y;
            Width = Size.X;
            Height = Size.Y;
        }

        public bool Intersects(Rectangle rectangle2)
        {
            return ((X < rectangle2.X + rectangle2.Width) && (X + Width > rectangle2.X) &&
                    (Y < rectangle2.Y + rectangle2.Height) && (Y + Height > rectangle2.Y));
        }
        public bool Intersects(Vector2 vector)
        {
            return ((X < vector.X) && (X + Width > vector.X) &&
                    (Y < vector.Y) && (Y + Height > vector.Y));
        }

        public static bool Intersects(Rectangle rectangle, Rectangle rectangle2)
        {
            return rectangle.Intersects(rectangle2);
        }

        public static bool Intersects(Rectangle rectangle, Vector2 vector)
        {
            return rectangle.Intersects(vector);
        }

        public static implicit operator Rectangle (System.Drawing.Rectangle rectangle)
        {
            return new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        public static implicit operator Rectangle(System.Drawing.RectangleF rectangle)
        {
            return new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        public static implicit operator System.Drawing.Rectangle(Rectangle rectangle)
        {
            return new System.Drawing.Rectangle((int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);
        }

        public static implicit operator System.Drawing.RectangleF(Rectangle rectangle)
        {
            return new System.Drawing.RectangleF((float)rectangle.X, (float)rectangle.Y, (float)rectangle.Width, (float)rectangle.Height);
        }

        public override string ToString()
        {
            return $"X: {X}; Y: {Y}; Width: {Width}; Height: {Height}";
        }
    }
}
