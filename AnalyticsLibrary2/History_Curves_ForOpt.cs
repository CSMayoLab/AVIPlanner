using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsLibrary2
{

    public class History_Curves_ForOpt
    {
        public string datasource { get; set; }
        public int Nfraction { get; set; }
        public Dictionary<string, Info_struct_by_each_curve> str_metrics_dict { get; set; }

        public History_Curves_ForOpt(string data_source, int N_fraction = 0, string Pre_JSON_dir = @"")
        {
            datasource = data_source;
            Nfraction = N_fraction; // 0 is for the set that includes all fractions for CRT.

            var filename = "Summary_dict__" + datasource + "__" + Nfraction.ToString() + ".json";
            str_metrics_dict = serialize.Load_JSON<Dictionary<string, Info_struct_by_each_curve>>(Pre_JSON_dir + filename);
            Console.WriteLine(filename + " loaded.");
        }


        public IEnumerable<double> get_metric_distribution(string str, constraint con)
        {
            try
            {
                string strID_TitleCase = str.Match_StrID_to_Standard_Name(datasource).Title_Case();

                if (!str_metrics_dict.Keys.Contains(strID_TitleCase)) throw new Exception("provided structure_name " + str + " cannot be converted to standard structure name by function Match_StrID_to_Standard_Name();\n Please consider using one of the following structure names:\n "
                    + string.Join("; ", str_metrics_dict.Keys.ToArray()));

                Info_struct_by_each_curve info_curves = str_metrics_dict[strID_TitleCase];

                string con_name = con.ToString2().Split(new string[] { "__" }, 2, StringSplitOptions.None)[0];
                return info_curves.curves_info.SelectMany(t => t.constraint_metrics).Where(t => t.Key.StartsWith(con_name)).Select(t => t.Value);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public IEnumerable<double> get_Mean_Gy_distribution(string str)
        {
            try
            {
                string strID_TitleCase = str.Match_StrID_to_Standard_Name(datasource).Title_Case();

                if (!str_metrics_dict.Keys.Contains(strID_TitleCase)) throw new Exception("provided structure_name " + str + " cannot be converted to standard structure name by function Match_StrID_to_Standard_Name();\n Please consider using one of the following structure names:\n "
                    + string.Join("; ", str_metrics_dict.Keys.ToArray()));

                Info_struct_by_each_curve info_curves = str_metrics_dict[strID_TitleCase];

                string con_name = "Mean_Gy";
                return info_curves.curves_info.SelectMany(t => t.constraint_metrics).Where(t => t.Key.StartsWith(con_name)).Select(t => t.Value);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }


}
