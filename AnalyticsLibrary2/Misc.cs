using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsLibrary2
{
    public static class Misc
    {
        public static string tabs(string previous_string)
        {
            var n = previous_string.Length;
            if (n < 8) return "\t\t";
            if (n >= 8 && n < 16) return "\t";
            return "";
        }

        public static int get_fraction_from_datasource_shown(string datasource_shown)
        {
            if (datasource_shown.IndexOf("Fraction_3") > -1) return 3;
            if (datasource_shown.IndexOf("Fraction_5") > -1) return 5;
            if (datasource_shown.IndexOf("Fraction_20") > -1) return 20;
            if (datasource_shown.IndexOf("__3_") > -1) return 3;
            if (datasource_shown.IndexOf("__5_") > -1) return 5;
            if (datasource_shown.IndexOf("__20_") > -1) return 20;
            return 0;
        }

        public static void parameter_check(string input_par, params string[] available_options)
        {
            bool flag_match = false;

            for (int i = 0; i < available_options.Length; i++)
            {
                if (input_par.Equals(available_options[i]))
                    flag_match = true;
            }

            if (flag_match == false)
                throw new ArgumentOutOfRangeException(string.Format("Input parameter [{0}] is not accepted in this function", input_par));
        }

        public static string array_to_string(int[] int_array, string open = "[", string close = "]", string between = " ", bool ifcount = true)
        {
            StringBuilder sb = new StringBuilder();

            if (ifcount == true)
            {
                foreach (var i in int_array.Distinct().OrderBy(t => t))
                {
                    sb.Append(int_array.Where(t => t == i).Count() + open + i + close + between);
                }
            }
            else
            {
                foreach (var i in int_array)
                {
                    sb.Append(open + i + close + between);
                }
            }

            sb.Remove(sb.Length - between.Length, between.Length);

            return sb.ToString();
        }

        //// print out a datarow
        //public static string print_DataRow(this DataRow line)
        //{
        //    StringBuilder rv = new StringBuilder();
        //    foreach (DataColumn col in line.Table.Columns)
        //    {
        //        rv.Append(col.ColumnName + ": " + line[col].ToString() + "; ");
        //    }
        //    return rv.ToString();
        //}

        public static string obj_to_string(object obj)
        {
            var _PropertyInfos = obj.GetType().GetProperties();
            var sb = new StringBuilder();
            foreach (var info in _PropertyInfos)
            {
                var value = info.GetValue(obj) ?? "(null)";
                sb.Append(info.Name + ": " + value.ToString() + "; ");
            }
            return sb.ToString();
        }

        // convert cGy to Gy in DVHCurve_ByVolumePercentList
        public static string DVHCurve_ByVolumePercentList_cGy_to_Gy(string DVHCurve_ByVolumePercentList)
        {
            if (DVHCurve_ByVolumePercentList == null) return null;

            StringBuilder rv = new StringBuilder();

            foreach (var p in DVHCurve_ByVolumePercentList.Trim('"').Split(';'))
            {
                var p2 = p.Split(',');
                rv.Append(string.Format("{0:F4},{1:F2};", Convert.ToDouble(p2[0]) / 100, Convert.ToDouble(p2[1])));
            }
            return rv.Remove(rv.Length - 1, 1).ToString();
        }


    }
}
