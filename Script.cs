namespace LIN
{
	class ScriptEntry
	{
		public string Text;
	}

	class Script
	{
		public List<ScriptEntry> ScriptData;

		public Script(string Filename)
		{
			this.ScriptData = new List<ScriptEntry>();
			var lines = System.IO.File.ReadAllLines(Filename);
			foreach (string line in lines)
			{
				ScriptEntry se = new ScriptEntry();
				se.Text = line;
				this.ScriptData.Add(se);
			}
		}
	}
}
