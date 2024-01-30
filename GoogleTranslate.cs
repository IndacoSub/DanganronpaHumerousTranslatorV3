using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using LIN;
using System.Diagnostics;
using static System.Net.WebRequestMethods;
using System.Reflection.Metadata.Ecma335;
namespace dr_lin
{
    public class GoogleTranslate
    {
        private static List<string> languages = new List<string>() 
        {
            "en",
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


        private static string danganronpaRegex = @"(<CLT 1>.*?<CLT>)|(<CLT 9>.*?<CLT>)|(<CLT 69>.*?<CLT>)|(<.*?>)|(\\n)|(\%.*?\%)|(\[.*?\])|(\"")";

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
            jsonResponse = stuff[0][0];

            return jsonResponse;
        }

        private static async Task<ScriptEntry> TranslateLine(ScriptEntry inp, int depth = 5)
        {
            inp.Text = await TranslateLine(inp.Text, depth);
            return inp;
        }

        private static async Task<string> FunnyTranslateText(string text, int repeat)
        {
            string newText = text;
            for (int i = 0; i < repeat; i++)
            {
                newText = await Translate(newText, languages[rnd.Next(languages.Count - 1)]);
            }
            return await Translate(newText, "en");
        }

        private static async Task<string> TranslateLine(string inp, int repeats = 5, int top = 0, ScriptEntry script = null)
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
                string newText = await FunnyTranslateText(text, repeats);
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
            Console.WriteLine(inp);
            return inp;
        }

        public static async Task TranslateFile(string filePath, string in_path, string out_path, string[] FilePathsIn, Game game = Game.Danganronpa1, int repeats = 10)
        {
            if (!filePath.EndsWith(".lin"))
                return;


            string new_path = filePath.Replace(in_path, out_path);

            if (System.IO.File.Exists(new_path))
            {
                return;
            }
            try
            {
                Script script = new Script(filePath, true, game);

                Dictionary<int, ScriptEntry> tobeReplaced = new Dictionary<int, ScriptEntry>();

                foreach (ScriptEntry e in script.ScriptData)
                {
                    if (e.Text == null)
                    {
                        continue;
                    }
                    if (e.Text.Length < 2)
                    {
                        continue;
                    }
                    if (e.Text == "..." || e.Text == "... ")
                    {
                        continue;
                    }
                    if (e.Text.StartsWith("[") && e.Text.EndsWith("]"))
                    {
                        continue;
                    }
                    tobeReplaced.Add(script.ScriptData.IndexOf(e), e);
                }
                if (tobeReplaced.Count == 0)
                {
                    return;
                }

                var options = new ParallelOptions { MaxDegreeOfParallelism = 500 };

                await System.Threading.Tasks.Parallel.ForEachAsync(tobeReplaced, options, async (pair, token) =>
                {
                    script.ScriptData[pair.Key] = await GoogleTranslate.TranslateLine(pair.Value, repeats);
                });

                ScriptWrite.WriteCompiled(script, new_path, game);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Translated " + filePath + " (" + FilePathsIn.ToList().IndexOf(filePath) + "/" + FilePathsIn.Length + ")");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error:\n" + filePath + "\n" + e);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public static async Task TranslateBatch(string[] FilePathsIn, string in_path, string out_path, Game game = Game.Danganronpa1, int repeats = 10)
        {
            foreach (string filePath in FilePathsIn)
            {
                await TranslateFile(filePath, in_path, out_path,  FilePathsIn, game: game,repeats: repeats);
            }
        }

        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }

        public static async Task TranslateDirectory(string in_path, string out_path, Game game = Game.Danganronpa1)
        {
            Console.WriteLine("Translating files from " + in_path + " to " + out_path);

            List<string> FilePathsIn = Directory.GetFiles(in_path).ToList().Where((x) => x.EndsWith(".lin") && !System.IO.File.Exists(x.Replace(in_path, out_path))).ToList() ;

            int threads = 70;

            List<List<string>> filePathChunks = SplitList(FilePathsIn, nSize: FilePathsIn.Count / threads).ToList();

            Console.WriteLine("Number of threads to be done: " + filePathChunks.Count);

            var options = new ParallelOptions { MaxDegreeOfParallelism = 300 };

            await System.Threading.Tasks.Parallel.ForEachAsync(filePathChunks, options, async (chunk, token) => 

                await GoogleTranslate.TranslateBatch(chunk.ToArray(), in_path, out_path, game: game, repeats: 10)

            );

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Finished!");
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.ReadKey();
        }
    }

}
