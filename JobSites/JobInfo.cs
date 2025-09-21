namespace JobSearcher.Job
{
    public class JobInfo
    {
        public required string Link { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public string? ImageLink { get; set; }
    }

    public class GlassDoorJobSearch
    {
        public required string JobSearched { get; set; }
        public required string PlaceToLookFor { get; set; }
    }
}