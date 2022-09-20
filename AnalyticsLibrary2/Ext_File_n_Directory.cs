using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsLibrary2
{
    public static class Ext_File_n_Directory
    {
        public static string toFileName(this string name, string with = "-")
        {
            return name.Replace(@"\", with).Replace(@"/", with).Replace(":", with);
        }

    }
}
