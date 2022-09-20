using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsLibrary2
{
    public static class Ext_Enforce_TG263
    {

        public static bool isTG263_standard(this string strn)
        {
            return strn == strn.Match_Std_TitleCase();
        }


        public static string[] Enforce_TG203(this string[] strns)
        {
            foreach (string strn in strns)
            {
                if (!strn.isTG263_standard())
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
