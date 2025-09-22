using JobSearcher.JobOpening;

namespace JobSearcher.UserReport
{
    public class UserReportModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string DataJson { get; set; } = default!;
        public bool SeenByUser { get; set; } = false;
    }

    
    
}
