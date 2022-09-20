using AnalyticsLibrary2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AP_lib
{
    public static class AP_Misc
    {
 
        //public static string ToString(this double d)
        //{
        //    return d.ToString("N2");
        //}
        
        
        public static Dictionary<string,decimal> parse_strn_priority(string config_str)
        {
            var prio_dict = new Dictionary<string, decimal>();
            try
            {
                foreach (string part in config_str.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var split_part = part.Split(':');

                    if(!split_part[0].isTG263_standard()) 
                    {
                        throw new Exception($"{split_part[0]} is not a TG263 standard structure name. Please rename it."); 
                    }

                    prio_dict.Add(split_part[0], decimal.Parse(split_part[1]));
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Cannot parse configuration string {config_str} into structure-priority dictionary.", e);
            }

            return prio_dict;
        }

        public static bool isTG263_standard(this string strn)
        {
            return strn == strn.Match_Std_TitleCase();
        }


        public static string[] Enforce_TG203(string[] strns)
        {
            foreach(string strn in strns)
            {
                if(!strn.isTG263_standard())
                {
                    string msg = $"[{strn}] from configuration file is not a TG263 standard name.";

                    if (!string.IsNullOrEmpty(strn.Match_Std_TitleCase()))
                    {
                        msg = msg + $" You may want to rename it as [{strn.Match_Std_TitleCase()}]";
                    }
                    throw new Exception(msg);
                }
            }

            return strns;
        }


    }
}
