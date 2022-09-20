using AP_lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPlan_HN
{
    public class gEUD_data
    {
        public gEUD_data(string structure_name, double a_value, double q25, double q50, double q75)
        {
            strn = structure_name;
            a = a_value;
            Q25 = q25;
            Q50 = q50;
            Q75 = q75;
        }

        public string strn { get; set; }
        public double a { get; set; }
        public double Q25 { get; set; }
        public double Q50 { get; set; }
        public double Q75 { get; set; }

        public static gEUD_data[] HN_list = new gEUD_data[]
        {
            new gEUD_data(AP_lib.TG263.Bone_Mandible, 1.3, 26.59,  33.93,   41.03)
            , new gEUD_data(AP_lib.TG263.Brain,    1.3,    1.64,   3.14,   5.80)
            , new gEUD_data(AP_lib.TG263.Brainstem,    1.2,    4.03,   8.77,   14.67)
            , new gEUD_data(AP_lib.TG263.Brainstem_PRV03, 1.4, 5.28,   10.47,  16.18)
            , new gEUD_data(AP_lib.TG263.Cavity_Oral, 1.2, 24.03   ,31.73, 41.78)
            , new gEUD_data(AP_lib.TG263.Cochlea_L,  1.0   ,2.69   ,6.57,  16.53)
            , new gEUD_data(AP_lib.TG263.Cochlea_R,  1.0   ,2.69   ,6.57,  16.53)
            , new gEUD_data(AP_lib.TG263.Esophagus,    1.1 ,15.12  ,18.73, 19.88)
            , new gEUD_data(AP_lib.TG263.Eye_L,  0.8,  3.32,   8.30,   14.49)
            , new gEUD_data(AP_lib.TG263.Eye_R,  0.8,  3.32,   8.30,   14.49)
            , new gEUD_data(AP_lib.TG263.Glnd_Lacrimal_L,  1.1,    5.98,   10.87,  16.31)
            , new gEUD_data(AP_lib.TG263.Glnd_Lacrimal_R,  1.1 ,5.98,  10.87,  16.31)
            , new gEUD_data(AP_lib.TG263.Glnd_Submand_L,   1.8 ,42.66, 60.58,  66.65)
            , new gEUD_data(AP_lib.TG263.Glnd_Submand_R,   1.8 ,42.66, 60.58,  66.65)
            , new gEUD_data(AP_lib.TG263.Larynx,   0.6,    17.79   ,18.85, 24.19)
            , new gEUD_data(AP_lib.TG263.Lens_L,    0.9,   3.16,   7.00,   9.43)
            , new gEUD_data(AP_lib.TG263.Lens_R,    0.9,   3.16,   7.00,   9.43)
            , new gEUD_data(AP_lib.TG263.Lips, 0.6,    10.17,  14.76,  20.46)
            , new gEUD_data(AP_lib.TG263.Lobe_Temporal_L,  1.0,    11.28,  19.85,  29.70)
            , new gEUD_data(AP_lib.TG263.Lobe_Temporal_R,  1.0,    11.28,  19.85,  29.70)
            , new gEUD_data(AP_lib.TG263.Musc_Constrict_I, 0.6,    17.99,  19.18,  30.84)
            , new gEUD_data(AP_lib.TG263.Musc_Constrict_S, 1.3,    37.60,  48.26,  56.31)
            , new gEUD_data(AP_lib.TG263.OpticChiasm,  1.0,    9.04,   24.50,  40.19)
            , new gEUD_data(AP_lib.TG263.OpticChiasm_PRV3, 0.9,    10.95,  23.79,  40.91)
            , new gEUD_data(AP_lib.TG263.OpticNrv_L,   0.7,    9.89,   18.77,  40.13)
            , new gEUD_data(AP_lib.TG263.OpticNrv_R,   0.7,    9.89,   18.77,  40.13)
            , new gEUD_data(AP_lib.TG263.OpticNrv_PRV03_L,    0.7, 9.72,   18.84,  40.37)
            , new gEUD_data(AP_lib.TG263.OpticNrv_PRV03_R,    0.7  ,9.72,  18.84,  40.37)
            , new gEUD_data(AP_lib.TG263.Parotid_L,  0.8,  22.48,  27.72,  36.30)
            , new gEUD_data(AP_lib.TG263.Parotid_R,  0.8,  22.48,  27.72,  36.30)
            , new gEUD_data(AP_lib.TG263.SpinalCord, 1.8,  16.82,  21.10,  23.97)
            , new gEUD_data(AP_lib.TG263.SpinalCord_PRV05, 1.8,    17.09,  21.53,  24.13)
        };
    }
}





