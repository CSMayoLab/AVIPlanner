using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Accord.Math;
using Accord.Math.Decompositions;
using Accord.Statistics;
using Accord.Statistics.Distributions.Univariate;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Reflection;


namespace AnalyticsLibrary2
{

    public static class DVHMetrics
    {
        /// <summary>
        ///  return generalized Equivalent Uniform Dose, Not necessarily EQD2, depends wheather the input DVHcurve is EQD2 or not.
        /// </summary>
        public static double gEUD(DVHCurve cdvh, double n)
        {
            double returnvalue = 0;
            double factor;
            double deltavolume;

            double checkvolume = 0.0f;

            if (n <= 0) throw new ArgumentOutOfRangeException("n in gEUD() has to be positive number!");

            cdvh.DVHPointsList = cdvh.DVHPointsList.OrderByDescending(t => t.Y).ThenBy(t => t.X).ToList(); // just play safe. if use the non-default constructor of DVHCurve(...), DVHPointList will be ordered descendingly already.

            if (n != 1)
            {
                double a = 1 / n;

                double test = Math.Pow(10.0f, a);
                test = Math.Pow(test, n);
                for (int i = 0; i < cdvh.DVHPointsList.Count - 1; i++)
                {
                    deltavolume = (cdvh.DVHPointsList[i].Y - cdvh.DVHPointsList[i + 1].Y) / 100.0f;   // TTT can give an error (*/0) with non-zero Min DVH curve
                    factor = (Math.Pow(cdvh.DVHPointsList[i + 1].X, a) + Math.Pow(cdvh.DVHPointsList[i].X, a)) / 2.0f;
                    returnvalue += deltavolume * factor;
                    checkvolume += deltavolume;
                }
                returnvalue = Math.Pow(returnvalue / checkvolume, n);
            }
            else
            {
                for (int i = 0; i < cdvh.DVHPointsList.Count - 1; i++)
                {
                    deltavolume = (cdvh.DVHPointsList[i].Y - cdvh.DVHPointsList[i + 1].Y) / 100.0f;
                    factor = (cdvh.DVHPointsList[i + 1].X + cdvh.DVHPointsList[i].X) / 2.0f;
                    returnvalue += deltavolume * factor;    //??? why, this is n = 1, not the condition n = 0.
                    checkvolume += deltavolume;
                }
            }
            return returnvalue == 0 ? double.NaN : returnvalue;
        }

        // return gEUD based on EQD2
        public static double gEUD(DVHCurve cdvh, double n, int nfractions, double alphabeta)
        {
            List<PointXY> eqd2c = new List<PointXY>();
            DVHCurve eqdvhc = new DVHCurve();
            eqdvhc = EQD2Curve(cdvh, nfractions, alphabeta);//convert DVH to EQD2Gy 
            return gEUD(eqdvhc, n);
        }

        public static DVHCurve EQD2Curve(DVHCurve cdvh, int nfractions, double alphabeta)
        {
            DVHCurve returnvalue = new DVHCurve(cdvh);
            returnvalue.DVHPointsList = new List<PointXY>();

            double factor = 2.0f / alphabeta;
            foreach (PointXY pxy in cdvh.DVHPointsList) returnvalue.DVHPointsList.Add(new PointXY(pxy.X * ((1.0f + ((pxy.X / nfractions) / alphabeta)) / (1.0f + (factor))), pxy.Y));

            return returnvalue;
        }

        public static double NTCP_LQ(DVHCurve cdvh, double n, double m, double TD50, int nfractions, double alphabeta)
        {
            double returnvalue = double.NaN;
            if (TD50 > 0 && m > 0 && n >= 0 && alphabeta > 0)
            {
                double geud = gEUD(cdvh, n, nfractions, alphabeta);
                returnvalue = (1.0f + MathFunctions.erf((geud - TD50) / (m * TD50) / 1.41421356237)) / 2.0f;
            }

            return returnvalue;
        }

        public static double NTCP_LQ(DVHCurve cdvh, double n, double m, double TD50)
        {
            double returnvalue = double.NaN;
            if (TD50 > 0 && m > 0 && n >= 0)
            {
                double geud = gEUD(cdvh, n);
                returnvalue = (1.0f + MathFunctions.erf((geud - TD50) / (m * TD50) / 1.41421356237)) / 2.0f;
            }
            return returnvalue;
        }

        public static double NTCP_LQ(double geud, double m, double TD50)
        {
            double returnvalue = double.NaN;
            if (TD50 > 0 && m > 0)
            {
                returnvalue = (1.0f + MathFunctions.erf((geud - TD50) / (m * TD50) / 1.41421356237)) / 2.0f;
            }
            return returnvalue;
        }

        public static double Mean_Gy(DVHCurve cdvh)
        {
            var DVHPointsList_o = cdvh.DVHPointsList.OrderBy(p => p.Y).ThenByDescending(p => p.X);

            var doses = DVHPointsList_o.Select(p => p.X).ToArray();
            var volumes = DVHPointsList_o.Select(p => p.Y).ToArray();

            return (even_out_coef(volumes).ElementwiseMultiply(doses)).Sum();
        }

        /// <summary>
        /// Calculate dose coverage of a DVH curve, for target dose.
        /// </summary>
        /// <param name="cdvh"></param>
        /// <param name="target_dose"></param>
        /// <returns>coverage in [0,1] interval, x100 into percentage</returns>
        public static double Dose_Coverage_area(DVHCurve cdvh, double target_dose)
        {
            var DVHPointsList_o = cdvh.DVHPointsList.OrderBy(p => p.Y).ThenByDescending(p => p.X);

            var doses = DVHPointsList_o.Select(p => p.X).ToArray();
            var volumes = DVHPointsList_o.Select(p => p.Y).ToArray();

            var dose_top = new double[doses.Count()];
            for (int i = 0; i < doses.Count(); i++) { dose_top[i] = Math.Min(doses[i], target_dose); }

            if (doses[0] > target_dose && doses.Last() < target_dose)
            {
                var ind_point_just_below_limit = dose_top.ToList().LastIndexOf(target_dose);
                volumes[ind_point_just_below_limit] = DVHMetrics.DVHMetricValue(cdvh, DVHMetricType.VxGy_Percent, target_dose);
            }

            return (even_out_coef(volumes).ElementwiseMultiply(dose_top)).Sum() / target_dose;
        }

        /// <summary>
        /// Calculate dose hotspot beyond certain limit of a DVH curve, for PTV only, need the dose limit placed on 0% volume. Use over_con_area() below for more general purposes.
        /// </summary>
        /// <param name="cdvh"></param>
        /// <param name="target_dose"></param>
        /// <returns>Hotspot in [0,1] interval, x100 into percentage</returns>
        public static double Dose_Hotspot(DVHCurve cdvh, double dose_limit)
        {
            var DVHPointsList_o = cdvh.DVHPointsList.OrderBy(p => p.Y).ThenByDescending(p => p.X);

            var doses = DVHPointsList_o.Select(p => p.X).ToArray();
            var volumes = DVHPointsList_o.Select(p => p.Y).ToArray();

            var dose_top = new double[doses.Count()];
            for (int i = 0; i < doses.Count(); i++) { dose_top[i] = Math.Max(doses[i], dose_limit); }

            if (doses[0] > dose_limit && doses.Last() < dose_limit)
            {
                var ind_point_just_below_limit = dose_top.IndexOf(dose_limit);
                volumes[ind_point_just_below_limit] = DVHMetrics.DVHMetricValue(cdvh, DVHMetricType.VxGy_Percent, dose_limit);
            }

            return (even_out_coef(volumes).ElementwiseMultiply(dose_top)).Sum() / dose_limit - 1;
        }

        /// <summary>
        /// Over Constraint Area: Calculate Dose[Gy]-Volume[%] area beyond certain constraint of a DVH curve.
        /// </summary>
        /// <param name="cdvh"></param>
        /// <param name="con"></param>
        /// <returns>in unit of Gy</returns>
        public static double over_con_area(DVHCurve cdvh, constraint con)
        {
            if (con.metric_type != DVHMetricType.DxPercent_Gy.ToString() || con.metric_parameter == 100) return double.NaN;

            var DVHPointsList_o = cdvh.DVHPointsList.Where(p => p.Y >= con.metric_parameter).OrderBy(p => p.Y).ThenByDescending(p => p.X).ToList();

            if (DVHPointsList_o.First().Y != con.metric_parameter) DVHPointsList_o.Insert(0, new PointXY(DVHMetrics.DVHMetricValue(cdvh, con), con.metric_parameter));

            if (DVHPointsList_o.Count < 2) return double.NaN;

            var doses = DVHPointsList_o.Select(p => p.X).ToArray();
            var volumes = DVHPointsList_o.Select(p => p.Y).ToArray();

            var dose_over = new double[doses.Count()];
            for (int i = 0; i < doses.Count(); i++) { dose_over[i] = Math.Max(doses[i] - con.limit, 0); }

            if (doses[0] > con.limit && doses.Last() < con.limit)
            {
                var ind_point_just_below_limit = dose_over.IndexOf(0);
                volumes[ind_point_just_below_limit] = DVHMetrics.DVHMetricValue(cdvh, DVHMetricType.VxGy_Percent, con.limit);
            }

            //return (even_out_coef(volumes).ElementwiseMultiply(dose_over)).Sum();
            return (even_out_coef(volumes).ElementwiseMultiply(dose_over)).Sum() * (100 - con.metric_parameter) / 100;
        }


        public static double DVHMetricValue(DVHCurve cdvh, constraint con)
        {
            double returvalue = DVHMetricValue(cdvh, DVHMetrics.metric_type_enum(con.metric_type), con.metric_parameter);
            return returvalue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cdvh"></param>
        /// <param name="dvhmt"></param>
        /// <param name="parameter">e.g. to calculate D95%[Gy], parameter is 95, not 0.95</param>
        /// <returns></returns>
        public static double DVHMetricValue(DVHCurve cdvh, DVHMetricType dvhmt, double parameter)
        {
            PointXY zeropoint = new PointXY(0.0f, 100.0f);
            double returvalue = double.NaN;

            if (cdvh.DVHPointsList.Count == 1 && cdvh.DVHPointsList[0].X == 0 && cdvh.DVHPointsList[0].Y > 0)
                return double.NaN;
            //cdvh.DVHPointsList.Add(new PointXY(0, 0));

            switch (dvhmt)
            {
                case DVHMetricType.Volume_cc:
                    returvalue = cdvh.Volume;
                    break;

                case DVHMetricType.Mean_Gy:
                    returvalue = Mean_Gy(cdvh);
                    break;

                case DVHMetricType.DxPercent_Gy:
                    returvalue = PointXY.PointXYInterpolate(cdvh.DVHPointsList, parameter, false);
                    break;

                case DVHMetricType.DCxPercent_Gy:
                    returvalue = DVHMetricValue(cdvh, DVHMetricType.DxPercent_Gy, 100.0f - parameter); // change from DCx to Dx ???
                    break;

                case DVHMetricType.DxPercent_Percent:
                    if (cdvh.NormDose != double.NaN) returvalue = 100.0f * (PointXY.PointXYInterpolate(cdvh.DVHPointsList, parameter, false) / (cdvh.NormDose ?? double.NaN));
                    break;

                case DVHMetricType.DCxPercent_Percent:
                    if (cdvh.NormDose != double.NaN) returvalue = DVHMetricValue(cdvh, DVHMetricType.DxPercent_Percent, 100.0f - parameter);
                    break;

                case DVHMetricType.Dxcc_Gy:
                    returvalue = PointXY.PointXYInterpolate(cdvh.DVHPointsList, 100.0f * parameter / cdvh.Volume, false);
                    break;

                case DVHMetricType.DCxcc_Gy:
                    returvalue = DVHMetricValue(cdvh, DVHMetricType.Dxcc_Gy, cdvh.Volume - parameter);
                    break;

                case DVHMetricType.Dxcc_Percent:
                    if (cdvh.NormDose != double.NaN) returvalue = 100.0f * (PointXY.PointXYInterpolate(cdvh.DVHPointsList, 100.0f * parameter / cdvh.Volume, false) / (cdvh.NormDose ?? double.NaN));
                    break;

                case DVHMetricType.DCxcc_Percent:
                    if (cdvh.NormDose != double.NaN) returvalue = DVHMetricValue(cdvh, DVHMetricType.DCxcc_Percent, cdvh.Volume - parameter);
                    break;

                case DVHMetricType.VxGy_Percent:
                    returvalue = PointXY.PointXYInterpolate(cdvh.DVHPointsList, parameter, true);
                    break;

                case DVHMetricType.CVxGy_Percent:
                    returvalue = 100.0f - DVHMetricValue(cdvh, DVHMetricType.VxGy_Percent, parameter);
                    break;

                case DVHMetricType.VxGy_cc:
                    returvalue = cdvh.Volume * PointXY.PointXYInterpolate(cdvh.DVHPointsList, parameter, true) / 100.0f;
                    break;

                case DVHMetricType.CVxGy_cc:
                    returvalue = cdvh.Volume - DVHMetricValue(cdvh, DVHMetricType.VxGy_cc, parameter);
                    break;

                case DVHMetricType.VxPercent_Percent:
                    if (cdvh.NormDose != double.NaN) returvalue = PointXY.PointXYInterpolate(cdvh.DVHPointsList, (cdvh.NormDose ?? double.NaN) * parameter / 100.0f, true);
                    break;

                case DVHMetricType.CVxPercent_Percent:
                    if (cdvh.NormDose != double.NaN) returvalue = 100.0f - DVHMetricValue(cdvh, DVHMetricType.VxPercent_Percent, parameter);
                    break;

                case DVHMetricType.VxPercent_cc:
                    if (cdvh.NormDose != double.NaN) returvalue = cdvh.Volume * PointXY.PointXYInterpolate(cdvh.DVHPointsList, (cdvh.NormDose ?? double.NaN) * parameter / 100.0f, true) / 100.0f;
                    break;

                case DVHMetricType.CVxPercent_cc:
                    if (cdvh.NormDose != double.NaN) returvalue = cdvh.Volume - DVHMetricValue(cdvh, DVHMetricType.VxPercent_cc, parameter);
                    break;

            }

            if (returvalue == -1)
            {
                Log3_static.Information($"DVHCurve_ID: {cdvh.DVHCurve_ID} produce -1 metric values. Please Check.");
            }

            if (double.IsNaN(returvalue))
            {
                Log3_static.Information($"DVHCurve_ID: {cdvh.DVHCurve_ID} produce NaN metric values for {dvhmt.ToString()} at parameter {parameter}. Please Check.");

                //Console.WriteLine("********** WARMING: DVHMetricValue(...) returns NaN **********");
            }
            return returvalue < 0 ? -1 : returvalue;
        }

        public static DVHMetricType metric_type_enum(string metric_type_string)
        {
            try
            {
                return (DVHMetricType)Enum.Parse(typeof(DVHMetricType), metric_type_string);
            }
            catch (ArgumentException)
            {
                Console.WriteLine("'{0}' is not a member of the DVHMetricType enumeration.", metric_type_string);
                throw;
            }
        }

        public static string sql_filter_fraction(int fraction, string datasource = "ANY", bool prefix_and = true)
        {
            if (fraction == 0 && datasource == "HN_ROAR") return ((prefix_and ? " AND " : " ") + @" NFractions_Delivered >= 23 ");

            if (fraction == 0) return " ";

            if (fraction < 0) throw new Exception("XXXXXXXXXX Fraction < 0  for function: where_sql_filter_fraction(int fraction). Something is WROGN! xxxxxxxxx");

            return ((prefix_and ? " AND " : " ") + @" NFractions_Delivered = @fraction ");
        }

        public static string fraction_convert(int fraction)
        {
            if (fraction == 0) return "ALL";
            else return fraction.ToString();
        }



        public static List<PointXY> get_DVH_points_from_SQL_line(string SQL_line_DVHCurve_ByVolumePercentList)
        {
            List<PointXY> cdvh_points = new List<PointXY>();
            var sep = new char[] { ';' };
            foreach (var p in SQL_line_DVHCurve_ByVolumePercentList.Trim('"', ' ').Split(sep, options: StringSplitOptions.RemoveEmptyEntries))
            {
                //Console.WriteLine(p);
                var p2 = p.Split(',');
                //Console.WriteLine("{0} {1}",p2[0],p2[1]);
                cdvh_points.Add(new PointXY(Convert.ToDouble(p2[0]), Convert.ToDouble(p2[1])));
            }
            return cdvh_points;
        }




        public static double GEM_from_constraint_metrics(Dictionary<string, double> constraint_metrics, constraint[] cons)
        {
            List<GEM_metric> GEM_inputs = new List<GEM_metric>();

            foreach (var metric in constraint_metrics)
            {
                var con = cons.Where(t => t.ToString2() == metric.Key);
                if (con.Count() == 1)
                {
                    GEM_inputs.Add(new GEM_metric(metric.Value, con.Single()));
                }
                else
                {
                    throw new Exception(metric.Key + " cannot be matched to a single constraint provided. Matches: " + con.Count() + " actually");
                }
            }

            return MathFunctions.GEM(GEM_inputs);
        }

        /// <summary>
        /// Calculate GEM
        /// </summary>
        /// <param name="input_DVH">Must be X = Dose in Gy, and Y = Volume in %</param>
        /// <param name="StructureID">no need to be standard StructureID</param>
        /// <param name="fraction">default to be 0, which ignore fractions</param>
        /// <returns></returns>
        public static double GEM_per_input_DVH(PointXY[] input_DVH, string StructureID, double volume_cc, int fraction = 0, string datasource = "HN_ROAR", string constraint_choice = "MC", double TotalDose_Planned = double.NaN)
        {
            return MathFunctions.GEM(GEM_inputs_from_one_input_DVH(input_DVH, StructureID, volume_cc, fraction, datasource, constraint_choice, TotalDose_Planned: TotalDose_Planned));
        }

        public static List<GEM_metric> GEM_inputs_from_one_input_DVH(PointXY[] input_DVH, string StructureID, double volume_cc, int fraction = 0, string datasource = "HN_ROAR", string constraint_choice = "MC", double TotalDose_Planned = double.NaN)
        {
            var filtered_constraints = constraint.constraints[datasource + "__" + constraint_choice].Where(x => x.StructureID.Match_StrID_to_Standard_Name(datasource) == StructureID.Match_StrID_to_Standard_Name(datasource)).
                Where(x => (x.fraction == fraction) || 0 == x.fraction);

            var GEM_inputs = new List<GEM_metric>();

            if (!filtered_constraints.Any()) return GEM_inputs;

            DVHCurve curve = new DVHCurve(
                input_DVH.ToList(),
                vol_cc: volume_cc,
                normdose_Gy: TotalDose_Planned,
                dut: DoseUnitType.Gy,   // Must be Gy
                vut: VolumeUnitType.percent   // Must be percent
                );

            foreach (var con in filtered_constraints)
            {
                double temp;

                temp = DVHMetricValue(
                        curve,
                        metric_type_enum(con.metric_type),
                        con.metric_parameter
                        );

                Console.WriteLine("{0} - [{1} - {2}] - {3,10} - {4,4} - {5} - {6} - {8}-{7,10:f3} - {9,10}", con.StructureID, con.fraction, fraction, con.metric_type, con.metric_parameter, con.source, "line.Patient_GUID", temp, con.limit, (temp - con.limit) / (con.q * con.limit));

                if (!double.IsNaN(temp))
                    GEM_inputs.Add(new GEM_metric(temp, con));
            }
            return GEM_inputs;
        }

        // This is older, but almost equivalent to 
        // newer calc.calculate_GEM_of_a_curve(DVHCurve curve, IEnumerable<constraint> filtered_constraints)
        public static double GEM_per_input_DVH(DVHCurve curve, int fraction = 0, string datasource = "HN_ROAR", string constraint_choice = "MC")
        {
            return MathFunctions.GEM(GEM_inputs_from_one_input_DVH(curve, fraction, datasource, constraint_choice));
        }

        public static List<GEM_metric> GEM_inputs_from_one_input_DVH(DVHCurve curve, int fraction = 0, string datasource = "HN_ROAR", string constraint_choice = "MC")
        {
            var filtered_constraints = constraint.constraints[datasource + "__" + constraint_choice].Where(x => x.StructureID.Match_StrID_to_Standard_Name(datasource) == curve.Structure.Match_StrID_to_Standard_Name(datasource)).
                Where(x => (x.fraction == fraction) || 0 == x.fraction);

            var GEM_inputs = new List<GEM_metric>();

            if (!filtered_constraints.Any()) return GEM_inputs;

            foreach (var con in filtered_constraints)
            {
                double temp = DVHMetricValue(curve, metric_type_enum(con.metric_type), con.metric_parameter);

                //Console.WriteLine("{0} - [{1} - {2}] - {3,10} - {4,4} - {5} - {6} - {8}-{7,10:f3} - {9,10}", con.StructureID, con.fraction, fraction, con.metric_type, con.metric_parameter, con.source, "line.Patient_GUID", temp, con.limit, (temp - con.limit) / (con.q * con.limit));

                if (!double.IsNaN(temp))
                    GEM_inputs.Add(new GEM_metric(temp, con));
            }
            return GEM_inputs;
        }


        [DataContract]
        public class GEM_details_per_Plan
        {
            [DataMember]
            public string Patient_MR { get; set; }
            [DataMember]
            public string CourseID { get; set; }
            [DataMember]
            public string PlanID { get; set; }
            [DataMember]
            public double PlanGEM { get; set; }
            [DataMember]
            public Dictionary<string, double> Str_GEMs { get; set; }
        }

  
        public static double[][] generate_quantile_curves(double[,] xx)
        {
            int r = xx.GetLength(0), c = xx.GetLength(1);
            var rows = new double[r - 2][];   // # of DVHcurves e.g. 52 for Kidney
            var cols = new double[c - 1][];     // # of at_points e.g. 101, seq(0:100)

            Console.WriteLine("rows: {0} and columns {1}", r, c);

            for (int i = 2; i < r; i++)
            {
                rows[i - 2] = new double[c - 1];
                for (int j = 1; j < c; j++)
                {
                    rows[i - 2][j - 1] = xx[i, j];
                }
            }

            for (int i = 1; i < c; i++)
            {
                cols[i - 1] = new double[r - 2];
                for (int j = 2; j < r; j++)
                {
                    cols[i - 1][j - 2] = xx[j, i];
                }
            }

            // Get Quantiles of the distribution of DVH metrics
            var quantilelist = new double[] { 5, 15, 25, 50, 75, 85, 95 };
            var quantiles = new double[cols.Length][];
            var quantile_lines = new double[quantilelist.Length][];
            for (int j = 0; j < quantilelist.Length; j++)
            {
                quantile_lines[j] = new double[cols.Length];
            }

            for (int i = 0; i < cols.Length; i++)
            {
                quantiles[i] = Stats.Quantiles(cols[i], quantilelist);
                for (int j = 0; j < quantilelist.Length; j++)
                {
                    quantile_lines[j][i] = quantiles[i][j];
                }
            }

            return quantile_lines;
        }

        public static double WES_calcuation_from_PreStatsDVH(DVHCurve input_DVH, serialize.StatsDVH_info sDVH, string constraint_choice = "MC")
        {
            return WES_calcuation_from_PreStatsDVH(input_DVH.DVHPointsList_31, sDVH, constraint_choice);
        }

        public static double WES_calcuation_from_PreStatsDVH(PointXY[] input_DVH_points, serialize.StatsDVH_info sDVH, string constraint_choice = "MC")
        {
            if (sDVH == null) return double.NaN;

            int num_cross_sections = sDVH.v_cross_sections.Count();

            if (input_DVH_points.Count() != serialize.volume_values.Count())
            {
                throw new Exception("input_DVH for WES calculation is not 31-standardized.");
            }
            // need to further restrict input_DVH strictly equal to serialize.volume_values.

            var p = (from v_cross_sec in sDVH.v_cross_sections
                     join dose in input_DVH_points on v_cross_sec.at_volume equals dose.Y
                     orderby v_cross_sec.at_volume
                     select (Stats.ecdf(v_cross_sec.ecdf, dose.X) / 100)).ToArray();

            if (p.Any(t => double.IsNaN(t)))
            {
                return double.NaN;
            }

            if (p.Length != input_DVH_points.Count()) throw new Exception("input_DVH do not have the same at_volume cross-sections as those in StatsDVH!");

            var PC1 = (from v_cross_sec in sDVH.v_cross_sections
                       orderby v_cross_sec.at_volume
                       select v_cross_sec.PC1).ToArray();

            var at_volume_values = (from v_cross_sec in sDVH.v_cross_sections
                                    orderby v_cross_sec.at_volume
                                    select v_cross_sec.at_volume).ToArray();

            var even_out_co = even_out_coef(at_volume_values);

            double[] weights = new double[num_cross_sections];

            if (constraint_choice == "NO")
            {
                weights = PC1.Abs().ElementwiseMultiply(even_out_co);
                return weights.ElementwiseMultiply(p).Sum() / (weights.Sum()); ;
            }

            double[] Ktau = new double[num_cross_sections];

            if (constraint_choice == "MC")
            {
                Ktau = (from v_cross_sec in sDVH.v_cross_sections
                        orderby v_cross_sec.at_volume
                        select v_cross_sec.KtauM).ToArray();
                weights = Ktau.ElementwiseMultiply(PC1.Abs()).ElementwiseMultiply(even_out_co);
                return weights.ElementwiseMultiply(p).Sum() / (weights.Sum());
            }

            if (constraint_choice == "DF")
            {
                Ktau = (from v_cross_sec in sDVH.v_cross_sections
                        orderby v_cross_sec.at_volume
                        select v_cross_sec.KtauD).ToArray();
                weights = Ktau.ElementwiseMultiply(PC1.Abs()).ElementwiseMultiply(even_out_co);
                return weights.ElementwiseMultiply(p).Sum() / (weights.Sum());
            }

            if (constraint_choice == "NTCP")
            {
                Ktau = (from v_cross_sec in sDVH.v_cross_sections
                        orderby v_cross_sec.at_volume
                        select v_cross_sec.Ktau_NTCP).ToArray();
                weights = Ktau.ElementwiseMultiply(PC1.Abs()).ElementwiseMultiply(even_out_co);
                return weights.ElementwiseMultiply(p).Sum() / (weights.Sum());
            }

            throw new Exception("Constraint_choice must be one the following: NO (WES); DF (GEM_WES); MC (GEM_pop_WES); NTCP (NTCP_WES)");
        }

        public static double[] even_out_coef(double[] array)
        {
            array = array.Select(t => Math.Round(t, 6)).ToArray(); // avoid imprecision of doubles

            var array_ordered = array.OrderBy(t => t).ToArray();
            //var x = array.Equals(array_ordered); // not equal
            //var y = array.SequenceEqual(array_ordered); // equal

            if (!array.SequenceEqual(array_ordered))
                throw new Exception("########## input array for even_out_coef() is not monotonically increasing. Something must be wrong! ##########");

            var len = array.Length;
            var return_array = new double[len];

            for (var i = 1; i < len - 1; i++)
            {
                return_array[i] = (array[i + 1] - array[i - 1]) / 2;
            }

            return_array[0] = (array[1] - array[0]) / 2;
            return_array[len - 1] = (array[len - 1] - array[len - 2]) / 2;

            //var x = return_array.Divide(array[len - 1] - array[0]).Sum();
            // normalize array to [0,1]
            return return_array.Divide(array[len - 1] - array[0]);
        }

        static double[] quantiles_on_volume_cross_section_ecdf = Enumerable.Range(0, 101).Select(x => (double)x).ToArray();


        public class volume_cross_section
        {
            public double at_volume { get; set; }
            public double PC1 { get; set; }
            public double KtauM { get; set; }
            public double KtauD { get; set; }
            public double Ktau_NTCP { get; set; }
            public PointXY[] ecdf { get; set; } // Here PointXY is not point on DVH curve. X is still dose, but Y is % quantile, instead of % volume.
        }

        public class info_per_curve
        {
            //public string Patient_MR { get; set; }
            //public string CourseID { get; set; }
            //public string PlanID { get; set; }

            public int TreatedPlan_ID { get; set; }

            //public double? TDp { get; set; } // TotalDose_Planned
            //public int? NFp { get; set; }    // NFractions_Planned
            public double? TDd { get; set; } // TotalDose_Delivered
            public int? NFd { get; set; }    // NFractions_Delivered

            public string StrID { get; set; }
            public double Volume_cc { get; set; }
            //public double Mean_Gy { get; set; }
            //public double Max_Gy { get; set; }

            public double GEM_DF { get; set; }
            public double GEM_MC { get; set; }
            public Dictionary<string, double> constraint_metrics { get; set; }

            public double NTCP { get; set; }
            public double gEUD { get; set; }

            public double WES { get; set; }
            public double WES_GEM { get; set; }
            public double WES_GEMpop { get; set; }
            public double WES_NTCP { get; set; }
        }
    }


}
