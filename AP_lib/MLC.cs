using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace AP_lib
{
    public static class MLC_misc
    {
        public static double[] test_open_leaves_range_inMM(Beam bm3)
        {
            //var bmName = bm3.Name;
            //var CollimatorAngle = bm3.ControlPoints.First().CollimatorAngle;
            //var GantryAngle = bm3.ControlPoints.First().GantryAngle;
            //var GantryAngle2 = bm3.ControlPoints[2].GantryAngle;
            //var JawPositions = bm3.ControlPoints[2].JawPositions;
            //var JawPositions80 = bm3.ControlPoints[80].JawPositions;

            int n_leaves = bm3.ControlPoints[0].LeafPositions.GetLength(1);
            var dists = new double[n_leaves];
            var x1min = new double[n_leaves];
            var x2max = new double[n_leaves];

            for (int i = 0; i < bm3.ControlPoints.Count; i++)
            {
                ControlPoint cp = bm3.ControlPoints[i];

                for (int j = 0; j < n_leaves; j++)
                {
                    double x1_x2 = cp.LeafPositions[1, j] - cp.LeafPositions[0, j];

                    if (x1_x2 > dists[j]) dists[j] = x1_x2;

                    //if (cp.LeafPositions[0, j] < x1min[j]) x1min[j] = cp.LeafPositions[0, j];
                    //if (cp.LeafPositions[1, j] > x2max[j]) x2max[j] = cp.LeafPositions[1, j];
                }
            }
            //double x1min_min = x1min.Min();
            //double x2max_max = x2max.Max();

            var ylimits = find_Y_limits(dists);
            return ylimits;
        }



        public static double[] find_Y_limits(double[] leaf_pair_dists)
        {
            double[] rv = new double[2];

            double zero_cutoff = 0.000001;

            if (leaf_pair_dists.Length != 60) throw new Exception("double[] leaf_pair_dists has to be double[60]; This is the only MLC geometry we know.");

            int i = leaf_pair_dists.ToList().FindIndex(t => t > zero_cutoff);

            rv[0] = map_i_lower(i);

            int j = leaf_pair_dists.ToList().FindLastIndex(t => t > zero_cutoff);

            rv[1] = map_i_upper(j);

            return rv;
        }


        static double map_i_lower(int i)
        {
            if (i < 0 || i > 59) throw new Exception("index of MLC leaf has to be between 0 and 59");

            double X1;
            if (i < 10) X1 = -20 + i;
            else if (i < 50) X1 = -10 + 0.5 * (i - 10);
            else X1 = 10 + (i - 50);

            return X1 * 10; // unit mm
        }

        static double map_i_upper(int i)
        {
            if (i < 0 || i > 59) throw new Exception("index of MLC leaf has to be between 0 and 59");

            double X1;
            if (i < 10) X1 = -20 + i + 1;
            else if (i < 50) X1 = -10 + 0.5 * (i - 10) + 0.5;
            else X1 = 10 + (i - 50) + 1;

            return X1 * 10; // unit mm
        }

    }
}
