using GoGetter;
using GoGetter.Ops;

class Program
{
	private static readonly string cs = "Data Source=localhost;Initial Catalog=TestingPocos;Integrated Security=True;TrustServerCertificate=True;";
	private static readonly string savePath = @"D:\UserData\OneDrive\Pictures\GoComics\";
	private static readonly DbOps dbOps = new(cs);
	private static readonly HttpClient client = new ();
	private static readonly HttpOps httpOps = new(client);
	private static readonly ImgFileOps imgFileOps = new(savePath);
	//private static readonly GoRunner goRunner = new(dbOps, httpOps, imgFileOps);
	private static readonly GoTester goTester = new(httpOps);

	static void Main(string[] args)
	{
		RegisterServices();
		MainAsync(args).Wait();
	}

	static async Task MainAsync(string[] _)
	{
		// Config ***************
		//var builder = new ConfigurationBuilder()
		//	.AddJsonFile("appsettings.json", true, true)
		//	.AddEnvironmentVariables();
		//var configurationRoot = builder.Build();
		//var appSettings = configurationRoot.Get<AppSettings>() ?? new();

		//await goRunner.FetchBatchAsync("perry-bible-fellowship", limit: 10); // tomthedancingbug tomtoles perry-bible-fellowship bliss calvinandhobbes doonesbury

		//await goRunner.InfillMissingImgFilesAsync();
		//await goRunner.ParseSrcFromImgTagAsync();

		await goTester.TestFetchAsync();

		Console.WriteLine("Done.");
		Console.ReadKey();
	}

	static void RegisterServices()
	{
		client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36");
	}
}
