//NOTICE: © 2022 The Regents of the University of Michigan

//The Chuck Mayo Lab - https://medicine.umich.edu/dept/radonc/research/research-laboratories/physics-laboratories 

//The software is solely for non-commercial, non-clinical research and education use in support of the publication.
//It is a decision support tool and not a surrogate for professional clinical guidance and oversight.
//The software calls APIs that are owned by Varian Medical Systems, Inc. (referred to here as Varian),
//and you should be aware you will need to obtain an API license from Varian in order to be able to use those APIs.
//Extending the [No Liability] aspect of these terms, you agree that as far as the law allows,
//Varian and Michigan will not be liable to you for any damages arising out of these terms or the use or nature of the software,
//under any kind of legal claim, and by using the software, you agree to indemnify the licensor and Varian in the event that the
//licensor or Varian is joined in any lawsuit alleging injury or death or any other type of damage (e.g., intellectual property infringement)
//arising out of the dissemination or use of the software.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsLibrary2
{
    public class PointXY
    {
        public double X;
        public double Y;


        public PointXY()
        {
            X = double.NaN;
            Y = double.NaN;

        }

        public PointXY(double xin, double yin)
        {
            X = xin;
            Y = yin;
        }

        public static double PointXYInterpolate(List<PointXY> PointList, double atpoint, bool IsXAxis)
        {
            if (PointList.Count < 2) return double.NaN;

            double returnvalue = double.NaN;

            int index;

            PointXY pxy_low = new PointXY();
            PointXY pxy_high = new PointXY();

            var PL_ordered_xy = PointList.OrderBy(x => x.X).ThenByDescending(x => x.Y).ToList();
            var PL_ordered_y = PointList.OrderBy(t => t.Y).ToList();



            double X_max = PL_ordered_xy.Last().X;
            double X_min = PL_ordered_xy[0].X;

            double Y_max = PL_ordered_y.Last().Y;
            double Y_min = PL_ordered_y[0].Y;

            for (int i = 0; i < PL_ordered_xy.Count - 1; i++)
            {
                if (PL_ordered_xy[i].Y < PL_ordered_xy[i + 1].Y - 0.0000001)
                {
                    Console.WriteLine("---- DVH curve is not monotonically decreasing! Please check the DVH Curve for this structure.");
                    Console.WriteLine("---- Dose increased at ----> [" + i + "]  Dose[" + PL_ordered_xy[i].X + "] Vol[" + PL_ordered_xy[i].Y + "]  <  Dose[" + PL_ordered_xy[i + 1].X + "] Vol[" + PL_ordered_xy[i + 1].Y + "- 0.000001]");
                    //throw new Exception("DVH curve is not monotomically decreasing!\nPlease check the DVH Curve for this structure.");

                    if (IsXAxis)
                    {
                        if (PL_ordered_xy[i].X < atpoint && atpoint < PL_ordered_xy[i + 1].X)
                        {
                            return double.NaN;
                        }
                    }
                    else
                    {
                        if (PL_ordered_xy[i + 1].Y < atpoint && atpoint < PL_ordered_xy[i].Y)
                        {
                            return double.NaN;
                        }
                    }
                    //return -1.0;
                }
            }

            bool IsInBetween = false;
            if (IsXAxis)
            {
                if (atpoint < 0) throw new ArgumentOutOfRangeException("atpoint (Metric_parameter input)", atpoint, "Seems your input Dose is Negtive, which should not happen");
                if (atpoint <= X_max && atpoint >= X_min) IsInBetween = true;
                if (atpoint > X_max) returnvalue = 0.0;
                if (atpoint < X_min && Math.Abs(PL_ordered_y.Last().Y - 100) < 0.001) returnvalue = PL_ordered_y.Last().Y;
            }
            else
            {
                if (atpoint < 0) throw new ArgumentOutOfRangeException("atpoint (Metric_parameter input)", atpoint, "Seems your input Volume is Negtive, which should not happen");

                if (atpoint <= Y_max && atpoint >= Y_min) IsInBetween = true;
            }

            if (IsInBetween)
            {
                if (IsXAxis)
                {
                    pxy_high = PL_ordered_xy.Where(x => x.X >= atpoint).First();

                    if (pxy_high.X == atpoint)
                        returnvalue = pxy_high.Y;
                    else
                    {
                        pxy_low = PL_ordered_xy.Where(x => x.X <= atpoint).Last();

                        if
                            (pxy_low.X == atpoint) returnvalue = pxy_low.Y;
                        else
                            returnvalue = pxy_low.Y + (atpoint - pxy_low.X) * ((pxy_high.Y - pxy_low.Y) / (pxy_high.X - pxy_low.X));
                    }
                }
                else
                {
                    pxy_high = PL_ordered_xy.Where(x => x.Y >= atpoint).Last();

                    if (pxy_high.Y == atpoint)
                        returnvalue = pxy_high.X;
                    else
                    {
                        pxy_low = PL_ordered_xy.Where(x => x.Y <= atpoint).First();

                        if
                            (pxy_low.Y == atpoint) returnvalue = pxy_low.X;
                        else
                            returnvalue = pxy_low.X + (atpoint - pxy_low.Y) * ((pxy_high.X - pxy_low.X) / (pxy_high.Y - pxy_low.Y));
                    }

                }
            }

            return returnvalue;
        }

    }

}
