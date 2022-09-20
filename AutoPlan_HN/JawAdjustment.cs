using AP_lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace AutoPlan_HN
{
    public static class JawAdjustment
    {
        public static void auto_adjust_jaw_position(this Beam bm3)
        {
            VRect<double> jaw = bm3.ControlPoints.First().JawPositions;

            double Y1_n = jaw.Y1;
            double Y2_n = jaw.Y2;

            var open_mlc = MLC_misc.test_open_leaves_range_inMM(bm3);

            bool adjust = false;

            if (jaw.Y1 < open_mlc[0])
            {
                Y1_n = open_mlc[0];
                adjust = true;
            }

            if (jaw.Y2 > open_mlc[1])
            {
                Y2_n = open_mlc[1];
                adjust = true;
            }

            if (adjust == true)
            {
                var bpars = bm3.GetEditableParameters();
                bpars.SetJawPositions(new VRect<double>(jaw.X1, Y1_n, jaw.X2, Y2_n));
                bm3.ApplyParameters(bpars);
            }

            string msg = $"Beam {bm3.Id} Jaw position: X1 {jaw.X1} Y1 {jaw.Y1} X2 {jaw.X2} Y2 {jaw.Y2}; MLC_Open: Y1 {open_mlc[0]} Y2 {open_mlc[1]}; \tY adjusted {adjust}";
            Console.WriteLine(msg);
            Log.logger.WriteLine(msg);
        }
    }
}
