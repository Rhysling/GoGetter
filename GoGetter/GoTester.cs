using GoGetter.Ops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoGetter;

public class GoTester(HttpOps httpOps)
{
	public async Task TestFetchAsync()
	{
		string source = "calvinandhobbes";
		string dk = "20250508";
		string outPath = $@"D:\yy\tp1\Go_{source}_{dk}.html";

		var result = await httpOps.FetchComicAsync(source, dk);
		if (result.IsSuccess)
		{
			File.WriteAllText(outPath, result.Value);
			Console.WriteLine($"Success: {outPath}");
		}
		else
		{
			Console.WriteLine($"Failed: {source}_{dk} - {result.HttpCode} - {result.Message}");
		}
	}
}

