using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace AP_lib
{
    public static class Jaw_Width
    {
        public static double find_max_to_axis_distance(List<Point3D> points, Point3D center, double mlc_rotation_angle = 0)
        {
            Point3D mlc_y_axis = new Point3D(0, 0, 1).RotCoordSys(new Vector3D(0, 1, 0), mlc_rotation_angle);

            var rv = points.Select(t =>
            {
                var xx = (t - center);
                double dist_to_rotation_axie = Vector3D.CrossProduct(xx, mlc_y_axis - new Point3D(0, 0, 0)).Length;
                return dist_to_rotation_axie;
            }).Max();

            return rv;
        }

        public static double[] find_limits_projected_to_axis(List<Point3D> points, Point3D center, double mlc_rotation_angle, Point3D axis_to_be_projected_on)
        {
            Point3D mlc_y_axis = axis_to_be_projected_on.RotCoordSys(new Vector3D(0, 1, 0), mlc_rotation_angle);

            var projections = points.Select(t =>
            {
                var xx = (t - center);
                double dist_to_rotation_axie = Vector3D.DotProduct(xx, mlc_y_axis - new Point3D(0, 0, 0));
                return dist_to_rotation_axie;
            }).ToList();

            var rv = new double[] { projections.Min(), projections.Max() };
            return rv;
        }


        public static List<Point3D> Rotate_Points(List<Point3D> points, Point3D rotcenter, Vector3D rotaxis, double rotangle)
        {
            List<Point3D> Coordinates_after_rotation = new List<Point3D>();

            points.ForEach(t =>
            {
                Vector3D t0 = t - rotcenter;
                Point3D t1 = new Point3D(t0.X, t0.Y, t0.Z);

                Point3D t2 = t1.RotCoordSys(rotaxis, rotangle);
                Coordinates_after_rotation.Add(t2);
            });

            return Coordinates_after_rotation;
        }


        public static JW_iso_angle Rotate_n_Scan_Limits(List<Point3D> points, Vector3D rotaxis, double[] rotangles, Point3D isocenter, double mlc_rotation_angle, double Margin_X_inMM, double Margin_Y_inMM)
        {
            //List<double> dists = new List<double>();
            List<double> proj_y_max = new List<double>();
            List<double> proj_y_min = new List<double>();

            List<double> proj_x_max = new List<double>();
            List<double> proj_x_min = new List<double>();

            foreach (double angle in rotangles)
            {
                List<Point3D> Coordinates_after_rotation = Rotate_Points(points, isocenter, rotaxis, angle);

                //double max_dist_1 = find_max_to_axis_distance(Coordinates_after_rotation, new Point3D(0, 0, 0), mlc_rotation_angle);
                //dists.Add(max_dist_1);

                var limits = find_limits_projected_to_axis(Coordinates_after_rotation, new Point3D(0, 0, 0), mlc_rotation_angle, new Point3D(0,0,1));
                proj_y_min.Add(limits[0]);
                proj_y_max.Add(limits[1]);

                var limits2 = find_limits_projected_to_axis(Coordinates_after_rotation, new Point3D(0, 0, 0), mlc_rotation_angle, new Point3D(-1, 0, 0));
                proj_x_min.Add(limits2[0]);
                proj_x_max.Add(limits2[1]);

                //Console.WriteLine($"Gantry Angle:  {angle.ToString("000")}\tx1: {limits2[0].ToString("N2")}\tx2: {limits2[1].ToString("N2")}\ty1: {limits[0].ToString("N2")}\ty2: {limits[1].ToString("N2")}");
            }

            //double dist_Max = dists.Max();
            double proj_x_MIN = Math.Max(proj_x_min.Min() - Margin_X_inMM, -200);
            double proj_x_MAX = Math.Min(proj_x_max.Max() + Margin_X_inMM, 200);
            double proj_y_MIN = Math.Max(proj_y_min.Min() - Margin_Y_inMM, -200);
            double proj_y_MAX = Math.Min(proj_y_max.Max() + Margin_Y_inMM, 200);

            Console.WriteLine($"Jaw Width Test Summary: mlc_angle: {mlc_rotation_angle}\t\tX1: {proj_x_MIN.ToString("N2")}\tX2: {proj_x_MAX.ToString("N2")}\tY1: {proj_y_MIN.ToString("N2")}\tY2: {proj_y_MAX.ToString("N2")} \tx width: {(proj_x_MAX- proj_x_MIN).ToString("N2")} y width: {(proj_y_MAX- proj_y_MIN).ToString("N2")}");

            return new JW_iso_angle() { mlc_rotation_angle = mlc_rotation_angle, proj_x_Min = proj_x_MIN, proj_x_Max = proj_x_MAX, proj_y_Min = proj_y_MIN, proj_y_Max = proj_y_MAX, isocenter = isocenter };
        }
    }
     
    public class JW_iso_angle
    {
        public double mlc_rotation_angle;
        //public double dist_Max;
        public double proj_y_Max;
        public double proj_x_Max;
        public double proj_y_Min;
        public double proj_x_Min;
        public Point3D isocenter;

        public double x_width { get { return proj_x_Max - proj_x_Min; } }
        public double y_width { get { return proj_y_Max - proj_y_Min; } }
    }

}
