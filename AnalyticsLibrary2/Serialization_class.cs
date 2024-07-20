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
using System.Runtime.Serialization.Json;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AnalyticsLibrary2
{
    public class serialize
    {

        public static string path = @"\\yourdirectoryfilepathhere\Pre_JSON_10n11_SummaryOnly\";  // Bring cohort summary files from 10 and 11 folders togather. And run merge_planGEM_n_MUperGy_files() on them. 

        public static string path_from = path;
        public static string path_to = path;

        public static void Save_to_JSON<T>(T contract, string filePath)
        {
            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(T));
            using (var stream = File.Create(filePath))
            {
                js.WriteObject(stream, contract);
            }
        }

        public static T Load_JSON<T>(string filePath)
        {
            try
            {
                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(T));
                using (var stream = File.OpenRead(filePath))
                {
                    T contract = (T)js.ReadObject(stream);
                    return contract;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("########## Cannot deserialize JSON file {0} ########## \n Error message: {1}", filePath, ex.Message);
                throw;
            }
        }





        public static readonly double[] volume_values = new double[] { 0, 0.5, 1, 2, 3, 4, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 96, 97, 98, 99, 99.5, 100 };





        public static void Output_constraints_parameters(string suffix = "", string set = "All")
        {
            var cons_list = new List<constraint>();
            foreach (var key in constraint.constraints.Keys)
            {
                if (set.ToUpper() != "ALL" && key.ToUpper() != set.ToUpper()) continue;

                foreach (var con in constraint.constraints[key])
                {
                    var con1 = new constraint(con);
                    con1.label = key;  // label indicates the origin of this constraint
                    cons_list.Add(con1);
                }
            }

            string out_file = (path + "Constraints_Parameters" + "_" + set + (string.IsNullOrEmpty(suffix) ? "" : "_" + suffix) + ".json");
            Save_to_JSON(cons_list, out_file);
        }


        public static void PrintOut_NTCP_parameters()
        {
            string out_file = (path + "NTCP_Parameters.json");
            Save_to_JSON(NTCP_parameters.NTCP_par_list, out_file);
        }



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
            public DVHMetrics.volume_cross_section[] v_cross_sections { get; set; }

            [DataMember]
            public double[] NTCP { get; set; }
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
            public DVHMetrics.info_per_curve[] curves_info { get; set; }
        }




    }
}

