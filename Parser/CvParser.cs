using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UglyToad.PdfPig;

namespace JobSearcher.Cv
{
    public class CvParserService : ICvParserService
    {
        public string ExtractText(Stream fileStream, string fileName)
        {
            if (fileStream == null || string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Invalid file stream or file name.");

            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".pdf" => ParsePdf(fileStream),
                ".docx" => ParseWordDocx(fileStream),
                _ => throw new NotSupportedException("Only PDF and DOCX files are supported.")
            };
        }


        private string ParsePdf(Stream pdfStream)
        {
            using var pdf = PdfDocument.Open(pdfStream);
            return string.Join("\n", pdf.GetPages().Select(p => p.Text));
        }


        private string ParseWordDocx(Stream docxStream)
        {
            using var doc = WordprocessingDocument.Open(docxStream, false);
            var body = doc.MainDocumentPart?.Document.Body;
            if (body == null) return string.Empty;

            return string.Join("\n", body.Descendants<Text>().Select(t => t.Text));
        }
    }
}
