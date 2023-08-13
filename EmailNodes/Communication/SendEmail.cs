﻿
using MailKit.Net.Smtp;
using MimeKit;
using Scriban;

namespace FileFlows.Communication
{
    public class SendEmail:Node
    {
        public override int Inputs => 1;
        public override int Outputs => 2;
        public override string Icon => "fas fa-envelope";

        public override FlowElementType Type => FlowElementType.Communication;
        public override bool FailureNode => true;

        [StringArray(1)]
        public string[] Recipients { get; set; }

        [TextVariable(2)]
        public string Subject { get; set; }

        [TextArea(3)]
        public string Body { get; set; }

        public override int Execute(NodeParameters args)
        {
            try
            {
                var settings = args.GetPluginSettings<PluginSettings>();

                if (string.IsNullOrEmpty(settings?.SmtpServer))
                {
                    args.Logger?.ELog(
                        "No SMTP Server configured, configure this under the 'Plugins > Email Nodes > Edit' page.");
                    return -1;
                }

                args.Logger?.ILog($"Got SMTP Server: {settings.SmtpServer} [{settings.SmtpPort}]");

                string body = RenderBody(args);

                string sender = settings.Sender ?? "fileflows@" + Environment.MachineName;
                string subject = args.ReplaceVariables(this.Subject ?? String.Empty)?.EmptyAsNull() ??
                                 "Email from FileFlows";

                SendMailKit(args, settings, sender, subject, body);

                //SendDotNet(args, settings, sender, subject, body);

                return 1;
            }
            catch (Exception ex)
            {
                args.Logger?.WLog("Error sending message: " + ex.Message);
                return 2;
            }
        }

        internal string RenderBody(NodeParameters args)
        {
            if (string.IsNullOrEmpty(this.Body))
                return string.Empty;

            string body = this.Body;
            var dict = new Dictionary<string, object>();
            foreach(string key in args.Variables.Keys)
            {
                string newKey = key.Replace(".", "");
                body = body.Replace(key, newKey);
                if (dict.ContainsKey(newKey) == false)
                    dict.Add(newKey, args.Variables[key]);
            }
            var template = Template.Parse(body);
            string result = template.Render(dict).Trim();
            return result;
        }

        private void SendDotNet(NodeParameters args, PluginSettings settings, string sender, string subject, string body)
        {
            args.Logger?.ILog($"Send using .NET internal mail library");
            System.Net.Mail.MailMessage message = new ();
            message.From = new System.Net.Mail.MailAddress(sender);
            foreach (var recipient in Recipients)
                message.To.Add(recipient);
            message.Subject = subject;
            message.Body = args.ReplaceVariables(body);


            System.Net.Mail.SmtpClient smtp = new ();
            smtp.Port = settings.SmtpPort;
            smtp.Host = settings.SmtpServer;
            if (string.IsNullOrEmpty(settings.SmtpUsername) == false)
            {
                args.Logger?.ILog("Sending using credientials");
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential(settings.SmtpUsername, settings.SmtpPassword);
                //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            }
            args.Logger?.ILog("About to send email");
            smtp.Send(message);
            args.Logger?.ILog("Email sent!");
        }

        private void SendMailKit(NodeParameters args, PluginSettings settings, string sender, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(sender, sender));
            foreach (var recipient in Recipients)
                message.To.Add(new MailboxAddress(recipient, recipient));
            message.Subject = subject;
            message.Body = new TextPart("plain")
            {
                Text = body
            };

            args.Logger?.ILog($"About to construct SmtpClient");
            using (var client = new SmtpClient())
            {
                args.Logger?.ILog($"Connecting to SMTP Server: {settings.SmtpServer}:{settings.SmtpPort}");
                client.Connect(settings.SmtpServer, settings.SmtpPort);

                if (string.IsNullOrEmpty(settings.SmtpUsername) == false)
                {
                    args.Logger?.ILog("Sending using credientials");
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(settings.SmtpUsername, settings.SmtpPassword);
                }
                args.Logger?.ILog($"About to send message");
                client.Send(message);
                args.Logger?.ILog($"Message sent");
                client.Disconnect(true);
            }
        }
    }
}
