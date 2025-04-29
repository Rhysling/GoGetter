using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using GoGetter.Models;

namespace GoGetter.Ops;

public static class Parser
{
	public static Comic ParseComic(ComicHtml comicHtml)
	{
		var comic = new Comic
		{
			Source = comicHtml.Source,
			DateKey = comicHtml.DateKey
		};

		if (string.IsNullOrWhiteSpace(comicHtml.Html))
		{
			comic.Message = "HTML is empty.";
			return comic;
		}

		var parser = new HtmlParser();
		IHtmlDocument doc = parser.ParseDocument(comicHtml.Html);

		comic.Message = doc.QuerySelector("title")?.TextContent ?? "Title not found.";

		var imgs = doc.QuerySelectorAll("div[class^=\"ShowComicViewer\"] img[class^=\"Comic\"]");
		var img = imgs.FirstOrDefault();
		if (img != null)
		{
			comic.IsFound = true;
			comic.ImgTag = img.OuterHtml;
			comic.ImgSrc = ParseImgTagSrc(img);
			return comic;
		}

		// V2
		imgs = doc.QuerySelectorAll("div[class^=\"ShowComicViewer\"] img");
		img = imgs.FirstOrDefault();
		if (img != null)
		{
			comic.Message = "V2 hit";
			comic.IsFound = true;
			comic.ImgTag = img.OuterHtml;
			comic.ImgSrc = ParseImgTagSrc(img);
			return comic;
		}

		// Comic Viewer
		imgs = doc.QuerySelectorAll("div[class^=\"ComicViewer\"] img");
		img = imgs.FirstOrDefault();
		if (img != null)
		{
			comic.Message = "Comic Viewer";
			comic.IsFound = true;
			comic.ImgTag = img.OuterHtml;
			comic.ImgSrc = ParseImgTagSrc(img);
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