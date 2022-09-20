using System;
using System.Linq;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using AnalyticsLibrary2;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using GalaSoft.MvvmLight;
using AP_lib;

namespace AutoPlan_WES_HN
{
    public class RetrieveFromEclipse15
    {
        // This may look like -- public static DVHCurve.generate_input_DVHcurve(PlanningItem planning_item, Structure str_VMS, double TotalDose_Planned).
        // Yes, the following function is developed on that. It should not belong to View Model, i.e. summary_vm_2_ps. Use the following one in the future.
        public static DVHCurve Get_DVH_From_PlanningItem(PlanningItem planning_item, Structure str_VMS, double TotalDose_Planned)
        {
            DVHData dvh = planning_item.GetDVHCumulativeData(str_VMS, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.2);

            if (dvh == null)
            {
                Log.Warning($"{str_VMS.Id} has no DVH curve.");
                return null;
                //throw new Exception("planning_item.GetDVHCumulativeData(str_VMS,...) returns null.");
            }
            if (dvh.CurveData.Count() < 2)
            {
                Log.Warning($"planning_item.GetDVHCumulativeData(str_VMS,...).Count returns DVH points < 2. {dvh.CurveData.Count()} to be precise");
                return null;
                //throw new Exception("planning_item.GetDVHCumulativeData(str_VMS,...).Count < 2");
            }

            List<DVHPoint> Ordered_CurveDate = dvh.CurveData.OrderBy(x => x.DoseValue.Dose).ToList();

            // Correct Max_Gy accurately.
            Ordered_CurveDate.RemoveAt(dvh.CurveData.Count() - 1);
            Ordered_CurveDate.Add(new DVHPoint(dvh.MaxDose, 0.0, "%"));

            // Correct Min_Gy accurately.
            double hundred_limits = 99.99999;
            if (Ordered_CurveDate.Any(t => t.Volume > hundred_limits))
            {
                List<int> inds = new List<int>();
                foreach (DVHPoint p in Ordered_CurveDate.Where(t => t.Volume > hundred_limits))
                {
                    int ind = Ordered_CurveDate.IndexOf(p);
                    inds.Add(ind);
                }
                foreach (int i in inds)
                {
                    //Log.Verbose("Correct volume to 100% from {origin:F18}% for structure {str}", Ordered_CurveDate[i].Volume, str_VMS.Id);
                    Ordered_CurveDate[i] = new DVHPoint(Ordered_CurveDate[i].DoseValue, 100, "%");
                }
            }

            if (Ordered_CurveDate.Any(t => t.Volume == 100))
            {
                if (dvh.MinDose.Dose != 0)
                {
                    DVHPoint point_min = Ordered_CurveDate.Last(t => t.Volume == 100);
                    int ind_min = Ordered_CurveDate.IndexOf(point_min);
                    Ordered_CurveDate.Insert(ind_min + 1, new DVHPoint(dvh.MinDose, 100, "%"));
                }
            }
            else
            {
                Log.Warning($"Missing complete DVH: Only {Ordered_CurveDate.First().Volume:F18}% volume exist for structure {str_VMS.Id}");
            }
            //Ordered_CurveDate.ForEach(t => Console.WriteLine("D: {0}; \t\t Volume: {1}", t.DoseValue.Dose, t.Volume));

            DVHCurve curve = new DVHCurve(Ordered_CurveDate.Select(x => new PointXY { X = x.DoseValue.Dose, Y = x.Volume }).ToList(),
                    dvh.Volume,
                    TotalDose_Planned,
                    DoseUnitType.Gy,
                    VolumeUnitType.percent
                    )
            { Structure = str_VMS.Id };

            //Log.Verbose($"{str_VMS.Id} DVH curve Retrieved: {JsonConvert.SerializeObject(curve.DVHPointsList_31, Keep2digitsConvertor)}");
            //Log.Warning("DVH curve Retrieved is {@curve}", curve);

            return curve;
        }

        public static DVHCurve Get_DVH_From_OptimizerResult(OptimizerResult optresult, Structure str_VMS, double TotalDose_Planned)
        {
            var optimizerDVH = optresult.StructureDVHs.SingleOrDefault(t => t.Structure.Id == str_VMS.Id);
            var Ordered_CurveDate = optimizerDVH?.CurveData?.ToList();

            if (Ordered_CurveDate == null)
            {
                Log.Warning($"{str_VMS.Id} has no DVH curve.");
                return null;
                //throw new Exception("planning_item.GetDVHCumulativeData(str_VMS,...) returns null.");
            }
            if (Ordered_CurveDate.Count() < 2)
            {
                Log.Warning($"planning_item.GetDVHCumulativeData(str_VMS,...).Count returns DVH points < 2. {Ordered_CurveDate.Count()} to be precise");
                return null;
                //throw new Exception("planning_item.GetDVHCumulativeData(str_VMS,...).Count < 2");
            }

            // no need to order any more from optresult
            //List<DVHPoint> Ordered_CurveDate = dvh_curvedata.OrderBy(x => x.DoseValue.Dose).ToList();

            // impossible and not necessary to correct min and max in OptimizerDVH.

            // Correct Max_Gy accurately.
            //Ordered_CurveDate.RemoveAt(dvh_curvedata.CurveData.Count() - 1);
            //Ordered_CurveDate.Add(new DVHPoint(dvh_curvedata.MaxDose, 0.0, "%"));

            // Correct Min_Gy accurately.
            //if (Ordered_CurveDate.Any(t => t.Volume > 100))
            //{
            //    List<int> inds = new List<int>();
            //    foreach (DVHPoint p in Ordered_CurveDate.Where(t => t.Volume > 100))
            //    {
            //        int ind = Ordered_CurveDate.IndexOf(p);
            //        inds.Add(ind);
            //    }
            //    foreach (int i in inds)
            //    {
            //        //Log.Verbose("Correct volume to 100% from {origin:F15}% for structure {str}", Ordered_CurveDate[i].Volume, str_VMS.Id);
            //        Ordered_CurveDate[i] = new DVHPoint(Ordered_CurveDate[i].DoseValue, 100, "%");
            //    }
            //}

            //if (Ordered_CurveDate.Any(t => t.Volume == 100))
            //{
            //    if (dvh_curvedata.MinDose.Dose != 0)
            //    {
            //        DVHPoint point_min = Ordered_CurveDate.Last(t => t.Volume == 100);
            //        int ind_min = Ordered_CurveDate.IndexOf(point_min);
            //        Ordered_CurveDate.Insert(ind_min + 1, new DVHPoint(dvh_curvedata.MinDose, 100, "%"));
            //    }
            //}
            //else
            //{
            //    Log.Verbose("Missing complete DVH: Only {origin:F15}% volume exist for structure {str}", Ordered_CurveDate.First().Volume, str_VMS.Id);
            //}
            //Ordered_CurveDate.ForEach(t => Console.WriteLine("D: {0}; \t\t Volume: {1}", t.DoseValue.Dose, t.Volume));

            DVHCurve curve = new DVHCurve(Ordered_CurveDate.Select(x => new PointXY { X = x.DoseValue.Dose, Y = x.Volume }).ToList(),
                    optimizerDVH.Structure.Volume,
                    TotalDose_Planned,
                    DoseUnitType.Gy,
                    VolumeUnitType.percent
                    )
            { Structure = str_VMS.Id };

            //Log.Verbose("{structure} DVH curve Retrieved: {curve}", str_VMS.Id, JsonConvert.SerializeObject(curve.DVHPointsList_31, Keep2digitsConvertor));

            return curve;
        }


        public static FloatJsonConverter Keep2digitsConvertor = new FloatJsonConverter();
    }

    public class FloatJsonConverter : JsonConverter
    {
        private string _FFormatString;

        public FloatJsonConverter(string FFormatString = "F2")
        {
            _FFormatString = FFormatString;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(double);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue(((double)value).ToString(_FFormatString, System.Globalization.CultureInfo.InvariantCulture));
        }
    }


    //public class con_evaluation
    //{
    //    public double achieved_value { get; set; }
    //    public constraint con { get; set; }
    //    public double conGEM { get; set; }
    //    public double OPT_priority { get; set; }
    //}

    public class str_evaluation
    {
        public String StructureName { get; set; }
        public double gEUD = double.NaN;
        public double WES = double.NaN;
        public double strGEM = double.NaN;
        public double strGEMpop = double.NaN;
        public double StrObjFuncValue = double.NaN;
        //public Structure strVMS { get; set; }
        //public DVHData dvhVMS { get; set; }

        [JsonIgnore]
        public DVHCurve dvh { get; set; }

        public List<RxConstraint> con_evas = new List<RxConstraint>();
    }

    /// <summary>
    /// An object that summarizes all the relevant information about the stage of the optimization, including achieved metrics values for each structure and constraint.
    /// </summary>
    public class planDVH_evaluation
    {
        public double PlanGEM { get; set; }
        public double PlanGEMpop { get; set; }
        public List<str_evaluation> str_evas = new List<str_evaluation>();
        public double TotalObjectiveFunctionValue = double.NaN;
        public int NumberOfIMRTOptimizerIterations;
        public double TotalDose = double.NaN;
        public static string datasource;
        public static int fraction;
        public static int JSON_set_fraction;

        public void save_to_JSON(string fileName, string label, bool if_append = false)
        {
            //string pejson = JsonConvert.SerializeObject(new { Label = label, planDVH_evaluation_summary = this}, new FloatJsonConverter("F4"));
            string pejson = JsonConvert.SerializeObject(this, new FloatJsonConverter("F4"));

            System.IO.File.WriteAllText(path: fileName, contents: pejson);
        }

        public List<double> con_ofvs {
            get {
                return str_evas.Where(t => !t.StructureName.is_zOptPTV_xxx_L() && t.con_evas.Any(c => c.OPT_priority > 0.1)).Select(t => t.StrObjFuncValue).ToList();

                //return str_evas.Where(t => t.con_evas.Any()).SelectMany(t => t.con_evas).Where(t => !t.StructureID.is_zOptPTV_xxx_L() && t.OPT_priority > 0.1).Select(t => t.ObjFuncValue).ToList();

                // in Standard Eclipse, this OFV list is on per-structure level; While in Research Eclipse, it is per-constraint. 
                // Here each structure is weighted equally, regardless it has 1 con or 3 cons.
                //var rv = str_evas.SelectMany(str => str.con_evas, (str, con) => new { str, con }).Where(t => !t.str.StructureName.is_zOptPTV_xxx_L() && t.con.OPT_priority > 0.1).Select(t => new { t.str.StructureName, t.str.StrObjFuncValue}).Distinct();
                //return rv.Select(t => t.StrObjFuncValue).ToList();
            }
        }

        public List<double> con_ofvs_ptv
        {
            get
            {
                return str_evas.Where(t => Regex.Match(t.StructureName, "^(z1)?zOptPTV_[highlowmid]{3,4}(_H)?$", RegexOptions.IgnoreCase).Success && t.con_evas.Any(c => c.OPT_priority > 0.1)).Select(t => t.StrObjFuncValue).ToList();

                //return str_evas.Where(t => t.con_evas.Any()).SelectMany(t => t.con_evas).Where(t => Regex.Match(t.StructureID, "^(z1)?zOptPTV_[highlowmid]{3,4}(_H)?$", RegexOptions.IgnoreCase).Success && t.OPT_priority > 0.1).Select(t => t.ObjFuncValue).ToList();

                // in Standard Eclipse, this OFV list is on per-structure level; While in Research Eclipse, it is per-constraint. 
                //var rv = str_evas.SelectMany(str => str.con_evas, (str, con) => new { str, con }).Where(t => Regex.Match(t.str.StructureName, "^zOptPTV_[highlowmid]*[_Hh]{0,2}$", RegexOptions.IgnoreCase).Success && t.con.OPT_priority > 0.1).Select(t => new { t.str.StructureName, t.str.StrObjFuncValue}).Distinct();
                //return rv.Select(t => t.StrObjFuncValue).ToList();
            }
        }

        public double ofv_average
        {
            get { return con_ofvs.Any() ? con_ofvs.Average() : double.NaN; }
        }

        public double ofv_average_ptv 
        {
            get { 
                return con_ofvs_ptv.Any() ? con_ofvs_ptv.Average() : double.NaN;
            }
        }

        public double ofv_ptv_max
        {
            get
            {
                return con_ofvs_ptv.Any() ? con_ofvs_ptv.Max() : double.NaN;
            }
        }

        static StatsDVH_ForOpt _sDVHs;

        public static StatsDVH_ForOpt sDVHs
        {
            get
            {
                if (_sDVHs is null || _sDVHs.datasource != planDVH_evaluation.datasource || _sDVHs.Nfraction != JSON_set_fraction) return (_sDVHs = new StatsDVH_ForOpt(planDVH_evaluation.datasource, JSON_set_fraction, Pre_JSON_dir: Config.Precalculated_JSON_files_dir));

                return _sDVHs;
            }
        }


        double ofv_change;
        double ofv_change_Percent;
        double ofv_percent_since_iterStart;

        static double lastObjFuncValue;
        static double ofv_init_at_this_iter;
        static int niter, ind;

        //static Serilog.Core.Logger lgr1 = new LoggerConfiguration()
        //                .MinimumLevel.Verbose()
        //                .MinimumLevel.Override("Microsoft", LogEventLevel.Fatal)
        //                //.WriteTo.Console()
        //                //.WriteTo.File(@"\\uhrofilespr1\EclipseScripts\Aria15\Development\JY\AP_Clinic\OFV_log_" + DateTime.Now.ToString("yyyy-MM-dd__HH-mm-ss") + ".txt", outputTemplate: "{Message:lj}{NewLine}")
        //                .CreateLogger();

        public void collapse_1n2_auxiliary_strs()
        {
            //str_evas.ForEach(t => { if(t.con_evas.Count ==1) t.con_evas.Single().ObjFuncValue = t.StrObjFuncValue});

            var orig_str_names = str_evas.Where(t => t.con_evas.Any(c => c.strn_orig == c.StructureID)).Select(t => t.StructureName).ToList();

            foreach ( string strn in orig_str_names)
            {
                var str_eva_o = str_evas.Single(t => t.StructureName == strn);
                var str_eva_1n2 = str_evas.Where(t => t.StructureName != strn && t.con_evas.Any(c => c.strn_orig == strn)).OrderBy(t => t.StructureName);
                
                str_eva_o.con_evas.AddRange(str_eva_1n2.SelectMany(t => t.con_evas));
                str_eva_o.StrObjFuncValue = str_eva_o.con_evas.Select(t => t.ObjFuncValue).Sum();

                str_eva_1n2.ToList().ForEach(t => str_evas.Remove(t));
            }
        }

        public void print(List<OAR_PTV_overlap> strn_ptv_overlap_dict = null, bool if_diff = false, bool if_over_area = false, bool if_log_OFV = false, string n_OPT_n_niter = "_", bool if_highlight_unmet_prio1_str = false)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Rx Doses: {1}; NFraction: {2}; Total PlanGEM: {0:F2}; Total PlanGEMpop: {4:F2}; Historical context: {3}; "
                 // + (double.IsNaN(TotalObjectiveFunctionValue) ? "-- dose read from PlanSetup." : String.Format("TotalObjectiveFunctionValue: {0:E3} - #IMRT_Iterations: {1}", TotalObjectiveFunctionValue, NumberOfIMRTOptimizerIterations))
                 + (double.IsNaN(TotalObjectiveFunctionValue) ? "" : String.Format("TotalObjectiveFunctionValue: {0:E3}", TotalObjectiveFunctionValue))
                , PlanGEM, TotalDose, fraction, datasource, PlanGEMpop);


            if (ofv_change >= 0) Console.ForegroundColor = ConsoleColor.Red; else Console.ForegroundColor = ConsoleColor.Green;
            //Console.Write("ObjFunc change: \t{0:e2};\t in percent\t{1:F2}%\t; OFV of iteration starts: {2:F2}%\t", ofv_change, ofv_change_Percent, ofv_percent_since_iterStart);
            Console.ForegroundColor = ConsoleColor.White;

            var con_ofvs = this.con_ofvs;
            var ofv_average = this.ofv_average;

            var con_ofvs_ptv = this.con_ofvs_ptv;
            var ofv_average_ptv = this.ofv_average_ptv;
            var ofv_ptv_max = this.ofv_ptv_max;

            Console.WriteLine("Average OFV per str: {0:E3} over {1} strs; Average OFV for PTV: {2:E3}, over {3} PTVs\n", ofv_average, con_ofvs.Count, ofv_average_ptv, con_ofvs_ptv.Count);

            if (!str_evas.Any()) { Console.WriteLine("No str_evaluation in this plan."); return; }

            var str_names = str_evas.Where(t => t.con_evas.Any()).Select(t => t.StructureName.ToUpper()).ToList();

            var nstr_zPTV_H = str_names.Where(t => Regex.Match(t, "^ZOPTPTV_HIGH(|_H|_L)$").Success).OrderBy(t => t).ToList();
            var nstr_zPTV_M = str_names.Where(t => Regex.Match(t, "^ZOPTPTV_MID(|_H|_L)$").Success).OrderBy(t => t).ToList();
            var nstr_zPTV_L = str_names.Where(t => Regex.Match(t, "^ZOPTPTV_LOW(|_H|_L)$").Success).OrderBy(t => t).ToList();

            var nstr_zOPT = str_names.Where(t => t.Contains("ZOPT")).OrderBy(t => t).ToList();
            if (nstr_zOPT.Count == 3) nstr_zOPT = new List<string> { nstr_zOPT[0], nstr_zOPT[2], nstr_zOPT[1] };

            var nstr_zDLA = str_names.Where(t => t.Contains("ZDLA")).ToList();
            if (nstr_zDLA.Count == 3) nstr_zDLA = new List<string> { nstr_zDLA[0], nstr_zDLA[2], nstr_zDLA[1] };

            var nstr_PTV = str_names.Where(t => t.Contains("PTV")).Except(nstr_zOPT).OrderBy(t => t).ToList();
            if (nstr_PTV.Count == 3) nstr_PTV = new List<string> { nstr_PTV[0], nstr_PTV[2], nstr_PTV[1] };
            //var nstr_CTV = str_names.Where(t => t.Contains("CTV")).Except(nstr_zOPT);
            //var nstr_GTV = str_names.Where(t => t.Contains("GTV")).Except(nstr_zOPT);
            //var nstr_ITV = str_names.Where(t => t.Contains("ITV")).Except(nstr_zOPT);

            var nstr_Body = str_evas.Where(t => t.StructureName.ToUpper().Contains("BODY")).Select(t => t.StructureName.ToUpper()).ToList();

            var nstr_ordered = nstr_zPTV_H.Concat(nstr_zPTV_M).Concat(nstr_zPTV_L).Concat(nstr_PTV).Concat(nstr_zDLA).Concat(nstr_Body).Concat(str_names.Except(nstr_zPTV_H).Except(nstr_zPTV_M).Except(nstr_zPTV_L).Except(nstr_PTV).Except(nstr_zDLA).OrderBy(t => t));

            Console.BackgroundColor = ConsoleColor.Blue;
            Console.WriteLine("Structure       Max[Gy] Mean    Min[Gy] Volume  PTV_overlappings        WES     strOFV    GEM   GEMpop  Constraint    Achieved  OA  qtl prio OFV     [MA, cvrg for PTV]     ... Constraint repeat ...     [HA for PTV]");

            Console.BackgroundColor = ConsoleColor.Black;
            int i_line = 0;
            foreach (string nstr in nstr_ordered)
            //foreach (str_evaluation str in str_evas)
            {
                str_evaluation str_e = str_evas.First(t => t.StructureName.ToUpper() == nstr);

                if (if_highlight_unmet_prio1_str == true && str_e.strGEM > 0.5 && (str_e.con_evas.Select(c => c.priority).Min() == 1 || str_e.con_evas.Select(c => c.OPT_priority).Max() >= Config.prio_1_5)) Console.BackgroundColor = ConsoleColor.DarkCyan; else Console.BackgroundColor = ConsoleColor.Black;

                if (str_e.StructureName.ToUpper().Contains("PTV")) Console.ForegroundColor = ConsoleColor.Red;
                else Console.ForegroundColor = ConsoleColor.White;

                Console.Write(str_e.StructureName.Length >= 16 ? "{0}" : (str_e.StructureName.Length >= 8 ? "{0}\t" : "{0}\t\t"), str_e.StructureName);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("{0:F2}\t{1:F2}\t{2:F2}\t", str_e.dvh.Max_Gy, str_e.dvh.Mean_Gy, str_e.dvh.Min_Gy);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write((str_e.dvh.Volume >= 10000 ? "{0:F1}\t" : "{0:F2}\t"), str_e.dvh.Volume);
                if (strn_ptv_overlap_dict != null && strn_ptv_overlap_dict.Any(t => t.StructureName == str_e.StructureName)) Console.Write(strn_ptv_overlap_dict.Single(t => t.StructureName == str_e.StructureName).ToString());
                else Console.Write(string.Format("{0, -18}", " cannot test"));
                Console.Write("\t");
                if (str_e.WES > 0.5) Console.ForegroundColor = ConsoleColor.Red; else Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("{0:F2}\t", str_e.WES);
                if (str_e.StrObjFuncValue >= ofv_ptv_max) Console.ForegroundColor = ConsoleColor.White;
                else if (str_e.StrObjFuncValue > ofv_average) Console.ForegroundColor = ConsoleColor.Red;
                else if (str_e.StrObjFuncValue > ofv_average / 2) Console.ForegroundColor = ConsoleColor.Yellow;
                else Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("{0,-10}", str_e.StrObjFuncValue.ToString("E2"));  // if StrOFV color scheme.
                if (str_e.strGEM > 0.5) Console.ForegroundColor = ConsoleColor.Red; else Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("{0:F2}\t", str_e.strGEM);
                if (str_e.strGEMpop > 0.5) Console.ForegroundColor = ConsoleColor.Red; else Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("{0:F2}\t", str_e.strGEMpop);

                foreach (RxConstraint con in str_e.con_evas.OrderByDescending(c => c.metric_parameter))
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("{0,-15}", con.ToString());

                    Console.ForegroundColor = ConsoleColor.Green;
                    if ((con.achieved_value > con.limit && con.ooo == OptimizationObjectiveOperator.Upper) |
                        (con.achieved_value < con.limit && con.ooo == OptimizationObjectiveOperator.Lower)) { Console.ForegroundColor = ConsoleColor.Red; }
                    if (if_diff) Console.Write("{0:00.00}", con.achieved_value - con.limit); else Console.Write("{0:00.00}", con.achieved_value);
                    double over_area = con.ooo == OptimizationObjectiveOperator.Upper ? str_e.dvh.Over_con_area(con) : (1 - str_e.dvh.Coverage_area(con.limit)) * con.limit;
                    if (if_over_area) { Console.Write(" {0:F2}Gy ", over_area); }

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("{0,3} ", Math.Round(con.dose_qtl, 0));
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(" {0,2} ", con.OPT_priority);

                    if (con.ObjFuncValue >= ofv_ptv_max) Console.ForegroundColor = ConsoleColor.White;
                    else if (con.ObjFuncValue > ofv_average) Console.ForegroundColor = ConsoleColor.Red;
                    else if (con.ObjFuncValue > ofv_average / 2) Console.ForegroundColor = ConsoleColor.Yellow;
                    else Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("{0,-10}", con.ObjFuncValue.ToString("E2")); // Not useful in standard Eclipse version, since all constraint level OFV is NaN.
                    Console.ForegroundColor = ConsoleColor.White;

                    //if (if_log_OFV == true) lgr1.Information("{str}\t{priority}\t{limit}\t{achieved}\t{over_area}\t{ofv}\t{ooo}\t{volume_cc}\t{con_param}\t{nOPT}\t{IMRTn}\t{niter}\t{totalOFV}", str_e.StructureName, con.OPT_priority, con.limit, con.achieved_value, over_area, str_e.StrObjFuncValue, con.ooo.ToString(), str_e.dvh.Volume, con.metric_parameter, n_OPT_n_niter.Split('_')[0], NumberOfIMRTOptimizerIterations, n_OPT_n_niter.Split('_')[1], TotalObjectiveFunctionValue);

                    //if (str.StructureName == "zOptPTV_High" | str.StructureName == "zOptPTV_Low" | str.StructureName == "zOptPTV_Mid")
                    if (str_e.StructureName.Contains("PTV"))
                    {
                        if (con.ooo == OptimizationObjectiveOperator.Lower && con.metric_parameter == 100)
                        {
                            double dose_target = con.limit;
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write("MA: {0:F2}% ", 100 - str_e.dvh.Coverage_area(dose_target) * 100); // Missed dose coverage area size

                            Console.Write("Cvrg: {0:F2}%\t", DVHMetrics.DVHMetricValue(str_e.dvh, DVHMetricType.VxGy_Percent, dose_target));

                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        if (con.ooo == OptimizationObjectiveOperator.Upper && con.metric_parameter == 0)
                        {
                            double dose_limit = con.limit;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("HA: {0:F2}%", str_e.dvh.Hotspot(dose_limit) * 100);  // over hot dose area size
                            //Console.Write("HA: {0:F2}% HA2: {1:F2}%", str.dvh.Hotspot(dose_limit) * 100, over_area / con.limit * 100);  // over hot dose area size
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                }
                Console.Write("\n");
                Console.ForegroundColor = ConsoleColor.White; Console.BackgroundColor = ConsoleColor.Black;
                if ((i_line + 1) % 5 == 0) Console.Write("-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------\n");
                i_line++;
            }

            if (if_highlight_unmet_prio1_str)
            {
                Console.Write("\nHighlighted "); Console.BackgroundColor = ConsoleColor.DarkCyan; Console.Write("StructureName"); Console.BackgroundColor = ConsoleColor.Black; Console.WriteLine(" are structures with unsatisfied priority 1 constraint from Planning Directive.");
            }
        }

        /// <summary>
        /// build a planDVH_evaluation object directly from OptimizerResult
        /// </summary>
        /// <param name="pi">ExternalPlanSetup</param>
        /// <param name="datasource">"HN_MROAR"</param>
        /// <param name="JSON_set_fraction">use 0 for CRT, 3 or 5 for SBRT</param>
        /// <param name="optresult">Where most of the data needed come from, including OFV, dvh, con</param>
        /// <param name="RxCons">This one provide its at_dose_quantile property for displaying the adjustment of dose limits</param>
        public planDVH_evaluation(ExternalPlanSetup pi, string datasource, int JSON_set_fraction, OptimizerResult optresult, List<RxConstraint> RxCons)
        {
            bool is_there_RxCons = true;
            if (RxCons is null || RxCons.Count == 0) { is_there_RxCons = false; }

            planDVH_evaluation.datasource = datasource;
            planDVH_evaluation.fraction = pi.NumberOfFractions ?? 0; 
            TotalObjectiveFunctionValue = optresult.TotalObjectiveFunctionValue;
            NumberOfIMRTOptimizerIterations = optresult.NumberOfIMRTOptimizerIterations;

            ofv_change = optresult.TotalObjectiveFunctionValue - lastObjFuncValue;
            ofv_change_Percent = ofv_change / lastObjFuncValue * 100;

            if (ind == 0 || optresult.NumberOfIMRTOptimizerIterations != niter)
            {
                ofv_init_at_this_iter = optresult.TotalObjectiveFunctionValue;
                niter = optresult.NumberOfIMRTOptimizerIterations;
            }

            ofv_percent_since_iterStart = optresult.TotalObjectiveFunctionValue / ofv_init_at_this_iter * 100;

            ind++;
            lastObjFuncValue = optresult.TotalObjectiveFunctionValue;

            TotalDose = (pi is ExternalPlanSetup) ? ((ExternalPlanSetup)pi).TotalDose.getDoseInGy() : double.NaN;
            StructureSet strsVMS = pi.StructureSet;

            //var sDVHs = new StatsDVH_ForOpt(datasource, fraction, Pre_JSON_dir: Config.Precalculated_JSON_files_dir);

            var strs_GEMinput = new List<GEM_metric>();
            var strs_GEMinput_pop = new List<GEM_metric>();
            serialize.StatsDVH_info sDVH;

            var all_strs_no_bolus = strsVMS.Structures.Where(s => s.Volume > 0 && !s.Id.ToLower().Contains("bolus")).OrderBy(t => t.Id);

            foreach (Structure str in all_strs_no_bolus)
            {
                DVHCurve dvh = RetrieveFromEclipse15.Get_DVH_From_OptimizerResult(optresult, str, TotalDose);

                if (dvh == null) continue;

                List<OptimizationObjective> objs = pi.OptimizationSetup.Objectives.Where(x => x.StructureId == str.Id).ToList();

                List<RxConstraint> con_evas = new List<RxConstraint>();

                double strOFV = optresult.StructureObjectiveValues.Single(t => t.Structure.Id == str.Id).Value;

                foreach (OptimizationObjective obj in objs)
                {
                    var RxCon = RxCons.SingleOrDefault(t => t.objHashCode == obj.GetHashCode());
                    if (obj is OptimizationPointObjective)
                    {
                        var obj_point = obj as OptimizationPointObjective;
                        double res1_value = double.NaN; // optresult.ObjectiveValues[obj.Id];
                        double achieved_value = DVHMetrics.DVHMetricValue(dvh, DVHMetricType.DxPercent_Gy, obj_point.Volume);
                        double obj_limit = obj_point.Dose.getDoseInGy();

                        con_evas.Add(new RxConstraint(str.Id, obj.Operator, obj_point.Volume, obj_point.Dose.getDoseInGy(), obj.Priority, is_there_RxCons ? RxCon.strn_orig : "")
                        { achieved_value = achieved_value, ObjFuncValue = res1_value, objHashCode = obj.GetHashCode(), dose_qtl = is_there_RxCons ? RxCon.dose_qtl : double.NaN, priority = is_there_RxCons ? RxCon.priority : 0, metric_type_orig = is_there_RxCons ? RxCon.metric_type_orig : null });
                    }
                    if (obj is OptimizationMeanDoseObjective)
                    {
                        var obj_mean = obj as OptimizationMeanDoseObjective;
                        double res1_value = double.NaN; // optresult.ObjectiveValues[obj.Id];
                        double achieved_value = DVHMetrics.DVHMetricValue(dvh, DVHMetricType.Mean_Gy, double.NaN);
                        double obj_limit = obj_mean.Dose.getDoseInGy();

                        con_evas.Add(new RxConstraint(str.Id, obj.Operator, double.NaN, obj_mean.Dose.getDoseInGy(), obj.Priority)
                        { achieved_value = achieved_value, ObjFuncValue = res1_value, objHashCode = obj.GetHashCode(), metric_type = "Mean_Gy", metric_parameter = -1, dose_qtl = is_there_RxCons ? RxCon.dose_qtl : double.NaN, priority = is_there_RxCons ? RxCon.priority : 0, metric_type_orig = is_there_RxCons ? RxCon.metric_type_orig : null });
                    }
                }

                var str1_GEMinput = calc.generate_GEMinputList_of_a_curve(dvh, constraint.filter_cons(datasource, dvh.Structure, JSON_set_fraction, "DF"));
                strs_GEMinput.AddRange(str1_GEMinput);

                var str1_GEMinput_pop = calc.generate_GEMinputList_of_a_curve(dvh, constraint.filter_cons(datasource, dvh.Structure, JSON_set_fraction, "MC"));
                strs_GEMinput_pop.AddRange(str1_GEMinput_pop);

                str_evas.Add(new str_evaluation()
                {
                    StructureName = str.Id,
                    strGEM = MathFunctions.GEM(str1_GEMinput),
                    strGEMpop = MathFunctions.GEM(str1_GEMinput_pop),
                    WES = DVHMetrics.WES_calcuation_from_PreStatsDVH(dvh, sDVHs.str_sDVH_dict.TryGetValue(str.Id.Match_StrID_to_Standard_Name(datasource).Title_Case(), out sDVH) ? sDVH : null, "NO"),
                    dvh = dvh,
                    con_evas = con_evas,
                    StrObjFuncValue = strOFV
                });
            }
            PlanGEM = MathFunctions.GEM(strs_GEMinput);
            PlanGEMpop = MathFunctions.GEM(strs_GEMinput_pop);
        }

        /// <summary>
        /// build a planDVH_evaluation object directly from ExternalPlanSetup
        /// </summary>
        /// <param name="pi">Planning item</param>
        /// <param name="datasource">"HN_MROAR"</param>
        /// <param name="JSON_set_fraction">use 0 for CRT, 3 or 5 for SBRT</param>
        /// <param name="RxCons">This one provide its at_dose_quantile property for displaying the adjustment of dose limits</param>
        public planDVH_evaluation(ExternalPlanSetup pi, string datasource, int JSON_set_fraction, List<RxConstraint> RxCons)
        {
            bool is_there_RxCons = true;
            if(RxCons is null || RxCons.Count == 0 ) { is_there_RxCons = false; }

            planDVH_evaluation.datasource = datasource; planDVH_evaluation.fraction = pi.NumberOfFractions ?? 0;
            TotalObjectiveFunctionValue = double.NaN;
            TotalDose = (pi is ExternalPlanSetup) ? ((ExternalPlanSetup)pi).TotalDose.getDoseInGy() : double.NaN;
            StructureSet strsVMS = pi.StructureSet;

            var strs_GEMinput = new List<GEM_metric>();
            var strs_GEMinput_pop = new List<GEM_metric>();
            serialize.StatsDVH_info sDVH;

           var all_strs_no_bolus = strsVMS.Structures.Where(s => s.Volume > 0 && !s.Id.ToLower().Contains("bolus")).OrderBy(t => t.Id);

            foreach (Structure str in all_strs_no_bolus)
            { 
                var dvh = RetrieveFromEclipse15.Get_DVH_From_PlanningItem(pi, str, TotalDose);

                if (dvh == null) continue;

                List<OptimizationObjective> objs = pi.OptimizationSetup.Objectives.Where(x => x.StructureId == str.Id).ToList();

                List<RxConstraint> con_evas = new List<RxConstraint>();

                foreach (OptimizationObjective obj in objs)
                {
                    var RxCon = RxCons.SingleOrDefault(t => t.objHashCode == obj.GetHashCode());
                    if (obj is OptimizationPointObjective)
                    {
                        var obj_point = obj as OptimizationPointObjective;
                        double res1_value = double.NaN;
                        double achieved_value = DVHMetrics.DVHMetricValue(dvh, DVHMetricType.DxPercent_Gy, obj_point.Volume);
                        double obj_limit = obj_point.Dose.getDoseInGy();

                        con_evas.Add(new RxConstraint(str.Id, obj.Operator, obj_point.Volume, obj_point.Dose.getDoseInGy(), obj.Priority, is_there_RxCons ? RxCon.strn_orig : "")
                        { achieved_value = achieved_value, ObjFuncValue = res1_value, objHashCode = obj.GetHashCode(), dose_qtl = is_there_RxCons ? RxCon.dose_qtl : double.NaN, priority = is_there_RxCons ? RxCon.priority : 0, metric_type_orig = is_there_RxCons ? RxCon.metric_type_orig : null });
                    }

                    if (obj is OptimizationMeanDoseObjective)
                    {
                        var obj_mean = obj as OptimizationMeanDoseObjective;
                        double res1_value = double.NaN;
                        double achieved_value = DVHMetrics.DVHMetricValue(dvh, DVHMetricType.Mean_Gy, double.NaN);
                        double obj_limit = obj_mean.Dose.getDoseInGy();

                        con_evas.Add(new RxConstraint(str.Id, obj.Operator, double.NaN, obj_mean.Dose.getDoseInGy(), obj.Priority)
                        { achieved_value = achieved_value, ObjFuncValue = res1_value, objHashCode = obj.GetHashCode(), metric_type = "Mean_Gy", dose_qtl = is_there_RxCons ? RxCon.dose_qtl : double.NaN, priority = is_there_RxCons ? RxCon.priority : 0, metric_type_orig = is_there_RxCons ? RxCon.metric_type_orig : null });
                    }
                }

                var str1_GEMinput = calc.generate_GEMinputList_of_a_curve(dvh, constraint.filter_cons(datasource, dvh.Structure, JSON_set_fraction, "DF"));
                strs_GEMinput.AddRange(str1_GEMinput);

                var str1_GEMinput_pop = calc.generate_GEMinputList_of_a_curve(dvh, constraint.filter_cons(datasource, dvh.Structure, JSON_set_fraction, "MC"));
                strs_GEMinput_pop.AddRange(str1_GEMinput_pop);

                var RxCon1_gEDU = RxCons.FirstOrDefault(t => t.StructureID == str.Id && t.tag == RxCon_types.gEUD);
                
                double gEUD = double.NaN;
                if(RxCon1_gEDU != null)
                {
                    gEUD = DVHMetrics.gEUD(dvh, 1 / RxCon1_gEDU.gEUD_a);
                    //Console.WriteLine($"{str.Id} gEUD: {gEUD}; a: {RxCon1_gEDU.gEUD_a}; Mean[Gy]: {dvh.Mean_Gy}");
                }

                str_evas.Add(new str_evaluation()
                {
                    StructureName = str.Id,
                    strGEM = MathFunctions.GEM(str1_GEMinput),
                    strGEMpop = MathFunctions.GEM(str1_GEMinput_pop),
                    WES = DVHMetrics.WES_calcuation_from_PreStatsDVH(dvh, sDVHs.str_sDVH_dict.TryGetValue(str.Id.Match_StrID_to_Standard_Name(datasource).Title_Case(), out sDVH) ? sDVH : null, "NO"),
                    dvh = dvh,
                    con_evas = con_evas,
                    StrObjFuncValue = double.NaN,
                    gEUD = gEUD
                });
            }
            PlanGEM = MathFunctions.GEM(strs_GEMinput);
            PlanGEMpop = MathFunctions.GEM(strs_GEMinput_pop);
        }

        public planDVH_evaluation(ExternalPlanSetup pi, string datasource, int fraction)
        {
            planDVH_evaluation.datasource = datasource; planDVH_evaluation.fraction = pi.NumberOfFractions ?? 0;
            TotalObjectiveFunctionValue = double.NaN;
            TotalDose = (pi is ExternalPlanSetup) ? ((ExternalPlanSetup)pi).TotalDose.getDoseInGy() : double.NaN;
            StructureSet strsVMS = pi.StructureSet;

            // load Statistical DVHs
            var sDVHs = new StatsDVH_ForOpt(datasource, fraction, Pre_JSON_dir: Config.Precalculated_JSON_files_dir);

            var strs_GEMinput = new List<GEM_metric>();
            var strs_GEMinput_pop = new List<GEM_metric>();
            serialize.StatsDVH_info sDVH = null;

            foreach (Structure str in strsVMS.Structures.OrderBy(t => t.Id))
            {
                //Console.WriteLine(str.Id);

                var dvh = RetrieveFromEclipse15.Get_DVH_From_PlanningItem(pi, str, TotalDose);
                if (dvh == null) continue;

                sDVHs.str_sDVH_dict.TryGetValue(str.Id.Match_StrID_to_Standard_Name(datasource).Title_Case(), out sDVH);

                List<OptimizationObjective> objs = pi.OptimizationSetup.Objectives.Where(x => x.StructureId == str.Id).ToList();

                List<RxConstraint> con_evas = new List<RxConstraint>();

                foreach (OptimizationObjective obj in objs)
                {
                    if (obj is OptimizationPointObjective)
                    {
                        //Console.WriteLine(obj.Id);
                        var obj_point = obj as OptimizationPointObjective;
                        double res1_value = double.NaN;
                        double achieved_value = DVHMetrics.DVHMetricValue(dvh, DVHMetricType.DxPercent_Gy, obj_point.Volume);
                        double obj_limit = obj_point.Dose.getDoseInGy();

                        point_on_StatsDVH p = sDVHs.find_qntl_for_point_on_StatsDVH(str: str.Id, at_volume_percent: obj_point.Volume, with_dose: obj_limit);

                        con_evas.Add(new RxConstraint(str.Id, obj.Operator, obj_point.Volume, obj_point.Dose.getDoseInGy(), obj.Priority)
                        { achieved_value = achieved_value, ObjFuncValue = res1_value, dose_qtl = p.at_quantile });
                    }
                }

                var str1_GEMinput = calc.generate_GEMinputList_of_a_curve(dvh, constraint.filter_cons(datasource, dvh.Structure, fraction, "DF"));
                strs_GEMinput.AddRange(str1_GEMinput);

                var str1_GEMinput_pop = calc.generate_GEMinputList_of_a_curve(dvh, constraint.filter_cons(datasource, dvh.Structure, fraction, "MC"));
                strs_GEMinput_pop.AddRange(str1_GEMinput_pop);

                str_evas.Add(new str_evaluation()
                {
                    StructureName = str.Id,
                    strGEM = MathFunctions.GEM(str1_GEMinput),
                    strGEMpop = MathFunctions.GEM(str1_GEMinput_pop),
                    WES = DVHMetrics.WES_calcuation_from_PreStatsDVH(dvh, sDVH, "NO"),
                    dvh = dvh,
                    con_evas = con_evas,
                    StrObjFuncValue = double.NaN
                });
            }
            PlanGEM = MathFunctions.GEM(strs_GEMinput);
            PlanGEMpop = MathFunctions.GEM(strs_GEMinput_pop);
        }
    }

    public class RxConstraint : constraint 
    {
        public RxConstraint() { }

        public RxConstraint(constraint t)
        {
            this.StructureID = t.StructureID;
            this.metric_type = t.metric_type;
            this.metric_parameter = t.metric_parameter;
            this.limit = t.limit;
            this.k = t.k;
            this.theta = t.theta;
            this.fraction = t.fraction;
            this.label = t.label;
            this.priority = t.priority;
            this.priority_decimal = t.priority_decimal;
            this.source = t.source;
        }

        public RxConstraint(string StructureID, OptimizationObjectiveOperator ooo, double at_percentage_volume, double limit, DVHMetricType metric_type, decimal priority_decimal)
        {
            this.StructureID = StructureID;
            this.metric_type = metric_type.ToString();
            this.ooo = ooo; 
            this.metric_parameter = at_percentage_volume; 
            this.limit = limit;
            this.priority_decimal = priority_decimal;
        }

        public RxConstraint(string StructureID, OptimizationObjectiveOperator ooo, double at_percentage_volume, double limit)
        {
            this.StructureID = StructureID;
            //metric_type = DVHMetricType.DxPercent_Gy.ToString();
            this.ooo = ooo; this.metric_parameter = at_percentage_volume; this.limit = limit; 
            achieved_value = double.NaN;
            dose_qtl = double.NaN;
            strn_orig = StructureID;
        }

        public RxConstraint(string StructureID, OptimizationObjectiveOperator ooo, double at_percentage_volume, double limit, double OPT_priority, DVHMetricType metric_type = DVHMetricType.DxPercent_Gy) : this(StructureID, ooo, at_percentage_volume, limit)
        {
            this.OPT_priority = OPT_priority;
            this.metric_type = metric_type.ToString();
        }

        public RxConstraint(string StructureID, OptimizationObjectiveOperator ooo, double at_volume, double limit, double OPT_priority, string strn_orig)
            : this(StructureID, ooo, at_volume, limit, OPT_priority)
        {
            this.strn_orig = strn_orig;
        }

        public bool if_limit_fixed { get; set; } = false;
        public bool if_break_down_Mean_Gy { get; set; } = true;

        public OAR_PTV_overlap opol { get; set; }
        public string opol_string 
        { 
            get {
                //return opol.shortString;
                return opol.ToString();
            }
        }

        public double volume { get { return opol.volume; } }

        [JsonProperty("prio_offset")]
        public decimal prio_offset { get; set; }

        [JsonProperty("gEUD_a")]
        public double gEUD_a { get; set; }
        [JsonProperty("OPT_priority")]
        public double OPT_priority { get; set; }
        [JsonProperty("Rx_scale")]
        public double Rx_scale { get; set; }
        public double dose_qtl { get; set; }
        public double achieved_value { get; set; }
        public double ObjFuncValue { get; set; }
        public double conGEM { get; set; }
        [JsonProperty("ooo")]
        public OptimizationObjectiveOperator ooo { get; set; }
        public string objID { get; set; }
        [JsonProperty("tag")]
        public string tag { get; set; }
        public int objHashCode { get; set; }
        //public OptimizationPointObjective obj { get; set; }
        public string metric_type_orig { get; set; }
        public string strn_orig { get; set; }

        public override string ToString()
        {
            string temp = base.ToString();
            if (ooo == OptimizationObjectiveOperator.Lower)
            {
                temp = temp.Replace('<', '>');
            }
            return temp;
        }
            

        //public static double con_priority_to_opt_priority(decimal prio_d, string metric_type)
        //{
        //    if (prio_d == (decimal)1) { return 100; } // { return metric_type == DVHMetricType.Mean_Gy.ToString() ? 80 : 100; }
        //    if (prio_d == (decimal)1.5) { return 90; }
        //    if (prio_d == (decimal)2) { return 80; }
        //    if (prio_d == (decimal)2.5) { return 50; }
        //    if (prio_d == (decimal)3) { return 30; }
        //    if (prio_d == (decimal)3.5) { return 20; }
        //    if (prio_d == (decimal)4) { return 10; }
        //    return 0;
        //}

    }

    public static class RxCon_types
    {
        public static string Point_with_fixed_limit = "Point_with_fixed_limit";
        public static string Mean_with_fixed_limit = "Mean_with_fixed_limit";
        public static string gEUD = "gEUD";
    }

    public static class ExtentionMethods
    {
        public static double getDoseInGy(this DoseValue dv)
        {
            if (dv.Unit == DoseValue.DoseUnit.Gy) return dv.Dose;
            if (dv.Unit == DoseValue.DoseUnit.cGy) return dv.Dose / 100;
            return double.NaN;
        }

        public static void Add_OPT_Objective_FromRxConstraint(this PlanSetup ps, RxConstraint RxCon)
        {
            if (!ps.StructureSet.has(RxCon.StructureID))
            {
                string msg = string.Format($"Warning: {RxCon.StructureID} doesn't exist in the structure set used. Objectives about it from Planning Directive are ignored.");
                ConsoleExt.WriteLineWithBackground(msg, ConsoleColor.Red);
                return;
            }

            if (ps.StructureSet.get(RxCon.StructureID).IsEmpty)
            {
                string msg = string.Format($"Warning: structure {RxCon.StructureID} is empty. Its objective [{RxCon}] is ignored.");
                ConsoleExt.WriteLineWithBackground(msg, ConsoleColor.Red);
                return;
            }

            if (RxCon.metric_type == DVHMetricType.DxPercent_Gy.ToString())
            {
                ps.AddPointObjective_FromRxConstraint(RxCon);
            }
            if (RxCon.metric_type == DVHMetricType.Mean_Gy.ToString())
            {
                ps.AddMeanDoseObjective_FromRxConstraint(RxCon);
            }
            if (RxCon.metric_type == DVHMetricType.gEUD.ToString())
            {
                ps.AddgEUDObjective_FromRxConstraint(RxCon);
            }
        }

        public static void AddPointObjective_FromRxConstraint(this PlanSetup ps, RxConstraint RxCon)
        {
             OptimizationPointObjective obj = ps.OptimizationSetup.AddPointObjective(ps.StructureSet.get(RxCon.StructureID),
                        RxCon.ooo, new DoseValue(RxCon.limit, "Gy"), RxCon.metric_parameter, RxCon.OPT_priority);
            
            //RxCon.objID = obj.Id; 

            RxCon.objHashCode = obj.GetHashCode(); // update objective HashCode in RxCons_used
            //RxCon.obj = obj;
        }

        public static void AddMeanDoseObjective_FromRxConstraint(this PlanSetup ps, RxConstraint RxCon)
        {
             var obj = ps.OptimizationSetup.AddMeanDoseObjective(ps.StructureSet.get(RxCon.StructureID),
                        new DoseValue(RxCon.limit, "Gy"), RxCon.OPT_priority);
            //RxCon.objID = obj.Id; 

            RxCon.objHashCode = obj.GetHashCode();
            //RxCon.obj = obj;
        }

        public static void AddgEUDObjective_FromRxConstraint(this PlanSetup ps, RxConstraint RxCon)
        {
            var obj = ps.OptimizationSetup.AddEUDObjective(ps.StructureSet.get(RxCon.StructureID), RxCon.ooo, new DoseValue(RxCon.limit, "Gy"), RxCon.gEUD_a, RxCon.OPT_priority);

            RxCon.objHashCode = obj.GetHashCode();
        }
    }

    public static class Misc2
    {
        public static double log_interpolation(double x, double x2, double x1, double y2, double y1)
        {
            if (y2 < y1)
                throw new Exception("achieved dose < limit dose; cannot do log interpolation.");

            if (x2 <= x1) return y2;
            if (x >= x2) return y2;
            if (x <= x1) return y1;

            double y = Math.Log((Math.E - 1) / (x2 - x1) * (x - x1) + 1) * (y2 - y1) + y1;
            return y;
        }
    }



}
