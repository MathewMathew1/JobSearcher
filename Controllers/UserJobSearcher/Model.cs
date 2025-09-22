using JobSearcher.JobOpening;

namespace JobSearcher.ViewModels
{
    public class SearchSectionViewModel
    {
        public Site Site { get; set; }
        public List<JobOpeningSearcherModel> Searches { get; set; } = new();
        public bool CanAddMore => Searches.Count < 3;
    }
}