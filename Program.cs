using dr_lin;

namespace LIN
{
	public static class Program
	{
		private static bool silentMode = false;
		public static void PrintLine<T>(T line)
		{
			if (!silentMode)
			{
				Console.WriteLine(line);
			}
		}

		static string TrimExtension(string path)
		{
			int len = path.LastIndexOf('.');
			return len == -1 ? path : path.Substring(0, len);
		}

		static void DisplayUsage()
		{
			Console.WriteLine("\nDanganronpaFunnyTranslator");
			Console.WriteLine("usage: danganronpafunnytranslator [options] input [output]\n");
			Console.WriteLine("options:");
			Console.WriteLine("-h, --help\t\tdisplay this message");
			Console.WriteLine("-drv3, --danganronpav3\tenable danganronpa v3 mode");
			Console.WriteLine("-s, --silent\t\tsuppress all non-error messages");
			Console.WriteLine();
			Environment.Exit(0);
		}

		static void Main(string[] args)
		{
			// Not by default on purpose
			bool is_v3 = false;
			string input, output;

			// Parse arguments
			List<string> plainArgs = new List<string>();
			if (args.Length == 0)
			{
				DisplayUsage();
			}

			foreach (string a in args)
			{
				if (a.ToLowerInvariant() == null)
				{
					continue;
				}
				if (a.StartsWith("-"))
				{
					if (a.ToLowerInvariant() == "-h" || a.ToLowerInvariant() == "--help") { DisplayUsage(); }
					if (a.ToLowerInvariant() == "-drv3" || a.ToLowerInvariant() == "--danganronpav3") { is_v3 = true; }
					if (a.ToLowerInvariant() == "-s" || a.ToLowerInvariant() == "--silent") { silentMode = true; }
					// if (a.ToLowerInvariant() == "-dmp" || a.ToLowerInvariant() == "--dump") { dump = true; }
				}
				else
				{
					plainArgs.Add(a.ToLowerInvariant());
				}
			}

			if (plainArgs.Count == 0 || plainArgs.Count > 2)
			{
				throw new Exception("error: incorrect arguments.");
			}
			else
			{
				input = plainArgs[0];
				output = plainArgs.Count == 2 ? plainArgs[1] : TrimExtension(input) + ".txt";
			}

			if (!is_v3)
			{
				Console.WriteLine("Please consider using the original version instead: https://github.com/morgana-x/DanganronpaHumerousTranslator");
				Console.WriteLine("This (stripped-down) version is *only* suited for Danganronpa V3 (--danganronpav3).");
				Console.WriteLine("As for support, we don’t plan to provide any assistance. You're on your own.");
				Console.WriteLine("While you’re welcome to request new features, we cannot guarantee their implementation.");
				Console.WriteLine("Thank you for your understanding.");
				return;
			}

			Console.WriteLine("Translating selected");
			GoogleTranslate.TranslateDirectory(input, output).Wait();
			return;
		}
	}
}
