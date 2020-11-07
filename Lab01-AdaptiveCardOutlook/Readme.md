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


Create 'assets' folder

