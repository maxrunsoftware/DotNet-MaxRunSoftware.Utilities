// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting.Provider;
using GenHTTP.Modules.Razor;
using GenHTTP.Modules.Scriban;
using GenHTTP.Modules.Websites;
using GenHTTP.Themes.AdminLTE;

namespace MaxRunSoftware.Utilities.Web.Tests;

public class GenHTTPServerSiteAuthTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
    [SkippableFact]
    public void Start_Server_NoTheme_Test()
    {
        LogLevel = LogLevel.Trace;
        var server = new GenHTTPServerSiteAuth(LoggerProvider.CreateLogger<GenHTTPServerSiteAuth>())
        {
            Port = TestConfig.DEFAULT_PORT,
            Theme = new GenHTTP.Themes.NoTheme.NoThemeInstance(),
        };
        server.Run();
    }
    
    [SkippableFact]
    public void Start_1Server_AdminLTE_Test()
    {
        LogLevel = LogLevel.Trace;
        var server = new GenHTTPServerSiteAuth(LoggerProvider.CreateLogger<GenHTTPServerSiteAuth>())
        {
            Port = TestConfig.DEFAULT_PORT,
            Theme = CreateThemeAdminLte(),
        };
        
        foreach (var (name, array) in TestConfig.RESOURCES)
        {
            server.Resources.Add(name, new GenHTTPResourceBytesBuilder().Name(name).Content(array));
        }
        
        server.Run();
    }
    
    /// <summary>
    /// https://github.com/Kaliumhexacyanoferrat/GenHTTP.Themes/blob/master/Demo/Program.cs
    /// </summary>
    /// <returns>GenHTTP theme AdminLTE</returns>
    private static ITheme CreateThemeAdminLte()
    {
        var menu = Menu.Empty().Add("{website}/", "Home");
        
        var notifications = ModRazor.Template<BasicModel>(Resource.FromString(CreateThemeAdminLte_Notifications)).Build();
        
        var theme = new AdminLteBuilder().Title("AdminLTE Theme")
            .Fullscreen()
            .Logo(Download.From(Resource.FromAssembly("logo.png")))
            .UserProfile((r, h) => new UserProfile("Some User", "/avatar.png", ""))
            .FooterLeft((r, h) => "Footer text on the left ...")
            .FooterRight((r, h) => "... and on the right (template by <a href=\"https://adminlte.io\" target=\"blank\">AdminLTE.io</a>)")
            .Sidebar((r, h) => "<h5>Sidebar Content</h5><p>This content is placed on the sidebar. Awesome.</p>")
            .Search((r, h) => new SearchBox(""))
            .MenuSearch((r, h) => new MenuSearchBox("Search ..."))
            .Header(menu)
            .Notifications(async (r, h) => await notifications.RenderAsync(new BasicModel(r, h)))
            .Build();
        
        
        return theme;
    } 
    
    /// <summary>
    /// https://github.com/Kaliumhexacyanoferrat/GenHTTP.Themes/blob/master/Demo/Resources/AdminLTE/Notifications.html
    /// </summary>
    private static readonly string CreateThemeAdminLte_Notifications =
        """
        <!-- Messages Dropdown Menu -->
        <li class="nav-item dropdown">
            <a class="nav-link" data-toggle="dropdown" href="#">
                <i class="far fa-comments"></i>
                <span class="badge badge-danger navbar-badge">3</span>
            </a>
            <div class="dropdown-menu dropdown-menu-lg dropdown-menu-right">
                <a href="#" class="dropdown-item">
                    <!-- Message Start -->
                    <div class="media">
                        <img src="/avatar.png" alt="User Avatar" class="img-size-50 mr-3 img-circle">
                        <div class="media-body">
                            <h3 class="dropdown-item-title">
                                Brad Diesel
                                <span class="float-right text-sm text-danger"><i class="fas fa-star"></i></span>
                            </h3>
                            <p class="text-sm">Call me whenever you can...</p>
                            <p class="text-sm text-muted"><i class="far fa-clock mr-1"></i> 4 Hours Ago</p>
                        </div>
                    </div>
                    <!-- Message End -->
                </a>
                <div class="dropdown-divider"></div>
                <a href="#" class="dropdown-item">
                    <!-- Message Start -->
                    <div class="media">
                        <img src="/avatar.png" alt="User Avatar" class="img-size-50 img-circle mr-3">
                        <div class="media-body">
                            <h3 class="dropdown-item-title">
                                John Pierce
                                <span class="float-right text-sm text-muted"><i class="fas fa-star"></i></span>
                            </h3>
                            <p class="text-sm">I got your message bro</p>
                            <p class="text-sm text-muted"><i class="far fa-clock mr-1"></i> 4 Hours Ago</p>
                        </div>
                    </div>
                    <!-- Message End -->
                </a>
                <div class="dropdown-divider"></div>
                <a href="#" class="dropdown-item">
                    <!-- Message Start -->
                    <div class="media">
                        <img src="/avatar.png" alt="User Avatar" class="img-size-50 img-circle mr-3">
                        <div class="media-body">
                            <h3 class="dropdown-item-title">
                                Nora Silvester
                                <span class="float-right text-sm text-warning"><i class="fas fa-star"></i></span>
                            </h3>
                            <p class="text-sm">The subject goes here</p>
                            <p class="text-sm text-muted"><i class="far fa-clock mr-1"></i> 4 Hours Ago</p>
                        </div>
                    </div>
                    <!-- Message End -->
                </a>
                <div class="dropdown-divider"></div>
                <a href="#" class="dropdown-item dropdown-footer">See All Messages</a>
            </div>
        </li>
        
        <li class="nav-item dropdown">
            <a class="nav-link" data-toggle="dropdown" href="#">
                <i class="far fa-bell"></i>
                <span class="badge badge-warning navbar-badge">15</span>
            </a>
            <div class="dropdown-menu dropdown-menu-lg dropdown-menu-right">
                <span class="dropdown-item dropdown-header">15 Notifications</span>
                <div class="dropdown-divider"></div>
                <a href="#" class="dropdown-item">
                    <i class="fas fa-envelope mr-2"></i> 4 new messages
                    <span class="float-right text-muted text-sm">3 mins</span>
                </a>
                <div class="dropdown-divider"></div>
                <a href="#" class="dropdown-item">
                    <i class="fas fa-users mr-2"></i> 8 friend requests
                    <span class="float-right text-muted text-sm">12 hours</span>
                </a>
                <div class="dropdown-divider"></div>
                <a href="#" class="dropdown-item">
                    <i class="fas fa-file mr-2"></i> 3 new reports
                    <span class="float-right text-muted text-sm">2 days</span>
                </a>
                <div class="dropdown-divider"></div>
                <a href="#" class="dropdown-item dropdown-footer">See All Notifications</a>
            </div>
        </li>
        """;
    
    
}
