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

using AutoPlan_WES_HN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnalyticsLibrary2;
using System.Collections.ObjectModel;
using VMS.TPS.Common.Model.Types;
using AP_lib;

namespace AutoPlan_HN
{

    public static partial class Esapi_exts
    {
        public static RxConstraint Apply_to_zPTV(this RxConstraint Rx1_cfg, double Rx_dose)
        {
            return new RxConstraint(Rx1_cfg.StructureID, Rx1_cfg.ooo, Rx1_cfg.metric_parameter, Rx_dose * Rx1_cfg.Rx_scale, Rx1_cfg.OPT_priority);
        }

        public static RxConstraint Apply_to_zPTV(this RxConstraint Rx1_cfg, string strID, double Rx_dose)
        {
            return new RxConstraint(strID, Rx1_cfg.ooo, Rx1_cfg.metric_parameter, Rx_dose * Rx1_cfg.Rx_scale, Rx1_cfg.OPT_priority);
        }

        public static decimal bound(this decimal val, decimal lower, decimal upper)
        {
            if (val < lower) return lower;
            if (val > upper) return upper;
            return val;
        }

    }


    public class Rxcon_extra_n_convert
    {
        public static History_Curves_ForOpt HistoryCurvesInfo;

        public static void Add_extra_generic_constraint(List<RxConstraint> RxCons_Used, List<RxConstraint> constraints_config)
        {
            var extra_cons_to_add_from_config = constraints_config.Where(t => t.tag.ToLower() == "extra_generic_constraint");

            foreach (RxConstraint Rx1_cfg in extra_cons_to_add_from_config)
            {
                RxCons_Used.Add(Rx1_cfg);
            }
        }

        public static void Add_fixed_limit_point_constraints(List<RxConstraint> RxCons_from_VM, List<RxConstraint> constraints_config)
        {
            var point_cons_to_add_from_config = constraints_config.Where(t => t.tag.ToLower() == "fixed_limit_point");

            foreach (RxConstraint Rx1_cfg in point_cons_to_add_from_config)
            {
                var Rxcon_this_str = RxCons_from_VM.Where(t => t.tag == "PD" && t.StructureID.Match_Std_TitleCase() == Rx1_cfg.StructureID);

                if (Rxcon_this_str.Count() == 0) continue;

                Rx1_cfg.priority_decimal = (Rxcon_this_str.Min(t => t.priority_decimal) + Rx1_cfg.prio_offset).bound(1M, 4M);
                Rx1_cfg.if_limit_fixed = true;

                RxCons_from_VM.Add(Rx1_cfg);
            }
        }

        public static void Adjust_priority_for_existing_constraints(List<RxConstraint> RxCons_from_VM, List<RxConstraint> constraints_config)
        {
            var point_cons_to_add_from_config = constraints_config.Where(t => t.tag.ToLower() == "adjust_priority");

            foreach (RxConstraint Rx1_cfg in point_cons_to_add_from_config)
            {
                var Rxcon1 = RxCons_from_VM.SingleOrDefault(t => t.tag == "PD" && t.StructureID.Match_Std_TitleCase() == Rx1_cfg.StructureID && t.metric_type == Rx1_cfg.metric_type && t.metric_parameter == Rx1_cfg.metric_parameter);

                if (Rxcon1 == null) continue;

                Rxcon1.priority_decimal = (Rxcon1.priority_decimal + Rx1_cfg.prio_offset).bound(1M, 4M);
                Rx1_cfg.if_limit_fixed = true;
            }
        }


        //public static void Add_extra_miscellaneous_constraints(List<RxConstraint> RxCons_from_VM)
        //{
        //    foreach(string strn_std in new string[] {TG263.Brainstem, TG263.OpticChiasm, TG263.OpticNrv_L, TG263.OpticNrv_R, TG263.SpinalCord })
        //    {
        //        var Rxcon1 = RxCons_from_VM.FirstOrDefault(c => c.StructureID.Match_Std_TitleCase() == strn_std);
        //        if (Rxcon1 != null)
        //        {
        //            RxCons_from_VM.Insert(RxCons_from_VM.IndexOf(Rxcon1), 
        //                new RxConstraint(Rxcon1.StructureID, OptimizationObjectiveOperator.Upper, 0, 48, DVHMetricType.Dxcc_Gy, Rxcon1.priority_decimal)
        //                {
        //                    if_limit_fixed = true
        //                });

        //            if(Rxcon1.metric_type == DVHMetricType.Dxcc_Gy.ToString() && Rxcon1.metric_parameter == 0.1)
        //            {
        //                Rxcon1.priority_decimal = Math.Min(Rxcon1.priority_decimal + 1, 4M);
        //            }
        //        }
        //    }


        //    foreach (string strn_std in new string[] {TG263.Brainstem_PRV03, TG263.OpticChiasm_PRV3, TG263.OpticNrv_PRV03_L, TG263.OpticNrv_PRV03_R, TG263.SpinalCord_PRV05 })
        //    {
        //        var Rxcon1 = RxCons_from_VM.FirstOrDefault(c => c.StructureID.Match_Std_TitleCase() == strn_std);
        //        if (Rxcon1 != null)
        //        {
        //            RxCons_from_VM.Insert(RxCons_from_VM.IndexOf(Rxcon1),
        //                new RxConstraint(Rxcon1.StructureID, OptimizationObjectiveOperator.Upper, 0, 48, DVHMetricType.Dxcc_Gy, Rxcon1.priority_decimal)
        //                {
        //                    if_limit_fixed = true
        //                });

        //            if(Rxcon1.metric_type == DVHMetricType.Dxcc_Gy.ToString() && Rxcon1.metric_parameter == 0.1)
        //            {
        //                Rxcon1.priority_decimal = Math.Min(Rxcon1.priority_decimal + 1, 4M);
        //            }
        //        }
        //    }


        //    foreach (string strn_std in new string[] { TG263.Larynx })
        //    {
        //        var Rxcon1 = RxCons_from_VM.FirstOrDefault(c => c.StructureID.Match_Std_TitleCase() == strn_std);
        //        if (Rxcon1 != null)
        //        {
        //            RxCons_from_VM.Insert(RxCons_from_VM.IndexOf(Rxcon1),
        //                new RxConstraint(Rxcon1.StructureID, OptimizationObjectiveOperator.Upper, -1, 20, DVHMetricType.Mean_Gy, Rxcon1.priority_decimal)
        //                {
        //                    if_limit_fixed = true,
        //                    if_break_down_Mean_Gy = false
        //                });
        //        }
        //    }
        //}


        public static void Add_Fixed_Mean_constraints(List<RxConstraint> RxCons_from_VM, List<RxConstraint> constraints_config)
        {
            var cons_to_add_from_config = constraints_config.Where(t => t.tag.ToLower() == "fixed_mean");

            foreach (RxConstraint Rx1_cfg in cons_to_add_from_config)
            {
                var Rxcon1 = RxCons_from_VM.FirstOrDefault(t => t.tag == "PD" && t.StructureID.Match_Std_TitleCase() == Rx1_cfg.StructureID);

                if (Rxcon1 == null) continue;

                Rx1_cfg.priority_decimal = (Rxcon1.opol.HML_ol == 0 && Rxcon1.priority_decimal == 1.5M) ? 2M : (Rxcon1.priority_decimal + 1).bound(1, 4);

                if (Rx1_cfg.StructureID == AP_lib.TG263.Larynx) Rx1_cfg.priority_decimal = Rxcon1.priority_decimal;

                Rx1_cfg.if_limit_fixed = true;
                Rx1_cfg.if_break_down_Mean_Gy = false;

                RxCons_from_VM.Add(Rx1_cfg);
            }
        }


        //public static void Add_Mean_Gy_constraints(List<RxConstraint> RxCons_from_VM) //, History_Curves_ForOpt HistoryCurvesInfo)
        //{
        //    string[] strns = RxCons_from_VM.Where(t=> t.metric_type == DVHMetricType.Mean_Gy.ToString() && t.limit > 0)
        //        .Select(t => t.StructureID).Distinct().ToArray();

        //    foreach (string strn in strns)
        //    {
        //        if (strn.Match_Std_TitleCase() == TG263.Larynx) continue;

        //        //var metric_dist = HistoryCurvesInfo.get_Mean_Gy_distribution(strn);
        //        //double history_mean = metric_dist.Any() ? metric_dist.Average() : 0;
        //        var Rxcon1 = RxCons_from_VM.First(t => t.metric_type == DVHMetricType.Mean_Gy.ToString() && t.limit > 0 && t.StructureID == strn);

        //        decimal prio = Math.Min(Rxcon1.priority_decimal + 1, 4M);

        //        if (Rxcon1.priority_decimal == 1.5M && Rxcon1.opol.HML_ol == 0)
        //        {
        //            prio = 2.0M;
        //        }

        //        RxCons_from_VM.Add(new RxConstraint(strn, OptimizationObjectiveOperator.Upper, -1, Rxcon1.limit, DVHMetricType.Mean_Gy, priority_decimal: prio) { if_break_down_Mean_Gy = false, if_limit_fixed = true});
        //    }
        //}


        //public static void Modify_fixed_limit_Mean_Gy_constraint_limits(List<RxConstraint> RxCons_from_VM, List<RxConstraint> constraints_config)
        //{
        //    var added_Mean_cons = RxCons_from_VM.Where(t => t.metric_type == DVHMetricType.Mean_Gy.ToString() && t.if_break_down_Mean_Gy == false && t.if_limit_fixed == true);

        //    foreach (RxConstraint Rxcon1 in added_Mean_cons)
        //    {
        //        RxConstraint Rx1_cfg = constraints_config.SingleOrDefault(t => t.StructureID == Rxcon1.StructureID.Match_Std_TitleCase());

        //        if (Rx1_cfg == null) continue;

        //        Rxcon1.limit = Rx1_cfg.limit;
        //    }
        //}


        public static void Overwrite_limit_for_PD_constraint(List<RxConstraint> RxCons_from_VM, List<RxConstraint> constraints_config)
        {
            var cons_to_change_from_config = constraints_config.Where(t => t.tag.ToLower() == "overwrite_limit");

            foreach (RxConstraint Rx1_cfg in cons_to_change_from_config)
            {
                RxConstraint Rxcon1 = RxCons_from_VM.SingleOrDefault(t => t.tag == "PD" && t.StructureID.Match_Std_TitleCase() == Rx1_cfg.StructureID && t.metric_type == Rx1_cfg.metric_type && t.metric_parameter == Rx1_cfg.metric_parameter);

                if (Rxcon1 == null) continue;

                Rxcon1.limit = Rx1_cfg.limit;
                Rxcon1.if_limit_fixed = true;
                Rxcon1.if_break_down_Mean_Gy = false;

            }
        }


        //public static void Modify_PD_constraint_limits(List<RxConstraint> RxCons_from_VM, List<RxConstraint> constraints_config)
        //{
        //    var cons_to_change_from_config = constraints_config.Where(t => t.metric_type == DVHMetricType.Dxcc_Gy.ToString());

        //    foreach (RxConstraint Rx1_cfg in cons_to_change_from_config)
        //    {
        //        RxConstraint Rxcon1 = RxCons_from_VM.SingleOrDefault(t => t.StructureID.Match_Std_TitleCase() == Rx1_cfg.StructureID && t.metric_type == Rx1_cfg.metric_type && t.metric_parameter == Rx1_cfg.metric_parameter);

        //        if (Rxcon1 == null) continue;

        //        Rxcon1.limit = Rx1_cfg.limit;
        //        Rxcon1.if_limit_fixed = true;

        //    }
        //}



        public static void Add_gEUD_prio0_constraints(List<RxConstraint> RxCons_from_VM)
        {
            string[] strns = RxCons_from_VM.Select(t => t.StructureID).Distinct().ToArray();

            foreach (string strn in strns)
            {
                string strn_std = strn.Match_Std_TitleCase();

                var gEUD1 = gEUD_data.HN_list.SingleOrDefault(t => t.strn == strn_std);

                if (gEUD1 == null) continue;

                Console.WriteLine($"adding {strn}: gEDU: {gEUD1.strn}  a:{gEUD1.a}");

                RxCons_from_VM.Add(new RxConstraint(strn, OptimizationObjectiveOperator.Upper, -1, gEUD1.Q50, DVHMetricType.gEUD, 0M) { gEUD_a = gEUD1.a });
            }
        }

        internal static void Modify_default_PD_constraint_priorities(constraint[] directive_cons, string config_string)
        {

            Dictionary<string,decimal> prio_dict = AP_Misc.parse_strn_priority(config_string);
            
            var cons = directive_cons.ToList();

            foreach(var con in cons)
            {
                if (prio_dict.ContainsKey(con.StructureID))
                {
                    con.priority_decimal = prio_dict[con.StructureID];
                }
            }
        }


    }
}
