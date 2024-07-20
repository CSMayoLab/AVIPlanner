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

using Accord.Statistics.Distributions.Univariate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsLibrary2
{

    public static class MathFunctions
    {

        public static double score_of_priorities(int[] priorities)
        {
            double rv = 0;
            foreach (int i in priorities)
            {
                rv += Math.Pow(2, -(i - 1));
            }
            return rv;
        }

        /// <summary>
        /// Numerical Recipies. intermediate function used in calculating erf, erfc
        /// </summary>
        /// <param name="z">double value >= 0</param>
        /// <returns></returns>
        //private static double  erfccheb(double z)
        //{
        //    double returnvalue = double.NaN;
        //    if (z >= 0)
        //    {

        //        double t;
        //        double ty;
        //        double tmp;
        //        double d = 0.0f;
        //        double dd = 0.0f;
        //        double [] cof = new double[] {
        //            -1.3026537197817094, 6.4196979235649026e-1,
        //         1.9476473204185836e-2,-9.561514786808631e-3,-9.46595344482036e-4,
        //         3.66839497852761e-4,4.2523324806907e-5,-2.0278578112534e-5,
        //         -1.624290004647e-6,1.303655835580e-6,1.5626441722e-8,-8.5238095915e-8,
        //         6.529054439e-9,5.059343495e-9,-9.91364156e-10,-2.27365122e-10,
        //         9.6467911e-11, 2.394038e-12,-6.886027e-12,8.94487e-13, 3.13092e-13,
        //         -1.12708e-13,3.81e-16,7.106e-15,-1.523e-15,-9.4e-17,1.21e-16,-2.8e-17};

        //        t = 2.0f/ (2.0f+ z);
        //        ty = 4.0f* t - 2.0f;
        //        for(int i = 27; i >= 0; i--)
        //        {
        //            tmp = d;
        //            d = ty * d - dd + cof[i];
        //            dd = tmp;
        //        }            
        //        returnvalue =  t * Math.Exp(-z * z + 0.5 * (cof[0] + ty * d) - dd);
        //    }
        //    return returnvalue;

        //}

        public static double erf(double x)
        {
            //Abromowitz and Stegun 7.1.26 error < 1.5x10^-7
            double returnvalue = double.NaN;
            double q = Math.Abs(x);

            double t = 1 / (1 + 0.3275911 * q);
            returnvalue = 0.0f;
            List<double> cof = new List<double> { 0.254829592, -0.284496736, 1.421413741, -1.453152027, 1.061405429 };
            for (int i = 1; i <= 5f; i++) returnvalue += cof[i - 1] * Math.Pow(t, i);
            returnvalue *= Math.Exp(-q * q);
            returnvalue = 1 - returnvalue;

            if (x < 0) returnvalue *= -1;

            return returnvalue;
        }

        public static double erfc(double x)
        {
            double returnvalue = double.NaN;

            if (x >= 0) returnvalue = 1.0f - erf(x);

            return returnvalue;
        }



        ///// <summary>
        ///// calculate Generalized Evaluation Metric (GEM) with Error function (cdf of Normal distr)
        ///// </summary>
        //public static double GEM(List<GEM_metric> GMs)
        //{
        //    double gem = double.NaN, weight = 0, weightSum = 0;

        //    // handle NaN, Inf, -Inf ---> leave to check at the INPUT layer

        //    // sum up for all metrics
        //    if (GMs.Any())
        //    {
        //        gem = 0;
        //        foreach (var m in GMs)
        //        {
        //            weight = Math.Pow(2, -(m.priority - 1));
        //            gem += weight * 0.5 * (1 + erf((m.planValue - m.constraint) / (m.q * m.constraint)));
        //            weightSum += weight;
        //        }
        //        gem = gem / weightSum;
        //    }

        //    return gem;
        //}
        /// <summary>
        /// calculate Generalized Evaluation Metric (GEM) with cdf of Gamma
        /// </summary>
        public static double GEM(List<GEM_metric> GMs, string Type = "gamma")
        {
            Misc.parameter_check(Type, "normal", "gamma");

            double gem = double.NaN, weight = 0, weightSum = 0;

            // handle NaN, Inf, -Inf ---> leave to check at the INPUT layer

            // sum up for all metrics
            if (Type == "normal")
            {
                if (GMs.Any())
                {
                    gem = 0;
                    foreach (var m in GMs)
                    {
                        weight = Math.Pow(2, -(m.priority - 1));
                        gem += weight * 0.5 * (1 + erf((m.planValue - m.constraint) / (m.q * m.constraint)));
                        weightSum += weight;
                    }
                    gem = gem / weightSum;
                }
            }

            if (Type == "gamma")
            {
                var GMs_valid = GMs.Where(m => (m.k > 0 && m.theta > 0)).ToList();
                if (GMs_valid.Any())
                {
                    gem = 0;
                    foreach (var m in GMs_valid)
                    {
                        weight = Math.Pow(2, -(m.priority - 1));

                        var gamma = new GammaDistribution(theta: 1, k: m.k);
                        gem += weight * gamma.DistributionFunction(m.planValue / m.theta);
                        weightSum += weight;
                    }
                    gem = gem / weightSum;
                }
            }
            return gem;
        }



        // Overload GEM with double[]s, construct List<GEM_metric>
        public static double GEM(double[] planValues, double[] constraints, double[] priorities, double[] qs)
        {
            if (planValues.Length != constraints.Length || constraints.Length != priorities.Length || priorities.Length != qs.Length)
            {
                Console.WriteLine("inputs dimensions do NOT match, please make sure 4 array have the same length");
                return double.NaN;
            }
            else
            {
                var GMs = new List<GEM_metric>();

                for (int i = 0; i < planValues.Length; i++)
                    GMs.Add(new GEM_metric() { planValue = planValues[i], constraint = constraints[i], priority = priorities[i], q = qs[i] });

                return GEM(GMs);
            }
        }
        // Overload GEM with doubles, construct one element List<GEM_metric>, primarily for test purpose with R.
        public static double GEM(double planValues, double constraints, double priorities, double qs)
        {
            var GMs = new List<GEM_metric> { new GEM_metric { planValue = planValues, constraint = constraints, priority = priorities, q = qs } };
            return GEM(GMs);
        }

        //public static double erf_old(double x)
        //{
        //    if (x >= 0.0f) return 1.0f - erfccheb(x);
        //    else return erfccheb(-x) - 1.0f;
        //}

        //public static double erfc_old(double x)
        //{
        //    if (x >= 0.0f) return erfccheb(x);
        //    else return 2.0 - erfccheb(-x);
        //}

        public static double gammln(double xx)
        {
            double returnvalue = double.NaN;
            double x;
            double tmp;
            double ser;

            double[] cof = new double[] { 76.18009173f, -86.50532033f, 24.01409822, -1.231739516, 0.120858003e-2f, -0.536382e-5f };
            x = xx - 1.0;
            tmp = x + 5.5;
            tmp -= (x + 0.5) * Math.Log(tmp);
            ser = 1.0;
            for (int j = 0; j <= 5; j++)
            {
                x += 1.0f;
                ser += cof[j] / x;
            }
            returnvalue = -tmp + Math.Log(2.50662827465 * ser);


            return returnvalue;

        }
        public static double factrl(int n)
        {
            double returnvalue = double.NaN;
            int ntop = 4;
            int j;
            double[] a = new double[33];
            a[0] = 1.0f;
            a[1] = 1.0f;
            a[2] = 2.0f;
            a[3] = 6.0f;
            a[4] = 24.0f;



            if (n >= 0)
            {
                if (n > 32) returnvalue = gammln(n + 1.0f);
                else
                {
                    while (ntop < n)
                    {
                        j = ntop++;
                        a[ntop] = a[j] * ntop;
                    }
                    returnvalue = a[n];
                }

            }

            return returnvalue;
        }
        public static double factln(int n)
        {
            double returnvalue = double.NaN;
            double[] a = new double[101];
            if (n >= 0)
            {
                returnvalue = 0.0f;
                if (n <= 1) returnvalue = 0.0f;
                else if (n < 20) for (int i = 1; i <= n; i++) returnvalue += Math.Log(i); //calculate exactly
                else if (n < 100) returnvalue = n * Math.Log(n) - n;//Stirling's Approximation
                else returnvalue = gammln(n + 1.0f);

            }

            return returnvalue;
        }
        public static double bico(int n, int k)
        {
            double returnvalue = double.NaN;
            if (n == k) returnvalue = 1;
            else if (n > k)
                returnvalue = Math.Floor(0.5 + Math.Exp(factln(n) - factln(k) - factln(n - k)));

            return returnvalue;

        }
        public static double betaz(double z, double w)
        {
            return Math.Exp(gammln(z) + gammln(w) - gammln(z + w));
        }


    }

}
