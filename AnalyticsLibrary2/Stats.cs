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
    public static class Stats
    {
        public static double Median(List<double> source)
        {
            return source.Median();
        }
        public static List<double> Quantiles(List<double> source, List<double> quantilelist)
        {
            return source.Quantiles(quantilelist);
        }
        // Interface overload to double[]
        public static double[] Quantiles(double[] source, double[] quantilelist)
        {
            return source.Quantiles(quantilelist).ToArray();
        }

        /// <summary>
        /// Empirical Cummulative Distribution function based on a look up array 
        /// </summary>
        /// <param name="ecdf_lookup_array">PointXY[]: which Y = ecdf(X)</param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double ecdf(PointXY[] ecdf_lookup_array, double x)
        {
            if (ecdf_lookup_array == null || ecdf_lookup_array.Count() < 2)
            {
                return double.NaN;
                //throw new InvalidOperationException("Cannot compute quantiles for an empty set.");
            }
            else
            {
                var sorted_lookup_array = from number in ecdf_lookup_array
                                          orderby number.X
                                          select number;

                //sorted_lookup_array.ToList().ForEach(t => Console.WriteLine($"X = {t.X}; Y = {t.Y}"));

                PointXY plow;
                PointXY phigh;

                if (x <= sorted_lookup_array.First().X)
                {
                    return 0;
                }
                else if (x >= sorted_lookup_array.Last().X)
                {
                    return 100;
                }
                else
                {
                    if (double.IsNaN(x))
                    {
                        return double.NaN;
                    }

                    plow = sorted_lookup_array.Where(number => number.X <= x).Last();
                    phigh = sorted_lookup_array.Where(number => number.X > x).First();
                    return (plow.Y + ((x - plow.X) / (phigh.X - plow.X)) * (phigh.Y - plow.Y));
                }
            }
        }

        /// <summary>
        /// Calculate quantiles(quantilelist) of given distribution(source)
        /// </summary>
        /// <param name="source">sample distribution</param>
        /// <param name="quantilelist">quantiles in unit 0 to 100</param>
        /// <returns></returns>
        public static PointXY[] Quantiles_for_ecdf(IEnumerable<double> source, IEnumerable<double> quantilelist)
        {
            List<PointXY> returnvalue = new List<PointXY>();
            source = source.Where(x => !double.IsNaN(x)).ToList(); // eliminate the double.NaNs 

            if (source.Count() < 2)
            {
                returnvalue.Add(new PointXY());
                return returnvalue.ToArray();
                //throw new InvalidOperationException("Cannot compute quantiles for an empty set.");
            }
            else
            {
                var sortedList = from number in source
                                 orderby number
                                 select number;

                List<PointXY> lookup = new List<PointXY>();
                double nitems = (double)source.Count();
                PointXY curpoint;
                for (int i = 0; i < nitems; i++)
                {
                    curpoint = new PointXY();
                    curpoint.Y = 100.0f * i / (nitems - 1);
                    curpoint.X = sortedList.ElementAt(i);
                    lookup.Add(curpoint);
                }

                PointXY plow;
                PointXY phigh;
                foreach (double q in quantilelist)
                {
                    if (q < 0 || q > 100) throw new Exception("q in quantile_list is not between 0% and 100%");
                    plow = lookup.Where(number => number.Y <= q).Last();
                    if (plow.Y < 100.0f)
                    {
                        phigh = lookup.Where(number => number.Y > q).First();
                        returnvalue.Add(new PointXY(Math.Round(plow.X + ((q - plow.Y) / (phigh.Y - plow.Y)) * (phigh.X - plow.X), 4), q));
                    }
                    else
                    {
                        returnvalue.Add(new PointXY(plow.X, plow.Y));
                        if (plow.Y != 100.0f) throw new Exception(String.Format("Why plow.Y is not 100 but {0}", plow.Y));
                    }
                }
            }
            return returnvalue.ToArray();
        }

        public static double Pearson(List<double> source1, List<double> source2)
        {
            double returnvalue = double.NaN;

            if (source1.Count() == source2.Count() && source1.Count() > 0 && source1.Count() < int.MaxValue)
            {
                double mean1 = source1.Average();
                double mean2 = source2.Average();
                List<RankItems> RankItemsList = new List<RankItems>();
                double npoints = source1.Count();
                for (int i = 0; i < npoints; i++) RankItemsList.Add(new RankItems(i, source1[i], 0, source2[i], 0));

                double sumproduct = RankItemsList.Select(x => (x.value1 - mean1) * (x.value2 - mean2)).Sum();
                double sumsquares1 = RankItemsList.Select(x => (x.value1 - mean1) * (x.value1 - mean1)).Sum();
                double sumsquares2 = RankItemsList.Select(x => (x.value2 - mean2) * (x.value2 - mean2)).Sum();

                if (sumsquares1 * sumsquares2 > 0)
                    returnvalue = sumproduct / Math.Sqrt(sumsquares1 * sumsquares2);
                else returnvalue = double.NaN;
            }

            return returnvalue;
        }

        public static double Spearman(List<double> source1, List<double> source2)
        {
            double returnvalue = double.NaN;

            List<RankItems> RankItemsList = new List<RankItems>();
            List<RankItems> SortList = new List<RankItems>();

            if (source1.Count() == source2.Count() && source1.Count() > 0 && source1.Count() < int.MaxValue)
            {

                double npoints = source1.Count();
                for (int i = 0; i < npoints; i++) RankItemsList.Add(new RankItems(i, source1[i], 0, source2[i], 0));

                //Rank the source1 values
                SortList = new List<RankItems>();
                SortList = RankItemsList.OrderBy(x => x.value1).ToList();
                for (int i = 0; i < npoints; i++) RankItemsList[SortList[i].index].rank1 = i + 1;

                //Rank the source2 values
                SortList = new List<RankItems>();
                SortList = RankItemsList.OrderBy(x => x.value2).ToList();
                for (int i = 0; i < npoints; i++) RankItemsList[SortList[i].index].rank2 = i + 1;

                List<double> r1 = RankItemsList.Select(x => (double)x.rank1).ToList();
                List<double> r2 = RankItemsList.Select(x => (double)x.rank2).ToList();

                returnvalue = Pearson(r1, r2);

            }

            return returnvalue;
        }

        // interface overload: KendallsTau with double[] input parameter.
        public static double[] KendallsTau(double[] data1, double[] data2)
        {
            if (data1.Length != data2.Length)
                throw new Exception("Two input vectors are not of the same length!");

            int IS = 0, j = 0, k = 0, n2 = 0, n1 = 0, n = 0, n_o;
            double svar, aa, a2, a1, tau = double.NaN, z_value = double.NaN, prob = double.NaN;

            n_o = data1.Length;

            for (j = 0; j < n_o - 1; j++)
            {
                if (double.IsNaN(data1[j]) || double.IsNaN(data2[j]))
                    continue;

                for (k = j + 1; k < n_o; k++)
                {
                    if (double.IsNaN(data1[k]) || double.IsNaN(data2[k]))
                        continue;

                    a1 = data1[j] - data1[k];
                    a2 = data2[j] - data2[k];
                    aa = a1 * a2;

                    if (aa != 0.0)
                    {
                        ++n1;
                        ++n2;
                        IS = IS + (aa > 0.0 ? 1 : -1);
                    }
                    else
                    {
                        if (a1 != 0) ++n1;
                        if (a2 != 0) ++n2;
                    }
                }

                n = n + 1;
            }
            if (!double.IsNaN(data1[n_o - 1]) && !double.IsNaN(data2[n_o - 1]))
                n = n + 1;

            if (n > 1)
            {
                tau = IS / (Math.Sqrt(n1) * Math.Sqrt(n2));
                svar = (4 * n + 10.0) / (9 * n * (n - 1));
                z_value = tau / Math.Sqrt(svar);
                prob = MathFunctions.erfc(Math.Abs(z_value) / 1.4142136);
            }

            return new double[4] { tau, z_value, prob, n };
        }

        public static double KendallsTau(List<double> source1, List<double> source2)
        {
            if (source1.Stdev() == 0 || source2.Stdev() == 0) return double.NaN;

            double returnvalue = double.NaN;
            int nc = 0;
            int nd = 0;

            int nextra1 = 0;
            int nextra2 = 0;

            List<RankItems> RankItemsList = new List<RankItems>();
            List<RankItems> SortList = new List<RankItems>();
            int npairs = 0;

            if (source1.Count() == source2.Count() && source1.Count() > 0 && source1.Count() < int.MaxValue)
            {
                double npoints = source1.Count();
                for (int i = 0; i < npoints; i++) RankItemsList.Add(new RankItems(i, source1[i], 0, source2[i], 0));

                //Rank the source1 values
                SortList = new List<RankItems>();
                SortList = RankItemsList.OrderBy(x => x.value1).ToList();
                for (int i = 0; i < npoints; i++) RankItemsList[SortList[i].index].rank1 = i + 1;

                //Rank the source2 values
                SortList = new List<RankItems>();
                SortList = RankItemsList.OrderBy(x => x.value2).ToList();
                for (int i = 0; i < npoints; i++) RankItemsList[SortList[i].index].rank2 = i + 1;

                //Calculate Kendalls Tau
                for (int i = 0; i < npoints; i++)
                {
                    for (int j = 0; j < npoints; j++)
                    {
                        if (i < j)
                        {
                            npairs++;
                            if ((RankItemsList[i].rank1 - RankItemsList[j].rank1) * (RankItemsList[i].rank2 - RankItemsList[j].rank2) > 0) nc++;
                            else if ((RankItemsList[i].rank1 - RankItemsList[j].rank1) * (RankItemsList[i].rank2 - RankItemsList[j].rank2) < 0) nd++;
                            else
                            {
                                if ((RankItemsList[i].rank1 - RankItemsList[j].rank1) != 0 && (RankItemsList[i].rank2 - RankItemsList[j].rank2) == 0) nextra1++;//if there is a tie in the rank2 add an extra rank1 pair
                                else if ((RankItemsList[i].rank1 - RankItemsList[j].rank1) == 0 && (RankItemsList[i].rank2 - RankItemsList[j].rank2) != 0) nextra2++; //if there is a tie in the rank1 add an extra rank2 pair 
                            }
                        }
                    }

                }
                //returnvalue = (double)(nc - nd) / (double)(npairs); // does not handle ties in the ranks

                returnvalue = (double)(nc - nd) / (Math.Sqrt((double)(nc + nd + nextra1)) * Math.Sqrt((double)(nc + nd + nextra2))); //includes ties in the ranks for the calculation
            }
            else returnvalue = double.NaN;

            return returnvalue;
        }
        static public ROCAnalysisResults ROCAnalysis(List<OutcomePoint> outcomespointlist)
        {
            ROCAnalysisResults returnvalue = new ROCAnalysisResults();

            List<double> valueslist = outcomespointlist.Select(x => x.Value).Distinct().OrderBy(x => x).ToList();
            List<ROCPoint> rocpointlist = new List<ROCPoint>();

            ROCPoint currocpoint = new ROCPoint();
            foreach (double curvalue in valueslist)
            {
                currocpoint = new ROCPoint();
                currocpoint.TruePositive = outcomespointlist.Where(x => x.Value >= curvalue).Where(x => x.Condition).Count();
                currocpoint.TrueNegatives = outcomespointlist.Where(x => x.Value < curvalue).Where(x => !x.Condition).Count();
                currocpoint.FalsePositives = outcomespointlist.Where(x => x.Value < curvalue).Where(x => x.Condition).Count();
                currocpoint.FalseNegatives = outcomespointlist.Where(x => x.Value >= curvalue).Where(x => !x.Condition).Count();

                currocpoint.Sensitivity = currocpoint.TruePositive / (currocpoint.TruePositive + currocpoint.FalseNegatives);
                currocpoint.SpecificityC = 1.0f - (currocpoint.TrueNegatives / (currocpoint.TrueNegatives + currocpoint.FalsePositives));

                currocpoint.Value = curvalue;
                rocpointlist.Add(currocpoint);
            }

            returnvalue.YoudenIndexThreshold = rocpointlist.OrderBy(x => x.Sensitivity - x.SpecificityC).Select(x => x.Value).First();
            currocpoint = new ROCPoint();
            currocpoint = rocpointlist.Where(x => x.Value == returnvalue.YoudenIndexThreshold).First();
            returnvalue.Sensitivity = currocpoint.Sensitivity;
            returnvalue.Specificity = 1.0f - currocpoint.SpecificityC;
            returnvalue.TruePositive = currocpoint.TruePositive;
            returnvalue.FalsePositives = currocpoint.FalsePositives;
            returnvalue.TrueNegatives = currocpoint.TrueNegatives;
            returnvalue.FalseNegatives = currocpoint.FalseNegatives;
            returnvalue.PositivePredictiveValue = currocpoint.TruePositive / (currocpoint.TruePositive + currocpoint.FalsePositives);
            returnvalue.NegativePredictiveValue = currocpoint.TrueNegatives / (currocpoint.TrueNegatives + currocpoint.FalseNegatives);



            return returnvalue;
        }


    }

    public class RankItems
    {
        public int index;
        public double value1;
        public int rank1;
        public double value2;
        public int rank2;

        public RankItems(int Index, double Value1, int Rank1, double Value2, int Rank2)
        {
            index = Index;
            value1 = Value1;
            rank1 = Rank1;
            value2 = Value2;
            rank2 = Rank2;

        }
    }


    public class ROCAnalysisResults
    {

        public double AUC { get; set; }
        public double YoudenIndexThreshold { get; set; }
        public double Sensitivity { get; set; }
        public double Specificity { get; set; }
        public double PositivePredictiveValue { get; set; }
        public double NegativePredictiveValue { get; set; }
        public int TruePositive { get; set; }
        public int FalsePositives { get; set; }
        public int TrueNegatives { get; set; }
        public int FalseNegatives { get; set; }
        public double FisherExact_p { get; set; }


    }
    public class ROCPoint
    {
        public double Value { get; set; }
        public double Sensitivity { get; set; }
        public double SpecificityC { get; set; }
        public int TruePositive { get; set; }
        public int FalsePositives { get; set; }
        public int TrueNegatives { get; set; }
        public int FalseNegatives { get; set; }

        public ROCPoint()
        {

        }
        public ROCPoint(double value, double sensitivity, double specificityc)
        {
            Value = value;
            Sensitivity = sensitivity;
            SpecificityC = specificityc;
        }
    }
    public class OutcomePoint
    {
        public double Value { get; set; }
        public bool Condition { get; set; }

        public OutcomePoint(double value, bool condition)
        {
            Value = value;
            Condition = condition;
        }
    }

}
