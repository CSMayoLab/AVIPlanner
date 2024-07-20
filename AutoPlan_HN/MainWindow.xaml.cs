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
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using VMS.TPS.Common.Model.API;
using AutoPlan_WES_HN;

namespace AutoPlan_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel, string OK_Button_Content = "", string window_title = "")
        {
            InitializeComponent();

            Title = window_title;

            _viewModel = viewModel;
            _viewModel.ExitRequested += (o, e) => Close();

            DataContext = _viewModel;

            CourseList.ItemsSource = _viewModel.Courses_in_patient;
            PlanList.ItemsSource = _viewModel.Plans_in_course;

            PTV_High.ItemsSource = _viewModel.strs_in_curstrset;
            PTV_Mid.ItemsSource = _viewModel.strs_in_curstrset;
            PTV_Low.ItemsSource = _viewModel.strs_in_curstrset;

            Combo_MachineID.ItemsSource = Config.MachineIDs;

            cons_list.ItemsSource = _viewModel.PD_Rxcons;

            if (OK_Button_Content != "") OK_Button_Name.Content = OK_Button_Content;
        }

     

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            //_viewModel.StartEclipse();
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            //_viewModel.StopEclipse();
        }

        private void PatientIdTextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            _viewModel.UpdatePatientMatches(PatientIdTextBox.Text + e.Text);
            PatientIdTextBox.IsDropDownOpen = true;
        }

        // Happens when user selects an item from the drop down
        private void PatientIdTextBox_OnDropDownClosed(object sender, EventArgs e)
        {
            var patientSummary = PatientIdTextBox.SelectedItem as PatientSummary;
            if (patientSummary != null)
            {
                PatientIdTextBox.Text = patientSummary.Id;
            }

            _viewModel.clear_all_errors_msg();

            _viewModel.OpenPatient();
        }


        private void CourseList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string cs = ((sender as ComboBox).SelectedItem as string);

            _viewModel.SelectedCourse = cs;

            warning_for_overwrite.Visibility = Visibility.Hidden;

            if (cs == _viewModel.hint_create_new_Course)
            {
                new_course_input.Visibility = Visibility.Visible;
            }
            else new_course_input.Visibility = Visibility.Hidden;
        }

        private void PlanList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string pl = ((sender as ComboBox).SelectedItem as string);

            _viewModel.SelectedPlan = pl;

            if_use_existing_beams.IsChecked = false;

            if (pl == _viewModel.hint_create_new_Plan)
            {
                new_plan_input.Visibility = Visibility.Visible;
                warning_for_overwrite.Visibility = Visibility.Hidden;
                if_use_existing_beams.Visibility = Visibility.Hidden;
            }
            else
            {
                new_plan_input.Visibility = Visibility.Hidden;
                if (!string.IsNullOrEmpty(pl))
                {
                    warning_for_overwrite.Visibility = Visibility.Visible;
                    if_use_existing_beams.Visibility = Visibility.Visible;
                }
            }
        }

        private void PatientIdTextBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.StructureSets_in_patient.Clear();
            _viewModel.Courses_in_patient.Clear();
            _viewModel.strs_in_curstrset.Clear();
            _viewModel.Plans_in_course.Clear();
            _viewModel.clear_strSet_n_PTV_guess();

            _viewModel.hd = 0; 
            _viewModel.md = 0; 
            _viewModel.ld = 0;
        }

        private void New_Course_Name_GotFocus(object sender, RoutedEventArgs e)
        {
            New_Course_Name.Text = "";
        }

        private void New_Plan_Name_GotFocus(object sender, RoutedEventArgs e)
        {
            New_Plan_Name.Text = "";
        }

        private void StructureSets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selected = ((sender as ComboBox).SelectedItem as string);

            New_strS_inputs.Visibility = Visibility.Hidden;

            if (string.IsNullOrEmpty(selected)) return;

            if (selected.StartsWith(_viewModel.dup_indent))
            {
                New_strS_inputs.Visibility = Visibility.Visible;
                //_viewModel.NewStructureSet = "AP_" + selected.Split(' ')[0]; -- move to VM
            }

            _viewModel.error_msg_strS = "";
        }

        private void New_Course_Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            //string input = (sender as TextBox).Text;

            //if (!string.IsNullOrEmpty(input) && input != _viewModel.hint_create_new_Course && input[0] != '$')
            //{
            //    _viewModel.error_msg_cs_pl = "CourseID must start with $ sign\n";
            //}
            //else
            //{
            //    _viewModel.error_msg_cs_pl = "";
            //}
        }

        private void New_Plan_Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            string input = (sender as TextBox).Text;

            //if (!string.IsNullOrEmpty(input) && input != _viewModel.hint_create_new_Plan && input[0] != '$')
            //{
            //    _viewModel.error_msg_cs_pl = "PlanID must start with $ sign\n";
            //}
            //else
            //{
            //    _viewModel.error_msg_cs_pl = "";
            //}
        }


        // The reason to place the checks here (instead of _viewModel) is that when the hd_text is not a number, binding will fail, and the input value won't pass to vm.
        private void OK_Button_Name_Click(object sender, RoutedEventArgs e)
        {
            double d = -1;
            Button btn = (sender as Button);

            if (!double.TryParse(hd_text.Text, out d) || d != _viewModel.hd || d <= 0.0)
            {
                _viewModel.error_msg = "PTV_High target dose is not valid";
                btn.Command = null;
                return;
            }

            if (!double.TryParse(md_text.Text, out d) || d != _viewModel.md)
            {
                _viewModel.error_msg = "PTV_Mid target dose is not valid";
                btn.Command = null;
                return;
            }

            if (!double.TryParse(ld_text.Text, out d) || d != _viewModel.ld)
            {
                _viewModel.error_msg = "PTV_Low target dose is not valid";
                btn.Command = null;
                return;
            }

            if (!double.TryParse(RxDose.Text, out d) || d != _viewModel.hd)
            {
                _viewModel.error_msg = "Rx dose is not valid or not equal to PTV_High";
                btn.Command = null;
                return;
            }

            int i= -1;
            if (!int.TryParse(Nfraction.Text, out i) || i != _viewModel.Nf || i <= 0)
            {
                _viewModel.error_msg = "NFraction is not valid positive integer";
                btn.Command = null;
                return;
            }

            btn.Command = _viewModel.RunScriptCommand;
        }

        private void if_use_existing_beams_Checked(object sender, RoutedEventArgs e)
        {
            if_4_beams.Visibility = Visibility.Hidden;
            Label_MachineID.Visibility = Visibility.Hidden;
            Combo_MachineID.Visibility = Visibility.Hidden;
        }

        private void if_use_existing_beams_Unchecked(object sender, RoutedEventArgs e)
        {
            if_4_beams.Visibility = Visibility.Visible;
            Label_MachineID.Visibility = Visibility.Visible;
            Combo_MachineID.Visibility = Visibility.Visible;
        }

        private void Combo_MachineID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selected = ((sender as ComboBox).SelectedItem as string);

            if (selected == "UM-EX4")
            {
                if_jaw_tracking.IsChecked = false;
                if_jaw_tracking.Visibility = Visibility.Hidden;
            } 
            else
            {
                if_jaw_tracking.Visibility = Visibility.Visible;
            }
        }

        //private void Expander_Expanded(object sender, RoutedEventArgs e)
        //{
        //    //_viewModel.Calculate_PTV_OAR_overlap_if_needed();
        //}


        private void cons_list_priority_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
