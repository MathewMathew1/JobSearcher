using System.ComponentModel.DataAnnotations.Schema;
using JobSearcher.Account;
using Microsoft.AspNetCore.Mvc;

namespace JobSearcher.Report
{


    public class UserReportSchedule
    {
        public int Id { get; set; }

        public int UserId { get; set; } = default!;
        [ValidTimeZone]
        public string TimeZoneId { get; set; } = default!;

        public bool IsActive { get; set; } = true;
        public UserInDatabase User { get; set; } = default!;

        public ICollection<ReportTime> ReportTimes { get; set; } = new List<ReportTime>();
    }

    public class ReportTime
    {
        public int Id { get; set; }
        public TimeSpan LocalTime { get; set; }

        public int UserReportScheduleId { get; set; }
        public UserReportSchedule UserReportSchedule { get; set; } = default!;
    }

    public class ReportCreateDto
    {
        public TimeSpan LocalTime { get; set; }
    }

    public class UserReportScheduleCreateDto
    {
        [ValidTimeZone]
        public required string TimeZoneId { get; set; }
        public required bool IsActive { get; set; }
    }
    
}