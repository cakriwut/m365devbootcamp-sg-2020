# Command Log 

```csharp
dotnet new  mvc -f netcoreapp3.1  --no-https true --language c# --name adaptive-demo

cd adaptive-demo

dotnet add package AdaptiveCards
dotnet add package AdaptiveCards.Templating
dotnet add package Microsoft.O365.ActionableMessages.Utilities

dotnet add package FluffySpoon.AspNet.NGrok

dotnet add package Microsoft.Extensions.Configuration.FileExtensions
dotnet add package Microsoft.Extensions.Configuration.Json

dotnet add package Microsoft.Graph
dotnet add package Microsoft.Identity.Client
dotnet add package System.IdentityModel.Tokens.Jwt

// Check if everything are working
dotnet run
```

# Send Email

1. Authenticate to Ngrox
2. Update Program.cs
3. Update Startup.cs
4. Create SendSurveyModel
5. Create 'assets' folder,  copy adaptive-card.json and email-body.html
6. Create MsalAuthenticationProvider.cs in Helpers folder
7. Create ActionCard in Services folder
8. Register ActionCard in Startup.cs
9. Update HomeController - ctor and add new action Survey
10. Add Survey View
11. Update _Layouts.cshtml
12. Update site.css


Create 'assets' folder


