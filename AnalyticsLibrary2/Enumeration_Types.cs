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
