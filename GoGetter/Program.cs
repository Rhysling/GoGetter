using GoGetter;
using GoGetter.Ops;
using PuppeteerSharp;

class Program
{
	private static readonly string cs = "Data Source=localhost;Initial Catalog=TestingPocos;Integrated Security=True;TrustServerCertificate=True;";
	private static readonly string savePath = @"D:\UserData\OneDrive\Pictures\GoComics\";
	private static readonly string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36";
	private static readonly DbOps dbOps = new(cs);
	private static readonly HttpClient client = new ();
	private static readonly HttpOps httpOps = new(client, userAgent);
	private static readonly ImgFileOps imgFileOps = new(savePath);
	private static IBrowser? browser;
	private static PuppetOps? puppetOps;
	private static GoRunner? goRunner;
	//private static GoTester? goTester;

	static void Main(string[] args)
	{
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

		// Build app elements
		await new BrowserFetcher().DownloadAsync();
		browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
		puppetOps = new(browser, userAgent);
		goRunner = new(dbOps, httpOps, imgFileOps, puppetOps);
		//goTester = new(puppetOps);

		//await goRunner.FetchBatchAsync("perry-bible-fellowship", limit: 10); // tomthedancingbug tomtoles perry-bible-fellowship bliss calvinandhobbes doonesbury
		await goRunner.InfillMissingToNowAsync("tomthedancingbug", limit: 10);

		//await goRunner.InfillMissingImgFilesAsync();
		//await goRunner.ParseSrcFromImgTagAsync();

		//await goTester.TestFetchAsync();

		await browser.CloseAsync();
		Console.WriteLine("Done.");
		Console.ReadKey();
	}

}
