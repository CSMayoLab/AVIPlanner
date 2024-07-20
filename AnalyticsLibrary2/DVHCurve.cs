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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsLibrary2
{

    public class DVHCurve
    {
        public int DVHCurve_ID;

        public string Structure;
        public double Volume;
        public double? NormDose;
        public List<PointXY> DVHPointsList { get; set; }

        public DoseUnitType DoseUnits { get { return DoseUnitType.Gy; } }
        public VolumeUnitType VolumeUnits { get { return VolumeUnitType.percent; } }

        public PointXY[] DVHPointsList_31
        {
            get
            {
                if (_DVHPointsList_31 == null)
                {
                    _DVHPointsList_31 = serialize.volume_values.ToList().Select(v => new PointXY() { X = DVHMetrics.DVHMetricValue(this, DVHMetricType.DxPercent_Gy, v), Y = v }).ToArray();
                }
                return _DVHPointsList_31;
            }
        }

        private PointXY[] _DVHPointsList_31 = null;

        public double Min_Gy { get { return DVHPointsList.Last(p => p.Y == 100).X; } }
        public double Max_Gy { get { return DVHPointsList.Last(p => p.Y == 0).X; } }
        public double Mean_Gy { get { return DVHMetrics.Mean_Gy(this); } }
        public double Coverage_area(double target_dose) { return DVHMetrics.Dose_Coverage_area(this, target_dose); }
        public double Hotspot(double dose_limit) { return DVHMetrics.Dose_Hotspot(this, dose_limit); }
        public double Over_con_area(constraint con) { return DVHMetrics.over_con_area(this, con); }


        public DVHCurve(IStatsDVH_input line) : this(
            DVHMetrics.get_DVH_points_from_SQL_line(line.DVHCurve_ByVolumePercentList),
            line.Volume_cc ?? double.NaN,
            line.TotalDose_Delivered,
            DoseUnitType.Gy,
            VolumeUnitType.percent
            )
        { DVHCurve_ID = line.DVHCurve_ID; }


        // normdose_Gy has to be Gy, not cGy, as the name suggested.
        public DVHCurve(List<PointXY> cdvh, double vol_cc, double? normdose_Gy, DoseUnitType dut, VolumeUnitType vut)
        {
            double dosefactor;
            double volfactor;

            Volume = vol_cc;
            NormDose = normdose_Gy;

            if (dut.Equals(DoseUnitType.Gy)) dosefactor = 1.0f;
            else if (dut.Equals(DoseUnitType.cGy)) dosefactor = 0.01f;
            else if (dut.Equals(DoseUnitType.percent)) dosefactor = (normdose_Gy / 100.0f) ?? double.NaN;
            else dosefactor = double.NaN;

            if (vut.Equals(VolumeUnitType.cc)) volfactor = 100.0f / vol_cc;
            else if (vut.Equals(VolumeUnitType.percent)) volfactor = 1.0f;
            else volfactor = double.NaN;

            DVHPointsList = new List<PointXY>();
            foreach (PointXY pxy in cdvh) DVHPointsList.Add(new PointXY(dosefactor * pxy.X, volfactor * pxy.Y));

            DVHPointsList = DVHPointsList.OrderByDescending(p => p.Y).ThenBy(p => p.X).ToList();

            if (DVHPointsList.First().Y < 99.9)
            {
                Log3_static.Warning($"DVH curve input is not complete: only {DVHPointsList.First().Y} volumn exists.");
            }
            else if (DVHPointsList.First().Y < 100.01)
            {
                Log3_static.Information("Extrapolate the end of the curve to cover 100% volume");
                DVHPointsList.First().Y = 100;
            }

            if (DVHPointsList.Last().Y > 0.1)
            {
                Log3_static.Warning($"DVH curve input is not complete: at least 0.1% volumn in the HIGH dose part is missing! Only { DVHPointsList.First().Y} volumn exists.");
            }
            else if (DVHPointsList.Last().Y > 0)
            {
                Log3_static.Information("Extrapolate the end of the curve to cover 0% volume");
                DVHPointsList.Last().Y = 0;
            }
        }

        public DVHCurve() { }

        public DVHCurve(DVHCurve inputdvhc)
        {
            this.DVHCurve_ID = inputdvhc.DVHCurve_ID;
            this.Structure = inputdvhc.Structure;
            //this.Min_Gy = inputdvhc.Min_Gy; // re-structured as readonly properties.
            //this.Max_Gy = inputdvhc.Max_Gy; // re-structured as readonly properties.
            this.Volume = inputdvhc.Volume;
            this.NormDose = inputdvhc.NormDose;
            //this.dvhpointslist = inputdvhc.dvhpointslist;

        }

        public static DVHCurve Combine_two_DVHCurves(DVHCurve dvh1, DVHCurve dvh2)
        {
            int n = 101;
            var d = new double[n]; var v1 = new double[n]; var v2 = new double[n]; var v = new double[n];
            var V1 = dvh1.Volume; var V2 = dvh2.Volume;
            var V = V1 + V2;
            List<PointXY> DVHPointsList = new List<PointXY>();

            double dmax = Math.Max(dvh1.Max_Gy, dvh2.Max_Gy);

            for (int i = 0; i < n; i++)
            {
                d[i] = i * dmax / 100;
                v1[i] = DVHMetrics.DVHMetricValue(dvh1, DVHMetricType.VxGy_Percent, d[i]);
                v2[i] = DVHMetrics.DVHMetricValue(dvh2, DVHMetricType.VxGy_Percent, d[i]);
                v[i] = (v1[i] * V1 + v2[i] * V2) / V;
                DVHPointsList.Add(new PointXY(d[i], v[i]));
            }

            if (Math.Abs(v[0] - 100) < 0.0000001) v[0] = 100;
            if (Math.Abs(v[100]) < 0.0000001) v[100] = 0;

            if (v[100] != 0 || v[0] != 100)
            {
                Console.WriteLine("v1[100] = {0}; v2[100] = {1}; d[100] = {2}; \n{3} \n{4}", v1[100], v2[100], d[100], Misc.obj_to_string(dvh1), Misc.obj_to_string(dvh2));
                throw new InvalidDataException();
            }

            return new DVHCurve(DVHPointsList, V, double.NaN, DoseUnitType.Gy, VolumeUnitType.percent);
        }
    }

}
