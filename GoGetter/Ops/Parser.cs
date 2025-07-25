﻿using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using GoGetter.Models;

namespace GoGetter.Ops;

public static class Parser
{
	public static Comic ParseComic(Comic comic, string html)
	{
		if (string.IsNullOrWhiteSpace(html))
		{
			comic.Message += " | HTML is empty.";
			return comic;
		}

		var parser = new HtmlParser();
		IHtmlDocument doc = parser.ParseDocument(html);

		// V1
		var imgs = doc.QuerySelectorAll("div[class^=\"ShowComicViewer\"] img[class^=\"Comic\"]");
		var img = imgs.FirstOrDefault();
		if (img != null)
		{
			comic.Message += " | V1 hit.";
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
			comic.Message += " | V2 hit.";
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
			comic.Message += " | ComicViewer hit.";
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