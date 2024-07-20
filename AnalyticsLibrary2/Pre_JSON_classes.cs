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
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsLibrary2
{
    [DataContract]
    public class StatsDVH_info
    {
        [DataMember]
        public string Datasource { get; set; }
        [DataMember]
        public int fraction { get; set; }
        [DataMember]
        public string StructureID { get; set; }
        [DataMember]
        public int Count { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Dictionary<string, double[]> gems { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public double[] volume_cc { get; set; }

        [DataMember]
        public volume_cross_section[] v_cross_sections { get; set; }

        [DataMember]
        public double[] NTCP { get; set; }
    }

    [DataContract]
    public class curves
    {
        [DataMember]
        public string Datasource { get; set; }
        [DataMember]
        public int fraction { get; set; }
        [DataMember]
        public string StructureID { get; set; }
        [DataMember]
        public int Count { get; set; }
        [DataMember]
        public double[,] xx { get; set; }
    }


    [DataContract]
    public class Info_plans
    {
        [DataMember]
        public string datasource { get; set; }
        [DataMember]
        public string constraint_choice { get; set; }
        [DataMember]
        public int fraction { get; set; }
        [DataMember]
        public double[] array { get; set; }
    }

    [DataContract]
    public class Info_struct
    {
        [DataMember]
        public string datasource { get; set; }
        [DataMember]
        public string constraint_choice { get; set; }
        [DataMember]
        public int fraction { get; set; }
        [DataMember]
        public string structureID { get; set; }
        [DataMember]
        public double[] volume_cc { get; set; }
        [DataMember]
        public Dictionary<string, double[]> constraint_metrics { get; set; }
    }

    [DataContract]
    public class Info_struct_by_each_curve
    {
        [DataMember]
        public string datasource { get; set; }
        [DataMember]
        public int fraction { get; set; }
        [DataMember]
        public string structureID { get; set; }
        [DataMember]
        public int Count { get; set; }
        [DataMember]
        public info_per_curve[] curves_info { get; set; }
    }


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
