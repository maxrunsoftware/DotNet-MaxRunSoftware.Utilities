using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;
using MaxRunSoftware.Utilities.Web.Server.GenHTTP;

namespace MaxRunSoftware.Utilities.Web.Tests;

public class WebServerSiteTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
    [SkippableFact]
    public void StartServerTest()
    {
        LogLevel = LogLevel.Trace;
        var resource =
"""
<form id="credentials" method="post" action=".">
    <div class="container">
        <div class="inputs">
            <div class="user_cont">
                <input type="text" id="username" name="username" placeholder="username" value="@Model.Data.Username" />
            </div>
            <div class="pass_cont">
                <input type="password" id="password" name="password" placeholder="password" />
            </div>
            @if (@Model.Data.ErrorMessage != null)
            {
                <div class="error_message">
                    @Model.Data.ErrorMessage
                </div>
            }
            <button>@Model.Data.ButtonCaption</button>
        </div>
    </div>
</form>           
""";
        
        var server = new WebServerSiteTests_Server(LoggerProvider.CreateLogger<WebServerSiteTests_Server>())
        {
            SetupResource = Resource.FromString(resource),
            LoginResource = Resource.FromString(resource),
            Port = 8080,
            
        };
        server.Run();
    }
    
    
}

public class WebServerSiteTests_User : IUser
{
    public string DisplayName => Username;
    public string Username { get; set; }
    public string Password { get; set; }
}

public class WebServerSiteTests_Server(ILogger log) : WebServerSite<WebServerSiteTests_User>(log)
{
    private readonly ILogger log = log;
    protected override void SetupExecute(IRequest request, string username, string password) => log.LogInformationMethod(new(request, username, password), string.Empty);
    
    protected override LoginUserResult LoginUser(IRequest request, string username, string password)
    {
        log.LogInformationMethod(new(request, username, password), string.Empty);
        if (username == "test")
        {
            return new(new() { Username = username, Password = "pass", }, null, null);
        }
        else
        {
            return new(null, "Invalid username or password", ResponseStatus.Forbidden);
        }
    }
    
    protected override ITheme StartAddTheme() => new GenHTTP.Themes.NoTheme.NoThemeInstance();
    
}
