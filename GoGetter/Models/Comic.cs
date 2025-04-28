namespace GoGetter.Models;

public class Comic
{
	public string DateKey { get; set; } = "";
	public string Source { get; set; } = "";
	public string ImgTag { get; set; } = "";
	public string? ImgSrc { get; set; }
	public string? ImgExt { get; set; }
	public bool IsFound { get; set; }
	public int HttpCode { get; set; }
	public string Message { get; set; } = "";
	public DateTime LastUpdate { get; set; }
	public bool HaveImgFile { get; set; }
}
