using CfrpAzureFunction;
using DinkToPdf;
using DinkToPdf.Contracts;

namespace AzureFunctionForEmailAndPDF
{
    public class HtmlToPdfHelper : IHtmlToPdfHelper
    {
        private readonly IConverter _converter;

        public HtmlToPdfHelper(IConverter converter)
        {
            _converter = converter;
        }
        public byte[] BuildPdf(string htmlContent)
        {
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
                },
                Objects = {
                    new ObjectSettings() {
                        HtmlContent = htmlContent,
                    }
                }
            };

            return _converter.Convert(doc);



        }
      
    }

    
}
