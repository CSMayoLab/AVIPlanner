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
    public enum DVHMetricType { Dxcc_Gy, DxPercent_Gy, Dxcc_Percent, DxPercent_Percent, VxGy_cc, VxGy_Percent, VxPercent_cc, VxPercent_Percent, DCxcc_Gy, DCxPercent_Gy, DCxcc_Percent, DCxPercent_Percent, CVxGy_cc, CVxGy_Percent, CVxPercent_cc, CVxPercent_Percent, Mean_Gy, Volume_cc, gEUD }

    public enum DoseUnitType { cGy, Gy, percent, EQ2Gy };
    public enum VolumeUnitType { cc, percent };


}
