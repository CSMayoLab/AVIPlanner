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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnalyticsLibrary2
{
    public static class StructureNamesMapping
    {
        public static string Title_Case(this string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;


            // First Match TG263 structure Names list.
            var TG263_match = TG263.TG263_StructureNames.SingleOrDefault(t => t.ToUpper() == s.ToUpper());

            if (!string.IsNullOrEmpty(TG263_match)) return TG263_match;



            s = s.ToLower();

            if (s.Equals("Bowel_Small_PRV", StringComparison.CurrentCultureIgnoreCase)) return "Bowel_Small_PRV";
            if (s.Equals("Brainstem_PRV03", StringComparison.CurrentCultureIgnoreCase)) return "Brainstem_PRV03";
            if (s.Equals("Brainstem_PRV05", StringComparison.CurrentCultureIgnoreCase)) return "Brainstem_PRV05";
            if (s.Equals("BrachialPlex_L", StringComparison.CurrentCultureIgnoreCase)) return "BrachialPlex_L";
            if (s.Equals("BrachialPlex_R", StringComparison.CurrentCultureIgnoreCase)) return "BrachialPlex_R";
            if (s.Equals("BrachialPlexs", StringComparison.CurrentCultureIgnoreCase)) return "BrachialPlexs";

            if (s.Equals("CaudaEquina", StringComparison.CurrentCultureIgnoreCase)) return "CaudaEquina";
            if (s.Equals("Cavity_Oral-PTV", StringComparison.CurrentCultureIgnoreCase)) return "Cavity_Oral-PTV";
            if (s.Equals("Duodenum_PRV", StringComparison.CurrentCultureIgnoreCase)) return "Duodenum_PRV";

            if (s.Equals("GreatVes", StringComparison.CurrentCultureIgnoreCase)) return "GreatVes";

            if (s.Equals("Lungs-GTV", StringComparison.CurrentCultureIgnoreCase)) return "Lungs-GTV";
            if (s.Equals("Lungs-ITV", StringComparison.CurrentCultureIgnoreCase)) return "Lungs-ITV";

            if (s.Equals("Liver-GTV", StringComparison.CurrentCultureIgnoreCase)) return "Liver-GTV";
            if (s.Equals("Liver-PTV", StringComparison.CurrentCultureIgnoreCase)) return "Liver-PTV";

            if (s.Equals("OpticChiasm", StringComparison.CurrentCultureIgnoreCase)) return "OpticChiasm";
            if (s.Equals("OpticChiasm_PRV3", StringComparison.CurrentCultureIgnoreCase)) return "OpticChiasm_PRV3";
            if (s.Equals("OpticNrv_L", StringComparison.CurrentCultureIgnoreCase)) return "OpticNrv_L";
            if (s.Equals("OpticNrv_PRV03_L", StringComparison.CurrentCultureIgnoreCase)) return "OpticNrv_PRV03_L";
            if (s.Equals("OpticNrv_R", StringComparison.CurrentCultureIgnoreCase)) return "OpticNrv_R";
            if (s.Equals("OpticNrv_PRV03_R", StringComparison.CurrentCultureIgnoreCase)) return "OpticNrv_PRV03_R";

            if (s.Equals("PenileBulb", StringComparison.CurrentCultureIgnoreCase)) return "PenileBulb";

            if (s.Equals("SacralPlex", StringComparison.CurrentCultureIgnoreCase)) return "SacralPlex";
            if (s.Equals("SpinalCord", StringComparison.CurrentCultureIgnoreCase)) return "SpinalCord";
            if (s.Equals("SpinalCord_PRV05", StringComparison.CurrentCultureIgnoreCase)) return "SpinalCord_PRV05";
            if (s.Equals("SpinalCanal", StringComparison.CurrentCultureIgnoreCase)) return "SpinalCanal";
            if (s.Equals("SpinalCanal_PRV5", StringComparison.CurrentCultureIgnoreCase)) return "SpinalCanal_PRV5";

            if (s.Equals("Stomach_PRV", StringComparison.CurrentCultureIgnoreCase)) return "Stomach_PRV";


            s = s.Replace("-ptv", "-PTV");
            s = s.Replace("prv", "PRV");

            char[] array = s.ToCharArray();
            // Handle the first letter in the string.
            if (char.IsLower(array[0]))
            {
                array[0] = char.ToUpper(array[0]);
            }
            // Scan through the letters, checking for spaces.
            // ... Uppercase the lowercase letters following spaces.
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i - 1] == ' ' || array[i - 1] == '_')
                {
                    if (char.IsLower(array[i]))
                    {
                        array[i] = char.ToUpper(array[i]);
                    }
                }
            }

            return new string(array);
        }

        public static string Match_Std_TitleCase(this string str_name, string datasource = "")
        {
            return str_name.Match_StrID_to_Standard_Name(datasource).Title_Case();
        }

        public static string Match_StrID_to_Standard_Name(this string sql_name, string datasource)
        {
            string returnvalue = sql_name.Trim().ToUpper();
            datasource = datasource.ToUpper();

            if ((returnvalue.ToLower().Contains("opt") && !Regex.Match(returnvalue, @"^OPT[ic _Nerve]{3,9}", RegexOptions.IgnoreCase).Success)
                || Regex.Match(returnvalue, @"^PTV", RegexOptions.IgnoreCase).Success
                || Regex.Match(returnvalue, @"^z", RegexOptions.IgnoreCase).Success
                //|| returnvalue.ToLower().Contains("-ptv") || returnvalue.ToLower().Contains("_ptv")
                ) return "";

            if ((new string[] { "Pharynx", "Ear_Middle_L", "Ear_Middle_R", "Ear_Internal_L", "Ear_Internal_R", "Glnd_Thyroid", "Brain", "Brainstem", "Brainstem_PRV03", "OpticChiasm", "OpticChiasm_PRV3", "Cochlea_L", "Cochlea_R", "Musc_Constrict_I", "Musc_Constrict_S", "SpinalCord", "SpinalCord_PRV05", "Esophagus", "Eye_L", "Eye_R", "Glnd_Lacrimal_L", "Glnd_Lacrimal_R", "Larynx", "Lens_L", "Lens_R", "Lips", "Bone_Mandible", "OpticNrv_L", "OpticNrv_PRV03_L", "OpticNrv_R", "OpticNrv_PRV03_R", "Cavity_Oral", "Parotid_L", "Parotid_R", "Parotid_Low", "Parotid_High", "Glnd_Submand_L", "Glnd_Submand_R", "Glnd_Submand_Low", "Glnd_Submand_High", "Lobe_Temporal_L", "Lobe_Temporal_R",
            "Bladder", "Rectum", "Bowel", "Colon_Sigmoid", "PenileBulb", "Femur_L", "Femur_R",
            "Femur_Head_R","Femur_Head_L",
            "Lungs-GTV", "Lungs-ITV", "Heart", "Stomach", "BrachialPlex_R", "BrachialPlex_L", "GreatVes", "Chestwall", "Trachea", "Bronchus_Main",
             "SpinalCanal", "SpinalCanal_PRV5",
             "Lungs", "Lung_L", "Lung_R", "Kidneys", "Kidney_L", "Kidney_R", "Liver", "Liver-GTV", "Duodenum", "Duodenum_PRV", "Bowel_Small", "Bowel_Small_PRV", "Stomach_PRV", "Chestwall", "Rib", "Colon", "Liver-PTV",

             "Skin", "Urethra", "SacralPlex", "CaudaEquina"
            }).Any(t => t.Equals(returnvalue, StringComparison.CurrentCultureIgnoreCase)))
                return returnvalue.ToUpper();




            // If matched with TG263, return directly.
            
            var TG263_match = TG263.TG263_StructureNames.SingleOrDefault(t => t.ToUpper() == returnvalue.ToUpper());

            if (!string.IsNullOrEmpty(TG263_match)) return TG263_match.ToUpper();





            //if (datasource == "HN_UPenn".ToUpper()) return match_structure_HNRegistry_Dev_UPenn(returnvalue).ToUpper();
            //if (datasource == "HN_UAB".ToUpper()) return match_structure_HNRegistry_Dev_UAB(returnvalue).ToUpper();
            //if (datasource == "HN_Dalhousie".ToUpper()) return match_structure_HNRegistry_Dev_Dalhousie(returnvalue).ToUpper();

            //if (returnvalue == "CT_BLADDER") return "BLADDER"; // temporay trick.
            //if (returnvalue == "CT_RECTUM") return "RECTUM";

            #region NameMapping LIVER_SBRT -----------------------------------------------------------------
            // Bowel_Small
            if (returnvalue == "BOWEL_SMALL" || returnvalue == "SMALL_BOWEL")
                return "Bowel_Small".ToUpper();

            // Chestwall
            if (returnvalue == "CHESTWALL/RIB" || returnvalue == "CHESTWALL")
                return "chestwall".ToUpper();

            // Kidney_R
            if (returnvalue == "KIDNEY_R" || returnvalue == "KIDNEY_RT")
                return "Kidney_R".ToUpper();

            // Kidney_L
            if (returnvalue == "KIDNEY_L" || returnvalue == "KIDNEY_LT")
                return "kidney_l".ToUpper();

            // Kidneys
            if (returnvalue == "KIDNEY_TOTAL" || returnvalue == "KIDNEYS")
                return "kidneys".ToUpper();

            // SpinalCanal
            if ((Regex.Match(returnvalue, @".*CORD$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @".*CANAL$", RegexOptions.IgnoreCase).Success) && (datasource.ToUpper() == "LUNG_CRT" || datasource.ToUpper() == "Esophagus".ToUpper()))
                return "spinalcanal".ToUpper();

            // SpinalCanal_PRV5
            if ((Regex.Match(returnvalue, @".*CORD.*", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @".*CANAL.*", RegexOptions.IgnoreCase).Success) && Regex.Match(returnvalue, @".*PRV0?5$", RegexOptions.IgnoreCase).Success && (datasource.ToUpper() == "LUNG_CRT" || datasource.ToUpper() == "Esophagus".ToUpper()))
                return "spinalcanal_prv5".ToUpper();

            // SpinalCord_PRV05
            if (Regex.Match(returnvalue, @"^(Expanded|PRV)?[Spinal _]*CORD[_PRV+ 0 expanded]*5[ _cmm]*(exp|expd|expanded)?$", RegexOptions.IgnoreCase).Success)
                return "spinalcord_prv05".ToUpper();

            // SpinalCord_PRV03
            if (Regex.Match(returnvalue, @"^(Expanded|PRV)?[Spinal _]*CORD[_PRV+ 0 expanded]*3[ _cmm]*(exp|expd|expanded)?$", RegexOptions.IgnoreCase).Success)
                return "spinalcord_prv03".ToUpper();

            // SpinalCord
            if (Regex.Match(returnvalue, @"^(Spinal)?[ _]?CORD$", RegexOptions.IgnoreCase).Success)
                return "spinalcord".ToUpper();

            //// Lungs // use the one below in HN section.
            //if (returnvalue == "LUNGS" || returnvalue == "LUNG_TOTAL")
            //    return "lungs".ToUpper(); // datasource ESOPHAGUS

            // Lungs-ITV
            if (Regex.Match(returnvalue, @".*LUNG.*-ITV.*", RegexOptions.IgnoreCase).Success)
                return "lungs-itv".ToUpper(); // datasource LUNG

            // BrachialPlex_L
            if (Regex.Match(returnvalue, @"^(Left|Lt|L)[,_ ]{0,2}Brac[hial_ ]{0,5}Pl(e)?x[us]{0,2}$", RegexOptions.IgnoreCase).Success ||
                Regex.Match(returnvalue, @"^Brac[hial_ ]{0,5}Pl(e)?x[us]{0,2}[,_ ]{0,2}(Left|Lt|L)$", RegexOptions.IgnoreCase).Success
                //(Regex.Match(returnvalue, @"^(Left|Lt|L_|L )", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"(Left|Lt|_L| L)$", RegexOptions.IgnoreCase).Success)
                )
                return "brachialplex_l".ToUpper(); // datasource LUNG

            // BrachialPlex_R
            if (Regex.Match(returnvalue, @"^(Right|Rt|R)[,_ ]{0,2}Brac[hial_ ]{0,5}Pl(e)?x[us]{0,2}$", RegexOptions.IgnoreCase).Success ||
                Regex.Match(returnvalue, @"^Brac[hial_ ]{0,5}Pl(e)?x[us]{0,2}[,_ ]{0,2}(Right|Rt|R)$", RegexOptions.IgnoreCase).Success
                //(Regex.Match(returnvalue, @"^(Right|Rt|R_|R )", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"(Right|Rt|_R| R)$", RegexOptions.IgnoreCase).Success)
                )
                return "brachialplex_r".ToUpper(); // datasource LUNG

            if (Regex.Match(returnvalue, @"^BRAC.*Pl(e)?x[us]{0,2}$", RegexOptions.IgnoreCase).Success)
                return "BrachialPlexs".Title_Case();

            // GreatVes
            if (Regex.Match(returnvalue, @"GREAT.*VES", RegexOptions.IgnoreCase).Success && (!Regex.Match(returnvalue, @"OPT", RegexOptions.IgnoreCase).Success) && (!Regex.Match(returnvalue, @".ITV", RegexOptions.IgnoreCase).Success))
                return "greatves".ToUpper(); // datasource LUNG_SBRT

            // Bronchus_Main
            if (Regex.Match(returnvalue, @"^BRONCH.*MAIN$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"^MAIN.*BRONCH.*", RegexOptions.IgnoreCase).Success)
                return "bronchus_main".ToUpper();
            #endregion

            #region NameMapping Prostate -----------------------------------------------------------------
            // Femur_L
            if (Regex.Match(returnvalue, @"^FEMUR.*_L$", RegexOptions.IgnoreCase).Success && !datasource.Contains("PROSTATE_SBRT"))
                return "femur_l".ToUpper();

            // Femur_R
            if (Regex.Match(returnvalue, @"^FEMUR.*_R$", RegexOptions.IgnoreCase).Success && !datasource.Contains("PROSTATE_SBRT"))
                return "femur_r".ToUpper();

            // Femur_Head_L
            if (Regex.Match(returnvalue, @"^FEMUR.*_L$", RegexOptions.IgnoreCase).Success && datasource.Contains("PROSTATE_SBRT"))
                return "femur_head_l".ToUpper();

            // Femur_Head_R
            if (Regex.Match(returnvalue, @"^FEMUR.*_R$", RegexOptions.IgnoreCase).Success && datasource.Contains("PROSTATE_SBRT"))
                return "femur_head_r".ToUpper();

            // PenileBulb
            if (Regex.Match(returnvalue, @"^PENILE.*BULB$", RegexOptions.IgnoreCase).Success)
                return "penilebulb".ToUpper();

            // Colon_Sigmoid
            if (returnvalue == "SIGMOID" || returnvalue == "COLON_SIGMOID")
                return "colon_sigmoid".ToUpper();
            #endregion

            #region NameMapping Head & Neck cancer ---------------------------------------------------------------

            if (Regex.Match(returnvalue, @"^Pituitary[ _Gland]{0,6}$", RegexOptions.IgnoreCase).Success)
                return "Pituitary".ToUpper();

            if (Regex.Match(returnvalue, @"^(Total)?[ _,]*Lung[s]?[ ,_]*(Total)?$", RegexOptions.IgnoreCase).Success
                //&& !Regex.Match(returnvalue, @"[, _Left]{2,6}", RegexOptions.IgnoreCase).Success && !Regex.Match(returnvalue, @"[, _Right]{2,7}", RegexOptions.IgnoreCase).Success
                )
                return "Lungs".ToUpper();

            if ((new string[] { "Thyroid", "Thyroid Gland" }).Any(t => t.Equals(returnvalue, StringComparison.CurrentCultureIgnoreCase)))
                return "Glnd_Thyroid".Title_Case();

            if ((new string[] { "Pharynx", "OAR_Pharynx", "OAR pharynx", "OARPharynx" }).Any(t => t.Equals(returnvalue, StringComparison.CurrentCultureIgnoreCase))) //  "Pharynx~^OAR" is not included, since Oral_Cavity~^OAR is clearly not Oral_Cavity in UAB dataset. Since ~ Means Partial, "Pharynx~OAR" is excluded also.
                return "Pharynx".Title_Case();

            if ((new string[] { "Ear_Mid_L", "L middle ear", "EAR_MID_L1" }).Any(t => t.Equals(returnvalue, StringComparison.CurrentCultureIgnoreCase)) ||
                Regex.Match(returnvalue, @"^Mid(dle)?[_ ,]+EAR[ _,]+L[eft]{0,3}$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"^L[eft]{0,3}[_, ]+Mid(dle)?[_ ,]+EAR$", RegexOptions.IgnoreCase).Success
                )
                return "Ear_Middle_L".Title_Case();

            if ((new string[] { "Ear_Mid_R", "R middle ear", "EAR_MID_R1" }).Any(t => t.Equals(returnvalue, StringComparison.CurrentCultureIgnoreCase)) ||
                Regex.Match(returnvalue, @"^Mid(dle)?[_ ,]+EAR[ _,]+R[ight]{0,4}$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"^R[ight]{0,4}[_, ]+Mid(dle)?[_ ,]+EAR$", RegexOptions.IgnoreCase).Success
                )
                return "Ear_Middle_R".Title_Case();

            if ((new string[] { "I_EarLt", "I_Ear_L", "In_EarLt", "Inner Ear, Left", "Inner_Ear_L" }).Any(t => t.Equals(returnvalue, StringComparison.CurrentCultureIgnoreCase)) ||
                Regex.Match(returnvalue, @"^(Inner|In|I)[_ ,]+EAR[ _,]+L[eft]{0,3}$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"^L[eft]{0,3}[_, ]+(Inner|In|I)[_ ,]+EAR$", RegexOptions.IgnoreCase).Success
                )
                return "Ear_Internal_L".Title_Case();

            if ((new string[] { "I_EarRt", "I_Ear_R", "In_EarRt", "Inner Ear, Right", "Inner_Ear_R" }).Any(t => t.Equals(returnvalue, StringComparison.CurrentCultureIgnoreCase)) ||
                Regex.Match(returnvalue, @"^(Inner|In|I)[_ ,]+EAR[ _,]+R[ight]{0,4}$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"^R[ight]{0,4}[_, ]+(Inner|In|I)[_ ,]+EAR$", RegexOptions.IgnoreCase).Success
                )
                return "Ear_Internal_R".Title_Case();

            if (Regex.Match(returnvalue, @"^Eye[_ ,]*L[eft]{0,3}$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"^L[eft]{0,3}[_, ]+eye$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"^Globe[_ ,]*L[eft]{0,3}$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"^L[eft]{0,3}[_, ]+Globe$", RegexOptions.IgnoreCase).Success)
                return "Eye_L".ToUpper();

            if (Regex.Match(returnvalue, @"^Eye[_ ,]*R[ight]{0,4}$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"^R[ight]{0,4}[_, ]+eye$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"^Globe[_ ,]*R[ight]{0,4}$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"^R[ight]{0,4}[_, ]+Globe$", RegexOptions.IgnoreCase).Success)
                return "Eye_R".ToUpper();


            if (Regex.Match(returnvalue, @"^((Glnd)?Subm[andibular]*[_ ,]*[sali]*[v]?[ary gland]*|SMG)[ _,]*L[eft]{0,3}$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"^L[eft]{0,3}[_, ]*(subm[andibular]*[_ ,]*[salivary gland]{0,8}|SMG)$", RegexOptions.IgnoreCase).Success)
                return "Glnd_Submand_L".ToUpper();

            if (Regex.Match(returnvalue, @"^((Glnd)?Subm[andibular]*[_ ,]*[sali]*[v]?[ary gland]*|SMG)[ _,]*R[ight]{0,4}$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"^R[ight]{0,4}[_, ]*(subm[andibular]*[_ ,]*[salivary gland]{0,8}|SMG)$", RegexOptions.IgnoreCase).Success)
                return "Glnd_Submand_R".ToUpper();

            if (Regex.Match(returnvalue, @"^L[eft]{0,3}[_ ,\-]{0,2}Par[otid]{0,4}[ _Gland]{0,6}$", RegexOptions.IgnoreCase).Success ||
                Regex.Match(returnvalue, @"^Parotid[_ ,\-]{0,2}L[eft]{0,3}$", RegexOptions.IgnoreCase).Success)
                return "Parotid_L".ToUpper();

            if (Regex.Match(returnvalue, @"^R[ight]{0,4}[_ ,\-]{0,2}Par[otid]{0,4}[ _Gland]{0,6}$", RegexOptions.IgnoreCase).Success ||
                Regex.Match(returnvalue, @"^Parotid[_ ,\-]{0,2}R[ight]{0,4}$", RegexOptions.IgnoreCase).Success)
                return "Parotid_R".ToUpper();

            if (Regex.Match(returnvalue, @"^Parotids$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"^(Total)?[,_ ]{0,2}Parotid[s]?[,_ ]{0,2}(Total)?$", RegexOptions.IgnoreCase).Success)
                return "Parotids".ToUpper();

            if (Regex.Match(returnvalue, @"^BRAIN[ _]?STEM$", RegexOptions.IgnoreCase).Success)
                return "brainstem".ToUpper();

            // Brainstem_PRV03
            if (Regex.Match(returnvalue, @"^(Expanded|PRV)?[ _]?(BRAINSTEM|BS)[_PRV+ 0 expanded]*3[ _cmm]*(exp|expd|expanded)?$", RegexOptions.IgnoreCase).Success)
                return "brainstem_prv03".ToUpper();

            if (Regex.Match(returnvalue, @"^(Expanded|PRV)?[ _]?(BRAINSTEM|BS)[_PRV+ 0 expanded]*5[ _cmm]*(exp|expd|expanded)?$", RegexOptions.IgnoreCase).Success)
                return "brainstem_prv05".ToUpper();

            // OpticChiasm_PRV3
            if (Regex.Match(returnvalue, @"^[Optic_ ]{0,6}CHIASM", RegexOptions.IgnoreCase).Success && (Regex.Match(returnvalue, @"PRV", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"\+", RegexOptions.IgnoreCase).Success) && Regex.Match(returnvalue, @"3", RegexOptions.IgnoreCase).Success)
                return "opticchiasm_prv3".ToUpper();

            // OpticChiasm
            if (Regex.Match(returnvalue, @"^[Optic_ ]{0,6}CHIASM$", RegexOptions.IgnoreCase).Success)
                return "opticchiasm".ToUpper();

            // Cochlea_L
            if (Regex.Match(returnvalue, @"^COCH[lea]{0,3}[_, ]*L[eft]{0,3}$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"^L[eft]{0,3}[_, ]*COCH[lea]{0,3}$", RegexOptions.IgnoreCase).Success)
                return "cochlea_l".ToUpper();

            // Cochlea_R
            if (Regex.Match(returnvalue, @"^COCH[lea]{0,3}[_, ]*R[ight]{0,4}$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"^R[ight]{0,4}[_, ]*COCH[lea]{0,3}$", RegexOptions.IgnoreCase).Success)
                return "cochlea_r".ToUpper();

            // Musc_Constrict_I
            if (Regex.Match(returnvalue, @"^[musc_ pharynx]*const[rictors_ ,]*I[nferior]{0,7}$", RegexOptions.IgnoreCase).Success ||
                Regex.Match(returnvalue, @"^I[nferior_, ]{0,9}[musc_ pharynx]*const[rictors]*$", RegexOptions.IgnoreCase).Success ||
                returnvalue == "IPC")
                return "musc_constrict_i".ToUpper();

            // Musc_Constrict_S
            if ((Regex.Match(returnvalue, @"^[musc_ pharynx]*const[rictors_ ,]*S[uperior]{0,7}$", RegexOptions.IgnoreCase).Success && !Regex.Match(returnvalue, @"^[musc_ pharynx]*const[rictors]*$", RegexOptions.IgnoreCase).Success) ||
                Regex.Match(returnvalue, @"^S[uperior_, ]{0,9}[musc_ pharynx]*const[rictors]*$", RegexOptions.IgnoreCase).Success ||
                returnvalue == "SPC")
                return "musc_constrict_s".ToUpper();

            // Musc_Constrict_M
            if (Regex.Match(returnvalue, @"^[musc_ pharynx]*const[rictors_ ,]*M[iddle]{0,5}$", RegexOptions.IgnoreCase).Success ||
                (Regex.Match(returnvalue, @"^M[iddle_, ]{0,7}[musc_ pharynx]*const[rictors]*$", RegexOptions.IgnoreCase).Success && !Regex.Match(returnvalue, @"^[musc_ pharynx]*const[rictors]*$", RegexOptions.IgnoreCase).Success) ||
                returnvalue == "MPC")
                return "musc_constrict_M".ToUpper();

            if (Regex.Match(returnvalue, @"^[musc_ pharynx]*const[rictors]*$", RegexOptions.IgnoreCase).Success)
                return "Musc_Constrict".ToUpper();

            if (Regex.Match(returnvalue, @"^[musc_ pharynx]*const[rictors ]*-[ ]?PTV$", RegexOptions.IgnoreCase).Success)
                return "Musc_Constrict-PTV".ToUpper();

            if (Regex.Match(returnvalue, @"^[Gland_ ]{0,6}Lac[rimal _gland]{0,12}[_ ,]*L[eft]{0,3}$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"^L[eft]{0,3}[_, ]+lac[rimal _gland]{0,12}$", RegexOptions.IgnoreCase).Success)
                return "Glnd_Lacrimal_L".ToUpper();

            if (Regex.Match(returnvalue, @"^[Gland_ ]{0,6}Lac[rimal _gland]{0,12}[_ ,]*R[ight]{0,4}$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"^R[ight]{0,4}[_, ]+lac[rimal _gland]{0,12}$", RegexOptions.IgnoreCase).Success)
                return "Glnd_Lacrimal_R".ToUpper();

            if (Regex.Match(returnvalue, @"^Lens[_ ,]*L[eft]{0,3}$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"^L[eft]{0,3}[_, ]+Lens$", RegexOptions.IgnoreCase).Success)
                return "Lens_L".ToUpper();

            if (Regex.Match(returnvalue, @"^Lens[_ ,]*R[ight]{0,4}$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"^R[ight]{0,4}[_, ]+Lens$", RegexOptions.IgnoreCase).Success)
                return "Lens_R".ToUpper();

            // Bone_Mandible
            if (Regex.Match(returnvalue, @"^(Bone)?[,_ ]{0,2}Mandible[Bone_ ]{0,6}$", RegexOptions.IgnoreCase).Success)
                return "bone_mandible".ToUpper();

            if (Regex.Match(returnvalue, @"^(Bone)?[,_ ]{0,2}Mandible[Bone_ ]{0,6}[ ]?-[ ]?PTV$", RegexOptions.IgnoreCase).Success)
                return "Bone_Mandible-PTV".Title_Case();

            // OpticNrv_PRV03_L
            if ((Regex.Match(returnvalue, @"^L[eft]{0,3}[ _,]{0,2}OPT[IC _,]*(N|Nrv|Nerve)$", RegexOptions.IgnoreCase).Success ||
                Regex.Match(returnvalue, @"^OPT[IC _,]*(N|Nrv|Nerve)[ _,]{0,2}L[eft]{0,3}$", RegexOptions.IgnoreCase).Success)
                && Regex.Match(returnvalue, @"(\+|PRV)", RegexOptions.IgnoreCase).Success
                && Regex.Match(returnvalue, @"3", RegexOptions.IgnoreCase).Success)
                return "opticnrv_prv03_l".ToUpper();

            // OpticNrv_PRV03_R
            if ((Regex.Match(returnvalue, @"^R[ight]{0,4}[ _,]{0,2}OPT[IC _,]*(N|Nrv|Nerve)$", RegexOptions.IgnoreCase).Success ||
                Regex.Match(returnvalue, @"^OPT[IC _,]*(N|Nrv|Nerve)[ _,]{0,2}R[ight]{0,4}$", RegexOptions.IgnoreCase).Success)
                && Regex.Match(returnvalue, @"(\+|PRV)", RegexOptions.IgnoreCase).Success
                && Regex.Match(returnvalue, @"3", RegexOptions.IgnoreCase).Success)
                return "opticnrv_prv03_r".ToUpper();

            // OpticNrv_L
            if (Regex.Match(returnvalue, @"^L[eft]{0,3}[ _,]{0,2}OPT[IC _,]*(N|Nrv|Nerve)$", RegexOptions.IgnoreCase).Success ||
                Regex.Match(returnvalue, @"^OPT[IC _,]*(N|Nrv|Nerve)[ _,]{0,2}L[eft]{0,3}$", RegexOptions.IgnoreCase).Success)
                return "opticnrv_l".ToUpper();

            // OpticNrv_R
            if (Regex.Match(returnvalue, @"^R[ight]{0,4}[ _,]{0,2}OPT[IC _,]*(N|Nrv|Nerve)$", RegexOptions.IgnoreCase).Success ||
                Regex.Match(returnvalue, @"^OPT[IC _,]*(N|Nrv|Nerve)[ _,]{0,2}R[ight]{0,4}$", RegexOptions.IgnoreCase).Success)
                return "opticnrv_r".ToUpper();

            // Cavity_Oral
            if (Regex.Match(returnvalue, @"^(Cavity|cav)?[,_ ]{0,2}Oral[,_ ]{0,2}(Cavity|cav)?$", RegexOptions.IgnoreCase).Success)
                return "cavity_oral".ToUpper();

            if (Regex.Match(returnvalue, @"^(Cavity|cav)?[,_ ]{0,2}Oral[,_ ]{0,2}(Cavity|cav)?[ ]?-[ ]?PTV$", RegexOptions.IgnoreCase).Success)
                return "Cavity_Oral-PTV".Title_Case();

            // Lobe_Temporal_L
            if (Regex.Match(returnvalue, @"^[Temporal _]{0,10}lobe[Temporal _]{0,10}[, _]?L[eft]{0,3}$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"^L[eft]{0,3}[, _]{0,2}[Temporal _]{0,10}lobe[Temporal _]{0,10}$", RegexOptions.IgnoreCase).Success)
                return "lobe_temporal_l".ToUpper();

            // Lobe_Temporal_R
            if (Regex.Match(returnvalue, @"^[Temporal _]{0,10}lobe[Temporal _]{0,10}[, _]?R[ight]{0,4}$", RegexOptions.IgnoreCase).Success || Regex.Match(returnvalue, @"^R[ight]{0,4}[, _]{0,2}[Temporal _]{0,10}lobe[Temporal _]{0,10}$", RegexOptions.IgnoreCase).Success)
                return "lobe_temporal_R".ToUpper();

            if (Regex.Match(returnvalue, @"^Eso[pha]{0,3}[gus]{0,3}$", RegexOptions.IgnoreCase).Success)
                return "Esophagus".Title_Case();

            if (Regex.Match(returnvalue, @"^Eso[pha]{0,3}[gus]{0,3}[ ]?-[ ]?PTV$", RegexOptions.IgnoreCase).Success)
                return "Esophagus-PTV".Title_Case();

            if (Regex.Match(returnvalue, @"^(OAR)?[ _]?Larynx[ _]?(OAR)?$", RegexOptions.IgnoreCase).Success)
                return "Larynx".Title_Case();

            if (Regex.Match(returnvalue, @"^Larynx[ ]?-[ ]?PTV$", RegexOptions.IgnoreCase).Success)
                return "Larynx-PTV".Title_Case();



            // Parotid Un-Involved
            if (Regex.Match(returnvalue, @"^[UN \-INVOLVED]{0,11}[ _,]{0,2}PAROTID[ _,]{0,2}[UN \-INVOLVED]{0,11}$", RegexOptions.IgnoreCase).Success)
                return "Parotid Un-Involved".ToUpper();

            // Parotid Involved
            if ((!Regex.Match(returnvalue, @".*UN.*INVOLVED.*", RegexOptions.IgnoreCase).Success) && Regex.Match(returnvalue, @"^[INVOLVED]{0,8}[ _,]{0,2}PAROTID[ _,]{0,2}[INVOLVED]{0,8}$", RegexOptions.IgnoreCase).Success)
                return "Parotid Involved".ToUpper();

            // Glnd_Submand Un-Involved
            if (Regex.Match(returnvalue, @"^[UN \-INVOLVED]{0,11}[ _,]{0,2}GLND_SUBMAND[ _,]{0,2}[UN \-INVOLVED]{0,11}$", RegexOptions.IgnoreCase).Success)
                return "Glnd_Submand Un-Involved".ToUpper();

            // Glnd_Submand Involved.
            if ((!Regex.Match(returnvalue, @".*UN.*INVOLVED.*", RegexOptions.IgnoreCase).Success) && Regex.Match(returnvalue, @"^[INVOLVED]{0,8}[ _,]{0,2}GLND_SUBMAND[ _,]{0,2}[INVOLVED]{0,8}$", RegexOptions.IgnoreCase).Success)
                return "Glnd_Submand Involved".ToUpper();
            #endregion

            return ""; // Only Emit TG263 standard name.
        }
    }
}
