using System.Text;

namespace LIN
{
	static class ScriptWrite
	{
		static public Encoding GetEncodingByLanguage(string language)
		{
			var encoding = Encoding.Default;

			switch (language)
			{
				case "it":
					encoding = Encoding.GetEncoding(1252); // ANSI
					break;
				default:
					break;
			}

			return encoding;
		}

		static public List<string> ReadFile(string filePath, string language)
		{
			var encoding = GetEncodingByLanguage(language);

			var lines = System.IO.File.ReadAllLines(filePath, encoding);
			return lines.ToList();
		}

		static public void WriteCompiled(List<string> dialogue, string Filename, string language)
		{
			var encoding = GetEncodingByLanguage(language);

			var parent_path = Directory.GetParent(Filename).FullName;
			if (!Directory.Exists(parent_path))
			{
				Directory.CreateDirectory(parent_path);
			}
			if (File.Exists(Filename))
			{
				File.Delete(Filename);
			}

			using (StreamWriter sw = new StreamWriter(Filename, false, encoding))
			{
				foreach (string line in dialogue)
				{
					sw.WriteLine(line.Normalize().Trim());
				}
				sw.Flush();
				sw.Close();
				sw.Dispose();
			}
		}
	}
}
