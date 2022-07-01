using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pass.Graph
{
    /// <summary>
    /// Email内容
    /// </summary>
    public class EmailObject
    {
        public Message message { get; set; }
        public string saveToSentItems { get; set; }
    }

    public class Message
    {
        public string subject { get; set; }
        public Body body { get; set; }
        public Torecipient[] toRecipients { get; set; }
    }

    public class Body
    {
        public string contentType { get; set; }
        public string content { get; set; }
    }

    public class Torecipient
    {
        public Emailaddress emailAddress { get; set; }
    }

    public class Emailaddress
    {
        public string address { get; set; }
    }

}
