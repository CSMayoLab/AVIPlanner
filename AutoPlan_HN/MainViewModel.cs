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

using AutoPlan_HN;
using AutoPlan_WES_HN;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using AnalyticsLibrary2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace AutoPlan_GUI
{
    public class MainViewModel : ViewModelBase
    {
        private const int MaximumNumberOfRecentPatientContexts = 5;

        //private readonly ScriptProxy _scriptProxy;
        //private readonly SettingsRepository _settingsRepo;

        private VMS.TPS.Common.Model.API.Application _app;
        public Patient _patient;

        private IEnumerable<PatientSummary> _allPatientSummaries;
        private SmartSearch _smartSearch;

        public MainViewModel(VMS.TPS.Common.Model.API.Application app, constraint[] planning_directive_cons) 
            : this(app)
        {
            planning_directive_cons.OrderBy(t => t.StructureID).ToList().ForEach(t => PD_cons.Add(t)); // Many need to create a deep copy action here. Let's see.
        }

        public MainViewModel(VMS.TPS.Common.Model.API.Application app)
        {
            OpenPatientCommand = new RelayCommand(OpenPatient);
            RunScriptCommand = new RelayCommand(RunScript);
            ExitCommand = new RelayCommand(Exit);

            LoadSettings();

            _app = app;
            LoadPatientSummaries();
        }

        public List<constraint> PD_cons { get; set; } = new List<constraint> { };
        public List<constraint> PD_cons_in_curStrS { get; set; } = new List<constraint> { };
        public ObservableCollection<RxConstraint> PD_Rxcons { get; set; } = new ObservableCollection<RxConstraint> { };

        public event EventHandler ExitRequested;
        public event EventHandler<string> UserMessaged;

        public ICommand OpenPatientContextCommand { get; private set; }

        public ICommand OpenPatientCommand { get; private set; }
        public ICommand RunScriptCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }

        public List<decimal> priorities { get; set; } = new List<decimal>() { 1,1.5M, 2, 2.5M, 3, 3.5M, 4 };

        private string _patientId;
        public string PatientId
        {
            get { return _patientId; }
            set { Set(ref _patientId, value); }
        }

        private string _MachineID = "BR1";
        public string MachineID
        {
            get { return _MachineID; }
            set { Set(ref _MachineID, value); }
        }

        private IEnumerable<PatientSummary> _patientMatches;
        public IEnumerable<PatientSummary> PatientMatches
        {
            get { return _patientMatches; }
            set { Set(ref _patientMatches, value); }
        }

        private IEnumerable<PlanningItemViewModel> _planningItems;
        public IEnumerable<PlanningItemViewModel> PlanningItems
        {
            get { return _planningItems; }
            private set { Set(ref _planningItems, value); }
        }


        private bool _shouldExit;
        public bool ShouldExit
        {
            get { return _shouldExit; }
            set { Set(ref _shouldExit, value); }
        }

        private bool _if_use_intermediate_dose = true;
        public bool if_use_intermediate_dose
        {
            get { return _if_use_intermediate_dose; }
            set { Set(ref _if_use_intermediate_dose, value); }
        }

        private bool _if_use_existing_beams = false;
        public bool if_use_existing_beams
        {
            get { return _if_use_existing_beams; }
            set { Set(ref _if_use_existing_beams, value); }
        }

        private bool _if_stop_before_OPT = false;
        public bool if_stop_before_OPT
        {
            get { return _if_stop_before_OPT; }
            set { Set(ref _if_stop_before_OPT, value); }
        }

        private bool _if_4_beams = false;
        public bool if_4_beams
        {
            get { return _if_4_beams; }
            set { Set(ref _if_4_beams, value); }
        }

        private bool _if_jaw_tracking = false;
        public bool if_jaw_tracking
        {
            get { return _if_jaw_tracking; }
            set { Set(ref _if_jaw_tracking, value); }
        }


        private string _NewStructureSet = "";
        public string NewStructureSet
        {
            get { return _NewStructureSet; }
            set { Set(ref _NewStructureSet, value); }
        }

        public string actual_strS = "";

        private string _Combobox_SelectedStrS = "";
        public string Combobox_SelectedStrS
        {
            get { return _Combobox_SelectedStrS; }
            set
            {
                Set(ref _Combobox_SelectedStrS, value);

                clear_all_errors_msg();
                hint_select_strS = "";
                Courses_in_patient.Clear();
                clear_strSet_n_PTV_guess();
                actual_strS = "";

                if (string.IsNullOrEmpty(value)) return;

                if (value.StartsWith(dup_indent))
                {
                    actual_strS = value.Substring(dup_indent.Length);
                    NewStructureSet = "AP_" + actual_strS;
                }
                else
                {
                    actual_strS = value;
                    NewStructureSet = value;
                }

                curstructset = _patient.StructureSets.Single(t => t.Id == actual_strS);

                curstructset_strns = curstructset.Structures.Select(t => t.Id).ToList();
                curstructset_strns_std = curstructset_strns.Select(t => t.Match_Std_TitleCase()).ToList();

                if (Config.Structures_trigger_4_beams.Any(t => curstructset_strns_std.Contains(t))) if_4_beams = true;

                guess_PTV_Structure_n_Doses(curstructset);

                PD_cons_in_curStrS = PD_cons.Where(t => curstructset_strns_std.Contains(t.StructureID)).ToList();
                Valid_StrSet_selected = true;

                Regenerate_PD_Rxcons(PD_cons_in_curStrS);

                //PlanningItems.Where(p=> p.PlanningItem.StructureSet.Id == actual_strS)
                PlanningItems.Where(p => p.CourseId.StartsWith("$")).Select(p => p.CourseId).Distinct().ToList().ForEach(c => Courses_in_patient.Add(c));
                Courses_in_patient.Insert(0, hint_create_new_Course);

            }
        }

        List<string> curstructset_strns_std = new List<string>();
        List<string> curstructset_strns = new List<string>();

        private string _selectedCourse = "";
        public string SelectedCourse
        {
            get { return _selectedCourse; }
            set
            {
                SelectedPlan = "";
                Plans_in_course.Clear();
                //PlanningItems.Where(p => p.CourseId == value && p.PlanningItem.StructureSet.Id == actual_strS)
                PlanningItems.Where(p => p.CourseId == value && p.PlanningItem != null).Select(p => p.PlanningItem.Id).ToList().ForEach(c => Plans_in_course.Add(c));
                Plans_in_course.Insert(0, hint_create_new_Plan);
                Set(ref _selectedCourse, value);
            }
        }

        private string _selectedPlan = "";
        public string SelectedPlan
        {
            get { return _selectedPlan; }
            set
            {
                Set(ref _selectedPlan, value);

                if (combobox_CourseSelected != hint_create_new_Course && combobox_PlanSelected != hint_create_new_Plan && !string.IsNullOrEmpty(value) && value != hint_create_new_Plan)
                {
                    var plan = PlanningItems.Single(p => p.Course.Id == SelectedCourse && p.PlanningItem.Id == value).PlanningItem;

                    if (plan.StructureSet.Id != NewStructureSet)
                    {
                        error_msg_cs_pl += $"Selected plan has a different structureSet [{plan.StructureSet.Id}] than specified [{NewStructureSet}]. Please create a new plan or change the structureSet selected above.\n";
                    }
                    else
                    {
                        error_msg_cs_pl = "";
                    }
                }
            }
        }

        private string _combobox_CourseSelected = "";
        public string combobox_CourseSelected
        {
            get { return _combobox_CourseSelected; }
            set { Set(ref _combobox_CourseSelected, value); }
        }

        private string _combobox_PlanSelected = "";
        public string combobox_PlanSelected
        {
            get { return _combobox_PlanSelected; }
            set
            {
                Set(ref _combobox_PlanSelected, value);
            }
        }

        public ObservableCollection<Data.PatientContext> RecentPatientContexts { get; private set; }

        private Data.PatientContext _selectedPatientContext;
        public Data.PatientContext SelectedPatientContext
        {
            get { return _selectedPatientContext; }
            set { Set(ref _selectedPatientContext, value); }
        }

        //[STAThread]
        //public void StartEclipse()
        //{
        //    _app = VMS.TPS.Common.Model.API.Application.CreateApplication();
        //    LoadPatientSummaries();
        //}

        //public void StopEclipse()
        //{
        //    _app.Dispose();
        //}

        public void UpdatePatientMatches(string searchText)
        {
            PatientMatches = _smartSearch.GetMatches(searchText);
        }

        protected virtual void OnExitRequested()
        {
            ExitRequested?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnUserMessaged(string message)
        {
            if (UserMessaged != null)
            {
                UserMessaged(this, message);
            }
        }

        public ObservableCollection<string> StructureSets_in_patient { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> strs_in_curstrset { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> Courses_in_patient { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> Plans_in_course { get; set; } = new ObservableCollection<string>();



        public void OpenPatient()
        {
            _app.ClosePatient();   // Close previous patient, if any

            if (string.IsNullOrEmpty(PatientId))
            {
                error_msg_pat = "Please open a patient first.";
                return;
            }

            _patient = _app.OpenPatientById(PatientId);

            if(_patient == null)
            {
                error_msg_pat = string.Format("Cannot open patient {0}", PatientId);
                OnUserMessaged("The patient \"" + PatientId + "\" was not found.");
                return;
            }

            _patient.BeginModifications();


            if (!_patient.StructureSets.Any()) { error_msg_strS = "There is no structureSet for this patient"; return; }

            StructureSets_in_patient.Clear();

            foreach (StructureSet strS in _patient.StructureSets)
            {
                //if (true && !strS.Id.StartsWith("AP_"))
                if (true)
                {
                    StructureSets_in_patient.Add(strS.Id);
                    StructureSets_in_patient.Add(dup_indent + strS.Id);
                }
            }

            PlanningItems = CreatePlanningItems();

            hint_select_strS = text_hint_select_strS;

            //StructureSets_in_patient.Insert(0, hint_select_strS);

            //if (!_patient.StructureSets.Any(t => t.Id == "AutoPlan"))
            //{
            //    var AP_structureSet = _patient.StructureSets.FirstOrDefault().Copy();
            //    AP_structureSet.Id = "AutoPlan";
            //    _app.SaveModifications();
            //}
        }

        public string dup_indent = " ---> duplicate ";


        private string _hint_select_strS = "";
        public string hint_select_strS
        {
            get { return _hint_select_strS; }
            set { Set(ref _hint_select_strS, value); }
        }

        public string text_hint_select_strS = "<-- Please select or duplicate a StructureSet";
        public string hint_create_new_Course = "create new course";
        public string hint_create_new_Plan = "create new plan";




        private StructureSet curstructset;

        public void clear_strSet_n_PTV_guess()
        {
            Valid_StrSet_selected = false;
            strs_in_curstrset.Clear();
            hd = 0; md = 0; ld = 0; 
            ptv_h = ""; ptv_l = ""; ptv_m = "";
        }

        public void guess_PTV_Structure_n_Doses(StructureSet curstructset)
        {
            if(curstructset == null)
            {
                Debugger.Break();
            }

            strs_in_curstrset.Add(""); // to allow users select "" str to skip ptv_m or ptv_l
            curstructset.Structures.Select(s =>s.Id).ToList().OrderBy(s => s).ToList().ForEach(s => strs_in_curstrset.Add(s));

            Structure ph = curstructset.Structures.FirstOrDefault(s => s.Id.is_name_PTV() && s.Id.ToUpper().Contains("HIGH") && s.Id.ToUpper().Contains("^"));

            Structure pm = curstructset.Structures.FirstOrDefault(s => s.Id.is_name_PTV() && s.Id.ToUpper().Contains("MID") && s.Id.ToUpper().Contains("^"));

            Structure pl = curstructset.Structures.FirstOrDefault(s => s.Id.is_name_PTV() && s.Id.ToUpper().Contains("LOW") && s.Id.ToUpper().Contains("^"));

            if (ph != null)
            {
                ptv_h = ph.Id;
                var split_name = ptv_h.Split('^');
                try { hd = Double.Parse(split_name.Last()); RxDose = hd; Nf = (int)Math.Round(hd / 2); } catch (Exception e) { hd = 0; }
            }
            if (pm != null)
            {
                ptv_m = pm.Id;
                var split_name = ptv_m.Split('^');
                try { md = Double.Parse(split_name.Last()); } catch (Exception e) { md = 0; }
            }
            if (pl != null)
            {
                ptv_l = pl.Id;
                var split_name = ptv_l.Split('^');
                try { ld = Double.Parse(split_name.Last()); } catch (Exception e) { ld = 0; }
            }
        }

        private string _ptv_h; // structure name for PTV High
        public string ptv_h
        {
            get { return _ptv_h; }
            set
            {
                Set(ref _ptv_h, value);
                if (Valid_StrSet_selected == true)
                {
                    Regenerate_PD_Rxcons(PD_cons_in_curStrS);
                }
            }
        }
        private double _hd; // PTV high Dose
        public double hd { get { return _hd; } set { Set(ref _hd, value); } }
        private string _ptv_m;
        public string ptv_m { 
            get { return _ptv_m; } 
            set 
            { 
                Set(ref _ptv_m, value); 
                if (Valid_StrSet_selected == true)
                {
                    Regenerate_PD_Rxcons(PD_cons_in_curStrS);
                }
            } }
        private double _md;
        public double md { get { return _md; } set { Set(ref _md, value); } }
        private string _ptv_l;
        public string ptv_l { 
            get { return _ptv_l; } 
            set
            { 
                Set(ref _ptv_l, value);
                if (Valid_StrSet_selected == true)
                {
                    Regenerate_PD_Rxcons(PD_cons_in_curStrS);
                }
            }
        }
        private double _ld;
        public double ld { get { return _ld; } set { Set(ref _ld, value); } }

        private double _RxDose;
        public double RxDose { get { return _RxDose; } set { Set(ref _RxDose, value); } }

        private int _Nf = 35;
        public int Nf { get { return _Nf; } set { Set(ref _Nf, value); } }


        private IEnumerable<PlanningItemViewModel> CreatePlanningItems()
        {
            // Convert to a List so that the PlanningItems are created immediately;
            // otherwise, there will be problems with listening to PropertyChanged
            return _patient.GetPlanningItems()
                .Select(p => new PlanningItemViewModel(p.Item1, p.Item2)).ToList();
        }



        private string _error_msg = "";
        public string error_msg { get { return _error_msg; } set { Set(ref _error_msg, value); } }

        private string _error_msg_pat = "";
        public string error_msg_pat { get { return _error_msg_pat; } set { Set(ref _error_msg_pat, value); } }

        private string _error_msg_cs_pl = "";
        public string error_msg_cs_pl { get { return _error_msg_cs_pl; } set { Set(ref _error_msg_cs_pl, value); } }

        private string _error_msg_strS = "";
        public string error_msg_strS { get { return _error_msg_strS; } set { Set(ref _error_msg_strS, value); } }


        public bool if_start_AP = false;


        private void check_if_all_info_provided()
        {
            clear_all_errors_msg();
            if (_patient == null)
            {
                error_msg_pat = "Please open one patient first\n"; return;
            }

            if (string.IsNullOrEmpty(actual_strS))
            {
                error_msg_strS = "Please choose a StructureSet to run AutoPlan on\n"; return;
            }

            if (NewStructureSet.Length > 16) { error_msg_strS = "New StructureSet ID is longer than max(16) characters\n"; return; }


            if (NewStructureSet.ToUpper() != Combobox_SelectedStrS.ToUpper() && StructureSets_in_patient.Select(s => s.ToUpper()).Contains(NewStructureSet.ToUpper()))
            {
                error_msg_strS = "The typed-in new StructureSet ID already existed\n"; return;
            }

            if (combobox_CourseSelected == hint_create_new_Course && Courses_in_patient.Select(s => s.ToUpper()).Contains(SelectedCourse.ToUpper()) && SelectedCourse != hint_create_new_Course)
            {
                error_msg_cs_pl = "The typed-in new CourseID already existed in this patient. Please select it from dropdown list.\n"; return;
            }

            if (combobox_PlanSelected == hint_create_new_Plan && Plans_in_course.Select(s => s.ToUpper()).Contains(SelectedPlan.ToUpper()) && SelectedPlan != hint_create_new_Plan)
            {
                error_msg_cs_pl = "The typed-in new PlanID already existed in this Course. Please select it from dropdown list.\n"; return;
            }

            if (string.IsNullOrEmpty(SelectedCourse)) error_msg_cs_pl += "Please select an existing Course or create a new Course\n";
            else
            {
                if (SelectedCourse == hint_create_new_Course) { error_msg_cs_pl += "Please type in a new Course ID\n"; return; }
                //if (SelectedCourse.First() != '$') { error_msg_cs_pl += "CourseID must start with $ sign\n"; }
            }
            if (SelectedCourse.Length > 16) error_msg_cs_pl += "New Course ID is longer than max(16) characters\n";

            if (string.IsNullOrEmpty(SelectedPlan)) error_msg_cs_pl += "Please select an existing Plan or create a new Plan\n";
            else
            {
                if (SelectedPlan == hint_create_new_Plan) { error_msg_cs_pl += "Please type in a new Plan ID\n"; return; }
                //if(SelectedPlan.First() != '$') { error_msg_cs_pl += "PlanID must start with $ sign\n"; }
            }
            if (SelectedPlan.Length > 13) error_msg_cs_pl += "New Plan ID is longer than max(13) characters\n";

            if (combobox_CourseSelected != hint_create_new_Course && combobox_PlanSelected != hint_create_new_Plan && !string.IsNullOrEmpty(SelectedPlan) && SelectedPlan != hint_create_new_Plan)
            {
                var plan = PlanningItems.Single(p => p.Course.Id == SelectedCourse && p.PlanningItem != null && p.PlanningItem.Id == SelectedPlan).PlanningItem;

                if (plan.StructureSet.Id != NewStructureSet)
                {
                    error_msg_cs_pl += $"Selected plan has a different structureSet [{plan.StructureSet.Id}] than specified [{NewStructureSet}]. Please create a new plan or change the structureSet selected above.\n";
                }
            }

            if (string.IsNullOrEmpty(ptv_h)) error_msg += "Must select a structure as PTV_High\n";

            if (curstructset.Structures.Single(t => t.Id == ptv_h).GetApprovalStatus_2() != StructureApprovalStatus.Approved)
            {
                error_msg += $"Selected PTV_High [{ptv_h}] must be approved.\n";
            }
            if (!string.IsNullOrEmpty(ptv_m))
            {
                if (curstructset.Structures.Single(t => t.Id == ptv_m).GetApprovalStatus_2() != StructureApprovalStatus.Approved)
                {
                    error_msg += $"Selected PTV_Mid [{ptv_m}] must be approved.\n";
                }
                if (ptv_m == ptv_h)
                {
                    error_msg += $"Selected PTV_Mid [{ptv_m}] and PTV_High [{ptv_h}] are the same.\n";
                }
            }
            if (!string.IsNullOrEmpty(ptv_l))
            {
                if (curstructset.Structures.Single(t => t.Id == ptv_l).GetApprovalStatus_2() != StructureApprovalStatus.Approved)
                {
                    error_msg += $"Selected PTV_Low [{ptv_l}] must be approved.\n";
                }
                if (ptv_l == ptv_h)
                {
                    error_msg += $"Selected PTV_Low [{ptv_l}] and PTV_High [{ptv_h}] are the same.\n";
                }
                if (ptv_l == ptv_m)
                {
                    error_msg += $"Selected PTV_Low [{ptv_l}] and PTV_Mid [{ptv_m}] are the same.\n";
                }
            }

            if (!(hd > 0 && hd < 150)) error_msg += "PTV_High Target Dose not in proper range (0, 150]\n";
            if (!string.IsNullOrEmpty(ptv_m) && !(md > 0 && md < 150)) error_msg += "PTV_Mid Target Dose not in proper range (0, 150]\n";
            if (!string.IsNullOrEmpty(ptv_l) && !(ld > 0 && ld < 150)) error_msg += "PTV_Low Target Dose not in proper range (0, 150]\n";

            if (! (Nf>0 && Nf <100)) error_msg += "Number of Fractions is not in proper range [1, 100]\n";
            if (!string.IsNullOrEmpty(ptv_m) && hd < md) error_msg += "PTV_High dose (" + hd + "Gy) cannot be lower than PTV_Mid dose (" + md + "Gy)\n";
            if (!string.IsNullOrEmpty(ptv_l) && hd < ld) error_msg += "PTV_High dose (" + hd + "Gy) cannot be lower than PTV_Low dose (" + ld + "Gy)\n";
            if (!string.IsNullOrEmpty(ptv_m) && !string.IsNullOrEmpty(ptv_l) && md < ld) error_msg += "PTV_Mid dose (" + md + "Gy) cannot be lower than PTV_Low dose (" + ld + "Gy)\n";
        }

        private bool is_all_errors_clear()
        {
            return error_msg == "" && error_msg_cs_pl == "" && error_msg_pat == "" && error_msg_strS == "";
        }

        public void clear_all_errors_msg()
        {
            error_msg = "";
            error_msg_cs_pl = "";
            error_msg_pat = "";
            error_msg_strS = "";
        }

        private void RunScript()
        {
            check_if_all_info_provided();
            if (!is_all_errors_clear()) return;

            if_start_AP = true;
            OnExitRequested();  // when there is no error, calling this function raise event ExitRequested(this, EventArgs.Empty) to close the GUI Window, and proceed to AutoPlan with parameters assigned in the GUI.
        }

        //public ObservableCollection<RxConstraint> Regenerate_PD_Rxcons(List<constraint> PD_cons_in_curStrS)
        public void Regenerate_PD_Rxcons(List<constraint> PD_cons_in_curStrS)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            PD_Rxcons.Clear();
            List<RxConstraint> PD_Rxcons_List = PD_cons_in_curStrS.Select(t => new RxConstraint(t)).ToList();

            Calculate_PTV_OAR_overlap(PD_Rxcons_List);

            PD_Rxcons_List.ForEach(t => PD_Rxcons.Add(t));

            Mouse.OverrideCursor = Cursors.Arrow;
        }


        public void Calculate_PTV_OAR_overlap(List<RxConstraint> PD_Rxcons)
        {
            var OverlapTester = new StrsOverlappingTester(curstructset);

            var strs = curstructset.Structures.Where(t => !t.IsEmpty && PD_Rxcons.Select(c => c.StructureID).Contains(t.Id.Match_Std_TitleCase())).OrderBy(t => t.Id);

            foreach (Structure str in strs)
            {
                var opol = new OAR_PTV_overlap(StructureName: str.Id, volume: str.Volume);

                if (!string.IsNullOrEmpty(ptv_h))
                {
                    Structure original_ptv_h = curstructset.Structures.Single(t => t.Id == ptv_h);
                    opol.PTV_H_overlap = OverlapTester.CalcOverLappingVolume(str, original_ptv_h);
                }

                if (!string.IsNullOrEmpty(ptv_m))
                {
                    Structure original_ptv_m = curstructset.Structures.Single(t => t.Id == ptv_m);
                    opol.PTV_M_overlap = OverlapTester.CalcOverLappingVolume(str, original_ptv_m);
                }

                if (!string.IsNullOrEmpty(ptv_l))
                {
                    Structure original_ptv_l = curstructset.Structures.Single(t => t.Id == ptv_l);
                    opol.PTV_L_overlap = OverlapTester.CalcOverLappingVolume(str, original_ptv_l);
                }

                PD_Rxcons.Where(t => t.StructureID == str.Id.Match_Std_TitleCase()).ToList().
                    ForEach(
                    Rxc => {
                        Rxc.opol = opol;
                        //Console.WriteLine($"{str.Id} opol {opol}");
                        decimal decimal_priority_adjustment = Priority_mapping.Adjustment_of_PD_decimal_priority(opol);
                        if (decimal_priority_adjustment > 0) Rxc.priority_decimal = decimal_priority_adjustment;
                    });
            }
        }

        public bool Valid_StrSet_selected = false;
        public bool extra_constraints_added = false;



        private IEnumerable<PlanSum> GetPlanSumsInScope()
        {
            return PlanningItems
                .Where(p => p.PlanningItem != null && p.PlanningItem is PlanSum && p.IsChecked)
                .Select(p => p.PlanningItem)
                .Cast<PlanSum>();
        }

        private void Exit()
        {
            WriteSettings();
            OnExitRequested();
            //System.Windows.Application.disp
            //Environment.Exit(0);
        }

     
        private Data.PlanningItem MapPlanningItemViewModelToData(
            PlanningItemViewModel planningItemVm)
        {
            if (planningItemVm == null)
            {
                return null;
            }

            return new Data.PlanningItem
            {
                Id = planningItemVm.Id,
                CourseId = planningItemVm.CourseId
            };
        }

        private void LoadSettings()
        {
            //var settings = _settingsRepo.ReadSettings();

            //if (settings != null)
            //{
            //    ShouldExit = settings.ShouldExitAfterScriptEnds;
            //    RecentPatientContexts =
            //        new ObservableCollection<Data.PatientContext>(settings.RecentPatientContexts);
            //}
            //else
            //{
            //    RecentPatientContexts = new ObservableCollection<Data.PatientContext>();
            //}
        }

        private void LoadPatientSummaries()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                _allPatientSummaries = _app.PatientSummaries.ToArray();
                _smartSearch = new SmartSearch(_allPatientSummaries);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void WriteSettings()
        {
            //var settings = new Data.Settings();

            //settings.ShouldExitAfterScriptEnds = ShouldExit;
            //settings.RecentPatientContexts = RecentPatientContexts.ToList();

            //_settingsRepo.WriteSettings(settings);
        }
    }


}
