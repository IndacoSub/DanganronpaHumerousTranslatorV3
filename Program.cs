using dr_lin;
using System.Text;

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

		static void DisplayUsage()
		{
			Console.WriteLine("\nDanganronpaFunnyTranslator");
			Console.WriteLine("usage: danganronpafunnytranslator [options] input-folder output-folder\n");
			Console.WriteLine("options:");
			Console.WriteLine("-h, --help\t\tdisplay this message");
			Console.WriteLine("-drv3, --danganronpav3\tenable danganronpa v3 mode");
			Console.WriteLine("-s, --silent\t\tsuppress all non-error messages");
			Console.WriteLine();
			Environment.Exit(0);
		}

		static void Main(string[] args)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			// False by default on purpose
			bool is_v3 = false;
			string input, output;

			List<string> my_args = args.ToList();

			// Parse arguments
			List<string> plainArgs = new List<string>();
			if (my_args.Count == 0)
			{
				DisplayUsage();
			}

			foreach (string a in my_args)
			{
				if (a.ToLowerInvariant() == null || a.Replace("-", "").Replace(" ", "").Length == 0)
				{
					continue;
				}
				string b = a;
				if (b.StartsWith("-"))
				{
					b = b.Replace("--", "-");
					if (b.ToLowerInvariant() == "-h" || b.ToLowerInvariant() == "-help") { DisplayUsage(); }
					if (b.ToLowerInvariant() == "-drv3" || b.ToLowerInvariant() == "-danganronpav3") { is_v3 = true; }
					if (b.ToLowerInvariant() == "-s" || b.ToLowerInvariant() == "-silent") { silentMode = true; }
				}
				else
				{
					plainArgs.Add(b.ToLowerInvariant());
				}
			}

			if (plainArgs.Count == 0 || plainArgs.Count > 2)
			{
				throw new Exception("error: incorrect arguments.");
			}
			else
			{
				input = plainArgs[0];
				output = plainArgs.Count == 2 ? plainArgs[1] : string.Empty;

				if (output.Length == 0)
				{
					Console.WriteLine("You forgot to specify an output directory");
					return;
				}

				if (File.Exists(output))
				{
					Console.WriteLine("The output argument must be a folder");
					return;
				}

				if (!Directory.Exists(output))
				{
					Console.WriteLine("The specified output directory doesn't exist, creating now...");
					Directory.CreateDirectory(output);
				}
			}

			if (!is_v3)
			{
				Console.WriteLine("Please consider using the original version instead: https://github.com/morgana-x/DanganronpaHumerousTranslator");
				Console.WriteLine("This (stripped-down) version is *only* suited for Danganronpa V3 (--danganronpav3).");
				Console.WriteLine("As for support, we don't plan to provide any assistance. You're on your own.");
				Console.WriteLine("While you're welcome to request new features, we cannot guarantee their implementation.");
				Console.WriteLine("We hope you understand.");
				return;
			}

			GoogleTranslate.TranslateDirectory(input, output).Wait();
		}
	}
}
