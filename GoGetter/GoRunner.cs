using GoGetter.Models;
using GoGetter.Ops;
using System.Text.RegularExpressions;

namespace GoGetter;

public class GoRunner(DbOps dbOps, HttpOps httpOps, ImgFileOps imgFileOps)
{
	

	public async Task FetchBatchAsync(string source, int limit = 10)
	{
		string dk = await dbOps.GetEarliestDateKeyAsync(source);
		DateOnly dt = DateOnly.Parse($"{dk[..4]}-{dk[4..6]}-{dk[6..8]}").AddDays(-1);

		int i = 0;

		while (i < limit)
		{
			dk = dt.ToString("yyyyMMdd");
			var result = await httpOps.FetchComicAsync(source, dk);

			var comic = Parser.ParseComic(result.Value);
			comic.HttpCode = result.HttpCode;
			comic.Message += "|" + (result.Message ?? "");

			if (!string.IsNullOrEmpty(comic.ImgSrc))
			{
				var resImg = await httpOps.FetchImageAsync(comic);

				if (resImg.IsSuccess)
				{
					await imgFileOps.SaveAsync(resImg.Value);
					comic.ImgExt = resImg.Value.Ext;
					comic.HaveImgFile = true;
				}
			}

			await dbOps.InsertComicAsync(comic);

			dt = dt.AddDays(-1);
			i += 1;
		}
	}

	public async Task GetMissingAsync()
	{
		string source = "tomtoles"; // tomtoles bliss

		List<Comic> comics = await dbOps.LoadMissingComicsAsync(source);
		int found = 0;

		foreach (var comic in comics)
		{
			var result = await httpOps.FetchComicAsync(source, comic.DateKey);

			if (!result.IsSuccess) continue;

			var newComic = Parser.ParseComic(result.Value);
			if (newComic.IsFound) found += 1;

			await dbOps.InsertComicAsync(newComic);
		}

		Console.WriteLine($"Source: {source}");
		Console.WriteLine($"Missing comics: {comics.Count}");
		Console.WriteLine($"Found comics: {found}");
	}

	public async Task ParseSrcAsync()
	{
		List<Comic> comics = await dbOps.LoadComicsAsync(source: null, limit: 0);

		int found = 0;

		var re = new Regex("src=\"(?<src>[^\"]+)\"");

		foreach (var comic in comics)
		{
			var m = re.Match(comic.ImgTag);
			if (m.Success)
			{
				comic.ImgSrc = m.Groups[1].Value;
				await dbOps.UpdateSrcAsync(comic.Source, comic.DateKey, comic.ImgSrc);
				found += 1; ;
			}

		}

		Console.WriteLine($"All: {comics.Count}");
		Console.WriteLine($"Found: {found}");
	}
}
