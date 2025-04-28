namespace GoGetter.Models;

public class ImgFile
{
	public string Source { get; set; } = "";
	public string DateKey { get; set; } = "";
	public string? Ext { get; set; }
	public byte[]? FileBytes { get; set; }
	public string FileName => IsValid ? ($"{Source}_{DateKey}.{Ext}") : "";
	public bool IsValid => !string.IsNullOrEmpty(Source) && !string.IsNullOrEmpty(DateKey) && !string.IsNullOrEmpty(Ext) && (FileBytes is not null);
}
