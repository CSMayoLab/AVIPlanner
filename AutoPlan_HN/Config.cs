using AP_lib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.Types;

namespace AutoPlan_WES_HN
{
    static class Config
    {
        public static string Logfile_dir = ConfigurationManager.AppSettings["Logfile_dir"];

        public static string Precalculated_JSON_files_dir = ConfigurationManager.AppSettings["Precalculated_JSON_files_dir"];

        public static string Constrains_Parameters_R_out_file = ConfigurationManager.AppSettings["Constrains_Parameters_R_out_file"];
        
        public static string constraints_config = ConfigurationManager.AppSettings["constraints_config"];

        public static double NTO_priority = double.Parse(ConfigurationManager.AppSettings["NTO_priority"]);
        public static double NTO_distanceFromTargetBorderInMM = double.Parse(ConfigurationManager.AppSettings["NTO_distanceFromTargetBorderInMM"]);
        public static double NTO_startDosePercentage = double.Parse(ConfigurationManager.AppSettings["NTO_startDosePercentage"]);
        public static double NTO_endDosePercentage = double.Parse(ConfigurationManager.AppSettings["NTO_endDosePercentage"]);
        public static double NTO_fallOff = double.Parse(ConfigurationManager.AppSettings["NTO_fallOff"]);


        public static bool if_add_zNape = bool.Parse(ConfigurationManager.AppSettings["if_add_zNape"]);
        public static double zNape_priority = double.Parse(ConfigurationManager.AppSettings["zNape_priority"]);
        public static double zNape_gEUD_limit_Gy = double.Parse(ConfigurationManager.AppSettings["zNape_gEUD_limit_Gy"]);
        public static double zNape_gEUD_a = double.Parse(ConfigurationManager.AppSettings["zNape_gEUD_a"]);
        public static double zNape_marginFromPTVsInMM = double.Parse(ConfigurationManager.AppSettings["zNape_marginFromPTVsInMM"]);

        public static bool if_add_zBuff = bool.Parse(ConfigurationManager.AppSettings["if_add_zBuff"]);
        public static double zBuff_priority = double.Parse(ConfigurationManager.AppSettings["zBuff_priority"]);
        public static double zBuff_gEUD_limit_Gy = double.Parse(ConfigurationManager.AppSettings["zBuff_gEUD_limit_Gy"]);
        public static double zBuff_gEUD_a = double.Parse(ConfigurationManager.AppSettings["zBuff_gEUD_a"]);
        public static double zBuff_marginFromPTVsInMM = double.Parse(ConfigurationManager.AppSettings["zBuff_marginFromPTVsInMM"]);


        public static List<string> MachineIDs = ConfigurationManager.AppSettings["MachineIDs"].Split(new char[] { ';' }).ToList();


        public static decimal OARs_into_zOptPTV_xxx_L_Priority = decimal.Parse(ConfigurationManager.AppSettings["OARs_into_zOptPTV_xxx_L_Priority"]);
        public static List<string> OARs_into_zOptPTV_xxx_L_list = ConfigurationManager.AppSettings["OARs_into_zOptPTV_xxx_L_list"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();


        public static List<string> Isocenter_location_precedence = ConfigurationManager.AppSettings["Isocenter_location_precedence"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        public static string isocenter_Y_at_OAR = ConfigurationManager.AppSettings["isocenter_Y_at_OAR"];
        public static string isocenter_Z_at_OAR = ConfigurationManager.AppSettings["isocenter_Z_at_OAR"];
        

        public static string Modify_default_PD_constraint_priorities = ConfigurationManager.AppSettings["Modify_default_PD_constraint_priorities"];

        public static string[] MeanBreakUp_affected_by_PTV_overlap = AP_Misc.Enforce_TG203(ConfigurationManager.AppSettings["MeanBreakUp_affected_by_PTV_overlap"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)); 

        public static double prio_1 = double.Parse(ConfigurationManager.AppSettings["prio_1"]);
        public static double prio_1_5 = double.Parse(ConfigurationManager.AppSettings["prio_1_5"]);
        public static double prio_2 = double.Parse(ConfigurationManager.AppSettings["prio_2"]);
        public static double prio_2_5 = double.Parse(ConfigurationManager.AppSettings["prio_2_5"]);
        public static double prio_3 = double.Parse(ConfigurationManager.AppSettings["prio_3"]);
        public static double prio_3_5 = double.Parse(ConfigurationManager.AppSettings["prio_3_5"]);
        public static double prio_4 = double.Parse(ConfigurationManager.AppSettings["prio_4"]);

        public static double prio_zDLA_High = double.Parse(ConfigurationManager.AppSettings["prio_zDLA_High"]);
        public static double prio_zDLA_Mid = double.Parse(ConfigurationManager.AppSettings["prio_zDLA_Mid"]);
        public static double prio_zDLA_Low = double.Parse(ConfigurationManager.AppSettings["prio_zDLA_Low"]);


        public static string zPTV_Low_name = ConfigurationManager.AppSettings["zPTV_Low_name"];
        public static string zPTV_Low_only_name = ConfigurationManager.AppSettings["zPTV_Low_only_name"];
        public static string zPTV_Mid_name = ConfigurationManager.AppSettings["zPTV_Mid_name"];
        public static string zPTV_Mid_only_name = ConfigurationManager.AppSettings["zPTV_Mid_only_name"];

        public static double[] Scan_angles_for_jaw_width = ConfigurationManager.AppSettings["Scan_angles_for_jaw_width"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList().Select(t => double.Parse(t)).ToArray();

        public static List<Beam_Config> Beams = Parse_config.Parse_beams_config(ConfigurationManager.AppSettings["beams_config_string"]);

        public static double diff_X_limit_inMM = double.Parse(ConfigurationManager.AppSettings["diff_X_limit_inMM"]);
        public static double diff_Y_limit_inMM = double.Parse(ConfigurationManager.AppSettings["diff_Y_limit_inMM"]);
        public static double Jaw_X_width_reduction_cutoff_inMM = double.Parse(ConfigurationManager.AppSettings["Jaw_X_width_reduction_cutoff_inMM"]);
        public static double Jaw_Y_width_reduction_cutoff_inMM = double.Parse(ConfigurationManager.AppSettings["Jaw_Y_width_reduction_cutoff_inMM"]);
        public static double Max_Jaw_X_Width_inMM = double.Parse(ConfigurationManager.AppSettings["Max_Jaw_X_Width_inMM"]);

        public static double JawMargin_X_inMM = double.Parse(ConfigurationManager.AppSettings["JawMargin_X_inMM"]);
        public static double JawMargin_Y_inMM = double.Parse(ConfigurationManager.AppSettings["JawMargin_Y_inMM"]);


        public static double zBuff_1st_expansion_margin_inMM = double.Parse(ConfigurationManager.AppSettings["zBuff_1st_expansion_margin_inMM"]);

        public static double[] zBuff_expansion_from_spinalCord_inMM = ConfigurationManager.AppSettings["zBuff_expansion_from_spinalCord_inMM"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList().Select(t => double.Parse(t)).ToArray();
        public static double[] zNape_expansion_from_spinalCord_inMM = ConfigurationManager.AppSettings["zNape_expansion_from_spinalCord_inMM"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList().Select(t => double.Parse(t)).ToArray();

        public static string PhotonVMATOptimization { get; internal set; } = ConfigurationManager.AppSettings["PhotonVMATOptimization"];

        public static string PhotonVolumeDose { get; internal set; } = ConfigurationManager.AppSettings["PhotonVolumeDose"];



        public static List<string> Structures_trigger_4_beams = AP_Misc.Enforce_TG203(ConfigurationManager.AppSettings["Structures_trigger_4_beams"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)).ToList();

    }

    public static class Parse_config
    {
        public static List<Beam_Config> Parse_beams_config(string beams_config_string)
        {
            var rv = new List<Beam_Config>();

            var beams = beams_config_string.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string beam1 in beams)
            {
                var parts = beam1.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                var rv1 = new Beam_Config();

                rv1.BeamName = parts[0];
                rv1.tableAngle = double.Parse(parts[1]);
                rv1.gantryAngle = double.Parse(parts[2]);
                rv1.gantryStop = double.Parse(parts[3]);

                string gd = parts[4];
                
                if (gd.ToUpper() == GantryDirection.Clockwise.ToString().ToUpper()) 
                { 
                    rv1.gantryDir = GantryDirection.Clockwise;
                }
                else if (gd.ToUpper() == GantryDirection.CounterClockwise.ToString().ToUpper())
                {
                    rv1.gantryDir = GantryDirection.CounterClockwise;
                }

                rv1.mlc_angle = double.Parse(parts[5]);

                rv.Add(rv1);
            }

            return rv;
        }
    }

    public class Beam_Config
    {
        public string BeamName;
        public double tableAngle;
        public double gantryAngle;
        public double gantryStop;

        public double mlc_angle;
        public GantryDirection gantryDir;
    }
}