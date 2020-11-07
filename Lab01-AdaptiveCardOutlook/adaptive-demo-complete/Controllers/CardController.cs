using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AdaptiveCards.Templating;
using FluffySpoon.AspNet.NGrok;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.O365.ActionableMessages.Utilities;
using Newtonsoft.Json.Linq;
using adaptive_demo.Models;

namespace adaptive_demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardController : ControllerBase
    {
        private readonly INGrokHostedService _ngrok;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CardController> _logger;
        public CardController(ILogger<CardController> logger, INGrokHostedService ngrok, IWebHostEnvironment env)
        {
            _ngrok = ngrok;
            _env = env;
            _logger = logger;
        }
        [HttpPost]
        
        public async Task<IActionResult> Post([FromBody]dynamic value, [FromHeader] string authorization)
        {
            var tunnel = await _ngrok.GetTunnelsAsync();
            var actionUrl = tunnel.Where(i => i.Proto == "https").Select(i => i.PublicUrl).FirstOrDefault();

            // Authentication 
            if (!AuthenticationHeaderValue.TryParse(authorization, out var headerValue))
            {
                return Unauthorized("Please authenticate");
            }

            var scheme = headerValue.Scheme;
            var parameter = headerValue.Parameter;


            if (!string.Equals(scheme,"bearer", StringComparison.OrdinalIgnoreCase) || 
                string.IsNullOrEmpty(parameter))
            {
                return Unauthorized("Incorrect authentication scheme");
            }

            var validator = new ActionableMessageTokenValidator();

            ActionableMessageTokenValidationResult result = await validator.ValidateTokenAsync(parameter, actionUrl);
            if(!result.ValidationSucceeded)
            {
                if (result.Exception != null)
                {
                    _logger.LogError(result.Exception.ToString());
                }

                return Unauthorized("Invalid authorization code");
            }

            var allowListDomain = new List<string> { "onmicrosoft.com", "libinuko.com" };

            if( allowListDomain.Where( i => result.Sender.EndsWith(i,StringComparison.InvariantCultureIgnoreCase)).Count() <= 0)
            {
                Response.Headers.Add("CARD-ACTION-STATUS", "Invalid sender or the action performer is not allowed.");               
                return Forbid("Invalid sender");
            }

            // Process valid request           
            JObject jObject = JObject.Parse(value.ToString());
            var feedbackResponse = jObject.ToObject<FeedbackModel>();

            // Fake feedback
            string fakeFeedbackPath = Path.Combine(_env.ContentRootPath, "assets", "fake-feedback.json");
            var fakeFeedback = JsonSerializer.Deserialize<List<FeedbackItem>>(
                    await System.IO.File.ReadAllTextAsync(fakeFeedbackPath, Encoding.UTF8)
                );

            fakeFeedback.Add(new FeedbackItem
            {
                name = result.Sender,
                comment = feedbackResponse.Comment,
                rating = feedbackResponse.Rating
            });

            await System.IO.File.WriteAllTextAsync(fakeFeedbackPath,JsonSerializer.Serialize(fakeFeedback));

            var feedbackSummary = new
            {
                average_rating = fakeFeedback.Average(i => i.rating),
                feedback = fakeFeedback,
                total_responses = fakeFeedback.Count()
            };

            // Response
            string cardJsonPath = Path.Combine(_env.ContentRootPath, "assets", "response-card.json");
            string cardJson = await System.IO.File.ReadAllTextAsync(cardJsonPath, Encoding.UTF8);

            var cardTemplate = new AdaptiveCardTemplate(cardJson);
            var card = cardTemplate.Expand(feedbackSummary);


            Response.Headers.Add("CARD-ACTION-STATUS", "The M365 Developer Bootcamp was received.");
            Response.Headers.Add("CARD-UPDATE-IN-BODY", "true");
            return Ok(card);

        }
    }
}