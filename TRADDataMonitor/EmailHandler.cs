using System;
using System.Collections.Generic;
using System.Text;
// added
using System.Net.Sockets;
using System.IO;
using System.Net.Mail;
using System.Net;
using MailKit.Net.Imap;
using System.Threading;
using MailKit;
using MailKit.Search;

namespace TRADDataMonitor
{
    public class EmailHandler
    {
        string recipientEmailAddress, senderEmailAddress, senderEmailPassword, senderEmailSmtpAddress, senderEmailSmtpPort;
        public EmailHandler(string recipientEmailAddress, string senderEmailAddress, string senderEmailPassword, string senderSmtpAddress, string senderSmptPort)
        {
            this.recipientEmailAddress = recipientEmailAddress;
            this.senderEmailAddress = senderEmailAddress;
            this.senderEmailPassword = senderEmailPassword;
            this.senderEmailSmtpAddress = senderSmtpAddress;
            this.senderEmailSmtpPort = senderSmptPort;
        }

        // Method to validate the email formatting (code from https://stackoverflow.com/questions/1365407/c-sharp-code-to-validate-email-address)
        public bool IsValidEmail(string email)
        {
            bool isValid = false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                isValid = false;
                return isValid;
            }
        }

        // Method to validate the SMTP address and port number(code from https://stackoverflow.com/questions/955431/how-to-validate-smtp-server)
        public bool IsValidSmtp(string hostAddress, int portNumber)
        {
            bool isValid = false;
            try
            {
                TcpClient smtpTest = new TcpClient();
                smtpTest.Connect(hostAddress, portNumber);
                if (smtpTest.Connected)
                {
                    NetworkStream ns = smtpTest.GetStream();
                    StreamReader sr = new StreamReader(ns);
                    if (sr.ReadLine().Contains("220"))
                    {
                        isValid = true;
                    }
                    smtpTest.Close();
                }
            }
            catch
            {
                isValid = false;
                return isValid;
            }
            return isValid;
        }

        // method that sends an email
        public void SendEmailAlert(double minThresh, double maxThresh, string hubName, string sensor, int portID, double val, string emailType)
        {
            string subject;
            string message;

            if (emailType == "test")
            {
                subject = "TEST ALERT";
                message = $"ALERT: This is a test alert. If you are recieving this then the email alert system is configured correctly";
            }
            else if (emailType == "broken")
            {
                subject = "THRESHOLD BROKEN ALERT";
                message = $"ALERT: Data from the {sensor} sensor connected to port {portID} on hub {hubName} exited the allowable range of {minThresh} to {maxThresh} with a value of {val}.";
            }
            else
            {
                subject = "THRESHOLD FIXED ALERT";
                message = $"ALERT: Data from the {sensor} sensor connected to port {portID} on hub {hubName} re-entered the allowable range of {minThresh} to {maxThresh} with a value of {val}.";
            }

            SmtpClient smtp01 = new SmtpClient(senderEmailSmtpAddress, Convert.ToInt32(senderEmailSmtpPort));
            NetworkCredential netCred = new NetworkCredential(senderEmailAddress, senderEmailPassword);

            System.Net.Mime.ContentType mimeType = new System.Net.Mime.ContentType("text/html");
            smtp01.Credentials = netCred;
            smtp01.EnableSsl = true;
            MailMessage msg = new MailMessage(senderEmailAddress, recipientEmailAddress, subject, message);
            smtp01.Send(msg);
        }

        // Overloaded email method for VOC and CO2
        public void SendEmailAlert(double minThresh, double maxThresh, string sensor, double val, string emailType)
        {
            string subject;
            string message;

            if (emailType == "fixed")
            {
                subject = "THRESHOLD BROKEN ALERT";
                message = $"ALERT: Data from the {sensor} sensor exited the allowable range of {minThresh} to {maxThresh} with a value of {val}.";
            }
            else
            {
                subject = "THRESHOLD FIXED ALERT";
                message = $"ALERT: Data from the {sensor} sensor re-entered the allowable range of {minThresh} to {maxThresh} with a value of {val}.";
            }

            SmtpClient smtp01 = new SmtpClient(senderEmailSmtpAddress, Convert.ToInt32(senderEmailSmtpPort));
            NetworkCredential netCred = new NetworkCredential(senderEmailAddress, senderEmailPassword);

            System.Net.Mime.ContentType mimeType = new System.Net.Mime.ContentType("text/html");
            smtp01.Credentials = netCred;
            smtp01.EnableSsl = true;
            MailMessage msg = new MailMessage(senderEmailAddress, recipientEmailAddress, subject, message);
            smtp01.Send(msg);
        }

        // Overloaded email method for GPS
        public void SendEmailAlert(double distanceThreshold, string sensor, double lat, double lng, double val)
        {
            string subject;
            string message;

            subject = "THRESHOLD BROKEN ALERT";
            message = $"ALERT: Data from the {sensor} sensor exited the allowable radius of {distanceThreshold} km with a value of {val} km. The current latitude and longitiude values are: {lat}, {lng}.";

            SmtpClient smtp01 = new SmtpClient(senderEmailSmtpAddress, Convert.ToInt32(senderEmailSmtpPort));
            NetworkCredential netCred = new NetworkCredential(senderEmailAddress, senderEmailPassword);

            System.Net.Mime.ContentType mimeType = new System.Net.Mime.ContentType("text/html");
            smtp01.Credentials = netCred;
            smtp01.EnableSsl = true;
            MailMessage msg = new MailMessage(senderEmailAddress, recipientEmailAddress, subject, message);
            smtp01.Send(msg);
        }

        // Function for retrieving email replies to the alerts (code from: https://stackoverflow.com/questions/545724/using-c-sharp-net-libraries-to-check-for-imap-messages-from-gmail-servers)
        public string RetrieveEmailReply(DateTime alertSent, string alertSubject)
        {
            try
            {
                string ret = "";

                using (var client = new ImapClient())
                {
                    using (var cancel = new CancellationTokenSource())
                    {
                        client.Connect("imap.gmail.com", 993, true, cancel.Token);

                        client.Authenticate(this.senderEmailAddress, this.senderEmailPassword, cancel.Token);

                        var inbox = client.Inbox;
                        inbox.Open(FolderAccess.ReadOnly, cancel.Token);

                        // download each message based on the message index
                        for (int i = 0; i < inbox.Count; i++)
                        {
                            var message = inbox.GetMessage(i, cancel.Token);
                            Console.WriteLine("Subject: {0}", message.Subject);
                        }

                        // search for the reply 
                        var query = SearchQuery.DeliveredAfter(alertSent)
                            .And(SearchQuery.SubjectContains(alertSubject));

                        foreach (var uid in inbox.Search(query, cancel.Token))
                        {
                            var message = inbox.GetMessage(uid, cancel.Token);
                            ret = message.Body.ToString();
                        }

                        client.Disconnect(true, cancel.Token);

                        return ret;
                    }
                }
            }
            catch (Exception ex)
            {
                string exception = ex.Message;
                throw;
            }
        }
    }
}
