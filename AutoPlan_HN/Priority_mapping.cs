using AP_lib;
using AutoPlan_WES_HN;
using AnalyticsLibrary2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace AutoPlan_HN
{
    public class Priority_mapping
    {

        public static decimal Adjustment_of_PD_decimal_priority(OAR_PTV_overlap opol)
        {
            string std_strn = opol.StructureName.Match_Std_TitleCase();

            if (strn_list_overlap_affect_BrokenUpMeanLevels.Contains(std_strn))
            {
                decimal rv;
                if (opol.HML_ol / opol.volume >= 0.7) rv = 4;
                else if (opol.HML_ol / opol.volume >= 0.5) rv = 2.5M;
                else if (opol.HML_ol / opol.volume >= 0.2) rv = 2;
                else rv = 1.5M;

                if (std_strn == AP_lib.TG263.Cavity_Oral) { return Math.Max(3, rv); }
                if (std_strn == AP_lib.TG263.Musc_Constrict_S) { return Math.Max(2, rv); }

                return rv;
            }

            return -1M; // no adjustment needed when return -1
        }

        
        public static string[] strn_list_overlap_affect_BrokenUpMeanLevels = Config.MeanBreakUp_affected_by_PTV_overlap;


        public static double map_to_OPT_priority( OAR_PTV_overlap opol, RxConstraint con, double at_vol_percent = -1)
        {
            string std_strn = con.StructureID.Match_Std_TitleCase();

            if(con.StructureID.Match_Std_TitleCase() != opol.StructureName.Match_Std_TitleCase())
            {
                throw new Exception($"{con.StructureID} um-match {opol.StructureName}");
            }

            if(strn_list_overlap_affect_BrokenUpMeanLevels.Contains(std_strn) 
                && con.metric_type == DVHMetricType.Mean_Gy.ToString() 
                && con.if_break_down_Mean_Gy == true)
            {
                if(con.priority_decimal == 1)
                {
                    if (at_vol_percent == Mean_con_breakup.lowDoseAtVol || at_vol_percent == Mean_con_breakup.midDoseAtVol)
                        return map_decimal_prio_to_OPT(1);
                    return map_decimal_prio_to_OPT(3);
                }
                else if(con.priority_decimal == 1.5M)
                {
                    if (opol.HML_ol / opol.volume == 0) 
                    {
                        if (at_vol_percent == Mean_con_breakup.lowDoseAtVol || at_vol_percent == Mean_con_breakup.midDoseAtVol)
                            return map_decimal_prio_to_OPT(1.5M);
                        return map_decimal_prio_to_OPT(3);
                    }
                    else
                    {
                        if (at_vol_percent == Mean_con_breakup.lowDoseAtVol || at_vol_percent == Mean_con_breakup.midDoseAtVol)
                            return map_decimal_prio_to_OPT(1.5M);
                        return map_decimal_prio_to_OPT(4);
                    }
                }
                else if(con.priority_decimal == 2)
                {
                    if (at_vol_percent == Mean_con_breakup.lowDoseAtVol || at_vol_percent == Mean_con_breakup.midDoseAtVol)
                        return map_decimal_prio_to_OPT(2);
                    return map_decimal_prio_to_OPT(4);
                }
                else if (con.priority_decimal == 2.5M)
                {
                    if (at_vol_percent == Mean_con_breakup.lowDoseAtVol || at_vol_percent == Mean_con_breakup.midDoseAtVol)
                        return map_decimal_prio_to_OPT(2.5M);
                    return map_decimal_prio_to_OPT(4);
                }
                else if (con.priority_decimal == 3M)
                {
                    if (at_vol_percent == Mean_con_breakup.lowDoseAtVol || at_vol_percent == Mean_con_breakup.midDoseAtVol)
                        return map_decimal_prio_to_OPT(3M);
                    return map_decimal_prio_to_OPT(4);
                }
                else if (con.priority_decimal == 3.5M)
                {
                    if (at_vol_percent == Mean_con_breakup.lowDoseAtVol || at_vol_percent == Mean_con_breakup.midDoseAtVol)
                        return map_decimal_prio_to_OPT(3.5M);
                    return map_decimal_prio_to_OPT(4);
                }
                else if (con.priority_decimal == 4M)
                {
                    return map_decimal_prio_to_OPT(4);
                }
            }


            return map_decimal_prio_to_OPT(con.priority_decimal);
        }

        public static double map_decimal_prio_to_OPT(decimal prio_d, decimal lower_to_by_user)
        {
            if (prio_d < lower_to_by_user) return map_decimal_prio_to_OPT(lower_to_by_user);
            
            return map_decimal_prio_to_OPT(prio_d);
        }


        public static double map_decimal_prio_to_OPT(decimal prio_d)
        {
            if (prio_d == (decimal)1) { return Config.prio_1; } 
            if (prio_d == (decimal)1.5) { return Config.prio_1_5; }
            if (prio_d == (decimal)2) { return Config.prio_2; }
            if (prio_d == (decimal)2.5) { return Config.prio_2_5; }
            if (prio_d == (decimal)3) { return Config.prio_3; }
            if (prio_d == (decimal)3.5) { return Config.prio_3_5; }
            if (prio_d == (decimal)4) { return Config.prio_4; }
            return 0;
        }
    }


    public static class Mean_con_breakup
    {
        public const double lowDoseAtVol = 90;
        public const double midDoseAtVol = 50;
        public const double highDoseAtVol = 5;
    }

}
