using System;

namespace Obf
{
	/// <summary>
	/// An irreversible number is a number that starts at 0 and increases until it reaches a maximum,
	/// then decrease until it reaches 0. Any attempt to break this rule will throw an exception.
	/// </summary>
	public class IrreversibleNumber
	{
		public int Value = 0;
		public int Direction = 1;
		public string ErrorUpMessage = "Can not increase value because it is on its way down.";
		public string ErrorBottomMessage = "Reached bottom.";

		public void Increase()
		{
			if (Value > 0 && Direction == -1) throw new Exception(ErrorUpMessage);

			Value++;
			Direction = 1;
		}

		public void Decrease()
		{
			if (Value == 0) throw new Exception(ErrorBottomMessage);

			Value--;
			Direction = -1;
		}
	}
}

