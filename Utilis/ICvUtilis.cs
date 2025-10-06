using JobSearcher.Account;
using JobSearcher.Cv;

namespace JobSearch.Utils
{
    public interface ICvUtils
    {
        Task<string> GetContentOfCvByUserIdAsync(CvInDatabase cv);
    }
}