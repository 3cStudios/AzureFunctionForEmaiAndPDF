namespace CfrpAzureFunction;

public interface IHtmlToPdfHelper
{
    byte[] BuildPdf(string htmlContent);
}