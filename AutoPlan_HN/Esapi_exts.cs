//NOTICE: © 2022 The Regents of the University of Michigan
//using System.Runtime.InteropServices.ComTypes;

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
using VMS.TPS.Common.Model.API;
using System.Reflection;
using VMS.TPS.Common.Model.Types;
using System.Runtime.CompilerServices;
using Lib3_ESAPI;
using AnalyticsLibrary2;

namespace ESAPI_Extensions
{
    public static partial class Esapi_exts
    {

        public static string ToString_coordinate(this VVector iso)
        {
            string coordinates = string.Format($"X: {iso.x:F0}mm;  Y: {iso.y:F0}mm;  Z:{iso.z:F0}mm;");
            return coordinates;
        }

        public static StructureApprovalStatus GetApprovalStatus(this Structure str)
        {
            return str.ApprovalHistory.OrderByDescending(a => a.ApprovalDateTime).First().ApprovalStatus;
        }

        public static void Check_n_Convert_to_HD(this Structure str)
        {
            if (str.IsHighResolution == false)
            {
                str.ConvertToHighResolution();
            }
        }
    }
}



public static partial class Esapi_exts
{
    public static StructureApprovalStatus GetApprovalStatus(this Structure str)
    {
        return str.ApprovalHistory.OrderByDescending(a => a.ApprovalDateTime).First().ApprovalStatus;
    }


    public static bool has_std_strn(this StructureSet strS, string strName)
    {
        return strS.Structures.Any(t => t.Id.Match_Std_TitleCase() == strName.Match_Std_TitleCase());
    }

    public static bool has(this StructureSet strS, string strName)
    {
        return strS.Structures.Any(t => t.Id.ToUpper() == strName.ToUpper());
    }

    public static bool has_exact(this StructureSet strS, string strName)
    {
        return strS.Structures.Any(t => t.Id == strName);
    }

    public static Structure get_std_strn(this StructureSet strS, string strName)
    {
        return strS.Structures.Single(t => t.Id.Match_Std_TitleCase() == strName.Match_Std_TitleCase());
    }

    public static Structure get(this StructureSet strS, string strName)
    {
        return strS.Structures.Single(t => t.Id.ToUpper() == strName.ToUpper());
    }

    public static Structure get_exact(this StructureSet strS, string strName)
    {
        return strS.Structures.Single(t => t.Id == strName);
    }


    public static string ToString_withVolume(this Structure str)
    {
        if (str == null) return string.Format("{0,3}", "");
        return string.Format("{0} {1}", str.Id, string.Format("({0:F2}cc)", str.Volume));
    }

    //public static string ToString_withVolume(this Structure str) // leave Empty spaces if str is null.
    //{
    //    if (str == null) return string.Format("{0,47}", "");
    //    //if (str == null) return "";
    //    return string.Format("{0,-16} {1,-12}", str.Id, string.Format("({0:F2}cc)", str.Volume));
    //}


    public static string ToString_VolDates(this Structure str)
    {
        string msg = str.Id + " " + Math.Round(str.Volume, 4) + "\n" +
            (str.IsHighResolution ? "HighRes" : "LowRes") + "\n" +
            str.ApprovalHistory.OrderByDescending(a => a.ApprovalDateTime).First().ApprovalStatus.ToString() + " " +
            str.ApprovalHistory.OrderByDescending(a => a.ApprovalDateTime).First().ApprovalDateTime.ToString() +
            "\nModification Date: " + str.HistoryDateTime.ToString();
        return msg;
    }

    public static StructureSetSummary Generate_StructureSetSummary(this StructureSet StrS, LogPoint lpv, string LogPointComment = "")
    {
        var sil = new StructureSetSummary(lpv, LogPointComment, StrS);

        return sil;
    }
}
