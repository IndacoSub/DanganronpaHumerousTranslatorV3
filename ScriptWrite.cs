namespace LIN
{
	static class ScriptWrite
	{
		static public void WriteCompiled(Script s, string Filename)
		{
			Directory.CreateDirectory(Directory.GetParent(Filename).FullName);

			using (FileStream fs = new FileStream(Filename, FileMode.OpenOrCreate))
			{
				using (StreamWriter sw = new StreamWriter(fs))
				{
					foreach (var sd in s.ScriptData)
					{
						sw.WriteLine(sd.Text.Normalize().Trim());
					}
					sw.Close();
					sw.Dispose();
				}
				fs.Close();
				fs.Dispose();
			}

			return;
		}
	}
}
