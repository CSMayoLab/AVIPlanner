using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using VMS.TPS.Common.Model.Types;

namespace AP_lib
{
    public static class Vector_Operations
    {
        /// <summary>
        /// Round isocenter coordinates to the nearest 5mm relative to UserOrigin.
        /// </summary>
        /// <param name="isocenter"></param>
        /// <param name="userOrigin"></param>
        /// <param name="gn">granularity / precision of rounding mm</param>
        /// <returns></returns>
        public static VVector Round_up_relative_coordinate(this VVector isocenter, VVector userOrigin, int gn = 5)
        {
            VVector dist = isocenter - userOrigin;
            isocenter = new VVector(Math.Round(dist.x / gn) * gn, Math.Round(dist.y / gn) * gn, Math.Round(dist.z / gn) * gn) + userOrigin;
            return isocenter;
        }

        public static string ToString_round(this Point3D pt, int digit = 1)
        {
            var rv = $"X:{Math.Round(pt.X, digit)}; Y:{Math.Round(pt.Y, digit)}; Z:{Math.Round(pt.Z, digit)}";
            return rv;
        }

        public static Point3D RotCoordSys(this Point3D p3d0, Vector3D rotaxis, double rotangle)
        {
            Point3D returnvalue = p3d0;
            rotaxis.Normalize();
            Matrix3D matrix = new Matrix3D();
            Quaternion q = new Quaternion(rotaxis, rotangle);
            matrix.Rotate(q);
            returnvalue = matrix.Transform(returnvalue);
            return returnvalue;
        }


        public static Rect3D BoundingBox(this List<Point3D> pl)
        {
            Rect3D returnvalue = new Rect3D();

            Point3D origin = new Point3D(pl.Min(x => x.X), pl.Min(x => x.Y), pl.Min(x => x.Z));
            Size3D box = new Size3D(pl.Max(x => x.X) - origin.X, pl.Max(x => x.Y) - origin.Y, pl.Max(x => x.Z) - origin.Z);
            returnvalue = new Rect3D(origin, box);

            return returnvalue;
        }

        public static Point3D BoxCenter(this Rect3D box)
        {
            var rv = new Point3D(box.X + box.SizeX / 2, box.Y + box.SizeY / 2, box.Z + box.SizeZ / 2);
            return rv;
        }

        public static Point3D VVectorToPoint3D(this VVector v)
        {
            return new Point3D(v.x, v.y, v.z);
        }

        public static VVector Point3D_to_VVector(this Point3D p)
        {
            return new VVector(p.X, p.Y, p.Z);
        }


        public static double DistanceTo(this Point3D p3d0, Point3D p3d1)
        {
            Point3D vector = p3d0.FromTo(p3d1);

            double length = vector.Length();

            return length;
        }


        public static Point3D CrossProduct(this Point3D p3d0, Point3D p3d1)
        {
            return new Point3D(p3d0.Y * p3d1.Z - p3d0.Z * p3d1.Y, -1 * (p3d0.X * p3d1.Z - p3d0.Z * p3d1.X), p3d0.X * p3d1.Y - p3d0.Y * p3d1.X);

        }

        public static double DotProduct(this Point3D p3d0, Point3D p3d1)
        {
            return p3d0.X * p3d1.X + p3d0.Y * p3d1.Y + p3d0.Z * p3d1.Z;

        }

        public static double Length(this Point3D p3d0)
        {
            return Math.Sqrt(p3d0.X * p3d0.X + p3d0.Y * p3d0.Y + p3d0.Z * p3d0.Z);
        }

        public static Point3D Norm(this Point3D p3d0)
        {
            double l = p3d0.Length();

            return l == 0 ? new Point3D(double.NaN, double.NaN, double.NaN) : new Point3D(p3d0.X / l, p3d0.Y / l, p3d0.Z / l);

        }

        public static double Area(this Point3D p3d0, Point3D p3d1)
        {
            return p3d0.CrossProduct(p3d1).Length() / 2.0;
        }

        public static Point3D FromTo(this Point3D p3d0, Point3D p3d1)
        {

            return new Point3D(p3d1.X - p3d0.X, p3d1.Y - p3d0.Y, p3d1.Z - p3d0.Z);
        }

        public static Point3D BaryCentricCoordinates(this Point3D point, Point3D tp0, Point3D tp1, Point3D tp2)
        {

            Point3D v01 = tp0.FromTo(tp1);

            Point3D v02 = tp0.FromTo(tp2);

            Point3D v0p = tp0.FromTo(point);

            double a = v01.Area(v02);

            double v = v01.Area(v0p) / a;

            double u = v02.Area(v0p) / a;

            return new Point3D(u, v, (1 - u - v));

        }

    }
}
