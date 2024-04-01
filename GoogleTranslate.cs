using LIN;
using System.Text.Json;
using System.Text.RegularExpressions;
namespace dr_lin
{
	public static class GoogleTranslate
	{
		private static readonly List<string> languages = new List<string>()
		{
			"en",
			"it",
			"es",
			"fr",
			"de",
			"pt",
			"ru",
			"zh",
			"ja",
			"ar",
			"ko",
			"hi",
			"bn",
			"id",
			"ms",
			"tr",
			"th",
			"vi",
			"nl",
			"sv"
		};

		private const string danganronpaRegex = @"(<CLT 1>.*?<CLT>)|(<CLT 9>.*?<CLT>)|(<CLT 69>.*?<CLT>)|(<.*?>)|(\\n)|(\%.*?\%)|(\[.*?\])";

		private static HttpClient translateClient = new HttpClient()
		{
			BaseAddress = new Uri("https://clients5.google.com"),
		};
		private static Random rnd = new Random();

		private async static Task<string> Translate(string inp, string la)
		{
			HttpResponseMessage response = await translateClient.GetAsync("/translate_a/t?client=dict-chrome-ex&sl=" + "auto" + "&tl=" + la + "&q=" + inp);

			response.EnsureSuccessStatusCode();
			string jsonResponse = await response.Content.ReadAsStringAsync();
			var stuff = JsonSerializer.Deserialize<List<List<string>>>(jsonResponse);

			return stuff[0][0];
		}

		private static async Task<string> FunnyTranslateText(string text, int repeat, string language)
		{
			string newText = text;
			for (int i = 0; i < repeat; i++)
			{
				string new_language = "";
				do
				{
					int it = rnd.Next(languages.Count - 1);
					new_language = languages[it];
					// Avoid translating in the final language
				} while (new_language == language);
				newText = await Translate(newText, new_language);
			}
			return await Translate(newText, language);
		}

		private static async Task<string> TranslateLine(string inp, string language, int repeats = 5)
		{
			Regex drSplitText = new Regex(danganronpaRegex);

			List<string> certifiedChunksToTranslate = new List<string>();

			Dictionary<string, string> translateDict = new Dictionary<string, string>();

			foreach (string chunk in drSplitText.Split(inp))
			{
				if (!drSplitText.IsMatch(chunk) && chunk.Trim().Replace(" ", "").Length > 1 && !chunk.StartsWith("["))
				{
					certifiedChunksToTranslate.Add(chunk.Trim().Replace("\0", "").Replace("\"", ""));
				}
			}

			var options = new ParallelOptions { MaxDegreeOfParallelism = certifiedChunksToTranslate.Count + 1 }; // I dont care if this will blow up my computer!

			await System.Threading.Tasks.Parallel.ForEachAsync(certifiedChunksToTranslate, options, async (text, token) =>
			{
				string newText = await FunnyTranslateText(text, repeats, language);
				translateDict.Add(text, newText);
			});

			foreach (var pair in translateDict)
			{
				string translatedText = pair.Value;
				string originalText = pair.Key;

				if (translatedText.Length < 1) // some horrible error if this happens
				{
					continue;
				}

				if (char.IsUpper(originalText[0])) // Make sure the translated text doesnt have an uppercase at start if the original doesn't
				{
					translatedText = translatedText.ToCharArray()[0].ToString().ToUpper() + translatedText.Substring(1);
				}

				translatedText = translatedText.Replace("\"", ""); // There shouldn't be any quotation marks

				inp = inp.Replace(originalText, translatedText);
			}
			//Console.WriteLine(inp);
			return inp;
		}

		public static async Task TranslateFile(string filePath, string in_path, string out_path, string[] FilePathsIn, string language, int repeats = 10)
		{
			if (!filePath.EndsWith(".txt"))
			{
				return;
			}

			string simple_name = Path.GetFileName(filePath);

			string new_path = Path.Combine(out_path, simple_name);

			if (new_path.Length == 0)
			{
				Console.WriteLine("New path length is zero!");
				return;
			}

			if (System.IO.File.Exists(new_path))
			{
				Console.WriteLine("File already exists: " + new_path + ", skipping...");
				return;
			}
			try
			{
				List<string> dialogue = ScriptWrite.ReadFile(filePath, language);

				if (dialogue.Count == 0)
				{
					Console.WriteLine("Empty file: " + filePath);
					return;
				}

				Dictionary<int, string> tobeReplaced = new Dictionary<int, string>();

				int i = 0;
				foreach (string e in dialogue)
				{
					i++;
					if (e == null)
					{
						continue;
					}
					if (e.Length < 2)
					{
						continue;
					}
					if (e == "..." || e == "... ")
					{
						continue;
					}
					if (e.Replace(" ", "").Length <= 0)
					{
						continue;
					}
					if (e.StartsWith("[") && e.EndsWith("]"))
					{
						continue;
					}
					if ((i == 1 && e.StartsWith("{")) || e.StartsWith("}"))
					{
						continue;
					}
					tobeReplaced.Add((i - 1), e);
				}
				if (tobeReplaced.Count == 0)
				{
					return;
				}

				var options = new ParallelOptions { MaxDegreeOfParallelism = 500 };

				await System.Threading.Tasks.Parallel.ForEachAsync(tobeReplaced, options, async (pair, token) =>
				{
					dialogue[pair.Key] = await GoogleTranslate.TranslateLine(pair.Value, language, repeats);
				});

				ScriptWrite.WriteCompiled(dialogue, new_path, language);

				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("Translated " + Path.GetFileName(filePath) + " (" + (FilePathsIn.ToList().IndexOf(filePath) + 1) + "/" + FilePathsIn.Length + ")");
				Console.ForegroundColor = ConsoleColor.Gray;
			}
			catch (Exception e)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Error:\n" + filePath + "\n" + e);
				Console.ForegroundColor = ConsoleColor.Gray;
			}
		}

		public static async Task TranslateBatch(string[] FilePathsIn, string in_path, string out_path, string language, int repeats = 10)
		{
			foreach (string filePath in FilePathsIn)
			{
				await TranslateFile(filePath, in_path, out_path, FilePathsIn, language, repeats: repeats);
			}
		}

		public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
		{
			for (int i = 0; i < locations.Count; i += nSize)
			{
				yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
			}
		}

		public static async Task TranslateDirectory(string in_path, string out_path)
		{
			Console.WriteLine("Translating files from " + in_path + " to " + out_path);

			List<string> FilePathsIn = Directory.GetFiles(in_path, "*.*", SearchOption.AllDirectories).ToList().Where((x) => x.EndsWith(".txt") && !x.Contains(".git")).ToList();

			if (FilePathsIn.Count == 0)
			{
				Console.WriteLine("No files were found!");
				return;
			}
			Console.WriteLine("Found " + FilePathsIn.Count + " files.");

			const int threads = 16;

			List<List<string>> filePathChunks = SplitList(FilePathsIn, nSize: Math.Max(1, FilePathsIn.Count / threads)).ToList();

			if (filePathChunks == null || filePathChunks.Count == 0)
			{
				Console.WriteLine("No chunks!");
				return;
			}

			Console.WriteLine("Number of threads to be done: " + filePathChunks.Count);

			var options = new ParallelOptions { MaxDegreeOfParallelism = 300 };

			// Replace "en" with another language if needed
			string language = "en";

			if (!languages.Contains(language))
			{
				Console.WriteLine("Google Translate may not support this language but I guess we'll see...");
			}

			await System.Threading.Tasks.Parallel.ForEachAsync(filePathChunks, options, async (chunk, token) =>

				await GoogleTranslate.TranslateBatch(chunk.ToArray(), in_path, out_path, language, repeats: 10)

			);

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Finished!");
			Console.ForegroundColor = ConsoleColor.Gray;

			//Console.ReadKey();
		}
	}
}
