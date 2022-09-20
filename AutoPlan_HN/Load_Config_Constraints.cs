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
