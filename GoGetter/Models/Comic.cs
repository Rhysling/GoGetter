namespace GoGetter.Models;

public class Comic
{
	public string DateKey { get; set; } = "";
	public string Source { get; set; } = "";
	public string Img { get; set; } = "";
	public string? Src { get; set; }
	public bool IsFound { get; set; }
	public int HttpCode { get; set; }
	public string Message { get; set; } = "";
	public DateTime LastUpdate { get; set; }
	public bool HaveImgFile { get; set; }
}
