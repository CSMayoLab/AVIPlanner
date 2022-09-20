using Lib3_ESAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace AutoPlan_HN
{
    public class StrsOverlappingTester
    {
        StructureSet strSet;
        Structure s2;

        public StrsOverlappingTester(StructureSet strS)
        {
            strSet = strS;
            s2 = strS.Get_or_Add_structure("CONTROL", "ztemp_str_AP2");
        }

        public double CalcOverLappingVolume(Structure A, Structure B)
        {
            double rv = 0;

            if (A.IsHighResolution == B.IsHighResolution)
            {
                s2.SegmentVolume = A.And(B);
                rv = s2.Volume;
            }
            else if (A.IsHighResolution)
            {
                s2.SegmentVolume = B.SegmentVolume;
                s2.ConvertToHighResolution();
                s2.SegmentVolume = A.And(s2);
                rv = s2.Volume;
            }
            else if (B.IsHighResolution)
            {
                s2.SegmentVolume = A.SegmentVolume;
                s2.ConvertToHighResolution();
                s2.SegmentVolume = B.And(s2);
                rv = s2.Volume;
            }

            return rv;
        }
    }
}
