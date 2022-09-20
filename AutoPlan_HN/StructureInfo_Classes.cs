using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Lib3_ESAPI
{

    public class StructureInfo
    {
        public string StructureId { get; set; }
        public double Volume { get; set; }
        public bool IsHighResolution { get; set; }
        public string DICOM_type { get; }
        public string ApprovalStatus { get; set; }
        public string ApprovedByUser { get; set; }

        public DateTime DateApprovalStatus { get; set; }
        public DateTime HistoryDateTime { get; set; }


        public string ToString_Name_Volume()
        {
            return string.Format("{0,-12} {1,12}", StructureId, string.Format("({0:F2}cc)", Volume));
        }

        public string ToString_Full()
        {
            return string.Format("{0} {1} {2} [{6}] {3} by {4} at {5}", StructureId, string.Format("({0:F2}cc)", Volume), IsHighResolution ? "HighRes" : "LowRes", ApprovalStatus, ApprovedByUser, DateApprovalStatus, DICOM_type);
            //return string.Format("{0,-12} {1,12} {2} {3} by {4} at {5}", StructureId, string.Format("({0:F2}cc)", Volume), IsHighResolution ? "HighRes" : "LowRes", ApprovalStatus, DateApprovalStatus, ApprovedByUser);
        }

        public StructureInfo(string structureid, double volume, bool ishighresolution, string DICOM_type, DateTime HistoryDateTime, string approvalstatus, string approvedByUser, DateTime dateapprovalstatus)
        {
            StructureId = structureid;
            Volume = volume;
            IsHighResolution = ishighresolution;
            this.DICOM_type = DICOM_type;
            this.HistoryDateTime = HistoryDateTime;
            ApprovalStatus = approvalstatus;
            ApprovedByUser = approvedByUser;
            DateApprovalStatus = dateapprovalstatus;
        }

        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                StructureInfo sif2 = (StructureInfo)obj;
                return (StructureId == sif2.StructureId) &&
                    (Volume == sif2.Volume) &&
                    (IsHighResolution == sif2.IsHighResolution) &&
                    (ApprovalStatus == sif2.ApprovalStatus) &&
                    (DateApprovalStatus == sif2.DateApprovalStatus) &&
                    (ApprovedByUser == sif2.ApprovedByUser) &&
                    (HistoryDateTime == sif2.HistoryDateTime);
            }
        }

        // Do not bother with calculating a hashcode, but overriding it stops a compiler warning
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class StructureInfoSummaryDiff
    {
        public List<StructureInfo> Added { get; set; }
        public List<StructureInfo> Deleted { get; set; }
        public Dictionary<StructureInfo, StructureInfo> Modified { get; set; }

        public StructureInfoSummaryDiff(StructureSetSummary PreSet, StructureSetSummary PostSet)
        {
            if (PreSet.TimeOfLogPoint > PostSet.TimeOfLogPoint) throw new Exception("The time signature of PreSet is after PostSet!");

            var pre_strns = PreSet.PointStructureInfos.Select(t => t.StructureId).ToList();
            var post_strns = PostSet.PointStructureInfos.Select(t => t.StructureId).ToList();

            Added = PostSet.PointStructureInfos.Where(t => !pre_strns.Contains(t.StructureId)).ToList();
            Deleted = PreSet.PointStructureInfos.Where(t => !post_strns.Contains(t.StructureId)).ToList();

            Modified = new Dictionary<StructureInfo, StructureInfo>();
            PostSet.PointStructureInfos.Where(t => pre_strns.Contains(t.StructureId) && !PreSet.PointStructureInfos.Single(s => s.StructureId == t.StructureId).Equals(t)).ToList()
                .ForEach(t => Modified.Add(PreSet.PointStructureInfos.Single(s => s.StructureId == t.StructureId), t));
        }

        public override string ToString()
        {
            string msg = "Added structures: [" + Added.Count + "]\n\n" + string.Join("\n", Added.Select(t => t.ToString_Name_Volume())) +
                         "\n\nDeleted structures: [" + Deleted.Count + "]\n\n" + string.Join("\n", Deleted.Select(t => t.ToString_Name_Volume())) +
                         "\n\nModified structures: [" + Modified.Count + "]\n\n" + string.Join("\n", Modified.Select(t => t.Key.ToString_Name_Volume() + " --> " + t.Value.ToString_Name_Volume()))
                ;
            return msg;
        }
    }

    public class StructureInfoSummaryPair
    {
        public StructureInfoSummaryPair(StructureInfo si1, StructureInfo si2)
        {
            s1 = si1; s2 = si2;
        }
        public StructureInfo s1;
        public StructureInfo s2;
    }

    public class StructureInfoSummaryDiff_ListAll
    {
        public List<StructureInfoSummaryPair> pairs_list { get; set; } = new List<StructureInfoSummaryPair>();

        public StructureInfoSummaryDiff_ListAll(StructureSetSummary PreSet, StructureSetSummary PostSet)
        {
            if (PreSet.TimeOfLogPoint > PostSet.TimeOfLogPoint) throw new Exception("The time signature of PreSet is after PostSet!");

            var pre_strns = PreSet.PointStructureInfos.Select(t => t.StructureId).ToList();
            var post_strns = PostSet.PointStructureInfos.Select(t => t.StructureId).ToList();

            var all_strns = pre_strns.Union(post_strns).Distinct().OrderBy(t => t);

            foreach (string strn in all_strns)
            {
                var si1 = PreSet.PointStructureInfos.SingleOrDefault(t => t.StructureId == strn);
                var si2 = PostSet.PointStructureInfos.SingleOrDefault(t => t.StructureId == strn);

                pairs_list.Add(new StructureInfoSummaryPair(si1, si2));
            }
        }

        public string ToString(string line_breaker = "")
        {
            string msg = "", m1 = "", m2 = "";

            foreach (var pr in pairs_list)
            {
                string strn = pr.s1?.StructureId ?? pr.s2.StructureId;

                if (pr.s1 == null) m1 = strn + " does not exist before"; else m1 = pr.s1.ToString_Full();
                if (pr.s2 == null) m2 = strn + " is deleted"; else m2 = pr.s2.ToString_Full();

                string prefix = "| ";
                if (pr.s1 == null) prefix = "> ";
                else if (pr.s2 == null) prefix = "< ";
                else if (!pr.s1.Equals(pr.s2)) prefix = "* ";

                msg += prefix + m1 + " ---> " + line_breaker + m2 + "\n";
            }

            msg += "\nSymbol annotation: > added; < deleted; * changed; | the same.";

            return msg;
        }
    }

    public class StructureSetSummary
    {
        public string StructureSetID { get; set; }
        public DateTime TimeOfLogPoint { get; set; }
        public LogPoint LogPoint { get; set; }
        public string LogPointComment { get; set; }
        public List<StructureInfo> PointStructureInfos { get; set; }

        private StructureSetSummary(DateTime timeoflogpoint, LogPoint logpoint, string logpointcomment, StructureSet strS)
        {
            StructureSetID = strS.Id;
            TimeOfLogPoint = timeoflogpoint;
            LogPoint = logpoint;
            LogPointComment = logpointcomment;
            PointStructureInfos = new List<StructureInfo>();

            foreach (Structure cs in strS.Structures.OrderBy(t => t.Id))
            {
                StructureApprovalHistoryEntry latest_approval_status = cs.ApprovalHistory.OrderByDescending(a => a.ApprovalDateTime).First();
                
                PointStructureInfos.Add(new StructureInfo(cs.Id, Math.Round(cs.Volume, 4), cs.IsHighResolution,
                    DICOM_type: cs.DicomType,
                    HistoryDateTime: cs.HistoryDateTime,
                    approvalstatus: latest_approval_status.ApprovalStatus.ToString(),
                    approvedByUser: latest_approval_status.UserId,
                    dateapprovalstatus: latest_approval_status.ApprovalDateTime)); ;
            }
        }

        public StructureSetSummary(LogPoint logpoint, string logpointcomment, StructureSet strS)
        : this(DateTime.Now, logpoint, logpointcomment, strS) { }
    }

    public enum LogPoint { StartValues, FinalValues, ApproveAllValues, Other }
}
