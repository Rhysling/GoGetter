using GoGetter.Models;
using Microsoft.Data.SqlClient;

namespace GoGetter.Ops;

public class DbOps(string connString)
{
	public async Task<List<Comic>> LoadComicsAsync(string source, string newestDateKey = "9999999", string oldestDateKey = "00000000", int limit = 20)
	{
		string top = limit > 0 ? $"TOP ({limit})" : "";

		string sql = $"""
			SELECT {top}
				[DateKey],
				[Source],
				[Img],
				[IsFound],
				[HttpCode],
				[Message],
				[LastUpdate],
				[HaveImgFile]
			FROM
				[Comics]
			WHERE
				[Source] = '{source}'
				AND [DateKey] <= '{newestDateKey}'
				AND [DateKey] >= '{oldestDateKey}'
			ORDER BY
				[DateKey] DESC;
			""";

		return await LoadComicsSqlAsync(sql);
	}

	public async Task<List<Comic>> LoadMissingComicsAsync(string source, string newestDateKey = "9999999", int limit = 20)
	{
		string top = limit > 0 ? $"TOP ({limit})" : "";

		string sql = $"""
			SELECT {top}
				[DateKey],
				[Source],
				[Img],
				[IsFound],
				[HttpCode],
				[Message],
				[LastUpdate],
				[HaveImgFile]
			FROM
				[Comics]
			WHERE
				[Source] = '{source}'
				AND [DateKey] <= '{newestDateKey}'
				AND [HttpCode] = 200
				AND [IsFound] = 0
			ORDER BY
				[DateKey] DESC;
			""";

		return await LoadComicsSqlAsync(sql);
	}

	public async Task<string> GetEarliestDateKeyAsync(string source)
	{
		string sql = $"""
			SELECT
				MIN([DateKey]) AS DateKey
			FROM
				[Comics]
			WHERE
				[Source] = '{source}';
			""";

		string dateKey;

		using var conn = new SqlConnection(connString);
		using var cmd = new SqlCommand(sql, conn);
		conn.Open();
		dateKey = (await cmd.ExecuteScalarAsync() ?? "").ToString()!;
		conn.Close();
		if (dateKey.Length != 8) dateKey = DateTime.Now.ToString("yyyyMMdd");

		return dateKey;
	}

	public async Task<bool> InsertComicAsync(Comic comic)
	{
		//string sqlCount = $"SELECT COUNT(*) FROM Comics WHERE (DateKey = '{comic.DateKey}' AND Source = '{comic.Source}');";
		string sqlDel = $"DELETE FROM Comics WHERE (DateKey = '{comic.DateKey}' AND Source = '{comic.Source}');";

		// Already exists?
		var cl = await LoadComicsAsync(comic.Source, comic.DateKey, comic.DateKey);
		if (cl.Count > 0)
		{
			if (cl[0].IsFound && !comic.IsFound)
				return false;

			using var connDel = new SqlConnection(connString);
			using var cmdDel = new SqlCommand(sqlDel, connDel);
			connDel.Open();
			await cmdDel.ExecuteNonQueryAsync();
			connDel.Close();
		}

		string sqlInsert = $"""
			INSERT INTO [Comics]
			(
				[DateKey],
				[Source],
				[Img],
				[IsFound],
				[HttpCode],
				[Message],
				[HaveImgFile]
			)
			VALUES
			(
				@DateKey,
				@Source,
				@Img,
				@IsFound,
				@HttpCode,
				@Message,
				@HaveImgFile
			);
			""";

		using var conn = new SqlConnection(connString);
		conn.Open();		

		using var cmdInsert = new SqlCommand(sqlInsert, conn);
		cmdInsert.Parameters.AddWithValue("@DateKey", comic.DateKey);
		cmdInsert.Parameters.AddWithValue("@Source", comic.Source);
		cmdInsert.Parameters.AddWithValue("@Img", comic.Img ?? "");
		cmdInsert.Parameters.AddWithValue("@IsFound", comic.IsFound);
		cmdInsert.Parameters.AddWithValue("@HttpCode", comic.HttpCode);
		cmdInsert.Parameters.AddWithValue("@Message", comic.Message);
		cmdInsert.Parameters.AddWithValue("@HaveImgFile", comic.HaveImgFile);

		await cmdInsert.ExecuteNonQueryAsync();
		conn.Close();
		return true;
	}


	// ***** PRIVATE *****

	private async Task<List<Comic>> LoadComicsSqlAsync(string sql)
	{
		var comics = new List<Comic>();

		using var conn = new SqlConnection(connString);
		using var cmd = new SqlCommand(sql, conn);
		
		conn.Open();
		using var rdr = await cmd.ExecuteReaderAsync();

		while (rdr.Read())
		{
			Comic c = new()
			{
				DateKey = rdr["DateKey"].ToString()!,
				Source = rdr["Source"].ToString()!,
				Img = rdr["Img"].ToString()!,
				HttpCode = (int)rdr["HttpCode"],
				Message = rdr["Message"].ToString()!,
				IsFound = rdr["IsFound"] != DBNull.Value && (bool)rdr["IsFound"],
				LastUpdate = DateTime.Parse(rdr["LastUpdate"].ToString()!),
				HaveImgFile = rdr["HaveImgFile"] != DBNull.Value && (bool)rdr["HaveImgFile"]
			};

			comics.Add(c);
		}		
		conn.Close();
		return comics;
	}

}