using GoGetter.Models;
using GoGetter.Ops;

namespace GoGetter;

public static class GoRunner
{
	private static readonly string cs = "Data Source=localhost;Initial Catalog=TestingPocos;Integrated Security=True;TrustServerCertificate=True;";
	private static readonly string savePath = @"D:\UserData\OneDrive\Pictures\GoComics";

	public static async Task RunAsync()
	{
		var db = new DbOps(cs);
		string source = "tomtoles"; // tomtoles bliss

		string dk = await db.GetEarliestDateKeyAsync(source);

		//DateOnly dt = DateOnly.Parse("2020-10-27");
		DateOnly dt = DateOnly.Parse($"{dk[..4]}-{dk[4..6]}-{dk[6..8]}").AddDays(-1);

		int i = 0;

		while (i < 10)
		{
			dk = dt.ToString("yyyyMMdd");
			var comic = await ParseComic.GoAsync(source, dk);
			await db.InsertComicAsync(comic);

			dt = dt.AddDays(-1);
			i += 1;
		}
	}

	public static async Task GetMissingAsync()
	{
		var db = new DbOps(cs);
		string source = "tomtoles"; // tomtoles bliss

		List<Comic> comics = await db.LoadMissingComicsAsync(source);
		int found = 0;

		foreach (var comic in comics)
		{
			var newComic = await ParseComic.GoAsync(source, comic.DateKey);
			if (newComic.IsFound) found += 1;
			await db.InsertComicAsync(newComic);
		}

		Console.WriteLine($"Source: {source}");
		Console.WriteLine($"Missing comics: {comics.Count}");
		Console.WriteLine($"Found comics: {found}");
	}
}
