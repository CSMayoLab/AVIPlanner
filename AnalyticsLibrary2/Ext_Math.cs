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
using System.Threading.Tasks;

namespace AnalyticsLibrary2
{
    public static class Ext_Math // test out that Name of the class does not matter, as long as it is public static.
    {
        public static double Median(this IEnumerable<double> source)
        {
            double returnvalue;
            source = source.Where(x => !double.IsNaN(x)).ToList(); // eliminate the double.NaNs 

            if (source.Count() == 0)
            {
                returnvalue = double.NaN;

                //throw new InvalidOperationException("Cannot compute median for an empty set.");
            }
            else
            {

                var sortedList = from number in source
                                 orderby number
                                 select number;

                int itemIndex = (int)sortedList.Count() / 2;

                if (sortedList.Count() % 2 == 0)
                {
                    // Even number of items. 
                    returnvalue = (sortedList.ElementAt(itemIndex) + sortedList.ElementAt(itemIndex - 1)) / 2;
                }
                else
                {
                    // Odd number of items. 
                    returnvalue = sortedList.ElementAt(itemIndex);
                }
            }
            return returnvalue;
        }
        public static double Median(this IEnumerable<int> source)
        {
            return (from num in source select (double)num).Median();
        }
        public static double Median(this IEnumerable<float> source)
        {
            return (from num in source select (double)num).Median();
        }


        /// <summary>
        /// Calculate the quantiles of the distribution at 0% (Q0, minimum),2.5%, 25%(Q1), 50%(median,Q2), 75%(Q3), 97.5%, 100%(Q4,maximum) 
        /// </summary>
        /// <param name="source">Source distribution</param>
        /// <returns>List of quantiles</returns>
        public static List<double> Quantiles(this IEnumerable<double> source)
        {
            List<double> quantiles = new List<double> { 0.0f, 2.5f, 25.0f, 50.0f, 75.0f, 97.5f, 100.0f };
            return Quantiles(source, quantiles);
        }


        /// <summary>
        /// Calculate the quantiles of the distribution at user specified percentages
        /// </summary>
        /// <param name="source">Source distribution</param>
        /// <param name="quantilelist">List of specified percentages [0,100]</param>
        /// <returns>List of quantiles</returns>

        // interface overload: Quantiles with double[] input signature.
        public static List<double> Quantiles(this double[] source, double[] quantilelist)
        {
            // Console.WriteLine("Overload Quantiles with double[] inputs");
            return Quantiles(source.ToList(), quantilelist.ToList());
        }

        /// <summary>
        /// Calculate specified quantiles for the source distribution
        /// </summary>
        /// <param name="source">data distribution</param>
        /// <param name="quantilelist">quantiles in range [0,100]</param>
        /// <returns></returns>
        public static List<double> Quantiles(this IEnumerable<double> source, List<double> quantilelist)
        {
            List<double> returnvalue = new List<double>();
            source = source.Where(x => !double.IsNaN(x)).ToList(); // eliminate the double.NaNs 

            if (source.Count() < 2)
            {
                for (int c = 0; c < quantilelist.Count; c++) returnvalue.Add(double.NaN);
                //throw new InvalidOperationException("Cannot compute quantiles for an empty set.");
            }
            else
            {
                var sortedList = from number in source
                                 orderby number
                                 select number;

                List<PointXY> lookup = new List<PointXY>();
                double nitems = (double)source.Count();
                PointXY curpoint = new PointXY();
                for (int i = 0; i < nitems; i++)
                {
                    curpoint = new PointXY();
                    curpoint.X = 100.0f * i / (nitems - 1);  // X is the increasing quantiles list from the data distribution source in %
                    curpoint.Y = sortedList.ElementAt(i);    // Y is the value at quantile X%
                    lookup.Add(curpoint);
                }
                //List<double>quantiles = new List<double>{0.0f,2.5f,25.0f,50.0f,75.0f,97.5f,100.0f};

                PointXY plow;
                PointXY phigh;
                foreach (double q in quantilelist)
                {
                    plow = lookup.Where(number => number.X <= q).Last();
                    if (plow.X < 100.0f)
                    {
                        phigh = lookup.Where(number => number.X > q).First();
                        returnvalue.Add(plow.Y + ((q - plow.X) / (phigh.X - plow.X)) * (phigh.Y - plow.Y));
                    }
                    else
                    {
                        returnvalue.Add(plow.Y);
                    }
                }
            }
            return returnvalue;
        }

        /// <summary>
        /// Calculate specified reverse quantiles for the source distribution
        /// </summary>
        /// <param name="source">data distribution</param>
        /// <param name="quantilelist">quantiles in range [0,100]</param>
        /// <returns></returns>
        public static List<double> Quantiles_reverse(this IEnumerable<double> source, List<double> values)
        {
            List<double> returnvalue = new List<double>();
            var source_orderd = source.Where(x => !double.IsNaN(x)).OrderBy(t => t).ToArray(); // eliminate the double.NaNs 
            double source_max = source_orderd.Max(), source_min = source_orderd.Min();
            int n_source = source_orderd.Count();

            if (source_max != source_orderd.Last()) throw new Exception("How could source_max != source.Last() in a sorted array?");

            if (n_source < 2)
            {
                for (int c = 0; c < values.Count; c++) returnvalue.Add(double.NaN);
            }
            else
            {
                var quantiles_at_source = source_orderd.Select((t, ind) => (double)ind / (n_source - 1) * 100).ToArray();

                foreach (double v in values)
                {
                    if (v >= source_max) { returnvalue.Add(100f); continue; }
                    if (v <= source_min) { returnvalue.Add(0f); continue; }

                    int il = Array.FindLastIndex(source_orderd, t => t <= v);
                    int iu = il + 1;

                    returnvalue.Add(quantiles_at_source[il] + ((v - source_orderd[il]) / (source_orderd[iu] - source_orderd[il])) * (quantiles_at_source[iu] - quantiles_at_source[il]));
                }
            }
            return returnvalue;
        }


        public static double Stdev(this IEnumerable<double> source)
        {
            double returnvalue;

            if (source.Count() < 2)
            {
                //returnvalue = double.NaN;
                returnvalue = 0;
                //throw new InvalidOperationException("Cannot compute quantiles for an empty set.");
            }
            else
            {

                double avg = source.Average();

                returnvalue = Math.Sqrt(source.Select(x => Math.Pow(x - avg, 2.0f)).Sum() / (source.Count() - 1));
            }
            return returnvalue;

        }
    }

}
