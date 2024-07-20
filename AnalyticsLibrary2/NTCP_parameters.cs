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
    public class NTCP_parameters
    {
        [DataMember]
        public string StructureID { get; set; }
        [DataMember]
        public string source { get; set; }
        [DataMember]
        public double n_vs { get; set; }
        [DataMember]
        public double alphabeta { get; set; }
        [DataMember]
        public double TD50 { get; set; }
        [DataMember]
        public double m { get; set; }

        public static NTCP_parameters[] NTCP_par_list = new NTCP_parameters[]
        {
            new NTCP_parameters() {StructureID = "BOWEL_SMALL", n_vs = 0.15, alphabeta = 2.5, TD50 = 55, m = 0.16},

            new NTCP_parameters() {StructureID = "DUODENUM", n_vs = 0.1, alphabeta = 2.5, TD50 = 56, m = 0.21},
            new NTCP_parameters() {StructureID = "ESOPHAGUS", n_vs = 0.06, alphabeta = 2.5, TD50 = 82.3, m = 0.11},

            new NTCP_parameters() {StructureID = "HEART", n_vs = 0.35, alphabeta = 2.5, TD50 = 48, m = 0.1},

            new NTCP_parameters() {StructureID = "LUNGS", n_vs = 0.99, alphabeta = 2.5, TD50 = 30.8, m = 0.37},
            new NTCP_parameters() {StructureID = "LIVER-GTV", n_vs = 0.97, alphabeta = 2.5, TD50 = 35.4, m = 0.12},

            new NTCP_parameters() {StructureID = "PAROTID_L", n_vs = 1, alphabeta = 2.5, TD50 = 40, m = 0.36},
            new NTCP_parameters() {StructureID = "PAROTID_R", n_vs = 1, alphabeta = 2.5, TD50 = 40, m = 0.36},
            new NTCP_parameters() {StructureID = "PAROTID INVOLVED", n_vs = 1, alphabeta = 2.5, TD50 = 40, m = 0.36},
            new NTCP_parameters() {StructureID = "PAROTID UN-INVOLVED", n_vs = 1, alphabeta = 2.5, TD50 = 40, m = 0.36},
            new NTCP_parameters() {StructureID = "PAROTID_Low", n_vs = 1, alphabeta = 2.5, TD50 = 40, m = 0.36},
            new NTCP_parameters() {StructureID = "PAROTID_High", n_vs = 1, alphabeta = 2.5, TD50 = 40, m = 0.36},

            new NTCP_parameters() {StructureID = "SPINALCORD", n_vs = 0.05, alphabeta = 2.5, TD50 = 66.5, m = 0.175},
            new NTCP_parameters() {StructureID = "SPINALCANAL", n_vs = 0.05, alphabeta = 2.5, TD50 = 66.5, m = 0.175},

            new NTCP_parameters() {StructureID = "STOMACH", n_vs = 0.1, alphabeta = 2.5, TD50 = 56, m = 0.21},
        };
    }
}
