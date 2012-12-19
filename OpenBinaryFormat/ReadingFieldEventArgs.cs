using System;

namespace Obf
{
	public class ReadingFieldEventArgs : EventArgs
	{
		public string Name;
		public int Type;
		public long BlockEnd;
		public bool Handled;
	}
}

