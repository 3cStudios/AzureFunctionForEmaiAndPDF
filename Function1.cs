using CfrpAzureFunction;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace AzureFunctionForEmailAndPDF
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHtmlToPdfHelper _htmlToPdfHelper;

        public Function1(ILogger<Function1> logger,IConfiguration configuration, IHtmlToPdfHelper htmlToPdfHelper)
        {
            _logger = logger;
            _configuration = configuration;
            _htmlToPdfHelper = htmlToPdfHelper;
        }

        [Function("Function1")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var form = JsonConvert.DeserializeObject<ContactUsHelper.ContactUsForm>(body);

            if (form == null)
            {
                return new BadRequestObjectResult("Request body is null");

            }
            if (!ContactUsHelper.ValidateFormValues(form))
            {
                return new BadRequestObjectResult("Form is incomplete");

            }
            if (!ContactUsHelper.ValidateConfiguration(_configuration))
            {
                return new BadRequestObjectResult("Configuration Failure");

            }

            var emailBody = ContactUsHelper.ProcessTemplateReplacements(form);
            const string subject = ContactUsHelper.ContactUsTitleSubject;
            var pdfByteArray = _htmlToPdfHelper.BuildPdf(emailBody);

            var client = new SendGridClient(_configuration["Email.SendGridApiKey"]);
            var from = new EmailAddress(_configuration["Email.DefaultFrom"]);
            var to = new EmailAddress(_configuration["Email.DefaultTo"]);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, emailBody);

            var file = Convert.ToBase64String(pdfByteArray);
            msg.AddAttachment("ExampleHtmlToPdf.pdf", file);

            var response = await client.SendEmailAsync(msg);
            if (response.IsSuccessStatusCode)
            {
                return new OkObjectResult("Mail sent successfully");
            }

            return new BadRequestObjectResult("Mail Failed to Send");
        }
    }
}
