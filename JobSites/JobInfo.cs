using System.ComponentModel.DataAnnotations;

namespace JobSearcher.Job
{
    public class JobInfo
    {
        public required string Link { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public string? ImageLink { get; set; }
    }

    public class JobSearchModel
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Only letters, numbers and spaces allowed.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "JobSearched must be between 3 and 100 characters.")]
        public required string JobSearched { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Only letters, numbers and spaces allowed.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "JobSearched must be between 3 and 100 characters.")]
        public required string Location { get; set; }
    }
}