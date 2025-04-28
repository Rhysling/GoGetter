using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using GoGetter.Models;

namespace GoGetter.Ops;

public static class Parser
{
	public static async Task<Comic> FetchAndParseAsync(string source, string dateKey)
	{
		ArgumentException.ThrowIfNullOrEmpty(source);
		ArgumentException.ThrowIfNullOrEmpty(dateKey);
		if (dateKey.Length != 8)
			throw new ArgumentException("DateKey must be 8 characters long", nameof(dateKey));

		string url = $"https://www.gocomics.com/{source}/{dateKey[..4]}/{dateKey[4..6]}/{dateKey[6..8]}";
		//string urlBase = "https://www.gocomics.com/peanuts/2025/04/17/"; bliss calvinandhobbes doonesbury tomthedancingbug

		var comic = new Comic
		{
			Source = source,
			DateKey = dateKey
		};

		using var client = new HttpClient();
		client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36");

		string html = "";
		try
		{
			html = await client.GetStringAsync(url);
		}
		catch (HttpRequestException ex)
		{
			comic.HttpCode = (int)(ex.StatusCode ?? 0);
			comic.Message = ex.Message;
			return comic;
		}
		catch (Exception ex)
		{
			comic.Message = ex.Message;
			return comic;
		}

		comic.HttpCode = 200;

		var parser = new HtmlParser();
		IHtmlDocument doc = parser.ParseDocument(html);

		comic.Message = doc.QuerySelector("title")?.TextContent ?? "Title not found.";

		var imgs = doc.QuerySelectorAll("div[class^=\"ShowComicViewer\"] img[class^=\"Comic\"]");
		var img = imgs.FirstOrDefault();
		if (img != null)
		{
			comic.IsFound = true;
			comic.Img = img.OuterHtml;
			comic.Src = ParseImgTagSrc(img);
			return comic;
		}

		// V2
		imgs = doc.QuerySelectorAll("div[class^=\"ShowComicViewer\"] img");
		img = imgs.FirstOrDefault();
		if (img != null)
		{
			comic.Message = "V2 hit";
			comic.IsFound = true;
			comic.Img = img.OuterHtml;
			comic.Src = ParseImgTagSrc(img);
			return comic;
		}

		// Comic Viewer
		imgs = doc.QuerySelectorAll("div[class^=\"ComicViewer\"] img");
		img = imgs.FirstOrDefault();
		if (img != null)
		{
			comic.Message = "Comic Viewer";
			comic.IsFound = true;
			comic.Img = img.OuterHtml;
			comic.Src = ParseImgTagSrc(img);
			return comic;
		}

		return comic;
	}


	private static string? ParseImgTagSrc(IElement imgTag)
	{
		if (imgTag is null) return null;

		if (imgTag.HasAttribute("src"))
			return imgTag.GetAttribute("src");

		return null;
	}

}