namespace LIN
{
	static class ScriptWrite
	{
		static public void WriteCompiled(List<string> dialogue, string Filename)
		{
			Directory.CreateDirectory(Directory.GetParent(Filename).FullName);

			using (FileStream fs = new FileStream(Filename, FileMode.OpenOrCreate))
			{
				using (StreamWriter sw = new StreamWriter(fs))
				{
					foreach (var sd in dialogue)
					{
						sw.WriteLine(sd.Normalize().Trim());
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
