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

using AP_lib;
using AutoPlan_HN;
using AnalyticsLibrary2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Lib3_ESAPI
{
    public static class Esapi_exts_Writable
    {

        public static SegmentVolume Get_HD_SegmentVolume(this Structure str, Structure temp_str)
        {
            if (str.IsHighResolution)
                return str.SegmentVolume;
            else
            {
                temp_str.SegmentVolume = str.SegmentVolume;
                temp_str.ConvertToHighResolution();
                return temp_str.SegmentVolume;
            }
        }


        public static void Empty_by_joining_Empty(this Structure str, StructureSet curstructset)
        {
            Structure s3 = curstructset.Recreate_structure("CONTROL", "zAP_temp_s3");

            if (str.IsHighResolution) s3.ConvertToHighResolution();

            str.SegmentVolume = str.And(s3); 
            
            curstructset.RemoveStructure(s3);
        }


        public static Structure Recreate_structure(this StructureSet strS, string DICOM_type, string StrName)
        {
            strS.remove_structure_if_exist(StrName);    

            return strS.AddStructure(DICOM_type, StrName);
        }


        public static void Recreate_Nape_structure(this StructureSet strS, List<Structure> PTVs, double[] expansion_from_spinalCord_inMM, double margin_from_PTVs_InMM, string StrName = "zNape")
        {
            Structure SpinalCord = strS.Structures.SingleOrDefault(t => t.Id.Match_Std_TitleCase() == AP_lib.TG263.SpinalCord);

            if (SpinalCord == null) return;

            var e = expansion_from_spinalCord_inMM;
            if (e.Length != 6)
            {
                throw new Exception("expansion_from_spinalCord_inMM must be double[6]");
            }

            Structure s3 = strS.Get_or_Add_structure("CONTROL", "s3");

            Structure body = strS.Structures.Single(x => x.DicomType == "EXTERNAL");

            var zNape = strS.Get_or_Add_structure("ORGAN", StrName);

            zNape.SegmentVolume = SpinalCord.AsymmetricMargin(new AxisAlignedMargins(StructureMarginGeometry.Outer, e[0], e[1], e[2], e[3], e[4], e[5]));

            zNape.SegmentVolume = zNape.Sub(SpinalCord.Margin(10));


            foreach(var ptv in PTVs)
            {
                if (zNape.IsHighResolution != ptv.IsHighResolution)
                {
                    if (zNape.IsHighResolution)
                    {
                        zNape.SegmentVolume = zNape.Sub(ptv.Get_HD_SegmentVolume(s3).Margin(margin_from_PTVs_InMM));
                    }

                    if (zNape.IsHighResolution == false)
                    {
                        zNape.ConvertToHighResolution();
                        zNape.SegmentVolume = zNape.Sub(ptv.Margin(margin_from_PTVs_InMM));
                    }
                }
                else
                {
                    zNape.SegmentVolume = zNape.Sub(ptv.Margin(margin_from_PTVs_InMM));
                }
            }


            if(zNape.IsHighResolution != body.IsHighResolution)
            {
                if (zNape.IsHighResolution)
                {
                    zNape.SegmentVolume = zNape.And(body.Get_HD_SegmentVolume(s3).Margin(-5));
                }

                if(zNape.IsHighResolution == false)
                {
                    zNape.ConvertToHighResolution();   
                    zNape.SegmentVolume = zNape.And(body.Margin(-5));
                }
            }
            else
            {
                zNape.SegmentVolume = zNape.And(body.Margin(-5));
            }

            strS.RemoveStructure(s3);
        }

        public static void Recreate_zBuff_structure(this StructureSet strS, List<Structure> PTVs, double[] expansion_from_spinalCord_inMM, double margin_from_PTVs_InMM, double margine_round_1, string StrName = "zBuff")
        {
            Structure SpinalCord = strS.Structures.SingleOrDefault(t => t.Id.Match_Std_TitleCase() == AP_lib.TG263.SpinalCord);

            if (SpinalCord == null) return;

            var e = expansion_from_spinalCord_inMM;
            if(e.Length != 6)
            {
                throw new Exception("expansion_from_spinalCord_inMM must be double[6]");
            }

            Structure s3 = strS.Get_or_Add_structure("CONTROL", "s3");

            Structure body = strS.Structures.Single(x => x.DicomType == "EXTERNAL");

            var zBuff = strS.Get_or_Add_structure("ORGAN", StrName);

            zBuff.SegmentVolume = SpinalCord.Margin(margine_round_1).AsymmetricMargin(new AxisAlignedMargins(StructureMarginGeometry.Outer, e[0], e[1], e[2], e[3], e[4], e[5]));


            foreach (var ptv in PTVs)
            {
                if (zBuff.IsHighResolution != ptv.IsHighResolution)
                {
                    if (zBuff.IsHighResolution)
                    {
                        zBuff.SegmentVolume = zBuff.Sub(ptv.Get_HD_SegmentVolume(s3).Margin(margin_from_PTVs_InMM));
                    }

                    if (zBuff.IsHighResolution == false)
                    {
                        zBuff.ConvertToHighResolution();
                        zBuff.SegmentVolume = zBuff.Sub(ptv.Margin(margin_from_PTVs_InMM));
                    }
                }
                else
                {
                    zBuff.SegmentVolume = zBuff.Sub(ptv.Margin(margin_from_PTVs_InMM));
                }
            }

            if (zBuff.IsHighResolution != body.IsHighResolution)
            {
                if (zBuff.IsHighResolution)
                {
                    zBuff.SegmentVolume = zBuff.And(body.Get_HD_SegmentVolume(s3).Margin(-5));
                }

                if (zBuff.IsHighResolution == false)
                {
                    zBuff.ConvertToHighResolution();
                    zBuff.SegmentVolume = zBuff.And(body.Margin(-5));
                }
            }
            else
            {
                zBuff.SegmentVolume = zBuff.And(body.Margin(-5));
            }

            strS.RemoveStructure(s3);
        }


        public static Structure Get_or_Add_structure(this StructureSet strS, string DICOM_type, string StrName)
        {
            if (strS.has(StrName))
            {
                return strS.get(StrName);
            }
            else
            {
                return strS.AddStructure(DICOM_type, StrName);
            }
        }

        public static Structure Add_structure_and_check(this StructureSet strS, string DICOM_type, string StrName, Application app = null)
        {
            var s = strS.AddStructure(DICOM_type, StrName);
            //app?.SaveModifications();

            if(s.DicomType != DICOM_type)
            {
                throw new Exception($"Newly created str {StrName} has DICOM type [{s.DicomType}] instead of the specified [{DICOM_type}]!");
            }

            if(s.Id != StrName)
            {
                throw new Exception($"Newly created str [{s.Id}] doesn't have the assigned name [{StrName}]!");
            }

            return s;
        }


        public static void remove_structure_if_exist(this StructureSet strS, string StrName)
        {
            var str = strS.Structures.SingleOrDefault(t => t.Id.ToUpper() == StrName.ToUpper());

            if (str == null) return;

            if (string.IsNullOrEmpty(str.DicomType))
            {
                throw new Exception($"[{str.Id}] has empty Dicom type, and this script cannot remove this structure. If possible, remove this structure manually and try again.");
            }

            strS.RemoveStructure(str);
        }


        public static Structure Get_or_Create_zTemp_str(this StructureSet strS, string TempStrName)
        {
            if (strS.Structures.Any(t => t.Id == TempStrName))
            {
                return strS.Structures.Single(t => t.Id == TempStrName);
            }

            // in case of different capitalization, remove and recreate.
            if (strS.Structures.Any(t => t.Id.ToUpper() == TempStrName.ToUpper() && t.Id != TempStrName))
            {
                var str = strS.Structures.Single(t => t.Id.ToUpper() == TempStrName.ToUpper() && t.Id != TempStrName);

                strS.RemoveStructure(str);

                //throw new Exception($"Temporary structure cann not be created or retrieved, since there is an existing structure with the same name {TempStrName} but differernt capitalization {str.Id}");
            }

            return strS.AddStructure("ORGAN", TempStrName);

        }

        public static void Remove_zTemp_str_if_exist(this StructureSet strS, string TempStrName)
        {
            if (strS.Structures.Any(t => t.Id.ToUpper() == TempStrName.ToUpper()))
            {
                strS.RemoveStructure(strS.Structures.Single(t => t.Id.ToUpper() == TempStrName.ToUpper()));
            }
        }

    }

}
