using Miniblog.UseCases.Abstract;

namespace Miniblog.Web.Controllers;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Atom;
using Microsoft.SyndicationFeed.Rss;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using UseCases.Settings;

using WebEssentials.AspNetCore.Pwa;

public class RobotsController : Controller
{
    private readonly IPostService _postService;
    private readonly WebManifest _manifest;
    private readonly IOptionsSnapshot<BlogSettings> _settings;

    public RobotsController(IPostService postService, IOptionsSnapshot<BlogSettings> settings, WebManifest manifest)
    {
        _postService = postService;
        _settings = settings;
        _manifest = manifest;
    }

    [Route("/robots.txt")]
    [OutputCache(Profile = "default")]
    public string RobotsTxt()
    {
        var sb = new StringBuilder();
        sb.AppendLine("User-agent: *")
            .AppendLine("Disallow:")
            .Append("sitemap: ")
            .Append(Request.Scheme)
            .Append("://")
            .Append(Request.Host)
            .AppendLine("/sitemap.xml");

        return sb.ToString();
    }

    [Route("/rsd.xml")]
    public void RsdXml()
    {
        EnableHttpBodySyncIO();

        var host = $"{Request.Scheme}://{Request.Host}";

        Response.ContentType = "application/xml";
        Response.Headers["cache-control"] = "no-cache, no-store, must-revalidate";

        using var xml = XmlWriter.Create(Response.Body, new XmlWriterSettings { Indent = true });
        xml.WriteStartDocument();
        xml.WriteStartElement("rsd");
        xml.WriteAttributeString("version", "1.0");

        xml.WriteStartElement("service");

        xml.WriteElementString("enginename", "Miniblog.Core");
        xml.WriteElementString("enginelink", "http://github.com/madskristensen/Miniblog.Core/");
        xml.WriteElementString("homepagelink", host);

        xml.WriteStartElement("apis");
        xml.WriteStartElement("api");
        xml.WriteAttributeString("name", "MetaWeblog");
        xml.WriteAttributeString("preferred", "true");
        xml.WriteAttributeString("apilink", $"{host}/metaweblog");
        xml.WriteAttributeString("blogid", "1");

        xml.WriteEndElement(); // api
        xml.WriteEndElement(); // apis
        xml.WriteEndElement(); // service
        xml.WriteEndElement(); // rsd
    }

    [Route("/feed/{type}")]
    public async Task Rss(string type)
    {
        EnableHttpBodySyncIO();

        Response.ContentType = "application/xml";
        var host = $"{Request.Scheme}://{Request.Host}";

        await using var xmlWriter = XmlWriter.Create(
            Response.Body,
            new XmlWriterSettings() { Async = true, Indent = true, Encoding = new UTF8Encoding(false) });
        var posts = await _postService.GetAllAsync(0, 10, HttpContext.RequestAborted);
        var writer = await GetWriter(
            type,
            xmlWriter,
            posts.Items.Max(p => p.PubDate)).ConfigureAwait(false);

        foreach (var post in posts.Items)
        {
            var item = new AtomEntry
            {
                Title = post.Title,
                Description = post.Content,
                Id = host + post.GetLink(),
                Published = post.PubDate,
                LastUpdated = post.LastModified,
                ContentType = "html",
            };

            foreach (var category in post.Categories)
            {
                item.AddCategory(new SyndicationCategory(category));
            }
            foreach (var tag in post.Tags)
            {
                item.AddCategory(new SyndicationCategory(tag));
            }

            item.AddContributor(new SyndicationPerson("test@example.com", _settings.Value.Owner));
            item.AddLink(new SyndicationLink(new Uri(item.Id)));

            await writer.Write(item).ConfigureAwait(false);
        }
    }

    [Route("/sitemap.xml")]
    public async Task SitemapXml()
    {
        EnableHttpBodySyncIO();

        var host = $"{Request.Scheme}://{Request.Host}";

        Response.ContentType = "application/xml";

        using var xml = XmlWriter.Create(Response.Body, new XmlWriterSettings { Indent = true });
        xml.WriteStartDocument();
        xml.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

        var posts = await _postService.GetAllAsync(0, int.MaxValue, HttpContext.RequestAborted);

        foreach (var post in posts.Items)
        {
            var lastMod = new[] { post.PubDate, post.LastModified };

            xml.WriteStartElement("url");
            xml.WriteElementString("loc", host + post.GetLink());
            xml.WriteElementString("lastmod", lastMod.Max().ToString("yyyy-MM-ddThh:mmzzz", CultureInfo.InvariantCulture));
            xml.WriteEndElement();
        }

        xml.WriteEndElement();
    }

    private async Task<ISyndicationFeedWriter> GetWriter(string? type, XmlWriter xmlWriter, DateTime updated)
    {
        var host = $"{Request.Scheme}://{Request.Host}/";

        if (type?.Equals("rss", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            var rss = new RssFeedWriter(xmlWriter);
            await rss.WriteTitle(_manifest.Name).ConfigureAwait(false);
            await rss.WriteDescription(_manifest.Description).ConfigureAwait(false);
            await rss.WriteGenerator("Miniblog.Core").ConfigureAwait(false);
            await rss.WriteValue("link", host).ConfigureAwait(false);
            return rss;
        }

        var atom = new AtomFeedWriter(xmlWriter);
        await atom.WriteTitle(_manifest.Name).ConfigureAwait(false);
        await atom.WriteId(host).ConfigureAwait(false);
        await atom.WriteSubtitle(_manifest.Description).ConfigureAwait(false);
        await atom.WriteGenerator("Miniblog.Core", "https://github.com/madskristensen/Miniblog.Core", "1.0").ConfigureAwait(false);
        await atom.WriteValue("updated", updated.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)).ConfigureAwait(false);
        return atom;
    }

    private void EnableHttpBodySyncIO()
    {
        var body = HttpContext.Features.Get<IHttpBodyControlFeature>();
        body!.AllowSynchronousIO = true;
    }
}
