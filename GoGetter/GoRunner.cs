using GoGetter.Models;
using GoGetter.Ops;
using System.Text.RegularExpressions;

namespace GoGetter;

public class GoRunner(DbOps dbOps, HttpOps httpOps, ImgFileOps imgFileOps, PuppetOps puppetOps)
{
	public async Task FetchBatchAsync(string source, int limit = 10)
	{
		string dk = await dbOps.GetEarliestDateKeyAsync(source);
		DateOnly dt = DateOnly.Parse($"{dk[..4]}-{dk[4..6]}-{dk[6..8]}").AddDays(-1);

		int i = 0;

		while (i < limit)
		{
			dk = dt.ToString("yyyyMMdd");

			var comic = new Comic
			{
				Source = source,
				DateKey = dk
			};

			var result = await puppetOps.FetchComicAsync(source, dk);
			comic.HttpCode = result.HttpCode;
			comic.Message = result.Message ?? "";

			if (result.IsSuccess)
				{
				comic = Parser.ParseComic(comic, result.Value);

				if (!string.IsNullOrWhiteSpace(comic.ImgSrc))
				{
					var resImg = await httpOps.FetchImageAsync(comic);

					if (resImg.IsSuccess)
					{
						await imgFileOps.SaveAsync(resImg.Value);
						comic.ImgExt = resImg.Value.Ext;
						comic.HaveImgFile = true;
					}
				}
			}

			await dbOps.InsertComicAsync(comic);

			dt = dt.AddDays(-1);
			i += 1;
		}
	}

	public async Task InfillMissingToNowAsync(string source, int limit = 10)
	{
		string dkStart = await dbOps.GetLatestDateKeyAsync(source);
		DateOnly dtStart = DateOnly.Parse($"{dkStart[..4]}-{dkStart[4..6]}-{dkStart[6..8]}").AddDays(1);
		DateOnly dtEnd = DateOnly.FromDateTime(DateTime.Now);
		if (dtStart > dtEnd)
			dtStart = dtEnd;

		var dtRange = Enumerable.Range(0, dtEnd.DayNumber - dtStart.DayNumber + 1)
										.Select(offset => dtStart.AddDays(offset)).ToArray();

		string dk;
		int i = 0, found = 0;
		int len = dtRange.Length;

		while (i < limit && i < len)
		{
			dk = dtRange[i].ToString("yyyyMMdd");

			var comic = new Comic
			{
				Source = source,
				DateKey = dk
			};

			var result = await puppetOps.FetchComicAsync(source, dk);
			comic.HttpCode = result.HttpCode;
			comic.Message = result.Message ?? "";

			if (result.IsSuccess)
			{
				found += 1;
				comic = Parser.ParseComic(comic, result.Value);

				if (!string.IsNullOrWhiteSpace(comic.ImgSrc))
				{
					var resImg = await httpOps.FetchImageAsync(comic);

					if (resImg.IsSuccess)
					{
						await imgFileOps.SaveAsync(resImg.Value);
						comic.ImgExt = resImg.Value.Ext;
						comic.HaveImgFile = true;
					}
				}
			}

			await dbOps.InsertComicAsync(comic);
			i += 1;
		}

		Console.WriteLine($"Range: {dtRange[0]} - {dtRange[len-1]}");
		Console.WriteLine($"Added Through: {dtRange[i-1]}");
		Console.WriteLine($"Found: {found}");
	}

	public async Task InfillMissingImgFilesAsync()
	{
		List<Comic> comics = await dbOps.LoadAllComicsAsync();
		comics = [.. comics.Where(c => c.HttpCode == 200 && !c.HaveImgFile)];

		int found = 0;
		foreach (var comic in comics)
		{
			if (!string.IsNullOrWhiteSpace(comic.ImgSrc))
			{
				var resImg = await httpOps.FetchImageAsync(comic);

				if (resImg.IsSuccess)
				{
					await imgFileOps.SaveAsync(resImg.Value);
					comic.ImgExt = resImg.Value.Ext;
					comic.HaveImgFile = true;
					await dbOps.InsertComicAsync(comic);
					found += 1;
				}
			}
		}

		Console.WriteLine($"All missing: {comics.Count}");
		Console.WriteLine($"Found: {found}");
	}

	public async Task ParseSrcFromImgTagAsync()
	{
		List<Comic> comics = await dbOps.LoadAllComicsAsync();
		comics = [.. comics.Where(c => !string.IsNullOrWhiteSpace(c.ImgTag) && string.IsNullOrEmpty(c.ImgSrc))];

		int found = 0;
		var re = new Regex("src=\"(?<src>[^\"]+)\"");

		foreach (var comic in comics)
		{
			var m = re.Match(comic.ImgTag);
			if (m.Success)
			{
				comic.ImgSrc = m.Groups[1].Value;
				await dbOps.UpdateImgSrcAsync(comic.Source, comic.DateKey, comic.ImgSrc);
				found += 1; ;
			}

		}

		Console.WriteLine($"All eligible: {comics.Count}");
		Console.WriteLine($"Found: {found}");
	}
}