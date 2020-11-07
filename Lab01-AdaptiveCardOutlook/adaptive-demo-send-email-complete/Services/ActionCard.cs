using AdaptiveCards;
using FluffySpoon.AspNet.NGrok;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using adaptive_demo.Helpers;

namespace adaptive_demo.Services
{
    public class ActionCard
    {
        private readonly ILogger<ActionCard> _logger;
        private readonly IConfiguration _config;
        private readonly INGrokHostedService _ngrok;

        public ActionCard(ILogger<ActionCard> logger, IConfiguration config, INGrokHostedService ngrok)
        {
            _logger = logger;
            _config = config;
            _ngrok = ngrok;
        }

        public async Task SendActionCardAsync(string messageBody, string cardBody, string userName, string userPassword, params string[] recipientEmails)
        {
            var mergeMessageBody = string.Format(messageBody, cardBody);
            var client = GetAuthenticatedGraphClient(_config, userName, userPassword);

            _logger.LogInformation(mergeMessageBody);
            var listOfRecipients = recipientEmails.Select(toAddress => new Recipient()
            {
                EmailAddress = new EmailAddress() { Address = toAddress }
            });


            if (listOfRecipients != null)
            {
                Message emailMessage = new Message()
                {
                    Subject = "M365 Developer Bootcamp 2020 Feedback Request",
                    ToRecipients = listOfRecipients,
                    Body = new ItemBody()
                    {
                        ContentType = BodyType.Html,
                        Content = mergeMessageBody
                    }
                };

                await client.Me.SendMail(emailMessage, true).Request().PostAsync();
            }

        }    

        private IAuthenticationProvider CreateAuthorizationProvider(IConfiguration config, string userName, SecureString userPassword)
        {
            var clientId = config["applicationId"];
            var authority = $"https://login.microsoftonline.com/{config["tenantId"]}/v2.0";

            List<string> scopes = new List<string>();
            scopes.Add("User.Read");
            scopes.Add("Mail.Send");

            var cca = PublicClientApplicationBuilder.Create(clientId)
                                                    .WithAuthority(authority)
                                                    .Build();
            return MsalAuthenticationProvider.GetInstance(cca, scopes.ToArray(), userName, userPassword);
        }

        private GraphServiceClient GetAuthenticatedGraphClient(IConfiguration config, string userName, string userPassword)
        {
            var authenticationProvider = CreateAuthorizationProvider(config, userName, new NetworkCredential("", userPassword).SecurePassword);
            var graphClient = new GraphServiceClient(authenticationProvider);
            return graphClient;
        }
    }
}
