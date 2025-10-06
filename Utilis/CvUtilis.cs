using JobSearcher.Account;
using JobSearcher.Cv;

namespace JobSearch.Utils
{
    public class CvUtils : ICvUtils
    {
        private readonly ICvStorageService _cvStorage;
        private readonly ICvParserService _cvParser;


        public CvUtils(ICvStorageService cvStorage, ICvParserService cvParser)
        {
            _cvStorage = cvStorage;
            _cvParser = cvParser;
        }

        public async Task<string> GetContentOfCvByUserIdAsync(CvInDatabase cv)
        {
            await using var inputStream = await _cvStorage.DownloadCvAsync(cv.AwsS3Key);
            await using var seekableStream = new MemoryStream();
            await inputStream.CopyToAsync(seekableStream);
            seekableStream.Position = 0;

            return _cvParser.ExtractText(seekableStream, cv.Filename);
        }
    }

}
