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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using AnalyticsLibrary2;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace AutoPlan_GUI
{
    namespace Views
    {
        public class Debug_Converter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return "Brainstem";
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return value;
            }
        }

        public class Converter_StructureID_To_Bool : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return true;// leave this convert to be always true.

                string strn = value as string;
                if (new string[] { "OpticNrv_L", "OpticNrv_R", "OpticChiasm", "Brainstem" }.Contains(strn.Match_Std_TitleCase()))
                {
                    return false;
                }

                return true;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return value;
            }
        }

        // priority combobox directly bound by SelectedValue, the following converter is not needed anymore.
        public class combobox_selectedindex_Converter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if ((decimal)value == 1) return 1M;
                if ((decimal)value == 3) return 3M;
                return null;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if ((int)value == 0) return 1;
                if ((int)value == 1) return 3;
                return null;
            }
        }



        public class con_ToString3_justTypeConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                if (value is constraint)
                {
                    var con = (constraint)value;
                    return con.ToString3_justType();
                };

                return null;
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                return null;
            }
        }


        public class ActivePlanSetupConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value == null)
                {
                    return "(No active plan.)";
                }
                else if (value is PlanningItem)
                {
                    return ((PlanningItem)value).Id;
                }

                return "(Not a plan.)";    // This should never happen
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        public class PlanningItemsConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is IEnumerable<PlanningItem>)
                {
                    var planningItems = (IEnumerable<PlanningItem>)value;
                    return string.Join(", ", planningItems.Select(p => p.Id));
                };

                return null;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }

    namespace Data
    {

        public class Settings
        {
            public Settings()
            {
                RecentPatientContexts = new List<PatientContext>();
            }

            public bool ShouldExitAfterScriptEnds { get; set; }
            public List<PatientContext> RecentPatientContexts { get; set; }
        }
    
    public class PlanningItem
        {
            public string Id { get; set; }
            public string CourseId { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as PlanningItem;

                if (other != null)
                {
                    return Id == other.Id && CourseId == other.CourseId;
                }

                return false;
            }

            public static bool operator ==(PlanningItem p1, PlanningItem p2)
            {
                // Handle case where p1 is null (p2 must be null to be equal)
                return ReferenceEquals(p1, null) ? ReferenceEquals(p2, null) : p1.Equals(p2);
            }

            public static bool operator !=(PlanningItem p1, PlanningItem p2)
            {
                return !(p1 == p2);
            }

            // Do not bother with calculating a hashcode,
            // but overriding it stops a compiler warning
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        public class PatientContext
        {
            public PatientContext()
            {
                PlanningItemsInScope = new List<PlanningItem>();
            }

            public string PatientId { get; set; }
            public PlanningItem ActivePlanSetup { get; set; }
            public List<PlanningItem> PlanningItemsInScope { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as PatientContext;

                if (other != null)
                {
                    return PatientId == other.PatientId
                        && ActivePlanSetup == other.ActivePlanSetup
                        && PlanningItemsInScope.SequenceEqual(other.PlanningItemsInScope);
                }

                return false;
            }

            // Do not bother with calculating a hashcode,
            // but overriding it stops a compiler warning
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

    }


    public static class Helper
    {
        public static bool is_name_PTV(this string str_name)
        {
            string s_up = str_name.ToUpper();

            return s_up.Contains("PTV")
                && !s_up.StartsWith("Z")
                && !s_up.Contains("!")
                && !s_up.Contains("EVA")
                && !s_up.Contains("OPT");
        }
    }


    public class PlanningItemViewModel : ViewModelBase
    {
        public PlanningItemViewModel(Course course, PlanningItem planningItem)
        {
            Course = course;
            PlanningItem = planningItem;
        }

        public Course Course { get; private set; }
        public PlanningItem PlanningItem { get; private set; }

        public string Id
        {
            get { return PlanningItem?.Id; }
        }

        public string CourseId
        {
            get { return Course?.Id; }
        }

        public DateTime? CreationDateTime
        {
            get { return PlanningItem?.CreationDateTime; }
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set { Set(ref _isChecked, value); }
        }
    }


    public static class EsapiExtensions
    {
        public static IEnumerable<Tuple<Course, PlanningItem>> GetPlanningItems(this Patient patient)
        {
            var planningItems = new List<Tuple<Course, PlanningItem>>();

            if (patient.Courses != null)
            {
                foreach (var course in patient.Courses)
                {
                    if (course.PlanSetups != null && course.PlanSetups.Count() > 0)
                    {
                        planningItems.AddRange(course.PlanSetups
                            .Select(p => new Tuple<Course, PlanningItem>(course, p)));
                    }

                    if (course.PlanSums != null && course.PlanSums.Count() > 0)
                    {
                        planningItems.AddRange(course.PlanSums
                            .Select(p => new Tuple<Course, PlanningItem>(course, p)));
                    }

                    if((course.PlanSetups == null && course.PlanSums == null) || (course.PlanSetups.Count() == 0 && course.PlanSums.Count() == 0)) 
                    {
                        planningItems.Add(new Tuple<Course, PlanningItem>(course, null));
                    }
                }
            }

            return planningItems;
        }

        public static StructureApprovalStatus GetApprovalStatus_2(this Structure str)
        {
            return str.ApprovalHistory.OrderByDescending(a => a.ApprovalDateTime).First().ApprovalStatus;
        }
    }

    public class SmartSearch
    {
        private const int MaximumResults = 20;

        private readonly IEnumerable<PatientSummary> _patients;

        public SmartSearch(IEnumerable<PatientSummary> patients)
        {
            _patients = patients;
        }

        public IEnumerable<PatientSummary> GetMatches(string searchText)
        {
            return !string.IsNullOrEmpty(searchText)
                ? _patients
                    .Where(p => IsMatch(p, searchText))
                    .OrderByDescending(p => p.CreationDateTime)
                    .Take(MaximumResults)
                : new PatientSummary[0];
        }

        private bool IsMatch(PatientSummary p, string searchText)
        {
            var searchTerms = GetSearchTerms(searchText);

            if (searchTerms.Length == 0)         // Nothing typed
            {
                return false;
            }
            else if (searchTerms.Length == 1)    // One word
            {
                return IsMatch(p.Id, searchTerms[0]) ||
                       IsMatch(p.LastName, searchTerms[0]) ||
                       IsMatch(p.FirstName, searchTerms[0]);
            }
            else                                 // Two or more words
            {
                return IsMatchWithLastThenFirstName(p, searchTerms) ||
                       IsMatchWithFirstThenLastName(p, searchTerms);
            }
        }

        private string[] GetSearchTerms(string searchText)
        {
            // Split by whitespace and remove any separators
            return searchText.Split().Select(t => t.Trim(',', ';')).ToArray();
        }

        private bool IsMatch(string actual, string candidate)
        {
            return actual.ToUpper().Contains(candidate.ToUpper());
        }

        private bool IsMatchWithLastThenFirstName(PatientSummary p, string[] searchTerms)
        {
            return IsMatch(p.LastName, searchTerms[0]) && IsMatch(p.FirstName, searchTerms[1]);
        }

        private bool IsMatchWithFirstThenLastName(PatientSummary p, string[] searchTerms)
        {
            return IsMatch(p.FirstName, searchTerms[0]) && IsMatch(p.LastName, searchTerms[1]);
        }
    }
}
