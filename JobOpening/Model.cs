using System.ComponentModel.DataAnnotations;
using JobSearcher.Account;

namespace JobSearcher.JobOpening
{
    public enum Site
    {
        GlassDoor,
        Indeed
    }

    public class JobOpeningSearcherModel
    {
        public int Id { get; set; }
        public required int UserId { get; set; }
        public required Site Site { get; set; }
        [StringLength(100)]
        public required string Location { get; set; }
        [StringLength(100)]
        public required string JobSearched { get; set; }
        public required bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class SearchInfo()
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Only letters, numbers and spaces allowed.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "JobSearched must be between 3 and 100 characters.")]
        public required string JobSearched { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Only letters, numbers and spaces allowed.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "JobSearched must be between 3 and 100 characters.")]
        public required string Location { get; set; }

        [Required]
        public required Site Site { get; set; }

        public bool IsActive { get; set; } = false;
    }

    
}