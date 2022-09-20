using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnalyticsLibrary2
{

    public class GEM_metric
    {
        public string name; // "StructureID + Metric(D0.5cc[Gy]) + fraction? "
        public double planValue;
        public double constraint;
        public double priority;
        public double q;
        public double k;
        public double theta;

        public GEM_metric() { }

        public GEM_metric(double achieved, constraint con)
        {
            planValue = achieved;
            constraint = con.limit;
            priority = con.priority;
            q = con.q;
            k = con.k;
            theta = con.theta;

            if (Regex.Match(con.metric_type, "^CV.*", RegexOptions.IgnoreCase).Success)
            {
                double reflex = 2 * con.limit - achieved;
                planValue = reflex >= 0 ? reflex : 0.0;
            }
        }
    }
}
