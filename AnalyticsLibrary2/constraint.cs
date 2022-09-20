using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsLibrary2
{

    [DataContract]
    public class constraint
    {
        [DataMember]
        public string StructureID { get; set; }
        [DataMember]
        public string metric_type { get; set; }
        [DataMember]
        public double metric_parameter { get; set; }
        [DataMember]
        public int fraction { get; set; }
        [DataMember]
        public double limit { get; set; }
        [DataMember]
        public string source { get; set; } // local or TG101
        [DataMember]
        public int priority { get; set; }
        [DataMember]
        public double q { get; set; } // slope of the error function
        [DataMember]
        public string label { get; set; } // label indicates the origin of this constraint (corresponding to the Keys in  Dictionary<string, constraint[]> constraints)
        [DataMember]
        public double k { get; set; } // gamma distribution shape par
        [DataMember]
        public double theta { get; set; } // gamma distribution scale par


        private decimal _priority_decimal;
        public decimal priority_decimal
        {
            get
            {
                if (_priority_decimal != 0)
                {
                    return _priority_decimal;
                }
                else
                {
                    _priority_decimal = (decimal)priority;
                    return _priority_decimal;
                }
            }
            set
            {
                _priority_decimal = value;
            }
        }

        public constraint(string structure_ID, int priority, string metric_type, double metric_parameter, double limit, double q = 0.05) // used by HN_ROAR
        {
            this.StructureID = structure_ID;
            this.metric_type = metric_type;
            this.metric_parameter = metric_parameter;
            this.limit = limit;
            this.priority = priority;
            this.q = q;
            this.k = 100 * limit;
            this.theta = 0.01;
        }

        public constraint(string structure_ID, int priority, DVHMetricType metric_type, double metric_parameter, double limit) // used by HN_ROAR
        {
            this.StructureID = structure_ID;
            this.metric_type = metric_type.ToString();
            this.metric_parameter = metric_parameter;
            this.limit = limit;
            this.priority = priority;
            this.k = 100 * limit;
            this.theta = 0.01;
        }

        public constraint(string structure_ID, int fraction, string metric_type, double metric_parameter, double limit, string source, int priority, double q) // used by LIVER_SBRT
        {
            this.StructureID = structure_ID;
            this.metric_type = metric_type;
            this.metric_parameter = metric_parameter;
            this.fraction = fraction;
            this.limit = limit;
            this.source = source;
            this.priority = priority;
            this.q = q;
            this.k = 100 * limit;
            this.theta = 0.01;
        }

        public constraint()
        {
            this.theta = 1.0;
            this.k = 1.0;
        }
        public constraint(double limit)
        {
            this.limit = limit;
            this.k = 100 * limit;
            this.theta = 0.01;
        }

        public constraint(constraint con)
        {
            StructureID = con.StructureID;
            metric_type = con.metric_type;
            metric_parameter = con.metric_parameter;
            fraction = con.fraction;
            limit = con.limit;
            priority = con.priority;
            label = con.label;
            source = con.source;
            k = con.k;
            theta = con.theta;
            q = con.q;
        }

        public string ToString2()
        {
            return metric_type + metric_parameter.ToString() + "__" + priority.ToString() + "__" + limit.ToString();
        }

        public string ToString3_justType()
        {
            return ToString().Split('<', '>')[0];
        }

        public override string ToString()
        {
            string xxx = metric_type;
            string rv = "";

            switch (metric_type)
            {
                case "Mean_Gy":
                    xxx = "Mean[Gy]";
                    break;

                case "Dxcc_Gy":
                    if (metric_parameter == 0) xxx = "Max[Gy]";
                    else xxx = string.Format("D{0}cc[Gy]", metric_parameter);
                    break;
                case "Dxcc_Percent":
                    if (metric_parameter == 0) xxx = "Max[%]";
                    else xxx = string.Format("D{0}cc[%]", metric_parameter);
                    break;
                case "DxPercent_Gy":
                    if (metric_parameter == 0) xxx = "Max[Gy]";
                    else if (metric_parameter == 100) xxx = "Min[Gy]";
                    else xxx = string.Format("D{0:##.#}%[Gy]", metric_parameter);
                    break;

                case "VxGy_cc":
                    xxx = string.Format("V{0}Gy[cc]", metric_parameter);
                    break;
                case "VxGy_Percent":
                    xxx = string.Format("V{0}Gy[%]", metric_parameter);
                    break;
                case "VxPercent_cc":
                    xxx = string.Format("V{0}%[cc]", metric_parameter);
                    break;
                case "VxPercent_Percent":
                    xxx = string.Format("V{0}%[%]", metric_parameter);
                    break;

                case "CVxGy_cc":
                    xxx = string.Format("CV{0}Gy[cc]", metric_parameter);
                    break;
            }

            rv = string.Format("{0}<{1}", xxx, Math.Round(limit, 2));

            if (metric_type != null && metric_type.Contains("CV"))
            {
                rv = string.Format("{0}>{1}", xxx, Math.Round(limit, 2));
            }

            return rv;

            //public enum DVHMetricType { Dxcc_Gy, DxPercent_Gy, Dxcc_Percent, DxPercent_Percent, VxGy_cc, VxGy_Percent, VxPercent_cc, VxPercent_Percent, DCxcc_Gy, DCxPercent_Gy, DCxcc_Percent, DCxPercent_Percent, CVxGy_cc, CVxGy_Percent, CVxPercent_cc, CVxPercent_Percent, Mean_Gy, Volume_cc }
        }

        /// <summary>
        /// read in and replace all constraint.constraints disctionary. Purge with all pre-existing value for each key in constraint.constraints[key]
        /// </summary>
        /// <param name="file_name"></param>
        public static void read_from_JSON(string file_name)
        {
            var cons = serialize.Load_JSON<constraint[]>(file_name); // it works wonderfully, you may need to remove some space in the json file if its constraints number are low.

            var cons_category = constraint.constraints.Keys.ToArray();

            foreach (var key in cons_category)
            {
                constraint.constraints[key] = cons.Where(x => x.label == key).ToArray();

                Log3_static.Debug("Read in [" + constraint.constraints[key].Count() + "] constraints for " + key);
            }
        }

        public static List<string> StructureNamesFromConstraints()
        {
            List<string> strNames = new List<string>();

            foreach (var cohort in constraints.Keys)
            {
                strNames.AddRange(constraints[cohort].Select(t => t.StructureID));
            }

            var rv = strNames.Distinct().ToList();

            return rv;
        }

        public static constraint[] constraints__LUNG_SBRT__Default = new constraint[]
        {
            new constraint(10) {StructureID = "Lungs-GTV", metric_type = "VxGy_Percent", metric_parameter = 20, priority = 1, fraction = 3},
            new constraint(15) {StructureID = "Lungs-GTV", metric_type = "VxGy_Percent", metric_parameter = 12.5 , priority = 1, fraction = 3},
            // only -ITV is included when both -ITV and -GTV are present.
            new constraint(10) {StructureID = "Lungs-ITV", metric_type = "VxGy_Percent", metric_parameter = 20, priority = 1, fraction = 3},
            new constraint(15) {StructureID = "Lungs-ITV", metric_type = "VxGy_Percent", metric_parameter = 12.5 , priority = 1, fraction = 3},

            new constraint(27) {StructureID = "Esophagus", metric_type = "Dxcc_Gy", metric_parameter = 0.1, priority = 1, fraction = 3},

            new constraint(30) {StructureID = "Heart", metric_type = "Dxcc_Gy", metric_parameter = 0.1, priority = 1, fraction = 3},

            new constraint(18) {StructureID = "SpinalCord", metric_type = "Dxcc_Gy", metric_parameter = 0.1, priority = 1, fraction = 3},
            new constraint(22.5) {StructureID = "Stomach", metric_type = "Dxcc_Gy", metric_parameter = 0.1, priority = 1, fraction = 3},
            new constraint(24) {StructureID = "BrachialPlex_R",   metric_type = "Dxcc_Gy", metric_parameter = 0.1, priority = 1, fraction = 3},
            new constraint(24) {StructureID = "BrachialPlex_L",   metric_type = "Dxcc_Gy", metric_parameter = 0.1, priority = 1, fraction = 3},

            new constraint(30) {StructureID = "GreatVes", metric_type = "Dxcc_Gy", metric_parameter = 0.1, priority = 1, fraction = 3},

            new constraint(30) {StructureID = "Chestwall", metric_type = "VxGy_cc", metric_parameter = 30 ,  priority = 1, fraction = 3},

            new constraint(30) {StructureID = "Trachea", metric_type = "Dxcc_Gy", metric_parameter = 0.1, priority = 1, fraction = 3},

            new constraint(30) {StructureID = "Bronchus_Main", metric_type = "Dxcc_Gy", metric_parameter = 0.1 ,  priority = 1, fraction = 3},


            // =================================== 5 fraction ====================================
            new constraint(10) {StructureID = "Lungs-GTV", metric_type = "VxGy_Percent", metric_parameter = 20, priority = 1, fraction = 5},
            new constraint(15) {StructureID = "Lungs-GTV", metric_type = "VxGy_Percent", metric_parameter = 12.5 , priority = 1, fraction = 5},
            // only -ITV is included when both -ITV and -GTV are present.
            new constraint(10) {StructureID = "Lungs-ITV", metric_type = "VxGy_Percent", metric_parameter = 20, priority = 1, fraction = 5},
            new constraint(15) {StructureID = "Lungs-ITV", metric_type = "VxGy_Percent", metric_parameter = 12.5 , priority = 1, fraction = 5},

            new constraint(52.5) {StructureID = "Esophagus", metric_type = "Dxcc_Gy", metric_parameter = 0.1, priority = 1, fraction = 5},
            new constraint(5) {StructureID = "Esophagus", metric_type = "VxGy_cc", metric_parameter = 27.5 , priority = 1, fraction = 5},

            new constraint(52.5) {StructureID = "Heart", metric_type = "Dxcc_Gy", metric_parameter = 0.1, priority = 1, fraction = 5},
            new constraint(15) {StructureID = "Heart", metric_type = "VxGy_cc", metric_parameter = 32 ,  priority = 1, fraction = 5},

            new constraint(25) {StructureID = "SpinalCord", metric_type = "Dxcc_Gy", metric_parameter = 0.1, priority = 1, fraction = 5},
            new constraint(27.5) {StructureID = "Stomach", metric_type = "Dxcc_Gy", metric_parameter = 0.1, priority = 1, fraction = 5},
            new constraint(30) {StructureID = "BrachialPlex_R",   metric_type = "Dxcc_Gy", metric_parameter = 0.1, priority = 1, fraction = 5},
            new constraint(30) {StructureID = "BrachialPlex_L",   metric_type = "Dxcc_Gy", metric_parameter = 0.1, priority = 1, fraction = 5},

            new constraint(52.5) {StructureID = "GreatVes", metric_type = "Dxcc_Gy", metric_parameter = 0.1, priority = 1, fraction = 5},
            new constraint(10) {StructureID = "GreatVes", metric_type = "VxGy_cc", metric_parameter = 47 ,  priority = 1, fraction = 5},

            new constraint(70) {StructureID = "Chestwall", metric_type = "VxGy_cc", metric_parameter = 30 ,  priority = 3, fraction =  5},

            new constraint(52.5) {StructureID = "Trachea", metric_type = "Dxcc_Gy", metric_parameter = 0.1, priority = 1, fraction = 5},
            new constraint(5) {StructureID = "Trachea", metric_type = "VxGy_cc", metric_parameter = 32 ,  priority = 1, fraction = 5},

            new constraint(52.5) {StructureID = "Bronchus_Main", metric_type = "Dxcc_Gy", metric_parameter = 0.1 ,  priority = 1, fraction = 5},
            new constraint(5) {StructureID = "Bronchus_Main", metric_type = "VxGy_cc", metric_parameter = 32 ,  priority = 1, fraction = 5},


        };

        public static constraint[] constraints__LUNG_CRT__Default = new constraint[]
        {
            new constraint(20) {StructureID = "Lungs-GTV", metric_type = "Mean_Gy", metric_parameter = -1, priority = 1},
            new constraint(35) {StructureID = "Lungs-GTV", metric_type = "VxGy_Percent", metric_parameter = 20, priority = 1},
            new constraint(65) {StructureID = "Lungs-GTV", metric_type = "VxGy_Percent", metric_parameter = 5 , priority = 3},
            // only -ITV is included when both -ITV and -GTV are present.
            new constraint(20) {StructureID = "Lungs-ITV", metric_type = "Mean_Gy", metric_parameter = -1, priority = 1},
            new constraint(35) {StructureID = "Lungs-ITV", metric_type = "VxGy_Percent", metric_parameter = 20, priority = 1},
            new constraint(65) {StructureID = "Lungs-ITV", metric_type = "VxGy_Percent", metric_parameter = 5 , priority = 3},

            new constraint(105) {StructureID = "Esophagus", metric_type = "Dxcc_Percent", metric_parameter = 0.03, priority = 1, source = "IMRT/VMAT"},
            new constraint(68) {StructureID = "Esophagus", metric_type = "Dxcc_Gy", metric_parameter = 2.0, priority = 1, source = "IMRT/VMAT"},
            new constraint(34) {StructureID = "Esophagus", metric_type = "Mean_Gy",      metric_parameter = -1 , priority = 1, source = "IMRT/VMAT"},

            //new constraint(105) {StructureID = "Heart", metric_type = "Dxcc_Percent", metric_parameter = 0.1, priority = 1,k=2232.712,theta=0.04703502},
            new constraint(70) {StructureID = "Heart", metric_type = "Dxcc_Gy", metric_parameter = 0.03, priority = 1 },
            new constraint(20) {StructureID = "Heart", metric_type = "Mean_Gy",      metric_parameter = -1 ,  priority = 3},
            new constraint(50) {StructureID = "Heart", metric_type = "VxGy_Percent", metric_parameter = 30, priority = 1},
            new constraint(25) {StructureID = "Heart", metric_type = "VxGy_Percent", metric_parameter = 50, priority = 1},

            new constraint(45) {StructureID = "SpinalCanal",      metric_type = "Dxcc_Gy", metric_parameter = 0.03, priority = 1},
            new constraint(50) {StructureID = "SpinalCanal_PRV5", metric_type = "Dxcc_Gy", metric_parameter = 0.03, priority = 1},
            new constraint(66) {StructureID = "BrachialPlex_R",   metric_type = "Dxcc_Gy", metric_parameter = 0.03, priority = 1},
            new constraint(66) {StructureID = "BrachialPlex_L",   metric_type = "Dxcc_Gy", metric_parameter = 0.03, priority = 1}
        };

        public static constraint[] constraints__LUNG_AAA__Default = new constraint[]
        {
            new constraint(60) {StructureID = "Esophagus", metric_type = "Dxcc_Gy", metric_parameter = 0.1, priority = 1},
            new constraint(34) {StructureID = "Esophagus", metric_type = "Mean_Gy", metric_parameter = -1 , priority = 1},

            new constraint(60) {StructureID = "Heart", metric_type = "Dxcc_Gy", metric_parameter = 0.1, priority = 1},
            new constraint(30) {StructureID = "Heart", metric_type = "Mean_Gy",      metric_parameter = -1 ,  priority = 1},
            new constraint(50) {StructureID = "Heart", metric_type = "VxGy_Percent", metric_parameter = 30, priority = 1},
            new constraint(35) {StructureID = "Heart", metric_type = "VxGy_Percent", metric_parameter = 40, priority = 1},

            new constraint(15) {StructureID = "Lungs-GTV", metric_type = "Mean_Gy", metric_parameter = -1, priority = 1},
            new constraint(35) {StructureID = "Lungs-GTV", metric_type = "VxGy_Percent", metric_parameter = 20, priority = 1},
            new constraint(65) {StructureID = "Lungs-GTV", metric_type = "VxGy_Percent", metric_parameter = 5 , priority = 1},

            new constraint(45) {StructureID = "SpinalCord", metric_type = "Dxcc_Gy", metric_parameter = 0.1, priority = 1},
        };

        public static constraint[] constraints__PROSTATE5__Default { get; set; } = new constraint[]
        {
            // ======= 5 fractions ========
            new constraint(42) {StructureID = "Bladder", metric_type = DVHMetricType.Dxcc_Gy.ToString(), metric_parameter = 0.1, priority = 1, fraction = 5},
            new constraint(41.2) {StructureID = "Bladder", metric_type = DVHMetricType.Dxcc_Gy.ToString(), metric_parameter = 0.5, priority = 1, fraction = 5},
            new constraint(50) {StructureID = "Bladder", metric_type = DVHMetricType.VxGy_cc.ToString(), metric_parameter = 26, priority = 1, fraction = 5},
            new constraint(25) {StructureID = "Bladder", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 26, priority = 1, fraction = 5},

            new constraint(29) {StructureID = "Bowel", metric_type = DVHMetricType.Dxcc_Gy.ToString(), metric_parameter = 0.1, priority = 1, fraction = 5},
            new constraint(27) {StructureID = "Bowel", metric_type = DVHMetricType.Dxcc_Gy.ToString(), metric_parameter = 1, priority = 1, fraction = 5 },

            new constraint(32) {StructureID = "Colon_Sigmoid", metric_type = DVHMetricType.Dxcc_Gy.ToString(), metric_parameter = 0.1, priority = 1, fraction = 5 },
            new constraint(30) {StructureID = "Colon_Sigmoid", metric_type = DVHMetricType.Dxcc_Gy.ToString(), metric_parameter = 1, priority = 1, fraction = 5 },

            new constraint(38) {StructureID = "Rectum", metric_type = DVHMetricType.Dxcc_Gy.ToString(), metric_parameter = 2, priority = 1, fraction = 5},
            new constraint(38.5) {StructureID = "Rectum", metric_type = DVHMetricType.Dxcc_Gy.ToString(), metric_parameter = 1, priority = 1, fraction = 5},
            new constraint(102) {StructureID = "Rectum", metric_type = DVHMetricType.Dxcc_Percent.ToString(), metric_parameter = 0.1, priority = 1, fraction = 5},
            new constraint(10) {StructureID = "Rectum", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 36, priority = 1, fraction = 5},
            new constraint(20) {StructureID = "Rectum", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 32, priority = 1, fraction = 5},
            new constraint(50) {StructureID = "Rectum", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 20, priority = 1, fraction = 5},

            //new constraint(99) {StructureID = "CTV_High", metric_type = DVHMetricType.VxPercent_Percent.ToString(), metric_parameter = 100, priority = 2, fraction = 5},
            //new constraint(99) {StructureID = "PTV!_High", metric_type = DVHMetricType.VxPercent_Percent.ToString(), metric_parameter = 99, priority = 2, fraction = 5},

            new constraint(5) {StructureID = "Femur_Head_L", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 22, priority = 3, fraction = 5 },
            new constraint(10) {StructureID = "Femur_Head_L", metric_type = DVHMetricType.VxGy_cc.ToString(), metric_parameter = 22, priority = 3 , fraction = 5},
            new constraint(5) {StructureID = "Femur_Head_R", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 22, priority = 3 , fraction = 5},
            new constraint(10) {StructureID = "Femur_Head_R", metric_type = DVHMetricType.VxGy_cc.ToString(), metric_parameter = 22, priority = 3 , fraction = 5},

            new constraint(40) {StructureID = "PenileBulb", metric_type = DVHMetricType.Dxcc_Gy.ToString(), metric_parameter = 0.1 , priority = 3 , fraction = 5},
            new constraint(30) {StructureID = "PenileBulb", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 22, priority = 3 , fraction = 5},
            new constraint(3) {StructureID = "PenileBulb", metric_type = DVHMetricType.VxGy_cc.ToString(), metric_parameter = 22, priority = 3 , fraction = 5},

            new constraint(25) {StructureID = "Skin", metric_type = DVHMetricType.Dxcc_Gy.ToString(), metric_parameter = 0.1 , priority = 3 , fraction = 5},
            new constraint(42) {StructureID = "Urethra", metric_type = DVHMetricType.Dxcc_Gy.ToString(), metric_parameter = 0.5 , priority = 3 , fraction = 5}


        };

        public static constraint[] constraints__PROSTATE20__Default = new constraint[]
        {

            // ======= 20 fractions ========
            new constraint(10) { StructureID = "Bladder", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 58, priority = 1, fraction = 20},
            new constraint(15) { StructureID = "Bladder", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 54, priority = 1, fraction = 20},
            new constraint(50) { StructureID = "Bladder", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 43, priority = 1, fraction = 20},

            new constraint(49) {StructureID = "Bowel", metric_type = DVHMetricType.Dxcc_Gy.ToString(), metric_parameter = 0.1, priority = 1 , fraction = 20},
            new constraint(46.6) {StructureID = "Bowel", metric_type = DVHMetricType.Dxcc_Gy.ToString(), metric_parameter = 1, priority = 1 , fraction = 20},

            new constraint(52.5) {StructureID = "CaudaEquina", metric_type = DVHMetricType.Dxcc_Gy.ToString(), metric_parameter = 0.1, priority = 1 , fraction = 20},
            new constraint(55) {StructureID = "Colon_Sigmoid", metric_type = DVHMetricType.Dxcc_Gy.ToString(), metric_parameter = 0.1, priority = 1 , fraction = 20},

            new constraint(5) {StructureID = "Femur_Head_L", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 43, priority = 1 , fraction = 20},
            new constraint(5) {StructureID = "Femur_Head_R", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 43, priority = 1 , fraction = 20},

            new constraint(33) {StructureID = "Kidneys", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 18, priority = 1 , fraction = 20},
            new constraint(30) {StructureID = "Liver", metric_type = DVHMetricType.Mean_Gy.ToString(), metric_parameter = -1 , priority = 1 , fraction = 20},
            new constraint(102) {StructureID = "Rectum", metric_type = DVHMetricType.Dxcc_Percent.ToString(), metric_parameter = 0.1, priority = 1, fraction = 20},

            new constraint(10) {StructureID = "Rectum", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 58, priority = 1, fraction = 20},
            new constraint(15) {StructureID = "Rectum", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 54, priority = 1, fraction = 20},
            new constraint(20) {StructureID = "Rectum", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 50, priority = 1, fraction = 20},
            new constraint(40) {StructureID = "Rectum", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 41, priority = 1, fraction = 20},
            new constraint(65) {StructureID = "Rectum", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 33, priority = 1, fraction = 20},

            new constraint(55) {StructureID = "SacralPlex", metric_type = DVHMetricType.Dxcc_Gy.ToString(), metric_parameter = 0.1, priority = 1 , fraction = 20},
            new constraint(43) {StructureID = "SpinalCanal", metric_type = DVHMetricType.Dxcc_Gy.ToString(), metric_parameter = 0.1, priority = 1 , fraction = 20},

            new constraint(35) { StructureID = "Bladder", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 33, priority = 3, fraction = 20},
            new constraint(10) { StructureID = "Bladder", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 54, priority = 3, fraction = 20},
            new constraint(6) { StructureID = "Bladder", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 58, priority = 3, fraction = 20},

            new constraint(30) {StructureID = "Kidneys", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 10, priority = 3 , fraction = 20},

            new constraint(45) {StructureID = "PenileBulb", metric_type = DVHMetricType.Mean_Gy.ToString(), metric_parameter = -1, priority = 3 , fraction = 20},

            new constraint(5) {StructureID = "Rectum", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 58, priority = 3, fraction = 20},
            new constraint(10) {StructureID = "Rectum", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 54, priority = 3, fraction = 20},
            new constraint(15) {StructureID = "Rectum", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 50, priority = 3, fraction = 20},
            new constraint(35) {StructureID = "Rectum", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 41, priority = 3, fraction = 20},
            new constraint(40) {StructureID = "Rectum", metric_type = DVHMetricType.VxGy_Percent.ToString(), metric_parameter = 33, priority = 3, fraction = 20},

        };

        public static constraint[] constraints__PROSTATE__Default = new constraint[]
        {
            new constraint(100) {StructureID = "Rectum", metric_type = "Dxcc_Percent", metric_parameter = 0.1, priority = 1},
            new constraint(15) {StructureID = "Rectum", metric_type = "VxGy_Percent", metric_parameter = 75, priority = 1},
            //new constraint(75) {StructureID = "Rectum", metric_type = "DxPercent_Gy", metric_parameter = 15, priority = 1, source ="Duplicate"},
            new constraint(25) {StructureID = "Rectum", metric_type = "VxGy_Percent", metric_parameter = 70, priority = 1},
            //new constraint(70) {StructureID = "Rectum", metric_type = "DxPercent_Gy", metric_parameter = 25, priority = 1, source ="Duplicate"},
            new constraint(35) {StructureID = "Rectum", metric_type = "VxGy_Percent", metric_parameter = 65, priority = 1},
            //new constraint(65) {StructureID = "Rectum", metric_type = "DxPercent_Gy", metric_parameter = 35, priority = 1, source ="Duplicate"},
            new constraint(50) {StructureID = "Rectum", metric_type = "VxGy_Percent", metric_parameter = 50, priority = 1},
            //new constraint(50) {StructureID = "Rectum", metric_type = "DxPercent_Gy", metric_parameter = 50, priority = 1, source ="Duplicate"},
            new constraint(5) {StructureID = "Rectum", metric_type = "VxGy_Percent", metric_parameter = 75, priority = 3},
            //new constraint(75) {StructureID = "Rectum", metric_type = "DxPercent_Gy", metric_parameter = 5, priority = 3, source ="Duplicate" ,k=31619.44 ,theta=0.002371983},
            new constraint(15) {StructureID = "Rectum", metric_type = "VxGy_Percent", metric_parameter = 70, priority = 3},
            //new constraint(70) {StructureID = "Rectum", metric_type = "DxPercent_Gy", metric_parameter = 15, priority = 3, source ="Duplicate"},
            new constraint(17) {StructureID = "Rectum", metric_type = "VxGy_Percent", metric_parameter = 65, priority = 3},
            //new constraint(65) {StructureID = "Rectum", metric_type = "DxPercent_Gy", metric_parameter = 17, priority = 3, source ="Duplicate" ,k=1680.461 ,theta=0.03868754},

            new constraint(25) {StructureID = "Bladder", metric_type = "VxGy_Percent", metric_parameter = 75, priority = 3},
            //new constraint(75) {StructureID = "Bladder", metric_type = "DxPercent_Gy", metric_parameter = 25, priority = 3, source ="Duplicate"},
            new constraint(35) {StructureID = "Bladder", metric_type = "VxGy_Percent", metric_parameter = 70, priority = 3},
            //new constraint(70) {StructureID = "Bladder", metric_type = "DxPercent_Gy", metric_parameter = 35, priority = 3, source ="Duplicate" ,k=33395.65 ,theta=0.002096102},
            new constraint(50) {StructureID = "Bladder", metric_type = "VxGy_Percent", metric_parameter = 65, priority = 3},
            //new constraint(65) {StructureID = "Bladder", metric_type = "DxPercent_Gy", metric_parameter = 50, priority = 3, source ="Duplicate" ,k=1792.796 ,theta=0.03626297},

            //new constraint(45) {StructureID = "Femur_R", metric_type = "Dxcc_Gy", metric_parameter = 0, priority = 3 ,k=240.3509 ,theta=0.1874862},
            //new constraint(45) {StructureID = "Femur_L", metric_type = "Dxcc_Gy", metric_parameter = 0, priority = 3 ,k=184.4174 ,theta=0.2444534},
            new constraint(5) {StructureID = "Femur_L", metric_type = "VxGy_Percent", metric_parameter = 50, priority = 1 },
            new constraint(0.1) {StructureID = "Femur_L", metric_type = "VxGy_cc", metric_parameter = 45, priority = 3 },
            new constraint(5) {StructureID = "Femur_R", metric_type = "VxGy_Percent", metric_parameter = 50, priority = 1 },
            new constraint(0.1) {StructureID = "Femur_R", metric_type = "VxGy_cc", metric_parameter = 45, priority = 3 },

            new constraint(1) {StructureID = "Bowel", metric_type = "VxGy_cc", metric_parameter = 50, priority = 1 },
            new constraint(0.1) {StructureID = "Bowel", metric_type = "VxGy_cc", metric_parameter = 54, priority = 1 },
            //new constraint(54) {StructureID = "Bowel", metric_type = "Dxcc_Gy", metric_parameter = 1, priority = 1},

            new constraint(1) {StructureID = "Colon_Sigmoid", metric_type = "VxGy_cc", metric_parameter = 60, priority = 1 },
            new constraint(0.1) {StructureID = "Colon_Sigmoid", metric_type = "VxGy_cc", metric_parameter = 64, priority = 1 },
            //new constraint(60) {StructureID = "Colon_Sigmoid", metric_type = "Dxcc_Gy", metric_parameter = 1, priority = 1},

            new constraint(52.5) {StructureID = "PenileBulb", metric_type = "Mean_Gy", metric_parameter = -1 , priority = 3 },
        };

        public static constraint[] constraints__PROSTATE__SBRT = new constraint[] // very complicated Planning Directive. Not sure if I want to implement all the requirement.
       {
           // looks like Prostate is Target, instead of Normal Tissue. 
            new constraint(120) {StructureID = "Prostate", metric_type = "Dxcc_Percent", metric_parameter = 0.1, priority = 2},
            new constraint(15) {StructureID = "Prostate", metric_type = "VxPercent_Percent", metric_parameter = 115, priority = 2},
            new constraint(10) {StructureID = "Prostate", metric_type = "VxPercent_cc", metric_parameter = 115, priority = 2},

            new constraint(105) {StructureID = "Rectum", metric_type = "Dxcc_Percent", metric_parameter = 0.1, priority = 1},
            new constraint(2) {StructureID = "Rectum", metric_type = "VxPercent_cc", metric_parameter = 100, priority = 1},
            new constraint(10) {StructureID = "Rectum", metric_type = "VxPercent_Percent", metric_parameter = 90, priority = 1},
            new constraint(20) {StructureID = "Rectum", metric_type = "VxPercent_Percent", metric_parameter = 81, priority = 1},
            new constraint(50) {StructureID = "Rectum", metric_type = "VxPercent_Percent", metric_parameter = 50, priority = 1},

            new constraint(110) {StructureID = "Bladder", metric_type = "Dxcc_Percent", metric_parameter = 0.1, priority = 1},
            new constraint(25) {StructureID = "Bladder", metric_type = "VxPercent_Percent", metric_parameter = 65, priority = 1},
            new constraint(50) {StructureID = "Bladder", metric_type = "VxPercent_cc", metric_parameter = 65, priority = 1},

            //new constraint(45) {StructureID = "Femur_L", metric_type = "Dxcc_Gy", metric_parameter = 0, priority = 3 ,k=184.4174 ,theta=0.2444534},
            //new constraint(5) {StructureID = "Femur_L", metric_type = "VxGy_Percent", metric_parameter = 50, priority = 1 },
            //new constraint(0.1) {StructureID = "Femur_L", metric_type = "VxGy_cc", metric_parameter = 45, priority = 3 },
            new constraint(5) {StructureID = "Femur_Head_R", metric_type = "VxPercent_Percent", metric_parameter = 54, priority = 3 },
            new constraint(10) {StructureID = "Femur_Head_R", metric_type = "VxPercent_cc", metric_parameter = 54, priority = 3 },
            new constraint(5) {StructureID = "Femur_Head_L", metric_type = "VxPercent_Percent", metric_parameter = 54, priority = 3 },
            new constraint(10) {StructureID = "Femur_Head_L", metric_type = "VxPercent_cc", metric_parameter = 54, priority = 3 },

            new constraint(100) {StructureID = "PenileBulb", metric_type = "Dxcc_Percent", metric_parameter = 0.1, priority = 3},
            new constraint(30) {StructureID = "PenileBulb", metric_type = "VxPercent_Percent", metric_parameter = 54, priority = 3},
            new constraint(3) {StructureID = "PenileBulb", metric_type = "VxPercent_cc", metric_parameter = 54, priority = 3},
       };


        public static constraint[] constraints__ESOPHAGUS__Default = new constraint[]{
            //new constraint(45) {StructureID = "SpinalCord", metric_type = "Dxcc_Gy", metric_parameter = 0, priority = 1},
            new constraint(45) {StructureID = "SpinalCanal", metric_type = "Dxcc_Gy", metric_parameter = 0.03, priority = 1},
            new constraint(50) {StructureID = "SpinalCanal_PRV5", metric_type = "Dxcc_Gy", metric_parameter = 0.03, priority = 1},

            new constraint(20) {StructureID = "Lungs", metric_type = "Mean_Gy", metric_parameter = -1 , priority = 1 },
            new constraint(35) {StructureID = "Lungs", metric_type = "VxGy_Percent", metric_parameter = 20, priority = 1 },
            new constraint(65) {StructureID = "Lungs", metric_type = "VxGy_Percent", metric_parameter = 5, priority = 1},

            new constraint(20) {StructureID = "Lung_L", metric_type = "Mean_Gy", metric_parameter = -1 , priority = 1 },
            new constraint(35) {StructureID = "Lung_L", metric_type = "VxGy_Percent", metric_parameter = 20, priority = 3 },
            new constraint(65) {StructureID = "Lung_L", metric_type = "VxGy_Percent", metric_parameter = 5, priority = 3},

            new constraint(20) {StructureID = "Lung_R", metric_type = "Mean_Gy", metric_parameter = -1 , priority = 1 },
            new constraint(35) {StructureID = "Lung_R", metric_type = "VxGy_Percent", metric_parameter = 20, priority = 3 },
            new constraint(65) {StructureID = "Lung_R", metric_type = "VxGy_Percent", metric_parameter = 5, priority = 3},

            new constraint(50) {StructureID = "Heart", metric_type = "VxGy_Percent", metric_parameter = 30, priority = 1},
            new constraint(25) {StructureID = "Heart", metric_type = "VxGy_Percent", metric_parameter = 50, priority = 1},
            new constraint(20) {StructureID = "Heart", metric_type = "Mean_Gy",      metric_parameter = -1 ,  priority = 3},

            new constraint(33) {StructureID = "Kidneys", metric_type = "VxGy_Percent", metric_parameter = 18, priority = 1},
            new constraint(33) {StructureID = "Kidney_L", metric_type = "VxGy_Percent", metric_parameter = 18, priority = 1},
            new constraint(33) {StructureID = "Kidney_R", metric_type = "VxGy_Percent", metric_parameter = 18, priority = 1},

            new constraint(21) {StructureID = "Liver", metric_type = "Mean_Gy",      metric_parameter = -1 , priority = 1},
            new constraint(30) {StructureID = "Liver", metric_type = "VxGy_Percent", metric_parameter = 30, priority = 1},
        };

        public static constraint[] constraints_HN_Default = new constraint[]
        {
            new constraint("Brain", 3, "Mean_Gy", -1, 60),
            new constraint("Brainstem", 1, "Dxcc_Gy", 0.10, 54),
            new constraint("Brainstem_PRV03", 1, "Dxcc_Gy", 0.10, 54),
            new constraint("OpticChiasm", 1, "Dxcc_Gy", 0.10, 54),
            new constraint("OpticChiasm_PRV3", 3, "Dxcc_Gy", 0.10, 54),
            new constraint("Cochlea_L", 1, "Dxcc_Gy", 0.10, 40),
            new constraint("Cochlea_R", 1, "Dxcc_Gy", 0.10, 40),
            new constraint("Musc_Constrict_I", 1, "Mean_Gy", -1, 20),
            new constraint("Musc_Constrict_S", 3, "Mean_Gy", -1, 50),
            //new constraint("Musc_Constrict_S", 3, DVHMetricType.DxPercent_Gy.ToString(), -1, 50),
            new constraint("SpinalCord", 1, "Dxcc_Gy", 0.10, 45),
            new constraint("SpinalCord_PRV05", 1, "Dxcc_Gy", 0.10, 50),
            new constraint("Esophagus", 1, "Mean_Gy", -1, 20),
            new constraint("Eye_L", 1, "Dxcc_Gy", 0.10, 40),
            new constraint("Eye_R", 1, "Dxcc_Gy", 0.10, 40),
            new constraint("Glnd_Lacrimal_L", 1, "Mean_Gy", -1, 30),
            new constraint("Glnd_Lacrimal_R", 1, "Mean_Gy", -1, 30),
            new constraint("Larynx", 1, "Mean_Gy", -1, 20),
            new constraint("Lens_L", 1, "Dxcc_Gy", 0.10, 10),
            new constraint("Lens_R", 1, "Dxcc_Gy", 0.10, 10),
            new constraint("Lips", 1, "VxGy_Percent", 35, 5),
            new constraint("Bone_Mandible", 3, "Dxcc_Gy", 0.10, 70),
            new constraint("OpticNrv_L", 1, "Dxcc_Gy", 0.10, 54),
            new constraint("OpticNrv_PRV03_L", 3, "Dxcc_Gy", 0.10, 54),
            new constraint("OpticNrv_R", 1, "Dxcc_Gy", 0.10, 54),
            new constraint("OpticNrv_PRV03_R", 3, "Dxcc_Gy", 0.10, 54),
            new constraint("Cavity_Oral", 3, "Mean_Gy", -1, 30),
            new constraint("Parotid_L", 3, "Mean_Gy", -1, 24),
            new constraint("Parotid_R", 3, "Mean_Gy", -1, 24),
            new constraint("Glnd_Submand_L", 3, "Mean_Gy", -1, 30),
            new constraint("Glnd_Submand_R", 3, "Mean_Gy", -1, 30),
            new constraint("Lobe_Temporal_L", 3, "Dxcc_Gy", 0.10, 60),
            new constraint("Lobe_Temporal_R", 3, "Dxcc_Gy", 0.10, 60),
            new constraint("Parotid Involved", 3, "Mean_Gy", -1, 24),
            new constraint("Parotid Un-involved", 3, "Mean_Gy", -1, 24),
            new constraint("Glnd_Submand Involved", 3, "Mean_Gy", -1, 30),
            new constraint("Glnd_Submand Un-involved", 3, "Mean_Gy", -1, 30),
            new constraint("Parotid_Low", 3, "Mean_Gy", -1, 24),
            new constraint("Parotid_High", 3, "Mean_Gy", -1, 24),
            new constraint("Glnd_Submand_Low", 3, "Mean_Gy", -1, 30),
            new constraint("Glnd_Submand_High", 3, "Mean_Gy", -1, 30)
        };


        public static constraint[] constraints__LIVER_SBRT__Default = new constraint[]
        {
            new constraint("Liver-GTV", 1, "CVxGy_cc", 15, 700) { fraction = 3},
            new constraint("Liver-GTV", 4, "Mean_Gy", -1, 80) { fraction = 3},

            new constraint("Kidney_L", 1, "VxGy_Percent", 16, 67) { fraction = 3},
            new constraint("Kidney_R", 1, "VxGy_Percent", 16, 67) { fraction = 3},

            new constraint("Kidneys", 1, "VxGy_cc", 16, 200) { fraction = 3},
            new constraint("Kidneys", 1, "VxGy_Percent", 16, 35) { fraction = 3},

            new constraint("SpinalCord", 1, "Dxcc_Gy", 0.5, 18) { fraction = 3},
            new constraint("Esophagus", 1, "Dxcc_Gy", 0.5, 27) { fraction = 3},
            new constraint("Heart", 1, "Dxcc_Gy", 0.5, 40) { fraction = 3},

            new constraint("Duodenum", 1, "Dxcc_Gy", 0.5, 24) { fraction = 3},
            new constraint("Duodenum_PRV", 1, "Dxcc_Gy", 0.5, 24) { fraction = 3},

            new constraint("Bowel_Small", 1, "Dxcc_Gy", 0.5, 24) { fraction = 3},
            new constraint("Bowel_Small_PRV", 1, "Dxcc_Gy", 0.5, 24) { fraction = 3},
            new constraint("Stomach", 1, "Dxcc_Gy", 0.5, 22.5) { fraction = 3},
            new constraint("Stomach_PRV", 1, "Dxcc_Gy", 0.5, 22.5) { fraction = 3},

            new constraint("Chestwall", 1, "VxGy_cc", 30, 70) { fraction = 3},
            new constraint("Rib", 1, "VxGy_cc", 30, 70) { fraction = 3},
            new constraint("Colon", 1, "Dxcc_Gy", 0.5, 24) { fraction = 3},

            // ======================= 5 fractions ===================================
            new constraint("Liver-GTV", 1, "CVxGy_cc", 15, 700) { fraction = 5},
            new constraint("Liver-GTV", 4, "Mean_Gy", -1, 80) { fraction = 5},

            new constraint("Kidney_L", 1, "VxGy_Percent", 17.5, 67) { fraction = 5},
            new constraint("Kidney_R", 1, "VxGy_Percent", 17.5, 67) { fraction = 5},

            new constraint("Kidneys", 1, "VxGy_cc", 16, 200) { fraction = 5},
            new constraint("Kidneys", 1, "VxGy_Percent", 16, 35) { fraction = 5},

            new constraint("SpinalCord", 1, "Dxcc_Gy", 0.5, 25) { fraction = 5},
            new constraint("Esophagus", 1, "Dxcc_Gy", 0.5, 52.5) { fraction = 5},
            new constraint("Esophagus", 1, "VxGy_cc", 27.5, 5) { fraction = 5},
            new constraint("Heart", 1, "Dxcc_Gy", 0.5, 52.5) { fraction = 5},
            new constraint ("Heart", 1, "VxGy_cc", 32, 15) { fraction = 5},

            new constraint("Duodenum", 1, "Dxcc_Gy", 0.5, 30) { fraction = 5},
            new constraint("Duodenum_PRV", 1, "Dxcc_Gy", 0.5, 30) { fraction = 5},

            new constraint("Bowel_Small", 1, "Dxcc_Gy", 0.5, 30) { fraction = 5},
            new constraint("Bowel_Small_PRV", 1, "Dxcc_Gy", 0.5, 30) { fraction = 5},
            new constraint("Stomach", 1, "Dxcc_Gy", 0.5, 30) { fraction = 5},
            new constraint("Stomach_PRV", 1, "Dxcc_Gy", 0.5, 30) { fraction = 5},

            new constraint("Chestwall", 1, "VxGy_cc", 35, 70) { fraction = 5},
            new constraint("Rib", 1, "VxGy_cc", 35, 70) { fraction = 5},
            new constraint("Colon", 1, "Dxcc_Gy", 0.5, 30) { fraction = 5}
        };


        public static constraint[] constraints__LIVER_CRT__Default = new constraint[]
        {
            new constraint("Liver-GTV", 1, "Dxcc_Gy", 0, 100),
            new constraint("Liver-GTV", 4, "Mean_Gy", -1, 80),

            new constraint("Liver-PTV", 4, "VxGy_cc", 16, 80),

            new constraint("Kidney_L", 1, "VxGy_Percent", 20, 50),
            new constraint("Kidney_R", 1, "VxGy_Percent", 20, 50),
            new constraint("Kidneys", 1, "VxGy_Percent", 20, 50),
            new constraint("Kidney_L", 1, "Dxcc_Gy", 0, 45),
            new constraint("Kidney_R", 1, "Dxcc_Gy", 0, 45),
            new constraint("Kidneys", 1, "Dxcc_Gy", 0, 45),

            new constraint("SpinalCord", 1, "Dxcc_Gy", 0, 45),

            new constraint("Duodenum", 1, "Dxcc_Gy", 1.0, 54),

            new constraint("Bowel_Small", 1, "Dxcc_Gy", 1.0, 54),
            new constraint("Stomach", 1, "Dxcc_Gy", 1.0, 54),
            new constraint("Stomach", 1, "VxGy_cc", 51, 5),

            new constraint("Chestwall", 1, "Dxcc_Percent", 0, 100),
            new constraint("Colon", 1, "Dxcc_Gy", 1.0, 65)
        };

        public static constraint[] constraints__HN_UAB__Default = new constraint[]
        {
            new constraint("SpinalCord", 1, "Dxcc_Gy", 0, 50),
            new constraint("SpinalCord_PRV05", 1, "Dxcc_Gy", 0.01, 52),
            new constraint("Brainstem", 1, "Dxcc_Gy", 0, 54),
            new constraint("Brainstem_PRV03", 1, "Dxcc_Gy", 0.03, 52),

            new constraint("Parotid_L", 2, "Mean_Gy", -1, 26),
            new constraint("Parotid_L", 2, DVHMetricType.DxPercent_Gy, 50, 30),
            new constraint("Parotid_R", 2, "Mean_Gy", -1, 26),
            new constraint("Parotid_R", 2, DVHMetricType.DxPercent_Gy, 50, 30),
            //new constraint("Parotids", 2, DVHMetricType.Dxcc_Gy, 20, 20),

            //new constraint("Glnd_Submand Contralateral", 2, "Mean_Gy", -1, 40),

            new constraint("Larynx", 2, "Mean_Gy", -1, 43.5),
            new constraint("Larynx", 2, DVHMetricType.Dxcc_Gy, 0, 68),
            new constraint("Larynx", 2, DVHMetricType.VxGy_Percent, 50, 20), // Translated from Minimize V50 in the Planning Directive. multiple studies with increased Toxicity with cutoff anywhere between 20%-50%.

            new constraint("Pharynx", 2, DVHMetricType.Mean_Gy, -1, 45),
            new constraint("Pharynx", 2, DVHMetricType.VxGy_Percent, 50, 33),
            new constraint("Pharynx", 2, DVHMetricType.VxGy_Percent, 60, 15),

            new constraint("Cochlea_L", 2, DVHMetricType.Mean_Gy, -1, 35),
            new constraint("Cochlea_R", 2, DVHMetricType.Mean_Gy, -1, 35),

            new constraint("BrachialPlex_L", 2, DVHMetricType.Dxcc_Gy, 0, 66),
            new constraint("BrachialPlex_R", 2, DVHMetricType.Dxcc_Gy, 0, 66),

            new constraint("Lips", 2, DVHMetricType.Mean_Gy, -1, 20),
            new constraint("Cavity_Oral", 2, DVHMetricType.Mean_Gy, -1, 30),
            new constraint("Cavity_Oral", 2, DVHMetricType.VxGy_cc, 60, 0.1),
            new constraint("Bone_Mandible", 2, "Dxcc_Gy", 0, 66),

            new constraint("Esophagus", 2, "Mean_Gy", -1, 30),
            new constraint("Esophagus", 2, DVHMetricType.VxGy_Percent, 54, 15),
            new constraint("Esophagus", 2, DVHMetricType.VxGy_Percent, 60, 15),

            new constraint("Glnd_Thyroid", 2, DVHMetricType.VxGy_Percent, 30, 62.5),
        };

        public static constraint[] constraints__HN_UPenn__Default = new constraint[]
        {
            new constraint("Brainstem", 1, "Dxcc_Gy", 0.03, 54),
            new constraint("OpticNrv_L", 1, "Dxcc_Gy", 0.03, 54),
            new constraint("OpticNrv_R", 1, "Dxcc_Gy", 0.03, 54),
            new constraint("OpticChiasm", 1, "Dxcc_Gy", 0.03, 54),

            new constraint("Musc_Constrict", 3, "Mean_Gy", -1, 50),
            new constraint("Musc_Constrict-PTV", 3, "Mean_Gy", -1, 40),

            new constraint("SpinalCord", 1, "Dxcc_Gy", 0.03, 45),
            new constraint("SpinalCord_PRV05", 1, "Dxcc_Gy", 0.03, 50),
            new constraint("Eye_L", 1, "Dxcc_Gy", 0.03, 45),
            new constraint("Eye_R", 1, "Dxcc_Gy", 0.03, 45),

            new constraint("Esophagus-PTV", 3, "Mean_Gy", -1, 20),
            new constraint("Larynx-PTV", 3, "Mean_Gy", -1, 20),
            new constraint("Cavity_Oral-PTV", 3, "Mean_Gy", -1, 20),
            new constraint("Bone_Mandible-PTV", 3, "Dxcc_Gy", 0.03, 70),

            new constraint("Ear_Middle_L", 3, "Mean_Gy", -1, 30),
            new constraint("Ear_Middle_R", 3, "Mean_Gy", -1, 30),

            new constraint("Parotid_L", 3, "Mean_Gy", -1, 26),
            new constraint("Parotid_R", 3, "Mean_Gy", -1, 26),
            new constraint("Parotid_L", 3, "Mean_Gy", -1, 20),
            new constraint("Parotid_R", 3, "Mean_Gy", -1, 20),
            new constraint("Glnd_Submand_L", 3, "Mean_Gy", -1, 30),
            new constraint("Glnd_Submand_R", 3, "Mean_Gy", -1, 30),

            new constraint("Lobe_Temporal_L", 3, "Mean_Gy", -1, 25),
            new constraint("Lobe_Temporal_R", 3, "Mean_Gy", -1, 25),
        };

        public static Dictionary<string, constraint[]> constraints = new Dictionary<string, constraint[]>()
        {
            //{"HN_ROAR__DF", constraints_HN_Default},
            //{"LIVER_SBRT__DF", constraints__LIVER_SBRT__Default},
            //{"LUNG_CRT__DF", constraints__LUNG_CRT__Default},
            //{"LUNG_SBRT__DF", constraints__LUNG_SBRT__Default},
            //{"PROSTATE__DF", constraints__PROSTATE__Default},
            //{"ESOPHAGUS__DF", constraints__ESOPHAGUS__Default},

            {"Liver_CRT__DF", constraints__LIVER_CRT__Default},
            {"Liver_SBRT__DF", constraints__LIVER_SBRT__Default},
            {"Lung_CRT__DF", constraints__LUNG_CRT__Default},
            {"Lung_SBRT__DF", constraints__LUNG_SBRT__Default},
            {"Esophagus__DF", constraints__ESOPHAGUS__Default},

            {"Liver_CRT__MC", new constraint[0]},
            {"Liver_SBRT__MC", new constraint[0]},
            {"Lung_CRT__MC", new constraint[0]},
            {"Lung_SBRT__MC", new constraint[0]},
            {"Esophagus__MC", new constraint[0]},

            //{"LUNG_AAA__DF", constraints__LUNG_AAA__Default},
            //{"LUNG_AAA__MC", new constraint[0]},

            {"HN_Salivary__DF", constraints_HN_Default},
            {"HN_Larynx__DF", constraints_HN_Default},
            {"HN_Oral__DF", constraints_HN_Default},
            {"HN_General__DF", constraints_HN_Default},
            {"HN_Salivary__MC", new constraint[0]},
            {"HN_Larynx__MC", new constraint[0]},
            {"HN_Oral__MC", new constraint[0]},
            {"HN_General__MC", new constraint[0]},


            {"Prostate5__DF", constraints__PROSTATE5__Default},
            {"Prostate20__DF", constraints__PROSTATE20__Default},
            {"Prostate5__MC", new constraint[0]},
            {"Prostate20__MC", new constraint[0]},

            {"Prostate_BED__DF", constraints__PROSTATE__Default},
            {"Prostate_SV__DF", constraints__PROSTATE__Default},
            {"Prostate_General__DF", constraints__PROSTATE__Default},
            {"Prostate_BED__MC", new constraint[0]},
            {"Prostate_SV__MC", new constraint[0]},
            {"Prostate_General__MC", new constraint[0]},

            {"HN_UAB__DF", constraints__HN_UAB__Default},
            {"HN_UAB__MC", new constraint[0]},

            {"HN_UPenn__DF", constraints__HN_UPenn__Default},
            {"HN_UPenn__MC", new constraint[0]},

            // Prostate_SBRT is not processed, due to too complicated constraints. see directive for details.
            // Liver_CRT is not processed, due to too few plans ~15, go to 2014 with _25 plans
        };
        private constraint con;

        public static constraint[] filter_cons(string datasource, string str_name, int fraction = 0, string constraint_choice = "DF")
        {
            Misc.parameter_check(constraint_choice, "DF", "MC");

            var cohort = datasource + "__" + constraint_choice;

            var strname = str_name.Match_StrID_to_Standard_Name(datasource).Title_Case();

            var cons0 = constraints[cohort];

            var cons1 = cons0.Where(c => c.StructureID == strname && (c.fraction == fraction || c.fraction == 0)).ToList();

            return cons1.ToArray();
        }

    }

}
