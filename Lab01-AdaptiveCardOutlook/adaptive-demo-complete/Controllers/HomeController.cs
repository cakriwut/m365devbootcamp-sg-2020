using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using adaptive_demo.Models;
using FluffySpoon.AspNet.NGrok;
using Microsoft.AspNetCore.Hosting;
using AdaptiveCards;
using System.IO;
using System.Text;
using adaptive_demo.Services;

namespace adaptive_demo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly INGrokHostedService _ngrok;
        private readonly IWebHostEnvironment _env;      
        private readonly ActionCard _email;
        public HomeController(ILogger<HomeController> logger, INGrokHostedService ngrok,IWebHostEnvironment env, ActionCard email)
        {
            _logger = logger;

            _ngrok = ngrok;
            _env = env;            
            _email = email;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Survey()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Survey(SendSurveyModel survey)
        {
            if (ModelState.IsValid)
            {
                var tunnel = await _ngrok.GetTunnelsAsync();
                var actionUrl = tunnel.Where(i => i.Proto == "https").Select(i => i.PublicUrl).FirstOrDefault();

                string cardJsonPath = Path.Combine(_env.ContentRootPath, "assets", "adaptive-card.json");
                string cardJson = await System.IO.File.ReadAllTextAsync(cardJsonPath, Encoding.UTF8);
                var parsedAdaptiveCard = AdaptiveCard.FromJson(cardJson);

                var cardActionSets = parsedAdaptiveCard.Card.Body.Where(i => i.Type == "ActionSet").Cast<AdaptiveActionSet>();
                foreach (var cardActionSet in cardActionSets)
                {
                    var actionFeedback = cardActionSet.Actions.Where(i => i.Title == "Submit Feedback" && i.Type == "Action.Http").FirstOrDefault();
                    if (actionFeedback != null)
                    {
                        actionFeedback.AdditionalProperties["url"] = $"{actionUrl}/api/card";
                    }
                }

                string htmlTemplatePath = Path.Combine(_env.ContentRootPath, "assets", "email-body.html");
                string htmlTemplate = await System.IO.File.ReadAllTextAsync(htmlTemplatePath, Encoding.UTF8);

                await _email.SendActionCardAsync(htmlTemplate, parsedAdaptiveCard.Card.ToJson(), survey.UserName, survey.UserPassword, survey.Recipients);
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
