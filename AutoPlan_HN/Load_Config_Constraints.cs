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

using AP_lib;
using AutoPlan_WES_HN;
using Newtonsoft.Json;
using AnalyticsLibrary2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutoPlan_HN
{
    public static class Load_Config_Constraints
    {
        public static List<RxConstraint> load()
        {
            string file_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\constraints_config.json";
            //string file_path = Config.constraints_config;

            //MessageBox.Show(file_path);

            string readText = File.ReadAllText(file_path);

            var constraints_config = JsonConvert.DeserializeObject<RxConstraint[]>(readText).ToList(); // .Where(t => t.tag != "extra_generic_constraint").ToList();

            foreach(RxConstraint Rx1 in constraints_config)
            {
                if (Rx1.StructureID.ToUpper().Contains("PTV") || Rx1.StructureID.ToUpper().StartsWith("Z")) continue;

                if (Rx1.StructureID.Match_Std_TitleCase() != Rx1.StructureID) 
                {
                    string message = $"[{Rx1.StructureID}] doesn't meet TG263 standard structure name [${Rx1.StructureID.Match_Std_TitleCase()}]. Please rename it following TG263 guidline in the folloiwng config file {Config.constraints_config}";

                    Log.Warning(message);
                    MessageBox.Show(message, "AutoPlan_HN");

                    throw new Exception("Non-standard structure name in constraint_config.json file.");
                }
            }

            return constraints_config.ToList();
        }
    }
}
