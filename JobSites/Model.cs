using System.Collections.Concurrent;

namespace JobSearcher.Job
{
    public class SearchedLink
    {
        public List<string> SearchedInDatabase { get; set; } = new();
        public ConcurrentBag<string> NewLinks { get; set; } = new();
    }

}