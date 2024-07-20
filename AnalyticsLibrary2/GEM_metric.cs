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
