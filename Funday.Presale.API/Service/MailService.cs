using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Funday.Presale.API.Service.Interface;
using Funday.Presale.API.Configure;

namespace Funday.Presale.API.Service
{
    public class MailService : IMail
    {
        private string smtpServer;
        private int smtpPort;
        private string mailAccount;
        private string mailPwd;
        private string isMailTest;

        public bool sendStatus = true;

        private readonly ILogger<IMail> _logger;

        public MailService(ILogger<IMail> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 送信件
        /// </summary>
        /// <param name="message">主旨與內容</param>
        /// <param name="mailTo">收件者(可用 ; 分隔)</param>
        /// <param name="mailFrom">寄件者(預設空值會用系統的)</param>
        /// <returns></returns>
        public async Task<bool> SendAsync(MimeMessage message, string mailTo, string mailFrom = "")
        {
            string realMail = mailTo;//測試

            smtpServer = ConfigHelper.GetAppConfig<string>("SMTP:smtpServer");
            smtpPort = ConfigHelper.GetAppConfig<int>("SMTP:smtpPort");
            mailAccount = ConfigHelper.GetAppConfig<string>("SMTP:mailAccount");
            mailPwd = ConfigHelper.GetAppConfig<string>("SMTP:mailPwd");
            mailFrom = string.IsNullOrWhiteSpace(mailFrom) ? ConfigHelper.GetAppConfig<string>("SMTP:mailFrom") : mailFrom;
            mailTo = string.IsNullOrWhiteSpace(mailTo) ? ConfigHelper.GetAppConfig<string>("SMTP:TestMailTo") : mailTo;

            isMailTest = ConfigHelper.GetAppConfig<string>("SMTP:IsTest"); //TEST
            if (isMailTest == "Y")
            {
                mailTo = ConfigHelper.GetAppConfig<string>("SMTP:TestMailTo"); //TEST

                message.Subject += $"  ({realMail})";//測試
            }

            string host = smtpServer;
            int port = smtpPort;
            bool useSsl = false;
            string from_username = mailAccount;
            string from_password = mailPwd;
            string from_name = mailFrom;
            string from_address = mailFrom;

            string[] mailToList = mailTo.Split(';');

            List<MailboxAddress> address = new List<MailboxAddress>();
            foreach (string mt in mailToList)
            {
                address.Add(new MailboxAddress(mt, mt));
            }

            message.From.Add(new MailboxAddress(from_name, from_address));
            message.To.AddRange(address);

            using var client = new SmtpClient
            {
                ServerCertificateValidationCallback = (s, c, h, e) => true
            };
            client.AuthenticationMechanisms.Remove("XOAUTH2");

            try
            {
                await client.ConnectAsync(host, port, useSsl);
            }
            catch (SmtpProtocolException ex)
            {
                //Console.WriteLine("Protocol error while trying to connect: {0}", ex.Message);
                sendStatus = false;
                _logger.LogError(ex, "Protocol error while trying to connect!");                               
            }

            if (client.Capabilities.HasFlag(SmtpCapabilities.Authentication))
            {
                try
                {
                    await client.AuthenticateAsync(from_username, from_password);                    
                }
                catch (AuthenticationException ex)
                {
                    //Console.WriteLine ("Invalid user name or password.");
                }catch(SmtpCommandException ex)
                {
                    //Console.WriteLine("Error trying to authenticate: {0}", ex.Message);
                    //Console.WriteLine("\tStatusCode: {0}", ex.StatusCode);
                }
                catch (SmtpProtocolException ex)
                {
                    //Console.WriteLine("Protocol error while trying to authenticate: {0}", ex.Message);
                }
            }


            
            try
            {
                await client.SendAsync(message);
            }
            catch (SmtpCommandException ex)
            {
                Console.WriteLine("Error sending message: {0}", ex.Message);
                Console.WriteLine("\tStatusCode: {0}", ex.StatusCode);

                switch (ex.ErrorCode)
                {
                    case SmtpErrorCode.RecipientNotAccepted:
                        Console.WriteLine("\tRecipient not accepted: {0}", ex.Mailbox);
                        break;
                    case SmtpErrorCode.SenderNotAccepted:
                        Console.WriteLine("\tSender not accepted: {0}", ex.Mailbox);
                        break;
                    case SmtpErrorCode.MessageNotAccepted:
                        Console.WriteLine("\tMessage not accepted.");
                        break;
                }
            }
            catch (SmtpProtocolException ex)
            {
                Console.WriteLine("Protocol error while sending message: {0}", ex.Message);
            }
            
            await client.DisconnectAsync(true);

            return sendStatus;
        }
    }
}
