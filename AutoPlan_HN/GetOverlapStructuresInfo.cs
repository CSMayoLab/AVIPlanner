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
using System.Windows.Media.Media3D;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Lib3_ESAPI
{
    public static partial class Esapi_exts
    {

        public static List<OverlapStructuresInfo> GetOverlapStructuresInfo(this StructureSet curss, List<Structure> PTVs, List<Structure> OARs, string zsn = "ztemp_str_SRSAS")
        {
            List<OverlapStructuresInfo> returnvalue = new List<OverlapStructuresInfo>();

            Structure zteststrct = curss.Get_or_Create_zTemp_str(zsn);
            Structure s2 = curss.Get_or_Create_zTemp_str(zsn.Substring(0,15)+"2");

            double overlap_percent;

            List<Point3D> p3d1 = new List<Point3D>();
            List<Point3D> p3d2 = new List<Point3D>();

            double mindist = double.NaN;
            double test = double.NaN;

            int deltak;
            int deltal;


            //Get information on overlaps and proximity

            for (int i = 0; i < PTVs.Count; i++)
            {
                for (int j = 0; j < OARs.Count; j++)
                {
                    if (PTVs[i].IsHighResolution && !OARs[j].IsHighResolution)
                    {
                        s2.SegmentVolume = OARs[j].SegmentVolume;
                        s2.ConvertToHighResolution();
                        zteststrct.SegmentVolume = (PTVs[i].And(s2));
                    }
                    else if (!PTVs[i].IsHighResolution && OARs[j].IsHighResolution)
                    {
                        s2.SegmentVolume = PTVs[i].SegmentVolume;
                        s2.ConvertToHighResolution();
                        zteststrct.SegmentVolume = (OARs[j].And(s2));
                    }
                    else
                    {
                        zteststrct.SegmentVolume = (PTVs[i].And(OARs[j]));
                    }


                    overlap_percent = Math.Round(zteststrct.Volume / OARs[j].Volume * 100, 2);

                    if (!zteststrct.IsEmpty)
                    {
                        returnvalue.Add(new OverlapStructuresInfo(PTVs[i].Id, OARs[j].Id, 0, overlap_percent));
                    }
                    else
                    {
                        p3d1 = PTVs[i].MeshGeometry.Positions.ToList();
                        p3d2 = OARs[j].MeshGeometry.Positions.ToList();
                        mindist = -1.0d;

                        deltak = (int)(Math.Pow(PTVs[i].Volume, 0.333) * 20.0d);
                        deltak = deltak < 1 ? 1 : deltak;

                        deltal = (int)(Math.Pow(OARs[j].Volume, 0.333) * 20.0d);
                        deltal = deltal < 1 ? 1 : deltal;

                        for (int k = 0; k < p3d1.Count; k += deltak)
                        {
                            for (int l = 0; l < p3d2.Count; l += deltal)
                            {
                                test = (p3d1[k].X - p3d2[l].X) * (p3d1[k].X - p3d2[l].X) + (p3d1[k].Y - p3d2[l].Y) * (p3d1[k].Y - p3d2[l].Y) + (p3d1[k].Z - p3d2[l].Z) * (p3d1[k].Z - p3d2[l].Z);
                                mindist = mindist < 0 ? test : (test < mindist ? test : mindist);
                            }

                        }

                        returnvalue.Add(new OverlapStructuresInfo(PTVs[i].Id, OARs[j].Id, Math.Sqrt(mindist) * 0.1d, overlap_percent));
                    }
                }
            }

            //curss.RemoveStructure(zteststrct);

            curss.RemoveStructure(s2);
            return returnvalue;
        }


    }


    public class OverlapStructuresInfo
    {
        public string PTV { get; set; }
        public string OAR { get; set; }

        public double Distance_cm { get; set; }
        public double OverlapFracRelStruct2 { get; set; }

        public OverlapStructuresInfo(string s1, string s2, double dist_cm, double ovlpfrac)
        {
            PTV = s1;
            OAR = s2;
            Distance_cm = dist_cm;
            OverlapFracRelStruct2 = ovlpfrac;
        }
    }
}
