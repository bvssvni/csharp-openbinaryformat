using System;

namespace Obf
{
	/// <summary>
	/// Contains information about a field.
	/// </summary>
	public class Field : EventArgs
	{
		public string Name;
		public int Type;
		public bool Handled;
		public bool ReadIt;
	}
}

