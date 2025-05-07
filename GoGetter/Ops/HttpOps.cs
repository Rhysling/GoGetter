using GoGetter.Models;
using ImageMagick;

namespace GoGetter.Ops;

public class HttpOps(HttpClient client)
{
	public async Task<HttpResult<string>> FetchComicAsync(string source, string dateKey)
	{
		ArgumentException.ThrowIfNullOrEmpty(source);
		ArgumentException.ThrowIfNullOrEmpty(dateKey);
		if (dateKey.Length != 8)
			throw new ArgumentException("DateKey must be 8 characters long", nameof(dateKey));

		string url = $"https://www.gocomics.com/{source}/{dateKey[..4]}/{dateKey[4..6]}/{dateKey[6..8]}";

		try
		{
			string html = await client.GetStringAsync(url);
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

	public async Task<HttpResult<ImgFile>> FetchImageAsync(Comic comic)
	{
		ImgFile imgFile = new() { Source = comic.Source, DateKey = comic.DateKey };
		int httpCode = 999;
		string message;

		try
		{
			imgFile.FileBytes = await client.GetByteArrayAsync(comic.ImgSrc);
			httpCode = 200;
			message = "Ok.";
		}
		catch (HttpRequestException ex)
		{
			httpCode = ex.StatusCode.HasValue ? (int)ex.StatusCode : 999;
			message = ex.Message;
		}
		catch (Exception ex)
		{
			message = ex.Message;
		}

		if (imgFile.FileBytes is not null)
		{
			imgFile.FileBytes = ConvertWebPToJpeg(imgFile.FileBytes);
			imgFile.Ext = "jpg";
		}

		return new HttpResult<ImgFile>(imgFile, message, httpCode);
	}


	private static byte[] ConvertWebPToJpeg(byte[] webpImage)
	{
		using var stream = new MemoryStream(webpImage);
		using var image = new MagickImage(stream);
		image.Format = MagickFormat.Jpeg;
		image.Quality = 100;

		using var outputStream = new MemoryStream();
		image.Write(outputStream);

		return outputStream.ToArray();
	}
}
