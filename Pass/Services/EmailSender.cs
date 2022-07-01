using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Pass.Services
{
    public class EmailSender : IEmailSender
    {
        private ILogger<EmailSender> _logger;
        private HtmlIo _htmlIo;
        public EmailSender(ILogger<EmailSender> logger, HtmlIo htmlIo)
        {
            _logger = logger;
            _htmlIo = htmlIo;
        }
        //private readonly HtmlIo _htmlIo = new HtmlIo();

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute(subject, message, email);
        }

        public async Task<Task> Execute(string subject, string message, string email)
        {
            var myEmailContext = new Graph.EmailObject();
            var addressList = new Graph.Torecipient[1];
            addressList[0] = new Graph.Torecipient { emailAddress = new Pass.Graph.Emailaddress { address = email } };
            //自定义格式
            //var _htmlIo = new HtmlIo(_logger);
            message = await _htmlIo.GetHtmlTempAsync(message);
            myEmailContext.message = new Pass.Graph.Message
            {
                subject = subject,
                body = new Pass.Graph.Body
                {
                    content = message,
                    contentType = "Html"
                },
                toRecipients = addressList
            };
            myEmailContext.saveToSentItems = "false";
            _logger.LogInformation("EmailSender已发送邮件");
            return Pass.Graph.GraphConstants.SendEmail(myEmailContext);
        }
    }
}