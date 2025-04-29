using GoGetter.Models;

namespace GoGetter.Ops;

public class ImgFileOps(string savePath)
{
	public async Task<bool> SaveAsync(ImgFile imgFile)
	{
		if (!imgFile.IsValid) return false;

		var filePath = Path.Combine(savePath, imgFile.FileName);
		await File.WriteAllBytesAsync(filePath, imgFile.FileBytes!);
		return true;
	}
}
