// Consolidate/Refacter many functions/method from cs file: Serialization_class.cs and AnalysisMethods.cs
// Centering around GEM, WES calculation. 
// Trying to make functions smaller (seperate of concerns).

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace AnalyticsLibrary2
{

    public interface IStatsDVH_input
    {
        int DVHCurve_ID { get; set; }

        int TreatedPlan_ID { get; set; } // or some unique identifier for each plan
        double? TotalDose_Delivered { get; set; }
        int? NFractions_Delivered { get; set; }
        double? TotalPlanMU { get; set; }
        
        string StructureID { get; set; }
        double? Volume_cc { get; set; }
        
        // either of the DVH formate
        string DVHCurve_ByVolumePercentList { get; set; }
        //string DVHCurve_ByDoseInterval { get; set; }


        // following are not absolutely necessary, but may keep in for convenience:
        //string Patient_MR { get; set; }
        //string CourseID { get; set; }
        //string PlanID { get; set; }
        double? Max_Gy { get; set; }
        //double? Mean_Gy { get; set; }
        string StandardStructureName { get; set; }
    }

  
    public class calc
    {
        public static double calculate_GEM_of_a_curve(DVHCurve curve, IEnumerable<constraint> filtered_constraints)
        {
            if (!filtered_constraints.Any()) return double.NaN;

            var GEM_inputs = generate_GEMinputList_of_a_curve(curve, filtered_constraints);

            double rv = MathFunctions.GEM(GEM_inputs);

            string cons_string = string.Join(" ", filtered_constraints.Select(t => t.ToString()));
            Log3_static.Debug($"Structure {curve.Structure} GEM is {rv:F2} with constraint {cons_string}");

            return rv;
        }

        public static List<GEM_metric> generate_GEMinputList_of_a_curve(DVHCurve curve, IEnumerable<constraint> filtered_constraints)
        {
            var GEM_inputs = new List<GEM_metric>();
            if (!filtered_constraints.Any()) return GEM_inputs;

            foreach (var con in filtered_constraints)
            {
                double temp = DVHMetrics.DVHMetricValue(curve, DVHMetrics.metric_type_enum(con.metric_type), con.metric_parameter);
                if (!double.IsNaN(temp)) GEM_inputs.Add(new GEM_metric(temp, con));
                Log3_static.Debug($"Achieved Metric {temp:F2} for constraint {con} in structure {curve.Structure}");
            }
            return GEM_inputs;
        }

    }



    public class Console_info
    {
        public static void warning_DVH_incomplete(string DVH_curve_string, double max_volume  )
        {
            Console.WriteLine("# ---------- input_curve DVHCurve_ByVolumePercentList is not complete ---------- #");
            Console.WriteLine(DVH_curve_string);
            Console.WriteLine("# ---------- curve_input.DVHPointsList[0].Y != 100; it is " + max_volume + " instead ---#");
        }
    }


    public class StatsDVH_ForOpt
    {
        public string datasource { get; set; }
        public int Nfraction { get; set; }
        public Dictionary<string, serialize.StatsDVH_info> str_sDVH_dict { get; set; }

        /// <summary>
        /// To create a service object StatsDVH_ForOpt for certain RT type and fraction.
        /// </summary>
        /// <param name="datasource"> plan type (i.e. patient cohort. e.g. HN_General)</param>
        /// <param name="Nfraction"> Plan Fractions. Only 0, 3, and 5 are valid inputs. Specifying 0 (default) will include all fractions for CRT. 3 and 5 are for SBRT.</param>
        /// <param name="Pre_JSON_dir">directory where pre-calculated JSON files reside</param>
        public StatsDVH_ForOpt(string data_source, int N_fraction = 0, string Pre_JSON_dir = @"\\uhrofilespr1\EclipseScripts\Aria15\Data\StatsDVH\Pre_JSON_9\")
        {
            datasource = data_source;
            Nfraction = N_fraction; // 0 is for the set that includes all fractions for CRT.

            var filename = "Summary_dict__" + datasource + "__" + Nfraction.ToString() + "__StatsDVH.json";

            str_sDVH_dict = serialize.Load_JSON<Dictionary<string, serialize.StatsDVH_info>>(Pre_JSON_dir + filename);

            //Console.WriteLine(Pre_JSON_dir + filename + " -------- loaded. \n Which contains StatsDVH for the following structures:\n");
            //str_sDVH_dict.Keys.ToList().ForEach(t => Console.Write(t + "\t"));
            Console.WriteLine("\n");
        }


        /// <summary>
        /// Find Dose and PC1 values at specified volume cross section and Dose_Quantile, for specified structure in specified datasource.
        /// </summary>
        /// <param name="str">name of structure</param>
        /// <param name="at_volume_percent">a double between [0, 100]</param>
        /// <param name="at_dose_quantile">a double between [0, 100]</param>
        /// <returns></returns>
        public point_on_StatsDVH find_point_on_StatsDVH(string str, double at_volume_percent, double at_dose_quantile)
        {
            try
            {
                if (at_volume_percent > 100 | at_volume_percent < 0) throw new Exception("at_volume_percent need to be a double between [0,100]");

                if (at_dose_quantile > 100 | at_dose_quantile < 0) throw new Exception("at_quantile need to be a double between [0,100]");

                // convert str to standard name in TG263
                string strID_TitleCase = str.Match_StrID_to_Standard_Name(datasource).Title_Case();

                if (!str_sDVH_dict.Keys.Contains(strID_TitleCase)) throw new Exception("provided structure_name " + str + "cannot be converted to standard structure name by function Match_StrID_to_Standard_Name();\n Please consider using one of the following structure names:\n "
                    + string.Join("; ", str_sDVH_dict.Keys.ToArray()));

                serialize.StatsDVH_info StatsDVH = str_sDVH_dict[strID_TitleCase];

                var pre_defined_volumes = StatsDVH.v_cross_sections.Select(t => t.at_volume).ToArray();
                // { 0, 0.5, 1, 2, 3, 4, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 96, 97, 98, 99, 99.5, 100 }
                // 31 % volume cross sections.

                if (!pre_defined_volumes.SequenceEqual(pre_defined_volumes.OrderBy(t => t).ToArray()))
                    throw new Exception("pre_defined_volumes from StatsDVH is not monotomically increasing. Something must be wrong!");

                var quantiles = StatsDVH.v_cross_sections[0].ecdf.Select(t => t.Y).ToArray();
                // ecdf is Emperical Cumulative Distribution Function, which has 0, 1, 2, 3, ..., 100  -- 101 quantiles in total.

                if (!quantiles.SequenceEqual(quantiles.OrderBy(t => t).ToArray()))
                    throw new Exception("ecdf from StatsDVH is not monotomically increasing. Something must be wrong!");

                // find boundaries for interpolation
                double v_scale, q_scale;

                // index of volume cross_section boundaries, lower and upper
                int ivl = Array.FindLastIndex(pre_defined_volumes, t => t <= at_volume_percent);
                int ivu = ivl + 1;
                if (ivu == 31)
                {
                    if (at_volume_percent != 100) throw new Exception("ivu == 31 but at_volume_percent != 100");
                    v_scale = 0;
                    ivu = 30;
                }
                else
                {
                    v_scale = (at_volume_percent - pre_defined_volumes[ivl]) / (pre_defined_volumes[ivu] - pre_defined_volumes[ivl]);
                }

                // index of quantile boundaries, lower and upper
                int iql = Array.FindLastIndex(quantiles, t => t <= at_dose_quantile);
                int iqu = iql + 1;
                if (iqu == 101)
                {
                    if (at_dose_quantile != 100) throw new Exception("iqu == 101 but at_quantile != 100");
                    q_scale = 0;
                    iqu = 30;
                }
                else
                {
                    q_scale = (at_dose_quantile - quantiles[iql]) / (quantiles[iqu] - quantiles[iql]);
                }

                double d11, d12, d21, d22, dv1, dv2, d, qv1, qv2, q;

                d11 = StatsDVH.v_cross_sections[ivl].ecdf[iql].X;
                d12 = StatsDVH.v_cross_sections[ivl].ecdf[iqu].X;

                d21 = StatsDVH.v_cross_sections[ivu].ecdf[iql].X;
                d22 = StatsDVH.v_cross_sections[ivu].ecdf[iqu].X;

                dv1 = d11 + q_scale * (d12 - d11);
                dv2 = d21 + q_scale * (d22 - d21);

                d = dv1 + v_scale * (dv2 - dv1);

                qv1 = StatsDVH.v_cross_sections[ivl].PC1;
                qv2 = StatsDVH.v_cross_sections[ivu].PC1;

                q = qv1 + v_scale * (qv2 - qv1);

                return new point_on_StatsDVH()
                {
                    datasource = datasource,
                    NFraction = Nfraction,
                    Structure = strID_TitleCase,
                    at_volume_percent = at_volume_percent,
                    at_quantile = at_dose_quantile,

                    Dose = d,
                    PC1 = q
                };
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        /// <summary>
        /// Find Dose_Quantile and PC1 values at specified volume cross section and dose, for specified structure in specified datasource.
        /// </summary>
        /// <param name="str">name of structure</param>
        /// <param name="at_volume_percent">a double between [0, 100]</param>
        /// <param name="at_dose_quantile">a double of Dose in [Gy]</param>
        /// <returns></returns>
        public point_on_StatsDVH find_qntl_for_point_on_StatsDVH(string str, double at_volume_percent, double with_dose)
        {
            try
            {
                if (at_volume_percent > 100 | at_volume_percent < 0) throw new Exception("at_volume_percent need to be a double between [0,100]");

                if (with_dose > 300 | with_dose < 0) throw new Exception("at_quantile need to be a double between [0, 300] Gy");

                // convert str to standard name in TG263
                string strID_TitleCase = str.Match_StrID_to_Standard_Name(datasource).Title_Case();

                if (!str_sDVH_dict.Keys.Contains(strID_TitleCase))
                {
                    return new point_on_StatsDVH()
                    {
                        datasource = datasource,
                        NFraction = Nfraction,
                        Structure = strID_TitleCase,
                        at_volume_percent = at_volume_percent,
                        Dose = with_dose,

                        at_quantile = double.NaN,
                        PC1 = double.NaN
                    };
                }
                //throw new Exception("provided structure_name " + str + "cannot be converted to standard structure name by function Match_StrID_to_Standard_Name();\n Please consider using one of the following structure names:\n "
                //    + string.Join("; ", str_sDVH_dict.Keys.ToArray()));

                serialize.StatsDVH_info StatsDVH = str_sDVH_dict[strID_TitleCase];

                var pre_defined_volumes = StatsDVH.v_cross_sections.Select(t => t.at_volume).ToArray();
                // { 0, 0.5, 1, 2, 3, 4, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 96, 97, 98, 99, 99.5, 100 }
                // 31 % volume cross sections.

                if (!pre_defined_volumes.SequenceEqual(pre_defined_volumes.OrderBy(t => t).ToArray()))
                    throw new Exception("pre_defined_volumes from StatsDVH is not monotomically increasing. Something must be wrong!");

                var quantiles = StatsDVH.v_cross_sections[0].ecdf.Select(t => t.Y).ToArray();
                // ecdf is Emperical Cumulative Distribution Function, which has 0, 1, 2, 3, ..., 100  -- 101 quantiles in total.

                if (!quantiles.SequenceEqual(quantiles.OrderBy(t => t).ToArray()))
                    throw new Exception("ecdf from StatsDVH is not monotomically increasing. Something must be wrong!");

                // find boundaries for interpolation
                double v_scale, q_scale;

                // index of volume cross_section boundaries, lower and upper
                int ivl = Array.FindLastIndex(pre_defined_volumes, t => t <= at_volume_percent);
                int ivu = ivl + 1;
                if (ivu == 31)
                {
                    if (at_volume_percent != 100) throw new Exception("ivu == 31 but at_volume_percent != 100");
                    v_scale = 0;
                    ivu = 30;
                }
                else
                {
                    v_scale = (at_volume_percent - pre_defined_volumes[ivl]) / (pre_defined_volumes[ivu] - pre_defined_volumes[ivl]);
                }

                // if at_volume_percent falls between two v_cross_sections, interpolate ecdf between them.
                var vs = StatsDVH.v_cross_sections[ivl].ecdf.Select((t, ind) => t.X + v_scale * (StatsDVH.v_cross_sections[ivu].ecdf[ind].X - t.X));

                double at_dose_quantile = vs.Quantiles_reverse(new List<double>() { with_dose }).First();

                // index of quantile boundaries, lower and upper
                int iql = Array.FindLastIndex(quantiles, t => t <= at_dose_quantile);
                int iqu = iql + 1;
                if (iqu == 101)
                {
                    if (at_dose_quantile != 100) throw new Exception("iqu == 101 but at_quantile != 100");
                    q_scale = 0;
                    iqu = 30;
                }
                else
                {
                    q_scale = (at_dose_quantile - quantiles[iql]) / (quantiles[iqu] - quantiles[iql]);
                }

                double d11, d12, d21, d22, dv1, dv2, d, qv1, qv2, q;

                d11 = StatsDVH.v_cross_sections[ivl].ecdf[iql].X;
                d12 = StatsDVH.v_cross_sections[ivl].ecdf[iqu].X;

                d21 = StatsDVH.v_cross_sections[ivu].ecdf[iql].X;
                d22 = StatsDVH.v_cross_sections[ivu].ecdf[iqu].X;

                dv1 = d11 + q_scale * (d12 - d11);
                dv2 = d21 + q_scale * (d22 - d21);

                d = dv1 + v_scale * (dv2 - dv1);

                qv1 = StatsDVH.v_cross_sections[ivl].PC1;
                qv2 = StatsDVH.v_cross_sections[ivu].PC1;

                q = qv1 + v_scale * (qv2 - qv1);

                double diff = d - with_dose;

                if (with_dose <= vs.Max() && with_dose >= vs.Min() && Math.Abs(diff) > 0.000001)
                {
                    throw new Exception("diff between with_dose and calculated dose doesn't match.");
                }

                return new point_on_StatsDVH()
                {
                    datasource = datasource,
                    NFraction = Nfraction,
                    Structure = strID_TitleCase,
                    at_volume_percent = at_volume_percent,
                    at_quantile = at_dose_quantile,

                    Dose = with_dose,
                    PC1 = q
                };
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public static void Test_And_Examples()
        {
            var foropt = new StatsDVH_ForOpt("HN_General", 0);

            Console.WriteLine("\n\n\n############## find_point_on_StatsDVH example 1, spot check #############\n\n");
            string str = "Parotid_r";
            double at_volume_percent = 8.5;
            double at_dose_quantile = 2.5;
            var p = foropt.find_point_on_StatsDVH(str, at_volume_percent, at_dose_quantile);
            Console.WriteLine(p.ToString());


            Console.WriteLine("\n\n\n\n\n ############## find_point_on_StatsDVH example 2, system check #############");
            double[] volume_values = new double[] { 0, 0.5, 5, 20, 22, 25, 50, 90, 99.5, 100 };
            double[] quantiles = new double[] { 0, 2.5, 20, 25, 50, 75, 90, 95, 97.5, 100 };
            foreach (var v in volume_values)
            {
                Console.WriteLine("\n \n Volume cross_section " + v + "% ---------------------- \n");
                foreach (var q in quantiles)
                {
                    p = foropt.find_point_on_StatsDVH(str, v, q);
                    Console.Write(p.ToString2() + " vs ");

                    var p2 = foropt.find_qntl_for_point_on_StatsDVH(str, v, p.Dose);
                    Console.WriteLine(p2.ToString2() + "; ");
                }
                Console.WriteLine("\n PCA weight for this volume cross_section is: " + p.PC1);
            }
        }


    }


    public class point_on_StatsDVH
    {
        public string datasource { get; set; }
        public int NFraction { get; set; }
        public string Structure { get; set; }

        public double at_volume_percent { get; set; }
        public double at_quantile { get; set; }

        public double Dose { get; set; }
        public double PC1 { get; set; }

        public override string ToString()
        {
            return Structure + " D" + at_volume_percent + "%[Gy] " + at_quantile + "% quantile: " + Math.Round(Dose, 2);
        }

        public string ToString2()
        {
            return at_quantile + "%: " + Math.Round(Dose, 2) + "Gy";
        }
    }
    
}
