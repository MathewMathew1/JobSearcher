namespace JobSearch.Utils
{
    public static class LinkHelper
    {
        public static string NormalizeLink(string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;

            try
            {
                var uri = new Uri(url);
                var pathOnly = uri.Scheme + "://" + uri.Host + uri.AbsolutePath;
                return pathOnly.TrimEnd('/').ToLowerInvariant();
            }
            catch
            {
                return url.TrimEnd('/').ToLowerInvariant();
            }
        }
        
        public static string NormalizeIndeedLink(string url)
        {
            if (string.IsNullOrEmpty(url)) return url;

            var uri = new Uri(url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);


            var keptParams = new Dictionary<string, string>();
            foreach (var key in new[] { "ad", "jk", "vjk" })
            {
                if (!string.IsNullOrEmpty(query[key]))
                    keptParams[key] = query[key]!;
            }

            var baseUrl = uri.GetLeftPart(UriPartial.Path);
            if (!keptParams.Any()) return baseUrl;

            var newQuery = string.Join("&", keptParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            return $"{baseUrl}?{newQuery}";
        }

    }



}


