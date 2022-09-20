using AP_lib;
using AutoPlan_GUI;
using AutoPlan_HN;
using ESAPI_Extensions;
using Lib3_ESAPI;
using Newtonsoft.Json;
using AnalyticsLibrary2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Media3D;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace AutoPlan_WES_HN
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.Title = "AutoPlan V" + version;

            audit_logger = new logger2(Config.Logfile_dir + Environment.UserName.toFileName() + DateTime.Now.ToString("_yyyy_MM") + ".log", AP_lib.Log_levels.debug);

            Log.logger = audit_logger;

            
            try
            {
                constraints_config = Load_Config_Constraints.load();
                
                using (VMS.TPS.Common.Model.API.Application app = VMS.TPS.Common.Model.API.Application.CreateApplication())
                {
                    Execute(app);

                    app.SaveModifications();
                    app.ClosePatient();
                    app.Dispose();

                    ConsoleExt.WriteLineWithBackground("\n\nProgram finished.\n");
                    Console.WriteLine("After inspecting the above plan summary, type in YES or Y and then press Enter Key to close this program.");
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                audit_logger.WriteLine(e.ToString());
                Console.WriteLine("\n\n\nType in YES or Y and Then Press Enter Key to Close this Console.");
            }

            string temp;
            do
            {
                temp = Console.ReadLine();
            } while (temp.Trim().ToUpper() != "YES" && temp.Trim().ToUpper() != "Y");
        }

        #region  ==========================================  Static fields of Program classs ========================================== 
        // If intermediate dose is calculated in Optimization.
        static bool if_intermediate_dose = true;
        static bool if_4_beams = false;
        static bool if_stop_before_OPT = false;
        static bool if_use_existing_beams = false;
        static bool if_jaw_tracking = false;
        static string MachineID;


        //static bool if_DLA = false; // whether include DLA in optimization
        static bool if_DLA = true;

        static bool if_break_mean_con = true; // whether break mean into 20 50 80 % volume point objectives.


        static List<RxConstraint> constraints_config;
        static List<RxConstraint> RxCons = new List<RxConstraint>();
        static List<RxConstraint> RxCons_used = new List<RxConstraint>();
        static List<RxConstraint> RxCons_from_VM = new List<RxConstraint>();

        static constraint[] directive_cons;

        static logger2 audit_logger;
        //static console_print cp = null;
        public static string datasource = "HN_General";
        public static int JSON_set_fraction = 0; // 0 for CRT StatsDVH, 3 or 5 for SBRT.
        public static List<TargetStructureDose> TSD = new List<TargetStructureDose>();
        //public static ExternalPlanSetup PS = null;

        public static double lastObjFuncValue = 0;
        public static double lastObjFuncValue_iter1 = 0;
        public static int ind = 0;
        public static int ind_str = 0;
        public static List<string> strns_history;
        public static List<string> strns_inplan_byvolume;
        public static List<OAR_PTV_overlap> strn_ptv_overlap_dict = new List<OAR_PTV_overlap>();
        public static StatsDVH_ForOpt foropt;
        public static History_Curves_ForOpt HistoryCurvesInfo;
        public static bool first_str = true;
        public static bool is_there_PTV_mid = false;
        public static bool is_there_PTV_low = false;


        static List<RxConstraint> Mean_Prio3_LowerEnd_Pressed = new List<RxConstraint>();
        private static string pt_cs_pl;
        private static VVector isocenter;

        private static List<JW_iso_angle> JWs;

        private static string msg_header { get { return "AutoPlan_HN " + version; } }
        private static string version = "";
        private static string asbl_location = Assembly.GetExecutingAssembly().Location;
        private static int n_existing_beams;
        static Structure s1;
        static Structure s2;
       
        static readonly Stopwatch sw = new Stopwatch();
        public static MainViewModel vm;
        #endregion

        static List<JW_iso_angle> calc_isocenter_n_JawWidth(StructureSet curstructset, string PTV_High_name, string PTV_Mid_name, string PTV_Low_name)
        {
            Structure ptv_all = curstructset.Get_or_Add_structure("CONTROL", "ztemp_str_AP1");
            s2 = curstructset.Recreate_structure("CONTROL", "ztemp_str_AP2"); // a temporary structure, to test the overlaping between structures. Will be removed later.

            ptv_all.SegmentVolume = Get_HD_SegmentVolume(curstructset.get(PTV_High_name));

            if (!string.IsNullOrEmpty(PTV_Mid_name))
            {
                ptv_all.SegmentVolume = ptv_all.Or(Get_HD_SegmentVolume(curstructset.get(PTV_Mid_name)));
            }
            if (!string.IsNullOrEmpty(PTV_Low_name))
            {
                ptv_all.SegmentVolume = ptv_all.Or(Get_HD_SegmentVolume(curstructset.get(PTV_Low_name)));
            }

            var Pbounds = ptv_all.MeshGeometry.Bounds;

            VVector userOrign = curstructset.Image.UserOrigin;

            Point3D center_ptv = ptv_all.CenterPoint.Round_up_relative_coordinate(userOrign).VVectorToPoint3D();

            //Point3D center_boundbox = new Point3D(Pbounds.X + Pbounds.SizeX * 0.5, Pbounds.Y + Pbounds.SizeY * 0.5, Pbounds.Z + Pbounds.SizeZ * 0.5);
            //double radius = new Point3D(Pbounds.SizeX * 0.5, Pbounds.SizeY * 0.5, Pbounds.SizeZ * 0.5).Length();
            //double dist_to_center = ptv_all.CenterPoint.VVectorToPoint3D().DistanceTo(new Point3D(Pbounds.X, Pbounds.Y, Pbounds.Z));

            List<Point3D> ptv_MeshPoints = ptv_all.MeshGeometry.Positions.ToList();

            if (false) // play ground test.
            {
                // calc jaw width and isocenter location.
                double mlc_angle = 15;
                Point3D t3 = new Point3D(1, 0, 0).RotCoordSys(new Vector3D(0, 0, 1), 30);
                Point3D t4 = new Point3D(1, 0, 0).RotCoordSys(new Vector3D(0, 0, 1), -30);
                Point3D t5 = new Point3D(1, 0, 0).RotCoordSys(new Vector3D(0, 1, 0), 30);
                Point3D t6 = new Point3D(1, 0, 0).RotCoordSys(new Vector3D(0, 1, 0), -30);

                List<Point3D> Coordinates_after_rotation = new List<Point3D>();
                var y_axis = new Vector3D(0, 1, 0);

                ptv_all.MeshGeometry.Positions.ToList().ForEach(t =>
                {
                    Point3D t2 = t.RotCoordSys(y_axis, mlc_angle);
                    Coordinates_after_rotation.Add(t2);
                });

                var rbox = Coordinates_after_rotation.BoundingBox();

                // width based on bounding box
                double xJaw_width = Math.Sqrt(rbox.SizeX * rbox.SizeX + rbox.SizeY * rbox.SizeY);
                double yJaw_width = rbox.SizeY;
                // recovery bounding box center in regular coordinate.
                var rboxCenter = rbox.BoxCenter();
                Point3D boxCenter = rboxCenter.RotCoordSys(y_axis, (-1) * mlc_angle);

                double max_to_ptv_center = Jaw_Width.find_max_to_axis_distance(ptv_MeshPoints, center_ptv);
                //double max_to_box_center = Jaw_Width.find_max_to_axis_distance(ptv_MeshPoints, center_boundbox);
            }

            double[] scan_angles = Config.Scan_angles_for_jaw_width;
            List<double> mlc_angles = Config.Beams.Select(t => t.mlc_angle).Distinct().ToList();

            var JW_ptv_center = new List<JW_iso_angle>();
            var JW_box_center = new List<JW_iso_angle>();
            var JW_OAR = new List<JW_iso_angle>();


            Structure s;
            VVector isocenter = new VVector();

            bool if_isocenter_candidate_OAR_found = false;
            string iso_OAR_candidate = "";

            foreach (string strn1 in Config.Isocenter_location_precedence)
            {
                if(strn1.Match_Std_TitleCase() != strn1)
                {
                    ConsoleExt.WriteLineWithBackground($"Warning: Isocenter_location_precedence [{strn1}] is not in TG263 standard and won't be used for isocenter locating.", ConsoleColor.Red);
                    continue;
                }
                if (curstructset.has_std_strn(strn1))
                {
                    s = curstructset.get_std_strn(strn1);
                    
                    isocenter = s.CenterPoint;
                    
                    if(Config.isocenter_Y_at_OAR.ToLower() == "posterior") isocenter.y = s.MeshGeometry.Bounds.Y + s.MeshGeometry.Bounds.SizeY;
                    if(Config.isocenter_Y_at_OAR.ToLower() == "anterior") isocenter.y = s.MeshGeometry.Bounds.Y;

                    if(Config.isocenter_Z_at_OAR.ToLower() == "superior") isocenter.z = s.MeshGeometry.Bounds.Z + s.MeshGeometry.Bounds.SizeZ;
                    if(Config.isocenter_Z_at_OAR.ToLower() == "inferior") isocenter.z = s.MeshGeometry.Bounds.Z;
                    
                    Console.WriteLine($"\nFind conventional isocenter location candidate: the {Config.isocenter_Y_at_OAR} and {Config.isocenter_Z_at_OAR} center of {strn1}\n");
                    if_isocenter_candidate_OAR_found = true;
                    iso_OAR_candidate = strn1;
                    break;
                }
            }

            if (if_isocenter_candidate_OAR_found == false)
            {
                Console.WriteLine($"\nNone of [{string.Join(",", Config.Isocenter_location_precedence)}] is found. Isocenter placed on the center of PTV_High");
            }

            foreach (var mlc_angle in mlc_angles)
            {
                Console.WriteLine("if isocenter placed on PTVcenter:");
                JW_ptv_center.Add(Jaw_Width.Rotate_n_Scan_Limits(ptv_MeshPoints, new Vector3D(0, 0, 1), scan_angles, center_ptv, mlc_angle, Config.JawMargin_X_inMM, Config.JawMargin_Y_inMM));
                
                //JW_box_center.Add(Jaw_Width.Rotate_n_Scan_Limits(ptv_MeshPoints, new Vector3D(0, 0, 1), scan_angles, center_boundbox, mlc_angle));

                if (if_isocenter_candidate_OAR_found)
                {
                    Console.WriteLine($"if isocenter placed on {iso_OAR_candidate}:");
                    JW_OAR.Add(Jaw_Width.Rotate_n_Scan_Limits(ptv_MeshPoints, new Vector3D(0, 0, 1), scan_angles, isocenter.Round_up_relative_coordinate(userOrign).VVectorToPoint3D(), mlc_angle, Config.JawMargin_X_inMM, Config.JawMargin_Y_inMM));
                }
                Console.WriteLine();
            }

            //if (false)
            //{
            //    double gn = 5; // granularity / precision of rounding mm
            //    VVector dist = isocenter - curstructset.Image.UserOrigin;
            //    isocenter = new VVector(Math.Round(dist.x / gn) * gn, Math.Round(dist.y / gn) * gn, Math.Round(dist.z / gn) * gn) + curstructset.Image.UserOrigin;
            //}

            //return JW_OAR;

            if (if_isocenter_candidate_OAR_found == false) return JW_ptv_center;

            if (JW_OAR.Any(t => Math.Abs(t.proj_y_Max + t.proj_y_Min) > Config.diff_Y_limit_inMM))
            {
                ConsoleExt.WriteLineWithBackground($"Isocenter move to PTV center to improve Y jaw imbalance. (i.e. |Y1 - Y2| > {Config.diff_Y_limit_inMM}mm when iso placed on {iso_OAR_candidate})", ConsoleColor.Red);
                return JW_ptv_center;
            }

            if(JW_OAR.Any(t => Math.Abs(t.proj_x_Max + t.proj_x_Min) > Config.diff_X_limit_inMM))
            {
                ConsoleExt.WriteLineWithBackground($"Isocenter move to PTV center to improve X jaw imbalance. (i.e. |X1 - X2| > {Config.diff_X_limit_inMM}mm when iso placed on {iso_OAR_candidate})", ConsoleColor.Red);
                return JW_ptv_center;
            }

            foreach(var mlc_angle in mlc_angles)
            {
                var jw_oar1 = JW_OAR.Single(t => t.mlc_rotation_angle == mlc_angle);
                var jw_ptv1 = JW_ptv_center.Single(t => t.mlc_rotation_angle == mlc_angle);

                var reduce = jw_oar1.x_width - jw_ptv1.x_width;
                if ( reduce > Config.Jaw_X_width_reduction_cutoff_inMM)
                {
                    ConsoleExt.WriteLineWithBackground($"Isocenter moved to PTV center from {iso_OAR_candidate}; X jaw width reduced by at least {reduce}mm.", ConsoleColor.Red);
                    return JW_ptv_center;
                }

                reduce = jw_oar1.y_width - jw_ptv1.y_width;
                if (reduce > Config.Jaw_Y_width_reduction_cutoff_inMM)
                {
                    ConsoleExt.WriteLineWithBackground($"Isocenter moved to PTV center from {iso_OAR_candidate}; Y jaw width reduced by at least {reduce}mm.", ConsoleColor.Red);
                    return JW_ptv_center;
                }
            }

            //string msg = $"Isocenter location will be set to: {isocenter.ToString_coordinate()}  User Origin: {curstructset.Image.UserOrigin.ToString_coordinate()}";
            //Console.WriteLine(msg);
            //audit_logger.WriteLine(msg);

            ConsoleExt.WriteLineWithBackground($"Isocenter placed on the above OAR candidate location.");
            return JW_OAR;
        }


        static void Execute(VMS.TPS.Common.Model.API.Application app)
        {
            MessageBox.Show("Before running AutoPlan for a patient, please close the patient in Eclipse first.", msg_header);

            // Update the hard-coded constraint parameters to the actual one in json file.
            AnalyticsLibrary2.constraint.read_from_JSON(Config.Precalculated_JSON_files_dir + Config.Constrains_Parameters_R_out_file);
            directive_cons = constraint.constraints[datasource + "__DF"];

            Rxcon_extra_n_convert.Modify_default_PD_constraint_priorities(directive_cons, Config.Modify_default_PD_constraint_priorities);


            ConsoleExt.WriteLineWithBackground("AutoPlan setting up ...");
            // vm contains the inputs collected from GUI, including Patient_MR, Course/Plan names, PTV structure names, target dose, fraction, if_use_intermediate dose etc..
            vm = new MainViewModel(app, directive_cons);
            var mainWindow = new MainWindow(vm, window_title: Console.Title);

            //XXX Replace directive_con with Rxconstraint collection.

            var GUI_Choice = mainWindow.ShowDialog();

            if (vm.if_start_AP == false)
            {
                ConsoleExt.WriteLineWithBackground("\n\n\nCancelled by user.\n\n");
                Console.WriteLine("\n\n\nType in YES or Y and Then Press Enter Key to Close this Console.");
                return;
            }

            Patient curpat = vm._patient;
            RxCons_from_VM = vm.PD_Rxcons.ToList();
            RxCons_from_VM.ForEach(t => t.tag = "PD");

            if_use_existing_beams = vm.if_use_existing_beams;
            if_intermediate_dose = vm.if_use_intermediate_dose;
            if_stop_before_OPT = vm.if_stop_before_OPT;
            if_4_beams = vm.if_4_beams;
            if_jaw_tracking = vm.if_jaw_tracking;
            MachineID = vm.MachineID;

            //if (curpat.HasModifiedData) // Seems it is always false : ( So, just remind user to close a patient first.
            //{
            //    MessageBox.Show("This patient has modified data. Please save changes and close this Patient in Eclipse first.\n\nProgram quit.");
            //    return;
            //}

            curpat.BeginModifications();

            audit_logger.WriteLine("");
            audit_logger.WriteLine("");

            audit_logger.WriteLine($"{Environment.UserName}\t{Environment.MachineName}\tAutoPlan__START\t{curpat.Id}\t{vm.SelectedCourse}\t{vm.SelectedPlan}\t{vm.NewStructureSet}\t{asbl_location}\t{version}");


            StructureSet StrS_toUseorBeCopied = curpat.StructureSets.Single(ss => ss.Id == vm.actual_strS);

            if (if_use_existing_beams == false)
            {
                //JWs = calc_isocenter_n_JawWidth(StrS_toUseorBeCopied, "Cochlea_L" , null, null);
                //JWs = calc_isocenter_n_JawWidth(StrS_toUseorBeCopied, "SpinalCord", null, null);

                JWs = calc_isocenter_n_JawWidth(StrS_toUseorBeCopied, vm.ptv_h, vm.ptv_m, vm.ptv_l);
                isocenter = JWs.First().isocenter.Point3D_to_VVector();


                double dist_iso_to_couch =
                    StrS_toUseorBeCopied.Structures.Single(x => x.DicomType == "EXTERNAL").MeshGeometry.Bounds.Y
                    + StrS_toUseorBeCopied.Structures.Single(x => x.DicomType == "EXTERNAL").MeshGeometry.Bounds.SizeY
                    - isocenter.y;

                if (dist_iso_to_couch > 150)
                {
                    var if_contitue = MessageBox.Show("The distance between beam isocenter and couch will be (" + (dist_iso_to_couch / 10).ToString("F2") + "cm) > 15cm, which may indicate inclining couch. This program cannot handle inclining couch cases.\n\nDo you want to continue?", msg_header, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);

                    if (if_contitue == MessageBoxResult.No) return;
                }
                if (dist_iso_to_couch <= 0)
                {
                    ConsoleExt.WriteLineWithBackground("Isocenter is placed beneath the boundary of Body. Script abort.", ConsoleColor.Red);
                    return;
                }
            }

            string strSn_to_use = vm.NewStructureSet;
            if (vm.actual_strS != vm.NewStructureSet)
            {
                var new_strS = StrS_toUseorBeCopied.Copy();
                new_strS.Id = vm.NewStructureSet;
                app.SaveModifications();
                ConsoleExt.WriteLineWithBackground(string.Format("\n\nCopy StructureSet [{0}] to create new StructureSet [{1}], which will be used in AutoPlan", vm.actual_strS, vm.NewStructureSet));
            }
            else
            {
                ConsoleExt.WriteLineWithBackground(string.Format("\n\nUse existing StructureSet [{0}]", vm.actual_strS));
            }

            #region ==========================================  setup/select course and plan ========================================== 
            ExternalPlanSetup cureps = null;
            Course curcourse = null;

            // if Course with specified Course ID exist.
            if (curpat.Courses.Where(x => x.Id == vm.SelectedCourse).Any()) curcourse = curpat.Courses.Where(x => x.Id == vm.SelectedCourse).Single();


            if (curcourse != null)
            {
                ConsoleExt.WriteLineWithBackground(string.Format("\nUse Existing Course: {0}\n", curcourse.Id));

                if (curcourse.ExternalPlanSetups.Where(x => x.Id == vm.SelectedPlan).Any())
                {
                    cureps = curcourse.ExternalPlanSetups.Single(x => x.Id == vm.SelectedPlan);
                    ConsoleExt.WriteLineWithBackground(string.Format("\nUse Existing Plan: {0}", vm.SelectedPlan));

                    if (cureps.StructureSet.Id != strSn_to_use)
                    {
                        MessageBox.Show(string.Format($"The selected Plan is using structureSet [{cureps.StructureSet.Id}], which is different from the specified structureSet [{strSn_to_use}].\n\nPlease change the structureSet or choose to create a new plan.\n\nProgram quit."), msg_header);
                        return;
                    }

                    if (if_use_existing_beams)
                    {
                        string msg;
                        if (!cureps.Beams.Any())
                        {
                            msg = "There is no Beam in this plan, while Use Existing Beams option is selected.";
                            audit_logger.WriteLine(msg, AP_lib.Log_levels.error);
                            MessageBox.Show(msg + "\n\nProgram quit.", msg_header);
                            return;
                        }

                        n_existing_beams = cureps.Beams.Count();
                        MachineID = cureps.Beams.First().TreatmentUnit.Id;

                        Console.WriteLine("Info: Existing Beams and their settings will be used. While existing optimization structures and constraints will not.\n\n");

                        var isocenters_distinct = cureps.Beams.Select(b => b.IsocenterPosition).Distinct();
                        if (isocenters_distinct.Count() > 1)
                        {
                            msg = "The isocenters of beams are not the same.\n\nProgram quit.";
                            MessageBox.Show(msg, msg_header);
                            return;
                        }
                        isocenter = cureps.Beams.First().IsocenterPosition;

                       

                        msg = "Isocenter location read from existing Beam: " + isocenter.ToString_coordinate();
                        Console.WriteLine(msg);
                        audit_logger.WriteLine(msg);

                        double dist_iso_to_couch =
                            StrS_toUseorBeCopied.Structures.Single(x => x.DicomType == "EXTERNAL").MeshGeometry.Bounds.Y
                            + StrS_toUseorBeCopied.Structures.Single(x => x.DicomType == "EXTERNAL").MeshGeometry.Bounds.SizeY
                            - isocenter.y;

                        if (dist_iso_to_couch > 150)
                        {
                            var if_contitue = MessageBox.Show("The existing beam isocenter is set (" + (dist_iso_to_couch / 10).ToString("F2") + "cm) > 15cm above couch.\n\nDo you want to continue?", msg_header, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

                            if (if_contitue == MessageBoxResult.No) return;
                        }

                    }
                    else
                    {
                        Console.WriteLine("Info: Beams, optimization structures, and constraints will be created in this plan");
                    }
                }
                else
                {
                    cureps = curcourse.AddExternalPlanSetup(curpat.StructureSets.Single(t => t.Id == strSn_to_use));
                    cureps.Id = vm.SelectedPlan;
                    app.SaveModifications();
                    ConsoleExt.WriteLineWithBackground(string.Format("\nCreated New Plan: {0}\n", vm.SelectedPlan));
                }
            }

            if (curcourse == null)
            {
                curcourse = curpat.AddCourse();
                curcourse.Id = vm.SelectedCourse;
                app.SaveModifications();
                ConsoleExt.WriteLineWithBackground(string.Format("\nCreated New Course: {0}\n", vm.SelectedCourse));


                cureps = curcourse.AddExternalPlanSetup(curpat.StructureSets.Single(t => t.Id == strSn_to_use));
                cureps.Id = vm.SelectedPlan;
                app.SaveModifications();
                ConsoleExt.WriteLineWithBackground(string.Format("\nCreated New Plan: {0}\n", vm.SelectedPlan));
            }

            #endregion

            pt_cs_pl = $"----- Patient: {curpat.Id}; Course: {curcourse.Id}; Plan: {cureps.Id}; StructureSet: {strSn_to_use}; {(if_use_existing_beams ? (n_existing_beams + " beams") : (if_4_beams ? "4 beams" : "3 beams"))}; JawTracking {if_jaw_tracking}; MachineID {MachineID} -----";

            Console.WriteLine(pt_cs_pl);

            app.SaveModifications();

            TSD.Add(new TargetStructureDose("PTV_High", vm.ptv_h, vm.hd));

            if (!string.IsNullOrEmpty(vm.ptv_m))
            {
                is_there_PTV_mid = true;
                TSD.Add(new TargetStructureDose("PTV_Mid", vm.ptv_m, vm.md));
            }

            if (!string.IsNullOrEmpty(vm.ptv_l))
            {
                is_there_PTV_low = true;
                TSD.Add(new TargetStructureDose("PTV_Low", vm.ptv_l, vm.ld));
            }


            // load Statistical StatsDVHs
            foropt = new StatsDVH_ForOpt(datasource, JSON_set_fraction, Pre_JSON_dir: Config.Precalculated_JSON_files_dir);

            // load historical metrics, like Mean[Gy]
            HistoryCurvesInfo = new History_Curves_ForOpt(datasource, JSON_set_fraction, Pre_JSON_dir: Config.Precalculated_JSON_files_dir);


            Log.logger.WriteLine($"if_use_existing_beams:{if_use_existing_beams};\tIf_4bm:{if_4_beams};\tIf_stop_before_OPT:{if_stop_before_OPT};\tIf_JawTracking:{if_jaw_tracking};\tMachineID:{MachineID};");

            AutoPlan(app, curpat, curcourse, cureps, TSD, vm.Nf);

            Log.logger.WriteLine($"{Environment.UserName}\t{Environment.MachineName}\tAutoPlan__FINISH\t{curpat.Id}\t{vm.SelectedCourse}\t{vm.SelectedPlan}\t{vm.NewStructureSet}\t{asbl_location}\t{version}");

        }


        public static void AutoPlan(VMS.TPS.Common.Model.API.Application curapp, Patient curpat, Course curcourse, ExternalPlanSetup cureps, List<TargetStructureDose> tsd, int nfractions)
        {
            StructureSet curstructset = cureps.StructureSet;
            string msg;

            var PreSet = curstructset.Generate_StructureSetSummary(Lib3_ESAPI.LogPoint.StartValues);

            // remove some temp structures that may be created during previous development.
            string temp_strName = "ztemp_str_AP2";
            s2 = curstructset.Recreate_structure("CONTROL", temp_strName); // a temporary structure, to test the overlaping between structures. Will be removed later.

            string temp_strName1 = "ztemp_str_AP1";
            s1 = curstructset.Recreate_structure("CONTROL", temp_strName1);

            #region  ========================================== extract Rx for PTVs, setup zOptPTV and zDLAs ========================================== 

            double doseobjectivevalue_high = tsd.Where(x => x.StandardTargetName == "PTV_High").Select(x => x.DoseToTarget).First();
            double doseobjectivevalue_mid = tsd.Where(x => x.StandardTargetName == "PTV_Mid").Select(x => x.DoseToTarget).FirstOrDefault();
            double doseobjectivevalue_low = tsd.Where(x => x.StandardTargetName == "PTV_Low").Select(x => x.DoseToTarget).FirstOrDefault();

            //double hs = 1.03; // PTV high dose scale 
            //double ls = 1.02; // PTV lower dose scale 
            double hs = constraints_config.Single(t => t.StructureID == CN.zOptPTV_High_H && t.ooo == OptimizationObjectiveOperator.Upper && t.metric_parameter == 0).Rx_scale;

            double doseobjectivevalue_high_upper = doseobjectivevalue_high * hs;

            // the following two will be changed if PTV_L, PTV_M overlap with PTV_H, and they will only used for display purpose on original PTVs; zOPT_PTV structure dose limits will be set directly from doseobjectivevalue_mid * ls/hs, since they don't overlap with each other for sure.
            double doseobjectivevalue_mid_upper = doseobjectivevalue_mid * hs;
            double doseobjectivevalue_low_upper = doseobjectivevalue_low * hs;



            curpat.BeginModifications();


            Structure zOptPTV_High = null, zOptPTV_Low = null, zOptPTV_Mid = null;
            Structure zdlahigh = null, zdlalow = null, zdlamid = null;



            ConsoleExt.WriteLineWithBackground("\nCreate zOptPTV_xxx and zDLA Structures ...");
            Console.WriteLine("This will take about 1 minute\n");


            // Construct zOptPTV_High and zOptPTV_Low
            zOptPTV_High = curstructset.Get_or_Add_structure("CONTROL", CN.zOptPTV_High);

            if (is_there_PTV_low) { zOptPTV_Low = curstructset.Get_or_Add_structure("CONTROL", CN.zOptPTV_Low);
                //zOptPTV_Low.SegmentVolume = zOptPTV_Low.Margin(-50);
                zOptPTV_Low.Empty_by_joining_Empty(curstructset); }

            if (is_there_PTV_mid) zOptPTV_Mid = curstructset.Get_or_Add_structure("CONTROL", CN.zOptPTV_Mid);

            zdlahigh = curstructset.Get_or_Add_structure("CONTROL", "zDLA_High");

            if (is_there_PTV_low) zdlalow = curstructset.Get_or_Add_structure("CONTROL", "zDLA_Low");

            if (is_there_PTV_mid) zdlamid = curstructset.Get_or_Add_structure("CONTROL", "zDLA_Mid");



            Structure oPTVm = null, oPTVl = null, qs;

            //original PTV_High
            Structure oPTVh = curstructset.Structures.Single(x => x.Id == tsd.Single(xx => xx.StandardTargetName == "PTV_High").TargetNameInPlan);



            zOptPTV_High.SegmentVolume = Get_HD_SegmentVolume(oPTVh);
            zOptPTV_High.Check_n_Convert_to_HD();

            curapp.SaveModifications();

            //zOptPTV_Mid
            if (is_there_PTV_mid)
            {
                oPTVm = curstructset.Structures.Single(x => x.Id == tsd.Single(xx => xx.StandardTargetName == "PTV_Mid").TargetNameInPlan);

                if (CalcOverLappingVolume(oPTVh, oPTVm) > 0) doseobjectivevalue_mid_upper = doseobjectivevalue_high;

                zOptPTV_Mid.SegmentVolume = zOptPTV_High.Or(Get_HD_SegmentVolume(oPTVm));
            }

            //zOptPTV_Low
            if (is_there_PTV_low)
            {
                oPTVl = curstructset.Structures.Single(x => x.Id == tsd.Single(xx => xx.StandardTargetName == "PTV_Low").TargetNameInPlan);

                foreach (TargetStructureDose q in tsd)
                {
                    qs = curstructset.Structures.Single(x => x.Id == q.TargetNameInPlan);
                    if (zOptPTV_Low.IsEmpty) zOptPTV_Low.SegmentVolume = Get_HD_SegmentVolume(qs);
                    else zOptPTV_Low.SegmentVolume = zOptPTV_Low.Or(Get_HD_SegmentVolume(qs));
                }

                if (is_there_PTV_mid)
                {
                    s1.SegmentVolume = Get_HD_SegmentVolume(oPTVh).Sub(Get_HD_SegmentVolume(oPTVm));
                    s2.SegmentVolume = s1.And(Get_HD_SegmentVolume(oPTVl));
                    if (s2.Volume > 0)
                    {
                        doseobjectivevalue_low_upper = doseobjectivevalue_high;
                    }
                    else
                    {
                        if (CalcOverLappingVolume(oPTVm, oPTVl) > 0) doseobjectivevalue_low_upper = doseobjectivevalue_mid;
                    }
                }
                else
                {
                    if (CalcOverLappingVolume(oPTVh, oPTVl) > 0) doseobjectivevalue_low_upper = doseobjectivevalue_high;
                }
            }

            curapp.SaveModifications();

            Structure body = curstructset.Structures.Single(x => x.DicomType.ToUpper() == "EXTERNAL");

            //Make a dose limiting annulus arround the high dose ptv optimization structure 
            zdlahigh.SegmentVolume = zOptPTV_High.SegmentVolume;
            zdlahigh.SegmentVolume = zdlahigh.Margin(8.0f);
            zdlahigh.SegmentVolume = zdlahigh.Sub(zOptPTV_High.Margin(1.0f));
            zdlahigh.SegmentVolume = zdlahigh.And(Get_HD_SegmentVolume(body));

            //Make a dose limiting annulus arround the Mid dose ptv optimization structure
            if (is_there_PTV_mid)
            {
                zdlamid.SegmentVolume = zOptPTV_Mid.SegmentVolume;
                zdlamid.SegmentVolume = zdlamid.Margin(8.0f);
                zdlamid.SegmentVolume = zdlamid.Sub(zOptPTV_Mid.Margin(1.0f));
                zdlamid.SegmentVolume = zdlamid.Sub(zOptPTV_High.Margin(5.0f));
                zdlamid.SegmentVolume = zdlamid.Sub(zdlahigh.Margin(1.0f));
                zdlamid.SegmentVolume = zdlamid.And(Get_HD_SegmentVolume(body));
            }

            //Make a dose limiting annulus arround the low dose ptv optimization structure
            if (is_there_PTV_low)
            {
                zdlalow.SegmentVolume = zOptPTV_Low.SegmentVolume;
                zdlalow.SegmentVolume = zdlalow.Margin(15.0f);
                zdlalow.SegmentVolume = zdlalow.Sub(zOptPTV_Low.Margin(1.0f));
                zdlalow.SegmentVolume = zdlalow.Sub(zOptPTV_High.Margin(5.0f));
                if (is_there_PTV_mid) zdlalow.SegmentVolume = zdlalow.Sub(zOptPTV_Mid.Margin(3.0f));
                zdlalow.SegmentVolume = zdlalow.Sub(zdlahigh.Margin(1.0f));
                if (is_there_PTV_mid) zdlalow.SegmentVolume = zdlalow.Sub(zdlamid.Margin(1.0f));
                zdlalow.SegmentVolume = zdlalow.And(Get_HD_SegmentVolume(body));
            }

            curapp.SaveModifications();
#endregion

            // Create beams
            if (if_use_existing_beams == false)
            {
                //Delete Current Beams
                cureps.Beams.ToList().ForEach(b => cureps.RemoveBeam(b));

                curapp.SaveModifications();

                //Add VMAT Beams
                //VVector isocenter = new VVector(Math.Round(ptv_high.CenterPoint.x / 10.0f) * 10.0f, Math.Round(ptv_high.CenterPoint.y / 10.0f) * 10.0f, Math.Round(ptv_high.CenterPoint.z / 10.0f) * 10.0f);
                ExternalBeamMachineParameters ebmp = new ExternalBeamMachineParameters(MachineID, "6X", 600, "ARC", null); // ? dose rate 600
                                                                                                                           //ExternalBeamMachineParameters ebmp = new ExternalBeamMachineParameters("UM-EX4", "6X", 600, "ARC", null); // ? dose rate 600

                var bf1 = Config.Beams[0]; var jw1 = JWs.Single(t => t.mlc_rotation_angle == bf1.mlc_angle);
                var bf2 = Config.Beams[1]; var jw2 = JWs.Single(t => t.mlc_rotation_angle == bf2.mlc_angle);
                var bf3 = Config.Beams[2]; var jw3 = JWs.Single(t => t.mlc_rotation_angle == bf3.mlc_angle);
                var bf4 = Config.Beams[3]; var jw4 = JWs.Single(t => t.mlc_rotation_angle == bf4.mlc_angle);

                Beam VMAT1temp = cureps.AddArcBeam(ebmp, 
                    new VRect<double>(Math.Min(jw1.proj_x_Min, 20), Math.Min(jw1.proj_y_Min,100), Math.Max(jw1.proj_x_Max,-20), Math.Max(jw1.proj_y_Max,-100)), 
                    bf1.mlc_angle, bf1.gantryAngle, bf1.gantryStop, bf1.gantryDir, bf1.tableAngle, isocenter);
                
                 Beam VMAT2temp = cureps.AddArcBeam(ebmp, 
                    new VRect<double>(Math.Min(jw2.proj_x_Min, 20), Math.Min(jw2.proj_y_Min,100), Math.Max(jw2.proj_x_Max, -20), Math.Max(jw2.proj_y_Max, -100)), 
                    bf2.mlc_angle, bf2.gantryAngle, bf2.gantryStop, bf2.gantryDir, bf2.tableAngle, isocenter);
                
                Beam VMAT3temp = cureps.AddArcBeam(ebmp, 
                    new VRect<double>(Math.Min(jw3.proj_x_Min, 20), Math.Min(jw3.proj_y_Min, 100), Math.Max(jw3.proj_x_Max, -20), Math.Max(jw3.proj_y_Max, -100)),
                    bf3.mlc_angle, bf3.gantryAngle, bf3.gantryStop, bf3.gantryDir, bf3.tableAngle, isocenter);

                //Beam VMAT2temp = cureps.AddArcBeam(ebmp, new VRect<double>(-100, -50, 100, 200), 340, angle1, angle2, GantryDirection.CounterClockwise, 0, isocenter);
                //Beam VMAT3temp = cureps.AddArcBeam(ebmp, new VRect<double>(-100, -100, 100, 100), 85, angle2, angle1, GantryDirection.Clockwise, 0, isocenter);


                //if (is_there_PTV_low)
                //{
                //    VMAT1temp.FitCollimatorToStructure(new FitToStructureMargins(10), zOptPTV_Low, true, true, false);
                //    VMAT2temp.FitCollimatorToStructure(new FitToStructureMargins(10), zOptPTV_Low, true, true, false);
                //    VMAT3temp.FitCollimatorToStructure(new FitToStructureMargins(10), zOptPTV_Low, true, true, false);
                //    //VMAT4temp.FitCollimatorToStructure(new FitToStructureMargins(10), zoptptvlow, true, true, false);
                //}
                //else if (is_there_PTV_mid)
                //{
                //    VMAT1temp.FitCollimatorToStructure(new FitToStructureMargins(10), zOptPTV_Mid, true, true, false);
                //    VMAT2temp.FitCollimatorToStructure(new FitToStructureMargins(10), zOptPTV_Mid, true, true, false);
                //    VMAT3temp.FitCollimatorToStructure(new FitToStructureMargins(10), zOptPTV_Mid, true, true, false);
                //}
                //else
                //{
                //    VMAT1temp.FitCollimatorToStructure(new FitToStructureMargins(10), zOptPTV_High, true, true, false);
                //    VMAT2temp.FitCollimatorToStructure(new FitToStructureMargins(10), zOptPTV_High, true, true, false);
                //    VMAT3temp.FitCollimatorToStructure(new FitToStructureMargins(10), zOptPTV_High, true, true, false);
                //}


                // adjust x width of beam 1 and 2 to under 22cm.

                double MaxJawWidth = Config.Max_Jaw_X_Width_inMM;

                var B1JawP = VMAT1temp.ControlPoints[0].JawPositions;
                var B1JawP_adj = new VRect<double>(B1JawP.X1, B1JawP.Y1, Math.Min(B1JawP.X1 + MaxJawWidth, B1JawP.X2), B1JawP.Y2);

                var B2JawP = VMAT2temp.ControlPoints[0].JawPositions;
                var B2JawP_adj = new VRect<double>(Math.Max(B2JawP.X1, B2JawP.X2 - MaxJawWidth), B2JawP.Y1, B2JawP.X2, B2JawP.Y2);

                Beam VMAT1 = cureps.AddArcBeam(ebmp, B1JawP_adj, bf1.mlc_angle, bf1.gantryAngle, bf1.gantryStop, bf1.gantryDir, bf1.tableAngle, isocenter);
                Beam VMAT2 = cureps.AddArcBeam(ebmp, B2JawP_adj, bf2.mlc_angle, bf2.gantryAngle, bf2.gantryStop, bf2.gantryDir, bf2.tableAngle, isocenter);

                VMAT1.Id = bf1.BeamName;
                VMAT2.Id = bf2.BeamName;

                double x1v = VMAT3temp.ControlPoints[0].JawPositions.X1;
                double x2v = VMAT3temp.ControlPoints[0].JawPositions.X2;
                double y1v = VMAT3temp.ControlPoints[0].JawPositions.Y1;
                double y2v = VMAT3temp.ControlPoints[0].JawPositions.Y2;


                if (if_4_beams == true)
                {
                    Beam VMAT3 = cureps.AddArcBeam(ebmp, new VRect<double>(-10, y1v, x2v, y2v), bf3.mlc_angle, bf3.gantryAngle, bf3.gantryStop, bf3.gantryDir, bf3.tableAngle, isocenter);
                    VMAT3.Id = bf3.BeamName;
                    Beam VMAT4 = cureps.AddArcBeam(ebmp, new VRect<double>(x1v, y1v,  10, y2v), bf4.mlc_angle, bf4.gantryAngle, bf4.gantryStop, bf4.gantryDir, bf4.tableAngle, isocenter);
                    VMAT4.Id = bf4.BeamName;

                    //Beam VMAT4 = cureps.AddArcBeam(ebmp, new VRect<double>(x1v, y1v, 10, y2v), 85, 179, 181, GantryDirection.CounterClockwise, 0, isocenter);
                }
                else
                {
                    Beam VMAT3 = cureps.AddArcBeam(ebmp, new VRect<double>(x1v, y1v, Math.Min(x1v + MaxJawWidth, x2v), y2v), bf3.mlc_angle, bf3.gantryAngle, bf3.gantryStop, bf3.gantryDir, bf3.tableAngle, isocenter);
                    VMAT3.Id = bf3.BeamName;
                }


                cureps.RemoveBeam(VMAT1temp);
                cureps.RemoveBeam(VMAT2temp);
                cureps.RemoveBeam(VMAT3temp);

                curapp.SaveModifications();
                ConsoleExt.WriteLineWithBackground("\nVMAT Beams Created.\n");
            }
            // end of if_create_beams == true;


            // Subtract zOptPTV_High out of zOptPTV_Low. Placed here due to the above FitCollimatorToStructure() used zoptptvlow (with all PTVs included). // no need to be placed here any more, since JWs has been calculated earlier, no need to FitCollimatorToStructure().

            Structure PTV_union = zOptPTV_High;

            if (is_there_PTV_low)
            {
                var str1 = curstructset.Get_or_Add_structure("CONTROL", Config.zPTV_Low_name);
                var str2 = curstructset.Get_or_Add_structure("CONTROL", Config.zPTV_Low_only_name);

                str1.SegmentVolume = zOptPTV_Low.SegmentVolume;
                str2.SegmentVolume = zOptPTV_Low.SegmentVolume;

                PTV_union = str1;

                if (is_there_PTV_mid)
                {
                    zOptPTV_Low.SegmentVolume = zOptPTV_Low.Sub(zOptPTV_Mid.Margin(1));

                    str2.SegmentVolume = str2.Sub(zOptPTV_Mid);
                }
                else
                {
                    zOptPTV_Low.SegmentVolume = zOptPTV_Low.Sub(zOptPTV_High.Margin(1));

                    str2.SegmentVolume = str2.Sub(zOptPTV_High);
                }
            }

            if (is_there_PTV_mid)
            {
                var str1 = curstructset.Get_or_Add_structure("CONTROL", Config.zPTV_Mid_name);
                var str2 = curstructset.Get_or_Add_structure("CONTROL", Config.zPTV_Mid_only_name);

                str1.SegmentVolume = zOptPTV_Mid.SegmentVolume;
                str2.SegmentVolume = zOptPTV_Mid.SegmentVolume;

                if (is_there_PTV_low == false) PTV_union = str1;

                zOptPTV_Mid.SegmentVolume = zOptPTV_Mid.Sub(zOptPTV_High.Margin(1));

                str2.SegmentVolume = str2.Sub(zOptPTV_High);
            }

            curapp.SaveModifications();

            #region  ========================================== Calculation models ========================================== 
            //cureps.SetCalculationModel(CalculationType.PhotonVMATOptimization, "PO_15511");
            //cureps.SetCalculationModel(CalculationType.PhotonIMRTOptimization, "PO_15511");
            //cureps.SetCalculationModel(CalculationType.PhotonVolumeDose, "AAA_15511");

            //cureps.SetCalculationModel(CalculationType.PhotonVMATOptimization, "PO_15605");
            //cureps.SetCalculationModel(CalculationType.PhotonIMRTOptimization, "PO_15605");
            //cureps.SetCalculationModel(CalculationType.PhotonVolumeDose, "AAA_15605");

            cureps.SetCalculationModel(CalculationType.PhotonVMATOptimization, Config.PhotonVMATOptimization);

            cureps.SetCalculationModel(CalculationType.PhotonVolumeDose, Config.PhotonVolumeDose);

            //cureps.SetCalculationModel(CalculationType.PhotonInfluenceMatrix, "AAA_1362306"); // extra specification I can, but likely not relavent.

            cureps.SetPrescription(nfractions, new DoseValue(tsd.Max(x => x.DoseToTarget) / nfractions, "Gy"), 1);

            curapp.SaveModifications();


            if (if_jaw_tracking == true)
            {
                if (MachineID == "UM-EX4")
                {
                    msg = $"Warning: JawTracking is not supported for selected machine [UM-EX4]. AutoPlan proceeds with JawTracking turned off.";
                    audit_logger.WriteLine(msg);
                    ConsoleExt.WriteLineWithBackground(msg, ConsoleColor.Red);
                }
                else
                {
                    cureps.OptimizationSetup.UseJawTracking = true;
                }
            }

            curapp.SaveModifications();
#endregion

            //remove previously added Rx constraints.
            foreach (var obj in cureps.OptimizationSetup.Objectives) { cureps.OptimizationSetup.RemoveObjective(obj); }


            //Console.WriteLine("\nSetting up Optimization objectives ------- \n");

            // Structures in StatsDVH file and Planning directive.
            strns_history = foropt.str_sDVH_dict.Keys.Intersect(RxCons_from_VM.Select(c => c.StructureID)).ToList();

            // Structures in StatsDVH and Planning directive and current plan.
            strns_inplan_byvolume = curstructset.Structures.Where(t => strns_history.Contains(t.Id.Match_Std_TitleCase())).OrderBy(t => t.Volume).Select(t => t.Id).ToList();


            // strs need to be carved out from zOptPTV_xxx into zOptPTV_xxx_L
            List<string> OARs_to_carve_into_zOptPTV_xxx_L = strns_inplan_byvolume.Where(
                t => RxCons_from_VM.Any(c => c.priority_decimal <= Config.OARs_into_zOptPTV_xxx_L_Priority && c.StructureID == t.Match_Std_TitleCase())
                || Config.OARs_into_zOptPTV_xxx_L_list.Contains(t.Match_Std_TitleCase())
                ).ToList();

            #region ==========================================  create _H/_L split of zOptPTVs if needed. ========================================== 
            ConsoleExt.WriteLineWithBackground("\nCreate zOptPTV_xxx_H/L Structures if needed ... ");
            Console.WriteLine("This will take about 1 minute\n");

            Structure zOptPTV_High_H = curstructset.Get_or_Add_structure("CONTROL", CN.zOptPTV_High_H);
            Structure zOptPTV_High_L = curstructset.Get_or_Add_structure("CONTROL", CN.zOptPTV_High_L); // zOptPTV_High_L.SegmentVolume = zOptPTV_High_L.Margin(-50);
            zOptPTV_High_L.Empty_by_joining_Empty(curstructset);

            Structure zOptPTV_Mid_H = curstructset.Get_or_Add_structure("CONTROL", CN.zOptPTV_Mid_H);
            Structure zOptPTV_Mid_L = curstructset.Get_or_Add_structure("CONTROL", CN.zOptPTV_Mid_L); // zOptPTV_Mid_L.SegmentVolume = zOptPTV_Mid_L.Margin(-50);
            zOptPTV_Mid_L.Empty_by_joining_Empty(curstructset);

            Structure zOptPTV_Low_H = curstructset.Get_or_Add_structure("CONTROL", CN.zOptPTV_Low_H);
            Structure zOptPTV_Low_L = curstructset.Get_or_Add_structure("CONTROL", CN.zOptPTV_Low_L); // zOptPTV_Low_L.SegmentVolume = zOptPTV_Low_L.Margin(-50);
            zOptPTV_Low_L.Empty_by_joining_Empty(curstructset);

            zOptPTV_High_L.Check_n_Convert_to_HD();
            zOptPTV_Mid_L.Check_n_Convert_to_HD();
            zOptPTV_Low_L.Check_n_Convert_to_HD();

            foreach (string strn in OARs_to_carve_into_zOptPTV_xxx_L)
            {
                Structure str = curstructset.Structures.Single(t => t.Id == strn);

                var str_SegmentVolume_HD = Get_HD_SegmentVolume(str);

                s2.SegmentVolume = str_SegmentVolume_HD.And(zOptPTV_High);
                if (s2.Volume > 0)
                {
                    zOptPTV_High_L.SegmentVolume = zOptPTV_High_L.Or(s2);
                }

                if (is_there_PTV_mid)
                {
                    s2.SegmentVolume = str_SegmentVolume_HD.And(zOptPTV_Mid);
                    if (s2.Volume > 0)
                    {
                        zOptPTV_Mid_L.SegmentVolume = zOptPTV_Mid_L.Or(s2);
                    }
                }

                if (is_there_PTV_low)
                {
                    s2.SegmentVolume = str_SegmentVolume_HD.And(zOptPTV_Low);
                    if (s2.Volume > 0)
                    {
                        zOptPTV_Low_L.SegmentVolume = zOptPTV_Low_L.Or(s2);
                    }
                }
            }

            curapp.SaveModifications();
            #endregion

            #region ========================================== Add constraints for zOptPTVs ==========================================

            if (zOptPTV_High_L.Volume > 0)
            {
                zOptPTV_High_H.SegmentVolume = zOptPTV_High.Sub(zOptPTV_High_L.Margin(5));
                zOptPTV_High_L.SegmentVolume = zOptPTV_High.Sub(zOptPTV_High_H.Margin(1));

                foreach (RxConstraint Rx1_cfg in constraints_config.Where(t => t.StructureID == CN.zOptPTV_High_H || t.StructureID == CN.zOptPTV_High_L))
                {
                    RxCons.Add(Rx1_cfg.Apply_to_zPTV(doseobjectivevalue_high));
                }
            }
            else
            {
                curstructset.RemoveStructure(zOptPTV_High_L); curstructset.RemoveStructure(zOptPTV_High_H);

                foreach (RxConstraint Rx1_cfg in constraints_config.Where(t => t.StructureID == CN.zOptPTV_High_H))
                {
                    RxCons.Add(Rx1_cfg.Apply_to_zPTV(CN.zOptPTV_High, doseobjectivevalue_high));
                }
            }



            if (zOptPTV_Mid_L.Volume > 0)
            {
                zOptPTV_Mid_H.SegmentVolume = zOptPTV_Mid.Sub(zOptPTV_Mid_L.Margin(5));
                zOptPTV_Mid_L.SegmentVolume = zOptPTV_Mid.Sub(zOptPTV_Mid_H.Margin(1));

                foreach (RxConstraint Rx1_cfg in constraints_config.Where(t => t.StructureID == CN.zOptPTV_Mid_H || t.StructureID == CN.zOptPTV_Mid_L))
                {
                    RxCons.Add(Rx1_cfg.Apply_to_zPTV(doseobjectivevalue_mid));
                }
            }
            else
            {
                curstructset.RemoveStructure(zOptPTV_Mid_L); curstructset.RemoveStructure(zOptPTV_Mid_H);
                if (is_there_PTV_mid)
                {
                    foreach (RxConstraint Rx1_cfg in constraints_config.Where(t => t.StructureID == CN.zOptPTV_Mid_H))
                    {
                        RxCons.Add(Rx1_cfg.Apply_to_zPTV(CN.zOptPTV_Mid, doseobjectivevalue_mid));
                    }
                }
            }

            if (zOptPTV_Low_L.Volume > 0)
            {
                zOptPTV_Low_H.SegmentVolume = zOptPTV_Low.Sub(zOptPTV_Low_L.Margin(5));
                zOptPTV_Low_L.SegmentVolume = zOptPTV_Low.Sub(zOptPTV_Low_H.Margin(1));

                foreach (RxConstraint Rx1_cfg in constraints_config.Where(t => t.StructureID == CN.zOptPTV_Low_H || t.StructureID == CN.zOptPTV_Low_L))
                {
                    RxCons.Add(Rx1_cfg.Apply_to_zPTV(doseobjectivevalue_low));
                }
            }
            else
            {
                curstructset.RemoveStructure(zOptPTV_Low_L); curstructset.RemoveStructure(zOptPTV_Low_H);
                if (is_there_PTV_low)
                {
                    foreach (RxConstraint Rx1_cfg in constraints_config.Where(t => t.StructureID == CN.zOptPTV_Low_H))
                    {
                        RxCons.Add(Rx1_cfg.Apply_to_zPTV(CN.zOptPTV_Low, doseobjectivevalue_low));
                    }
                }
            }

            curapp.SaveModifications();
            #endregion

            #region  ========================================== calculate OAR PTV overlap  ========================================== 
            foreach (var str in curstructset.Structures.
                Where(t => !t.IsEmpty
                &&
                    (
                    strns_history.Contains(t.Id.Match_Std_TitleCase()) ||
                    t.Id.StartsWith("zOptPTV", StringComparison.OrdinalIgnoreCase) ||
                    t.Id.StartsWith("zDLA", StringComparison.OrdinalIgnoreCase) ||
                    t.Id.StartsWith("Body", StringComparison.OrdinalIgnoreCase) ||
                    t.Id == oPTVh?.Id || t.Id == oPTVm?.Id || t.Id == oPTVl?.Id
                    )
                ).OrderBy(t => t.Id))
            {
                if (str.Id.Equals("Air", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var opol = new OAR_PTV_overlap(StructureName: str.Id, volume: str.Volume);

                opol.PTV_H_overlap = CalcOverLappingVolume(str, zOptPTV_High);

                if (is_there_PTV_mid)
                {
                    opol.PTV_M_overlap = CalcOverLappingVolume(str, zOptPTV_Mid);
                }

                if (is_there_PTV_low)
                {
                    opol.PTV_L_overlap = CalcOverLappingVolume(str, zOptPTV_Low);
                }

                strn_ptv_overlap_dict.Add(opol);
#if DEBUG
                Console.WriteLine($"{str.Id,-16}{str.Volume:F2}\t{opol,-24}\t{opol.HML_ol:F2}\t{opol.shortString}\t{opol.StructureName}");
#endif
            }

            #endregion


            #region =========== added priority 0 objectives on original PTVs for display purpose ===========================

            RxCons.Add(new RxConstraint(oPTVh.Id, OptimizationObjectiveOperator.Lower, 100, doseobjectivevalue_high, 0));
            RxCons.Add(new RxConstraint(oPTVh.Id, OptimizationObjectiveOperator.Upper, 0, doseobjectivevalue_high_upper, 0));
            if (if_DLA == true) RxCons.Add(new RxConstraint(zdlahigh.Id, OptimizationObjectiveOperator.Upper, 0, doseobjectivevalue_high, Config.prio_zDLA_High));

            if (is_there_PTV_mid)
            {
                RxCons.Add(new RxConstraint(oPTVm.Id, OptimizationObjectiveOperator.Lower, 100, doseobjectivevalue_mid, 0));
                RxCons.Add(new RxConstraint(oPTVm.Id, OptimizationObjectiveOperator.Upper, 0, doseobjectivevalue_mid_upper, 0));
                if (if_DLA == true) RxCons.Add(new RxConstraint(zdlamid.Id, OptimizationObjectiveOperator.Upper, 0, doseobjectivevalue_mid, Config.prio_zDLA_Mid));
            }

            if (is_there_PTV_low)
            {
                RxCons.Add(new RxConstraint(oPTVl.Id, OptimizationObjectiveOperator.Lower, 100, doseobjectivevalue_low, 0));
                RxCons.Add(new RxConstraint(oPTVl.Id, OptimizationObjectiveOperator.Upper, 0, doseobjectivevalue_low_upper, 0));
                if (if_DLA == true) RxCons.Add(new RxConstraint(zdlalow.Id, OptimizationObjectiveOperator.Upper, 0, doseobjectivevalue_low, Config.prio_zDLA_Low));
            }
            #endregion


            //cureps.OptimizationSetup.AddNormalTissueObjective(100, 3, 100.0f, 30.0f, 0.2f);
            cureps.OptimizationSetup.AddNormalTissueObjective(
                priority: Config.NTO_priority,
                distanceFromTargetBorderInMM: Config.NTO_distanceFromTargetBorderInMM, 
                startDosePercentage: Config.NTO_startDosePercentage,
                endDosePercentage: Config.NTO_endDosePercentage, 
                fallOff: Config.NTO_fallOff
                );

            curapp.SaveModifications();

            #region =================================== Rxcon_extra_n_convert ==============================================
            //Rxcon_extra_n_convert.Add_extra_miscellaneous_constraints(RxCons_from_VM);
            Rxcon_extra_n_convert.Add_fixed_limit_point_constraints(RxCons_from_VM, constraints_config);
            Rxcon_extra_n_convert.Adjust_priority_for_existing_constraints(RxCons_from_VM, constraints_config);


            ConsoleExt.WriteLineWithBackground("\nAdd Mean[Gy] constraints with limits from constraint_config.json, for each OAR which has a Mean[Gy] constraint on Planning Directive.");
            //Rxcon_extra_n_convert.Add_Mean_Gy_constraints(RxCons_from_VM);
            //Rxcon_extra_n_convert.Modify_fixed_limit_Mean_Gy_constraint_limits(RxCons_from_VM, constraints_config);
            Rxcon_extra_n_convert.Add_Fixed_Mean_constraints(RxCons_from_VM, constraints_config);


            ConsoleExt.WriteLineWithBackground("\nModify some limits based on constraint_config.json.");
            Rxcon_extra_n_convert.Overwrite_limit_for_PD_constraint(RxCons_from_VM, constraints_config);

            ConsoleExt.WriteLineWithBackground("\nAdd gEUD constraint with priority 0 and limit at pre-defined value, for each OAR on Planning Directive.");
            Rxcon_extra_n_convert.Add_gEUD_prio0_constraints(RxCons_from_VM);
#endregion

            #region ================================= add zNape and zBuff and constraints on them ==============================

            var oPTV_list = new List<Structure> {oPTVh};
            if (is_there_PTV_mid) oPTV_list.Add(oPTVm);
            if (is_there_PTV_low) oPTV_list.Add(oPTVl);

            if (Config.if_add_zNape == true)
            {
                curstructset.Recreate_Nape_structure(oPTV_list, Config.zNape_expansion_from_spinalCord_inMM, Config.zNape_marginFromPTVsInMM);
                Structure zNape = curstructset.Structures.FirstOrDefault(t => t.Id == "zNape");
                if (zNape != null)
                {
                    Console.WriteLine($"add {zNape.Id} gEUD constraint.");

                    RxCons.Add(new RxConstraint(zNape.Id, OptimizationObjectiveOperator.Upper, -1, Config.zNape_gEUD_limit_Gy, Config.zNape_priority, DVHMetricType.gEUD) { gEUD_a = Config.zNape_gEUD_a });
                }
            }

            if (Config.if_add_zBuff == true)
            {
                curstructset.Recreate_zBuff_structure(oPTV_list, Config.zBuff_expansion_from_spinalCord_inMM, Config.zBuff_marginFromPTVsInMM, Config.zBuff_1st_expansion_margin_inMM);
                Structure zBuff = curstructset.Structures.FirstOrDefault(t => t.Id == "zBuff");
                if (zBuff != null)
                {
                    Console.WriteLine($"add {zBuff.Id} gEUD constraint.");

                    RxCons.Add(new RxConstraint(zBuff.Id, OptimizationObjectiveOperator.Upper, -1, Config.zBuff_gEUD_limit_Gy, Config.zBuff_priority, DVHMetricType.gEUD) { gEUD_a = Config.zBuff_gEUD_a });
                }
            }
            #endregion

            // Initialize optimization objectives.
            // Convert Head and Neck directive constraints to Dx%[Gy] point objectives stored in RxCons, with dose from StatsDVH 50% dose quantile
            // RxCons is a list that temporarily hold all constraints that is going to apply to Optimization. It also stores additional info for display purposes.
            var StatsDVH_dose_quantile_init = 35;

            List<string> strns = curstructset.Structures.Where(s => s.Volume > 0 && !s.Id.ToLower().Contains("bolus")).Select(t => t.Id).ToList();

            foreach (string strn in strns)
            {
                Structure str = curstructset.Structures.Single(t => t.Id == strn);
                string strn_std = str.Id.Match_Std_TitleCase();

                if (!strns_history.Contains(strn_std)) continue;
                
                //OAR_PTV_overlap opol = strn_ptv_overlap_dict.FirstOrDefault(t => t.StructureName == strn);

                List<RxConstraint> cons_str = RxCons_from_VM.Where(t => t.StructureID == strn_std).ToList();

                foreach (RxConstraint con in cons_str) 
                {
                    if (con.metric_type == DVHMetricType.Dxcc_Gy.ToString())
                    {
                        if (con.if_limit_fixed == true)
                        {
                            RxCons.Add(new RxConstraint(str.Id, OptimizationObjectiveOperator.Upper, Math.Min(100, con.metric_parameter / str.Volume * 100), con.limit, Priority_mapping.map_decimal_prio_to_OPT(con.priority_decimal)) { priority = con.priority, metric_type_orig = DVHMetricType.Dxcc_Gy.ToString(), tag = RxCon_types.Point_with_fixed_limit });
                        }
                        else
                        {
                            point_on_StatsDVH p = foropt.find_point_on_StatsDVH(str: str.Id, at_volume_percent: Math.Min(100, con.metric_parameter / str.Volume * 100), at_dose_quantile: StatsDVH_dose_quantile_init);
                            RxCons.Add(new RxConstraint(str.Id, OptimizationObjectiveOperator.Upper, p.at_volume_percent, p.Dose, Priority_mapping.map_decimal_prio_to_OPT(con.priority_decimal)) { dose_qtl = StatsDVH_dose_quantile_init, priority = con.priority, metric_type_orig = DVHMetricType.Dxcc_Gy.ToString() });
                        }
                    }

                    if (con.metric_type == DVHMetricType.Mean_Gy.ToString())
                    {
                        if (con.if_break_down_Mean_Gy == false) {
                            
                            if (con.if_limit_fixed == true)
                            {
                                // StatsDVH Limit; Add Mean[Gy] constraint with limit from directive.
                                // var metric_dist = HistoryCurvesInfo.get_metric_distribution(str.Id, con);
                                RxCons.Add(new RxConstraint(str.Id, OptimizationObjectiveOperator.Upper, -1, con.limit, Priority_mapping.map_decimal_prio_to_OPT(con.priority_decimal), DVHMetricType.Mean_Gy) { tag = RxCon_types.Mean_with_fixed_limit });
                            }
                        
                        }

                        if (con.if_break_down_Mean_Gy == true && con.if_limit_fixed == false)
                        {
                            var p = foropt.find_point_on_StatsDVH(str: str.Id, at_volume_percent: Mean_con_breakup.lowDoseAtVol, at_dose_quantile: StatsDVH_dose_quantile_init);
                            RxCons.Add(new RxConstraint(str.Id, OptimizationObjectiveOperator.Upper, p.at_volume_percent, p.Dose, Priority_mapping.map_to_OPT_priority(con.opol, con, p.at_volume_percent)) { dose_qtl = StatsDVH_dose_quantile_init, priority = con.priority, metric_type_orig = "Mean_Gy" });

                            //if (con.priority == 3 && opol != null && opol.HML_ol / str.Volume * 100 > Mean_con_breakup.midDoseAtVol - 10) OPT_Priority = 10;

                            p = foropt.find_point_on_StatsDVH(str: str.Id, at_volume_percent: Mean_con_breakup.midDoseAtVol, at_dose_quantile: StatsDVH_dose_quantile_init);
                            RxCons.Add(new RxConstraint(str.Id, OptimizationObjectiveOperator.Upper, p.at_volume_percent, p.Dose, Priority_mapping.map_to_OPT_priority(con.opol, con, p.at_volume_percent)) { dose_qtl = StatsDVH_dose_quantile_init, priority = con.priority, metric_type_orig = "Mean_Gy" });

                            //if (con.priority == 3 && opol != null && opol.HML_ol > 0) OPT_Priority = 10;

                            p = foropt.find_point_on_StatsDVH(str: str.Id, at_volume_percent: Mean_con_breakup.highDoseAtVol, at_dose_quantile: StatsDVH_dose_quantile_init);
                            RxCons.Add(new RxConstraint(str.Id, OptimizationObjectiveOperator.Upper, p.at_volume_percent, p.Dose, Priority_mapping.map_to_OPT_priority(con.opol, con, p.at_volume_percent)) { dose_qtl = StatsDVH_dose_quantile_init, priority = con.priority, metric_type_orig = "Mean_Gy" });

                        }

                    }

                    if (con.metric_type == DVHMetricType.VxGy_Percent.ToString())
                    {
                        point_on_StatsDVH p = foropt.find_point_on_StatsDVH(str: str.Id, at_volume_percent: con.limit, at_dose_quantile: StatsDVH_dose_quantile_init);
                        RxCons.Add(new RxConstraint(str.Id, OptimizationObjectiveOperator.Upper, p.at_volume_percent, p.Dose, Priority_mapping.map_decimal_prio_to_OPT(con.priority_decimal)) { dose_qtl = StatsDVH_dose_quantile_init, priority = con.priority, metric_type_orig = DVHMetricType.VxGy_Percent.ToString() });
                    }

                    if(con.metric_type == DVHMetricType.gEUD.ToString())
                    {
                        RxCons.Add(new RxConstraint(str.Id, OptimizationObjectiveOperator.Upper, con.metric_parameter, con.limit, Priority_mapping.map_decimal_prio_to_OPT(con.priority_decimal), DVHMetricType.gEUD) {gEUD_a = con.gEUD_a, tag = RxCon_types.gEUD});
                    }

                    if(con.metric_type == DVHMetricType.DxPercent_Gy.ToString())
                    {
                        RxCons.Add(new RxConstraint(str.Id, OptimizationObjectiveOperator.Upper, con.metric_parameter, con.limit, Priority_mapping.map_decimal_prio_to_OPT(con.priority_decimal)));
                    }

                }
                
            }
            curapp.SaveModifications();
            ConsoleExt.WriteLineWithBackground("\nOptimization Constraints Set.\n");

#if DEBUG
            curstructset.Structures.OrderBy(t => t.Id).ToList().ForEach(t => Console.WriteLine(t.Id));
#endif

            // add selected constaints to cureps.OptimizationSetup with AddPointObjective(...) method.
            // Chuck, by filter down the list, we can add only constraints on certain structure, like PTVs only.
            RxCons_used = RxCons
                //.Where(t => t.StructureID.StartsWith("zOptPTV"))
                //.Where(t => t.StructureID.Contains("PTV") || t.StructureID.StartsWith("zDLA") || t.StructureID.StartsWith("Larynx") || t.StructureID == "Musc_Constrict_I")
                //.Where(t => t.StructureID.StartsWith("zOptPTV") || t.StructureID.StartsWith("zDLA") || t.OPT_priority >= 80) // only Priority 1 constraints
                //.Where(t => !t.StructureID.StartsWith("zDLA") ) // only Priority 1 constraints
                //.Where(t => t.OPT_priority >= 80)
                .ToList();
            //#if DEBUG
            //                RxCons_used.OrderBy(t => t.objHashCode).ToList().ForEach(t => Console.WriteLine("{0}\t{1}\t{2}\t{3}", t.StructureID, t.strn_orig, t.ToString(), t.objHashCode));
            //#endif

            Rxcon_extra_n_convert.Add_extra_generic_constraint(RxCons_used, constraints_config);
            foreach (var obj in cureps.OptimizationSetup.Objectives) { cureps.OptimizationSetup.RemoveObjective(obj); }

            RxCons_used.ForEach(t => cureps.Add_OPT_Objective_FromRxConstraint(t));

            curapp.SaveModifications();



            if (if_stop_before_OPT)
            {
                ConsoleExt.WriteLineWithBackground("\nStop before optimization as user instructed.");
                return;
            }


            OptimizerResult optresult = null;

            // --------------------------------- 1st OPT ----------------------------
            ConsoleExt.WriteLineWithBackground("\nStarting Optimization ...");
            Console.WriteLine("This will take about 10 minutes; please check back around " + DateTime.Now.AddMinutes(10).ToString("hh:mm:ss tt"));

            try // Optimization
            {
                sw.Start();
                if (if_intermediate_dose == true)
                {
                    OptimizationOptionsVMAT opt_VMAT3 = new OptimizationOptionsVMAT(OptimizationIntermediateDoseOption.UseIntermediateDose, "");
                    optresult = cureps.OptimizeVMAT(opt_VMAT3);
                }
                if (if_intermediate_dose == false) { optresult = cureps.OptimizeVMAT(); }

                sw.Stop();
                curapp.SaveModifications();

            }
            catch (Exception e)
            {
                ConsoleExt.WriteLineWithBackground("\n\nError: Optimization failed due to the following error\n", ConsoleColor.Red);
                Console.WriteLine(e.Message);
                Console.WriteLine("\n\nYou may want to manually correct this error and try again in Eclipse ExternalBeamPlanning.\n\n");
                return;
            }


            if (optresult.Success == false)
            {
                ConsoleExt.WriteLineWithBackground("\n\nError: Optimization failed for unclear reason. All optimization objectives have been setup. Please try to run optimization manually from Eclipse. You may be able to fix the cause of the problem there.", ConsoleColor.Red);
                return;
            }

            if (optresult.StructureDVHs == null || optresult.StructureDVHs.Count() == 0)
            {
                ConsoleExt.WriteLineWithBackground("\n\nError: Optimization result has no StructureDVHs. All optimization objectives have been setup. Please try to run optimization manually from Eclipse. You may be able to fix the cause of the problem there.", ConsoleColor.Red);
                return;
            }

            ConsoleExt.WriteLineWithBackground("\nOptimization finished successfully. Print summary. TimeElapsed: " + sw.Elapsed.ToString("hh\\:mm\\:ss"));

            // Print out in the console a summary of optresult.
            var plan_eva_opt = new planDVH_evaluation(cureps, datasource, JSON_set_fraction, optresult, RxCons_used);
            //plan_eva_opt.print(cp, strn_ptv_overlap_dict, if_over_area: true, if_highlight_unmet_prio1_str: true);

            //plan_eva_opt.collapse_1n2_auxiliary_strs();
            Console.WriteLine(pt_cs_pl);
            plan_eva_opt.print(strn_ptv_overlap_dict, if_over_area: true, if_highlight_unmet_prio1_str: true);



            //var not_met_Prio_1_cons = plan_eva_opt.str_evas.SelectMany(str => str.con_evas, (str, con) => new { str, con }).Where(t => t.str.strGEM > 0.5 && t.con.priority == 1).ToList();
            var not_met_Prio_1_cons = plan_eva_opt.str_evas.Where(t => t.strGEM > 0.5 && t.con_evas.Any(c => c.priority == 1)).ToList();

            if (not_met_Prio_1_cons.Count > 0)
            {
                Console.WriteLine("\n--------- after 1st Opt, [{0}] structure(s) have unmet priority 1 constraint(s) -----------", not_met_Prio_1_cons.Count);
            }

            Console.WriteLine("\nFor optimal viewing, please widen the console and reduce font size (right click the console header --> Properties --> Font)");

            curapp.SaveModifications();

            Console.WriteLine("Adjust jaw opening to remove static leave pairs if any.");
            cureps.Beams.ToList().ForEach(t => t.auto_adjust_jaw_position());


            ConsoleExt.WriteLineWithBackground("\n\n\nStarting Dose Calculation ...");
            Console.WriteLine("This will take about 5 minutes; please check back around " + DateTime.Now.AddMinutes(5).ToString("hh:mm:ss tt"));

            try // Dose Calculation
            {
                sw.Restart();
                CalculationResult calcresult = cureps.CalculateDose();
                sw.Stop();

                curapp.SaveModifications();

                ConsoleExt.WriteLineWithBackground("\nDose Calculation finished successfully. Print summary. TimeElapsed: " + sw.Elapsed.ToString("hh\\:mm\\:ss"));

                plan_eva_opt = new planDVH_evaluation(cureps, datasource, JSON_set_fraction, RxCons_used);

                //plan_eva2.collapse_1n2_auxiliary_strs();
                Console.WriteLine(pt_cs_pl);
                plan_eva_opt.print(strn_ptv_overlap_dict, if_over_area: true, if_highlight_unmet_prio1_str: true);

            }
            catch (Exception e)
            {
                ConsoleExt.WriteLineWithBackground("\n\nError: Dose calculation failed due to the following error\n", ConsoleColor.Red);
                Console.WriteLine(e.Message);
                Console.WriteLine("\n\nYou can manually correct this error and manually try dose calculation again in Eclipse ExternalBeamPlanning.\n\n");
                return;
            }


            //ConsoleExt.WriteLineWithBackground("\nPlan normalization set to 101.5%.");
            //cureps.PlanNormalizationValue = 101.5;



            //ConsoleExt.WriteLineWithBackground("\nAdjust the limit of Mean[Gy] and gEUD constraints with priority 0 to those achieved values...");

            //foreach (var Rxcon1 in RxCons_used)
            //{
            //    if (!(Rxcon1.tag == RxCon_types.gEUD || Rxcon1.tag == RxCon_types.Mean_with_fixed_limit)) continue;

            //    var str_eva1 = plan_eva_opt.str_evas.Single(t => t.StructureName == Rxcon1.StructureID);
                
            //    if (Rxcon1.tag == RxCon_types.Mean_with_fixed_limit)
            //    {
            //        //Rxcon1.limit = Math.Round(str_eva1.dvh.Mean_Gy, 2);
            //        //Console.WriteLine($"{Rxcon1.StructureID}; limit assigned Mean[Gy]: {str_eva1.dvh.Mean_Gy}");
            //    }

            //    if (Rxcon1.tag == RxCon_types.gEUD)
            //    {
            //        Rxcon1.limit = Math.Round(str_eva1.gEUD, 2);
            //        //Console.WriteLine($"{Rxcon1.StructureID}; limit assigned gEUD: {str_eva1.gEUD}; a: {Rxcon1.gEUD_a}; Mean[Gy]: {str_eva1.dvh.Mean_Gy}");
            //    }
            //}

            //foreach (var obj in cureps.OptimizationSetup.Objectives) { cureps.OptimizationSetup.RemoveObjective(obj); }

            //RxCons_used.ForEach(t => cureps.Add_OPT_Objective_FromRxConstraint(t));



            curstructset.RemoveStructure(s1);
            curstructset.RemoveStructure(s2);
            curapp.SaveModifications();


            // display summary comparing all structures.
            var PostSet = curstructset.Generate_StructureSetSummary(Lib3_ESAPI.LogPoint.FinalValues);

            var pair_list = new Lib3_ESAPI.StructureInfoSummaryDiff_ListAll(PreSet, PostSet);
            ConsoleExt.WriteLineWithBackground("\n\nSummary of all structures before ---> after AutoPlan");
            msg = "\n" + pt_cs_pl + "\n" + pair_list.ToString("\n  ");
            Console.WriteLine(msg);


            // remind the user to attach bolus manually.
            if (curstructset.Structures.Any(x => x.DicomType == "BOLUS"))
            {
                string bolusnames = string.Join(", ", curstructset.Structures.Where(x => x.DicomType == "BOLUS").Select(s => s.Id));

                MessageBox.Show("Bolus [" + bolusnames + "] found in this StructureSet. Please remember to attach it/them to the beam manually if not have done yet.", msg_header);
            }
        }

        static SegmentVolume Get_HD_SegmentVolume(Structure str)
        {
            if (str.IsHighResolution)
                return str.SegmentVolume;
            else
            {
                s2.SegmentVolume = str.SegmentVolume;
                s2.ConvertToHighResolution();
                return s2.SegmentVolume;
            }
        }

        public static double CalcOverLappingVolume(Structure A, Structure B)
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



    public class OAR_PTV_overlap
    {
        public string shortString
        {
            get
            {
                if (PTV_H_overlap > 0) return string.Format("H_{0:F1}", PTV_H_overlap);
                if (PTV_M_overlap > 0) return string.Format("M_{0:F1}", PTV_M_overlap);
                if (PTV_L_overlap > 0) return string.Format("L_{0:F1}", PTV_L_overlap);
                return "None";
            }
        }

        public override string ToString()
        {
            string rv = "";
            if (PTV_H_overlap > 0) rv = rv + string.Format("H_{0:F1} ", PTV_H_overlap);
            if (PTV_M_overlap > 0) rv = rv + string.Format("M_{0:F1} ", PTV_M_overlap);
            if (PTV_L_overlap > 0) rv = rv + string.Format("L_{0:F1}", PTV_L_overlap);

            return string.Format("{0, -18}", rv);

            //return string.Format("H_{0:F1} M_{1:F1} L_{2:F1}", PTV_H_overlap, PTV_M_overlap, PTV_L_overlap);
        }


        // Here assuming zPTV_High, zPTV_low, zPTV_Mid are not over lapping one other.
        public double H_ol { get { return PTV_H_overlap; } }
        public double HM_ol { get { return PTV_H_overlap + PTV_M_overlap; } }
        public double HML_ol { get { return PTV_H_overlap + PTV_M_overlap + PTV_L_overlap; } }

        public string StructureName;
        public double PTV_H_overlap = 0;
        public double PTV_M_overlap = 0;
        public double PTV_L_overlap = 0;
        public double volume = 0;

        public OAR_PTV_overlap(string StructureName, double volume)
        {
            this.StructureName = StructureName;
            this.volume = volume;
        }
    }


    public class TargetStructureDose
    {
        public string StandardTargetName;
        public string TargetNameInPlan;
        public double DoseToTarget;

        public TargetStructureDose(string stn, string tnip, double dtt)
        {
            StandardTargetName = stn;
            TargetNameInPlan = tnip;
            DoseToTarget = dtt;
        }
    }

    public static class ExtensionMethods
    {
        public static bool is_zOptPTV_xxx_L(this string StructureName)
        {
            //return Regex.Match(StructureName, "zOptPTV_[highlowmid]{3,4}_[Ll](M1)?$", RegexOptions.IgnoreCase).Success;
            return Regex.Match(StructureName, "(z1)?zOptPTV_[highlowmid]{3,4}_[Ll]$", RegexOptions.IgnoreCase).Success;
        }

        public static string shorten_if_long(this string strn, int min_len = 15, int start = 9, int end = 10)
        {
            return strn.Length >= min_len ? strn.Substring(0, start - 1) + strn.Substring(end, strn.Length - end) : strn;
        }

        public static string remove_last_n_char(this string strn, int n = 2)
        {
            return strn.Substring(0, strn.Length - n);
        }
    }
}
