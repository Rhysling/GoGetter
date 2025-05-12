using GoGetter.Models;
using PuppeteerSharp;

namespace GoGetter.Ops;

public class PuppetOps(IBrowser browser, string userAgent)
{
	public async Task<HttpResult<string>> FetchComicAsync(string source, string dateKey)
	{
		ArgumentException.ThrowIfNullOrEmpty(source);
		ArgumentException.ThrowIfNullOrEmpty(dateKey);
		if (dateKey.Length != 8)
			throw new ArgumentException("DateKey must be 8 characters long", nameof(dateKey));

		string url = $"https://www.gocomics.com/{source}/{dateKey[..4]}/{dateKey[4..6]}/{dateKey[6..8]}";
		var page = await browser.NewPageAsync();
		await page.SetUserAgentAsync(userAgent);
		page.DefaultNavigationTimeout = 3000; // 3 seconds
		
		try
		{
			var response = await page.GoToAsync(url, WaitUntilNavigation.Networkidle2);
			if (!response.Ok)
			{
				return new HttpResult<string>("", response.Status.ToString(), (int)response.Status);
			}

			string html = await page.GetContentAsync();
			return new HttpResult<string>(html, "Ok.", 200);
		}
		catch (HttpRequestException ex)
		{
			int statusCode = ex.StatusCode.HasValue ? (int)ex.StatusCode : 999;
			return new HttpResult<string>("", ex.Message, statusCode);
		}
		catch (Exception ex)
		{
			return new HttpResult<string>("", ex.Message);
		}
	}

}
