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

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Layouting.Provider;
using GenHTTP.Modules.Razor.Providers;
using GenHTTP.Modules.Websites;
using GenHTTP.Modules.Websites.Sites;

namespace MaxRunSoftware.Utilities.Web;

public record GenHTTPServerSiteScript(
    string Path,
    bool Asynchronous
);

public record GenHTTPServerSiteStyle(
    string Path
);

public class GenHTTPServerSite(ILogger log) : GenHTTPServer(log)
{
    #region Options
    
    public virtual IDictionary<string, IBuilder<IResource>> Resources { get; set; } = new Dictionary<string, IBuilder<IResource>>(StringComparer.OrdinalIgnoreCase);
    public virtual IDictionary<string, object> Controllers { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    
    public virtual ITheme? Theme { get; set; }
    
    public virtual IList<GenHTTPServerSiteStyle> Styles { get; set; } = new List<GenHTTPServerSiteStyle>();
    public virtual IList<GenHTTPServerSiteScript> Scripts { get; set; } = new List<GenHTTPServerSiteScript>();
    
    #endregion Options
    
    #region Setup
    
    protected virtual LayoutBuilder AddResource(LayoutBuilder layout, string name, IBuilder<IResource> resource) => layout.Add(name, Download.From(resource));
    
    protected virtual LayoutBuilder AddResources(LayoutBuilder layout)
    {
        foreach (var (name, resource) in Resources)
        {
            layout = AddResource(layout, name, resource);
        }
        
        return layout;
    }
    
    protected virtual LayoutBuilder AddController(LayoutBuilder layout, string name, object controller) => layout.Add(name, Controller.From(controller));
    
    protected virtual LayoutBuilder AddControllers(LayoutBuilder layout)
    {
        foreach (var (name, controller) in Controllers)
        {
            layout = AddController(layout, name, controller);
        }
        
        return layout;
    }
    
    protected virtual LayoutBuilder AddLayoutAdditional(LayoutBuilder layout) => layout;
    
    protected virtual WebsiteBuilder AddTheme(WebsiteBuilder builder, ITheme? theme) => theme == null ? builder : builder.Theme(theme);
    
    protected virtual WebsiteBuilder AddContent(WebsiteBuilder builder, IHandlerBuilder content) => builder.Content(content);
    
    protected virtual WebsiteBuilder CreateWebsite()
    {
        var website = Website.Create();
        website = AddTheme(website, Theme);
        
        foreach (var content in CreateContent())
        {
            website = AddContent(website, content);
        }
        
        return website;
    }
    
    protected virtual IEnumerable<IHandlerBuilder> CreateContent()
    {
        var layout = Layout.Create();
        layout = AddResources(layout);
        layout = AddControllers(layout);
        layout = AddLayoutAdditional(layout);
        return [layout, ];
    }
    
    protected override IServerHost CreateHost() => base.CreateHost().Handler(CreateWebsite());
    
    #endregion Setup
    
    protected virtual T AddPageStyles<T>(T builder) where T : IPageAdditionBuilder<T>
    {
        foreach (var o in Styles)
        {
            builder.AddStyle(o.Path);
        }
        return builder;
    }
    
    protected virtual T AddPageScripts<T>(T builder) where T : IPageAdditionBuilder<T>
    {
        foreach (var o in Scripts)
        {
            builder.AddScript(o.Path, asynchronous: o.Asynchronous);
        }
        return builder;
    }
    
    protected virtual T AddPageAdditions<T>(T builder) where T : IPageAdditionBuilder<T>
    {
        builder = AddPageStyles(builder);
        builder = AddPageScripts(builder);
        return builder;
    }
}
