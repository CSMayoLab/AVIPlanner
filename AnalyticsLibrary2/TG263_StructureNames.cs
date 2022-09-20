﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public static class TG263
{
    public static List<string> TG263_StructureNames = new List<string> {
"A_Aorta",
"A_Aorta_Asc",
"A_Brachiocephls",
"A_Carotid",
"A_Carotid_L",
"A_Carotid_R",
"A_Celiac",
"A_Coronary",
"A_Coronary_L",
"A_Coronary_R",
"A_Femoral_Cflx_L",
"A_Femoral_Cflx_R",
"A_Femoral_L",
"A_Femoral_R",
"A_Humeral_Cflx_L",
"A_Humeral_Cflx_R",
"A_Humeral_L",
"A_Humeral_R",
"A_Hypophyseal_I",
"A_Hypophyseal_S",
"A_Iliac_Cflx_L",
"A_Iliac_Cflx_R",
"A_Iliac_Ext_L",
"A_Iliac_Ext_R",
"A_Iliac_Int_L",
"A_Iliac_Int_R",
"A_Iliac_L",
"A_Iliac_R",
"A_LAD",
"A_Mesenteric_I",
"A_Mesenteric_S",
"A_Pulmonary",
"A_Subclavian",
"A_Subclavian_L",
"A_Subclavian_R",
"A_Vertebral",
"A_Vertebral_L",
"A_Vertebral_R",
"Acetabulum_L",
"Acetabulum_R",
"Acetabulums",
"AirWay_Dist",
"AirWay_Prox",
"Anus",
"Appendix",
"Arytenoid",
"Arytenoid_L",
"Arytenoid_R",
"Atrium",
"Atrium_L",
"Atrium_R",
"Bag_Bowel",
"Bag_Ostomy",
"BileDuct_Common",
"Bladder",
"Bladder_Wall",
"Bladder-CTV",
"Body",
"Body-PTV",
"Bolus",
"Bolus_xxmm",
"Bone",
"Bone_Ethmoid",
"Bone_Frontal",
"Bone_Hyoid",
"Bone_Ilium",
"Bone_Ilium_L",
"Bone_Ilium_R",
"Bone_Incus",
"Bone_Incus_L",
"Bone_Incus_R",
"Bone_Ischium_L",
"Bone_Ischium_R",
"Bone_Lacrimal",
"Bone_Lacrimal_L",
"Bone_Lacrimal_R",
"Bone_Mandible",
"Bone_Mastoid",
"Bone_Mastoid_L",
"Bone_Mastoid_R",
"Bone_Nasal",
"Bone_Nasal_L",
"Bone_Nasal_R",
"Bone_Occipital",
"Bone_Palatine",
"Bone_Palatine_L",
"Bone_Palatine_R",
"Bone_Parietal",
"Bone_Parietal_L",
"Bone_Parietal_R",
"Bone_Pelvic",
"Bone_Pelvic_L",
"Bone_Pelvic_R",
"Bone_Sphenoid",
"Bone_Temporal",
"Bone_Temporal_L",
"Bone_Temporal_R",
"Bone_Zygomatic_L",
"Bone_Zygomatic_R",
"Bone_Zygomatics",
"BoneMarrow",
"BoneMarrow_Act",
"Boost",
"Bowel",
"Bowel_Large",
"Bowel_Small",
"BrachialPlex_L",
"BrachialPlex_R",
"BrachialPlexs",
"Brain",
"Brain-CTV",
"Brain-GTV",
"Brain-PTV",
"Brainstem",
"Brainstem_Core",
"Brainstem_PRV",
"Brainstem_PRVxx",
"Brainstem_Surf",
"Breast_L",
"Breast_R",
"Breasts",
"Bronchus",
"Bronchus_L",
"Bronchus_Main",
"Bronchus_Main_L",
"Bronchus_Main_R",
"Bronchus_PRVxx",
"Bronchus_R",
"Canal_Anal",
"Carina",
"Cartlg_Thyroid",
"CaudaEquina",
"Cavernosum",
"Cavity_Nasal",
"Cavity_Oral",
"Cecum",
"Cerebellum",
"Cerebrum",
"Cerebrum_L",
"Cerebrum_R",
"Cervix",
"Chestwall",
"Chestwall_L",
"Chestwall_R",
"Cist_Pontine",
"Cist_Suprasellar",
"Clavicle_L",
"Clavicle_R",
"CN_III",
"CN_III_L",
"CN_III_R",
"CN_IX",
"CN_IX_L",
"CN_IX_R",
"CN_V",
"CN_V_L",
"CN_V_R",
"CN_VI",
"CN_VI_L",
"CN_VI_R",
"CN_VII",
"CN_VII_L",
"CN_VII_R",
"CN_VIII",
"CN_VIII_L",
"CN_VIII_R",
"CN_XI",
"CN_XI_L",
"CN_XI_R",
"CN_XII",
"CN_XII_L",
"CN_XII_R",
"Cochlea",
"Cochlea_L",
"Cochlea_R",
"Colon",
"Colon_Ascending",
"Colon_Decending",
"Colon_PTVxx",
"Colon_Sigmoid",
"Colon_Transverse",
"Cornea",
"Cornea_L",
"Cornea_R",
"CribriformPlate",
"Cricoid",
"Cricopharyngeus",
"CTV",
"Dens",
"Diaphragm",
"Duodenum",
"Ear_External_L",
"Ear_External_R",
"Ear_Externals",
"Ear_Internal_L",
"Ear_Internal_R",
"Ear_Internals",
"Ear_Middle",
"Ear_Middle_L",
"Ear_Middle_R",
"Edema",
"Elbow",
"Elbow_L",
"Elbow_R",
"E-PTV_Ev05_xxxx",
"E-PTV_xxxx",
"Esophagus",
"Esophagus_I",
"Esophagus_M",
"Esophagus_NAdj",
"Esophagus_S",
"Eval",
"External",
"Eye_L",
"Eye_R",
"Eyes",
"Femur_Base_L",
"Femur_Base_R",
"Femur_Head_L",
"Femur_Head_R",
"Femur_Joint_L",
"Femur_Joint_R",
"Femur_L",
"Femur_Neck_L",
"Femur_Neck_R",
"Femur_R",
"Femur_Shaft_L",
"Femur_Shaft_R",
"Femurs",
"Fibula",
"Fibula_L",
"Fibula_R",
"Foley",
"Fossa_Jugular",
"Fossa_Posterior",
"Gallbladder",
"Genitals",
"Glnd_Adrenal_L",
"Glnd_Adrenal_R",
"Glnd_Lacrimal",
"Glnd_Lacrimal_L",
"Glnd_Lacrimal_R",
"Glnd_Parathyroid",
"Glnd_Subling_L",
"Glnd_Subling_R",
"Glnd_Sublings",
"Glnd_Submand_L",
"Glnd_Submand_R",
"Glnd_Submands",
"Glnd_Thymus",
"Glnd_Thyroid",
"Glottis",
"GreatVes",
"GreatVes_NAdj",
"GrowthPlate_L",
"GrowthPlate_R",
"GTV",
"Hardpalate",
"Heart",
"Hemisphere_L",
"Hemisphere_R",
"Hemispheres",
"Hippocampi",
"Hippocampus_L",
"Hippocampus_R",
"Humerus_L",
"Humerus_R",
"Hypothalmus",
"Hypothalmus_PRV",
"Hypothalmus_PRVx",
"IDL",
"Ileum",
"ITV",
"Jejunum",
"Jejunum_Ileum",
"Joint_Elbow",
"Joint_Elbow_L",
"Joint_Elbow_R",
"Joint_Glenohum",
"Joint_Glenohum_L",
"Joint_Glenohum_R",
"Joint_Surface",
"Joint_TM",
"Joint_TM_L",
"Joint_TM_R",
"Kidney_Cortex",
"Kidney_Cortex_L",
"Kidney_Cortex_R",
"Kidney_Hilum_L",
"Kidney_Hilum_R",
"Kidney_Hilums",
"Kidney_L",
"Kidney_L-GTV",
"Kidney_Pelvis_L",
"Kidney_Pelvis_R",
"Kidney_R",
"Kidney_R-GTV",
"Kidney-GTV",
"Kidneys",
"Knee",
"Knee_L",
"Knee_R",
"Laryngl_Pharynx",
"Larynx",
"Larynx_SG",
"Leads",
"Lens",
"Lens_L",
"Lens_R",
"Lig_Hepatogastrc",
"Lips",
"Liver",
"Liver-CTV",
"Liver-GTV",
"LN",
"LN_Ax_Apical",
"LN_Ax_Apical_L",
"LN_Ax_Apical_R",
"LN_Ax_Central_L",
"LN_Ax_Central_R",
"LN_Ax_Centrals",
"LN_Ax_L",
"LN_Ax_L1_L",
"LN_Ax_L1_R",
"LN_Ax_L2_L",
"LN_Ax_L2_R",
"LN_Ax_L3_L",
"LN_Ax_L3_R",
"LN_Ax_Lateral_L",
"LN_Ax_Lateral_R",
"LN_Ax_Laterals",
"LN_Ax_Pectoral_L",
"LN_Ax_Pectoral_R",
"LN_Ax_Pectorals",
"LN_Ax_R",
"LN_Ax_Subscap_L",
"LN_Ax_Subscap_R",
"LN_Ax_Subscaps",
"LN_Brachioceph_L",
"LN_Brachioceph_R",
"LN_Brachiocephs",
"LN_Bronchpulm_L",
"LN_Bronchpulm_R",
"LN_Bronchpulms",
"LN_Diaphragmatic",
"LN_Iliac_Ext_L",
"LN_Iliac_Ext_R",
"LN_Iliac_Int_L",
"LN_Iliac_L",
"LN_Iliac_R",
"LN_IMN_L",
"LN_IMN_R",
"LN_IMNs",
"LN_Inguinofem",
"LN_Inguinofem_L",
"LN_Inguinofem_R",
"LN_Intercostals",
"LN_L",
"LN_Ligamentarter",
"LN_lliac_Int_R",
"LN_Mediastinals",
"LN_Neck_IA_L",
"LN_Neck_IA_R",
"LN_Neck_IB_L",
"LN_Neck_IB_R",
"LN_Neck_II_L",
"LN_Neck_II_R",
"LN_Neck_IIA_L",
"LN_Neck_IIA_R",
"LN_Neck_IIB_L",
"LN_Neck_IIB_R",
"LN_Neck_III_L",
"LN_Neck_III_R",
"LN_Neck_IV_L",
"LN_Neck_IV_R",
"LN_Neck_V_L",
"LN_Neck_V_R",
"LN_Neck_VA_L",
"LN_Neck_VA_R",
"LN_Neck_VB_L",
"LN_Neck_VB_R",
"LN_Neck_VC_L",
"LN_Neck_VC_R",
"LN_Neck_VI_L",
"LN_Neck_VI_R",
"LN_Neck_VII_L",
"LN_Neck_VII_R",
"LN_Obturator_L",
"LN_Obturator_R",
"LN_Paraaortic",
"LN_Paramammary_L",
"LN_Paramammary_R",
"LN_Paramammarys",
"LN_Parasternal_L",
"LN_Parasternal_R",
"LN_Parasternals",
"LN_Pelvic_L",
"LN_Pelvic_R",
"LN_Pelvics",
"LN_Portahepatis",
"LN_Presacral_L",
"LN_Presacral_R",
"LN_Pulmonary_L",
"LN_Pulmonary_R",
"LN_Pulmonarys",
"LN_R",
"LN_Sclav_L",
"LN_Sclav_R",
"LN_Supmammary_L",
"LN_Supmammary_R",
"LN_Supmammarys",
"LN_Trachbrnchs",
"LN_Trachbrnchs_L",
"LN_Trachbrnchs_R",
"Lobe_Frontal",
"Lobe_Frontal_L",
"Lobe_Frontal_R",
"Lobe_Occipital",
"Lobe_Occipital_L",
"Lobe_Occipital_R",
"Lobe_Parietal",
"Lobe_Parietal_L",
"Lobe_Parietal_R",
"Lobe_Temporal",
"Lobe_Temporal_L",
"Lobe_Temporal_R",
"Lung_L",
"Lung_LLL",
"Lung_LUL",
"Lung_R",
"Lung_RLL",
"Lung_RML",
"Lung_RUL",
"Lungs",
"Lungs-CTV",
"Lungs-GTV",
"Lungs-ITV",
"Lungs-PTV",
"Malleus",
"Malleus_L",
"Malleus_R",
"Markers",
"Maxilla",
"Maxilla_L",
"Maxilla_R",
"Mediastinum",
"Musc",
"Musc_Constrict",
"Musc_Constrict_I",
"Musc_Constrict_M",
"Musc_Constrict_S",
"Musc_Digastric_L",
"Musc_Digastric_R",
"Musc_Masseter",
"Musc_Masseter_L",
"Musc_Masseter_R",
"Musc_Platysma_L",
"Musc_Platysma_R",
"Musc_Pterygoid_L",
"Musc_Pterygoid_R",
"Musc_Sclmast_L",
"Musc_Sclmast_R",
"Musc_Temporal_L",
"Musc_Temporal_R",
"Nasalconcha_LI",
"Nasalconcha_RI",
"Nasopharynx",
"Nose",
"Nrv_Peripheral",
"Nrv_Root",
"OpticChiasm",
"OpticChiasm_PRV",
"OpticChiasm_PRVx",
"OpticNrv",
"OpticNrv_L",
"OpticNrv_PRV",
"OpticNrv_PRV_L",
"OpticNrv_PRV_R",
"OpticNrv_PRVxx_L",
"OpticNrv_PRVxx_R",
"OpticNrv_R",
"Orbit_L",
"Orbit_R",
"Oropharynx",
"Ovaries",
"Ovary_L",
"Ovary_R",
"Pacemaker",
"Palate_Soft",
"PancJejuno",
"Pancreas",
"Pancreas_Head",
"Pancreas_Tail",
"Parametrium",
"Parotid_L",
"Parotid_R",
"Parotids",
"PenileBulb",
"Penis",
"Pericardium",
"Perineum",
"Peritoneum",
"Pharynx",
"Pineal",
"Pituitary",
"Pituitary_PRVxx",
"Pons",
"Postop",
"Preop",
"Proc_Condyloid_L",
"Proc_Condyloid_R",
"Proc_Coronoid_L",
"Proc_Coronoid_R",
"Prostate",
"ProstateBed",
"Prosthesis",
"Pterygoid_Lat_L",
"Pterygoid_Lat_R",
"Pterygoid_Med_L",
"Pterygoid_Med_R",
"PTV",
"PubicSymphys",
"PubicSymphys_L",
"PubicSymphys_R",
"Radius_L",
"Radius_R",
"Rectal_Wall",
"Rectum",
"Retina_L",
"Retina_PRVxx_L",
"Retina_PRVxx_R",
"Retina_R",
"Retinas",
"Rib",
"Rib01_L",
"Rib01_R",
"Rib02_L",
"Rib02_R",
"Rib03_L",
"Rib03_R",
"Rib04_L",
"Rib04_R",
"Rib05_L",
"Rib05_R",
"Rib06_L",
"Rib06_R",
"Rib07_L",
"Rib07_R",
"Rib08_L",
"Rib08_R",
"Rib09_L",
"Rib09_R",
"Rib10_L",
"Rib10_R",
"Rib11_L",
"Rib11_R",
"Rib12_L",
"Rib12_R",
"SacralPlex",
"Sacrum",
"Scalp",
"Scapula_L",
"Scapula_R",
"Scar",
"Scar_Boost",
"Scrotum",
"SeminalVes",
"SeminalVes_Dist",
"SeminalVes_Prox",
"Sinus_Ethmoid",
"Sinus_Frontal",
"Sinus_Frontal_L",
"Sinus_Frontal_R",
"Sinus_Maxilry",
"Sinus_Maxilry_L",
"Sinus_Maxilry_R",
"Sinus_Sphenoid",
"Sinus_Sphenoid_L",
"Sinus_Sphenoid_R",
"Skin",
"Skin_Perineum",
"Skin_Peritoneum",
"Skull",
"Spc",
"Spc_Bowel",
"Spc_Bowel_Small",
"Spc_Retrophar_L",
"Spc_Retrophar_R",
"Spc_Retrophars",
"Spc_Retrosty",
"Spc_Retrosty_L",
"Spc_Retrosty_R",
"Spc_Supraclav_L",
"Spc_Supraclav_R",
"Sphincter_Anal",
"SpinalCanal",
"SpinalCanal_PRV",
"SpinalCanal_PRVx",
"SpinalCord",
"SpinalCord_Cerv",
"SpinalCord_Lum",
"SpinalCord_PRV",
"SpinalCord_PRVxx",
"SpinalCord_Sac",
"SpinalCord_Thor",
"Spleen",
"Spleen_Hilum",
"Spongiosum",
"Stapes",
"Stapes_L",
"Stapes_R",
"Stomach",
"Stomach_PRVxx",
"Strct",
"Strct_Suprapatel",
"Surf_Eye",
"SurgicalBed",
"Sys_Ventricular",
"Tendon",
"Tendon_Quad",
"Testis",
"Testis_L",
"Testis_R",
"ThecalSac",
"Thoracic_Duct",
"Tongue",
"Tongue_All",
"Tongue_Base",
"Tongue_Base_L",
"Tongue_Base_R",
"Tongue_Oral",
"Tongue_Oral_L",
"Tongue_Oral_R",
"Tonsil",
"Trachea",
"Trachea_NAdj",
"TumorBed",
"Ureter_L",
"Ureter_R",
"UreterDivert",
"Ureters",
"Urethra",
"Urethra_Prostatc",
"Uterus",
"V_Azygos",
"V_Brachioceph_L",
"V_Brachioceph_R",
"V_Iliac_Ext_L",
"V_Iliac_Ext_R",
"V_Iliac_Int_L",
"V_Iliac_Int_R",
"V_Iliac_L",
"V_Iliac_R",
"V_Jugular",
"V_Jugular_Ext_L",
"V_Jugular_Ext_R",
"V_Jugular_Int_L",
"V_Jugular_Int_R",
"V_Portal",
"V_Pulmonary",
"V_Subclavian_L",
"V_Subclavian_R",
"V_Subclavians",
"V_Venacava_I",
"V_Venacava_S",
"Vagina",
"Vagina_Surf",
"VaginalCuff",
"Valve",
"Valve_Aortic",
"Valve_Mitral",
"Valve_Pulmonic",
"Valve_Tricuspid",
"VB",
"VB_C",
"VB_C1",
"VB_C2",
"VB_C3",
"VB_C4",
"VB_C5",
"VB_C6",
"VB_C7",
"VB_L",
"VB_L1",
"VB_L1",
"VB_L2",
"VB_L3",
"VB_L4",
"VB_L5",
"VB_S",
"VB_S1",
"VB_S2",
"VB_S3",
"VB_S4",
"VB_S5",
"VB_T",
"VB_T01",
"VB_T02",
"VB_T03",
"VB_T04",
"VB_T05",
"VB_T06",
"VB_T07",
"VB_T08",
"VB_T09",
"VB_T10",
"VB_T11",
"VB_T12",
"VBs",
"Ventricle",
"Ventricle_L",
"Ventricle_R",
"VocalCord_L",
"VocalCord_R",
"VocalCords",
"Vomer",
"Vulva",
"Wall_Vagina" };

    public static string get_Standard_StructureName(string strName)
    {
        var rv = TG263_StructureNames.SingleOrDefault(t => t.ToUpper() == strName.ToUpper());

        if (rv != null)
        {
            return rv;
        }
        else
        {
            var match = Regex.Match(strName, @"\d{1,2}$", RegexOptions.IgnoreCase);

            string strName_shorten = strName.Substring(0, strName.Length - match.Length);

            if(match.Success && TG263_StructureNames.Any(t => t.ToUpper().StartsWith(strName_shorten.ToUpper())))
            {
                return strName;
            }
            else
            {
                var match2 = Regex.Match(strName, @"\d{1,2}_[LR]$", RegexOptions.IgnoreCase);

                string strName_shorten2 = strName.Substring(0, strName.Length - match2.Length);

                if (match2.Success && TG263_StructureNames.Any(t => t.ToUpper().StartsWith(strName_shorten2.ToUpper())))
                {
                    return strName;
                }

                throw new System.Exception($"Structure name provided [{strName}] is not TG263 standard. Please rename it following TG263 guideline.");
            }
        }
       
    }

}
