namespace JobSearcher.JobOpening
{
    public interface IJobOpeningSearcher
    {
        Task<JobOpeningSearcherModel> CreateJobOpeningSearch(SearchInfo searchInfo, int UserId);
        Task<JobOpeningSearcherModel> UpdateJobOpeningSearch(SearchInfo searchInfo, int id, int UserId);
        Task<List<JobOpeningSearcherModel>> GetSearchesByUser(int UserId);
        Task<List<JobOpeningSearcherModel>> GetSearchesByUserAndSite(int UserId, Site site);
        Task DeleteSearchById(int id, int UserId);
    }
}