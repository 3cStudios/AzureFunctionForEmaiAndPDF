using System.Text;
using Microsoft.Extensions.Configuration;

namespace AzureFunctionForEmailAndPDF
{
    public class ContactUsHelper
    {
        public const string ContactUsTemplate = "\r\n<table cellspacing=\"0\" cellpadding=\"0\" width=\"100%\">\r\n    <tr>\r\n        <td width=\"60%\">\r\n            &nbsp;\r\n        </td>\r\n        <td width=\"40%\" style=\"text-align: right;\">\r\n            <table cellspacing=\"0\" cellpadding=\"0\" align=\"right\">\r\n                <tr>\r\n                    <td style=\"font-size:12px;font-family:'lucida grande', tahoma, verdana, arial, sans-serif;color:#000000; font-weight:bold;\">\r\n                        Date:\r\n                    </td>\r\n                    <td style=\"font-size:12px;font-family: 'lucida grande', tahoma, verdana, arial, sans-serif;\">\r\n                        {%DATE%}\r\n                    </td>\r\n                </tr>\r\n            </table>\r\n        </td>\r\n    </tr>\r\n</table>\r\n<br />\r\n<br />\r\n<table cellspacing=\"0\" cellpadding=\"5\" width=\"100%\">\r\n    <tr>\r\n        <td style=\"font-size:12px; font-family: 'lucida grande', tahoma, verdana, arial, sans-serif;color:#000000; font-weight:bold; width:100px\">Reason For Contact:</td>\r\n        <td style=\"font-size:12px; font-family: 'lucida grande', tahoma, verdana, arial, sans-serif;\">{%REASON%}</td>\r\n    </tr>\r\n    <tr>\r\n        <td style=\"font-size:12px;font-family: 'lucida grande', tahoma, verdana, arial, sans-serif;color:#000000; font-weight:bold; width:100px\">Name:</td>\r\n        <td style=\"font-size:12px;font-family: 'lucida grande', tahoma, verdana, arial, sans-serif;\">{%NAME%}</td>\r\n    </tr>\r\n    <tr>\r\n        <td style=\"font-size:12px;font-family: 'lucida grande', tahoma, verdana, arial, sans-serif;color:#000000; font-weight:bold;\">Phone:</td>\r\n        <td style=\"font-size:12px;font-family: 'lucida grande', tahoma, verdana, arial, sans-serif;\">{%PHONE%}</td>\r\n    </tr>\r\n    <tr>\r\n        <td style=\"font-size:12px;font-family: 'lucida grande', tahoma, verdana, arial, sans-serif;color:#000000; font-weight:bold;\">Email Address:</td>\r\n        <td style=\"font-size:12px;font-family: 'lucida grande', tahoma, verdana, arial, sans-serif;\">{%EMAILADDRESS%}</td>\r\n    </tr>\r\n    <tr>\r\n        <td style=\"font-size:12px;font-family: 'lucida grande', tahoma, verdana, arial, sans-serif;color:#000000; font-weight:bold;\">Comment:</td>\r\n        <td style=\"font-size:12px;font-family: 'lucida grande', tahoma, verdana, arial, sans-serif;\">{%COMMENT%}</td>\r\n\r\n    </tr>\r\n</table>\r\n<br />";
        public const string ContactUsTitleSubject = "Corp Website Contact Us";

        public class ContactUsForm
        {
            public string Reason { get; set; }
            public string Comment { get; set; }
            public string Name { get; set; }
            public string EmailAddress { get; set; }
            public string Phone { get; set; }

        }

        public static bool ValidateFormValues(ContactUsForm contactUsForm)
        {
            var sb = new StringBuilder();
            if (string.IsNullOrEmpty(contactUsForm.Reason))
            {
                sb.AppendLine("Reason is required.");
            }
            if (string.IsNullOrEmpty(contactUsForm.Name))
            {
                sb.AppendLine("Name is required.");
            }

            if (string.IsNullOrEmpty(contactUsForm.EmailAddress))
            {
                sb.AppendLine("Email Address is required.");
            }
            if (string.IsNullOrEmpty(contactUsForm.Phone))
            {
                sb.AppendLine("Phone Number is required.");
            }
            return sb.Length <= 0;
        }

        public static bool ValidateConfiguration(IConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration["Email.SendGridApiKey"]))
            {
                throw new Exception("SendGrid API Key is missing");
            }


            if (string.IsNullOrEmpty(configuration["Email.DefaultFrom"]))
            {
                throw new Exception("Email.DefaultFrom is missing");
            }
            
            if (string.IsNullOrEmpty(configuration["Email.DefaultTo"]))
            {
                throw new Exception("Email.DefaultTo");
            }
            return true;
        }

        public static string ProcessTemplateReplacements(ContactUsForm form)
        {
            var messageBody = ContactUsTemplate;
            var localTime = DateTime.Now;
            var estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var estTime = TimeZoneInfo.ConvertTime(localTime, estZone);

            messageBody = messageBody.Replace("{%REASON%}", form.Reason);
            messageBody = messageBody.Replace("{%COMMENT%}", form.Comment);
            messageBody = messageBody.Replace("{%NAME%}", form.Name);
            messageBody = messageBody.Replace("{%EMAILADDRESS%}", form.EmailAddress);
            messageBody = messageBody.Replace("{%PHONE%}", form.Phone);
            messageBody = messageBody.Replace("{%DATE%}", estTime.ToShortDateString());

            var emailBody = Constants.EmailBaseTemplate;
            emailBody = emailBody.Replace("{%TITLELINE%}", ContactUsTitleSubject);
            emailBody = emailBody.Replace("{%FORMCONTENT%}", messageBody);
            return emailBody;
        }
    }
}
