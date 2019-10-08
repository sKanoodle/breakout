using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectInput;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Breakout
{
    static class Utils
    {
        public static Vector2? GetIntersectionPoint(Line line1, Line line2)
        {
            float distance;
            return GetIntersectionPoint(line1, line2, out distance);
        }

        public static Vector2? GetIntersectionPoint(Line line, LineSegment lineSegment)
        {
            float distance;
            return GetIntersectionPoint(line, lineSegment, out distance);
        }

        public static Vector2? GetIntersectionPoint(LineSegment lineSegment1, LineSegment lineSegment2)
        {
            float distance;
            return GetIntersectionPoint(lineSegment1, lineSegment2, out distance);
        }

        public static Vector2? GetIntersectionPoint(Line line1, Line line2, out float distance)
        {
            distance = 0;

            //parallel
            if (CrossProduct(line1.Direction, line2.Direction) == 0)
                return null;

            Vector2 c = line2.Position - line1.Position;
            distance = CrossProduct(c, line2.Direction) / CrossProduct(line1.Direction, line2.Direction);
            return line1.Position + (line1.Direction * distance);
        }

        public static Vector2? GetIntersectionPoint(Line line, LineSegment lineSegment, out float distance)
        {
            Vector2? point = GetIntersectionPoint(line, lineSegment.ToLine(), out distance);

            if (point.HasValue == false)
                return null;

            if (IsPointOnLineSegment(point.Value, lineSegment))
                return point;

            return null;
        }

        public static Vector2? GetIntersectionPoint(LineSegment lineSegment1, LineSegment lineSegment2, out float distance)
        {
            Vector2? point = GetIntersectionPoint(lineSegment1.ToLine(), lineSegment2, out distance);

            if (point.HasValue == false)
                return null;

            if (IsPointOnLineSegment(point.Value, lineSegment1))
                return point;

            return null;
        }

        public static Vector2? GetIntersectionPoint(Line line, Circle circle, out Vector2? secondIntersection)
        {
            secondIntersection = null;

            float h = circle.Position.X;
            float k = circle.Position.Y;
            float r = circle.Radius;

            float x0 = line.Position.X;
            float y0 = line.Position.Y;
            float x1 = line.Position.X + line.Direction.X;
            float y1 = line.Position.Y + line.Direction.Y;

            float a = (x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0);
            float b = 2 * (x1 - x0) * (x0 - h) + 2 * (y1 - y0) * (y0 - k);
            float c = (x0 - h) * (x0 - h) + (y0 - k) * (y0 - k) - r * r;

            float discriminant = b * b - 4 * a * c;

            //miss
            if (discriminant < 0)
                return null;

            //tangent
            if (discriminant == 0)
            {
                float t = -b / (2 * a);
                return line.Position + line.Direction * t;
            }

            //full intersection
            float discriminantSqrt = (float)Math.Sqrt(discriminant);

            float t1 = (-b - discriminantSqrt) / (2 * a);
            float t2 = (-b + discriminantSqrt) / (2 * a);

            secondIntersection = line.Position + line.Direction * t2;
            return line.Position + line.Direction * t1;
        }

        public static float CrossProduct(Vector2 a, Vector2 b)
        {
            return (a.X * b.Y) - (b.X * a.Y);
        }

        public static bool IsPointOnLineSegment(Vector2 point, LineSegment lineSegment)
        {
            float ABLength = lineSegment.Length;
            float APLength = (point - lineSegment.StartPosition).Length();
            float PBLength = (lineSegment.EndPosition - point).Length();
            if (IsNearlyEqual(ABLength, APLength + PBLength, 0.00001f))
                return true;
            return false;
        }

        public static bool IsNearlyEqual(float a, float b, float epsilon)
        {
            if (a == b)
                return true;
            return IsNearlyEqual(Convert.ToDouble(a), Convert.ToDouble(b), epsilon);
        }

        public static bool IsNearlyEqual(double a, double b, float epsilon)
        {
            if (a == b)
                return true;

            double absA = Math.Abs(a);
            double absB = Math.Abs(b);
            double diff = Math.Abs(a - b);

            if (a == 0 || b == 0 || diff < double.Epsilon)
                return diff < epsilon;

            return diff / (absA + absB) < epsilon;
        }

        public static Vector2 Reflect(Vector2 vector, Vector2 reflector)
        {
            Vector2 normal = Vector2.Normalize(Orthogonalize(reflector));
            return Vector2.Reflect(vector, normal);
        }

        public static Vector2 ReflectCircular(Vector2 batIntersection, Bat bat, Ball ball)
        {
            float length = ball.Direction.Length();
            Line normalFromBatAtBallIntersection = new Line()
            {
                Position = batIntersection,
                Direction = new Vector2(0, 1),
            };
            float radius = (bat.Size.X + ball.Size.X) / 2f;
            Circle virtualBatCircle = new Circle()
            {
                Position = new Vector2((bat.Position.X - ball.Size.X) + radius, bat.Position.Y - ball.Size.Y),
                Radius = radius,
            };
            Vector2? secondIntersection;
            Vector2? directionVectorEndPoint = GetIntersectionPoint(normalFromBatAtBallIntersection, virtualBatCircle, out secondIntersection);

            if (directionVectorEndPoint.HasValue == false)
                throw new Exception();

            Vector2 result = directionVectorEndPoint.Value - virtualBatCircle.Position;
            result.Normalize();
            return result * length;
        }

        public static Vector2 Orthogonalize(Vector2 vector)
        {
            return new Vector2(-vector.Y, vector.X);
        }

        //TODO: bias towards up/down movement
        public static Vector2 RandomizeVector(Vector2 vector, float percentage)
        {
            Random random = new Random();
            float length = vector.Length();

            double angle = Math.Atan2(vector.Y, vector.X);
            double angleDistortion = Math.PI * percentage / 100;
            double newAngle = random.NextDouble(angle - angleDistortion, angle + angleDistortion);

            //returning the given vector is not not random
            if (IsAngleInDifferentQuadrant(angle, newAngle))
                return vector;

            float x = Convert.ToSingle(Math.Cos(newAngle));
            float y = Convert.ToSingle(Math.Sin(newAngle));
            return new Vector2(x, y) * length;
        }

        private static bool IsAngleInDifferentQuadrant(double angle1, double angle2)
        {
            if (Math.PI < Math.Abs(angle1) || Math.PI < Math.Abs(angle2))
                //angles are too big or too small, return without checking
                //for my use-case, a false negative does not cause an error
                //TODO: transform angles to be between Pi and -Pi
                return true;
            //angle1 %= Math.PI * 2;
            //angle2 %= Math.PI * 2;

            int quadrant = GetQuadrant(angle1);
            if (quadrant == 0)
                return true;
            if (quadrant == GetQuadrant(angle2))
                return false;
            return true;
        }

        private static int GetQuadrant(double angle)
        {
            float epsilon = 0.001f;
            double angleAbs = Math.Abs(angle);
            //is angle parallel to axes
            if (IsNearlyEqual(Math.PI, angleAbs, epsilon) || IsNearlyEqual(Math.PI / 2, angleAbs, epsilon) || IsNearlyEqual(0, angleAbs, epsilon))
                return 0;

            if (Math.PI / 2 < angle)
                return 2;
            else if (0 < angle)
                return 1;
            else if (-Math.PI / 2 < angle)
                return 4;
            else if (-Math.PI < angle)
                return 3;
            else
                throw new Exception();
        }
    }
}

//beweis für schnittpunktberechnung
//2 geraden mit vektordefinition
//r1(t1) = p1 + t1 * d1
//r2(t2) = p2 + t2 * d2
//gleiche punkte => schnittpunkt
//r1(t1) = r2(t2)
//zweiter teil der gleichungen ist also auch gleich
//p1 + t1 * d1 = p2 + t2 * d2 |-p1
//t1 * d1 = p2 + t2 * d2 - p1 |-(t2 * d2)
//p2 - p1 = t1 * d1 - t2 * d2
//punkt c eingeführt
//c = p2 - p1
//punkt c eingefügt
//c = t1 * d1 - t2 * d2
//gleichung für die x-werte nach t2 umstellen
//cx = t1 * d1x - t2 * d2x |+(t2 * d2x)
//cx + t2 * d2x = t1 * d1x |-cx
//t2 * d2x = t1 * d1x - cx |/d2x
//t2 = (t1 * d1x - cx) / d2x
//t2 in die gleichung für die y-werte
//cy = t1 * d1y - t2 * d2y
//cy = t1 * d1y - (t1 * d1x - cx) / d2x * d2y |-(t1 * d1y)
//cy - t1 * d1y = - (t1 * d1x - cx) / d2x * d2y |*(-1)
//-cy + t1 * d1y = (t1 * d1x - cx) / d2x * d2y |*d2x
//-cy * d2x + t1 * d1y * d2x = (t1 * d1x - cx) * d2y |+(cy * d2x)
//t1 * d1y * d2x = (t1 * d1x - cx) * d2y + cy * d2x
//t1 * d1y * d2x = t1 * d1x * d2y -cx * d2y + cy * d2x |-(t1 * d1x * d2y)
//t1 * d1y * d2x - t1 * d1x * d2y = -cx * d2y + cy * d2x
//t1 * (d1y * d2x - d1x * d2y) = -cx * d2y + cy * d2x |/(d1y * d2x - d1x * d2y)
//t1 = (-cx * d2y + cy * d2x) / (d1y * d2x - d1x * d2y)
//t1 = -(-cx * d2y + cy * d2x) / -(d1y * d2x - d1x * d2y)
//t1 = (-cy * d2x + cx * d2y) / (-d1y * d2x + d1x * d2y)
//gleichung aus dem programm
//t = CrossProduct(c, Direction2) / CrossProduct(Direction1, Direction2)
//t = (-cy * d2x + cx * d2y) / (-d1y * d2x + d1x * d2y)
//stimmt überein
//(-cy * d2x + cx * d2y) / (-d1y * d2x + d1x * d2y) = (-cy * d2x + cx * d2y) / (-d1y * d2x + d1x * d2y)
//stimmt
//t1 = t

//q.e.d.