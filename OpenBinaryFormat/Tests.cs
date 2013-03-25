using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Obf
{
	[TestFixture()]
	public class Tests
	{
		[Test()]
		public void TestSkipBlock()
		{
			var mem = new System.IO.MemoryStream();
			var f = OpenBinaryFormat.ToMemory(mem);

			var person = f.StartBlock("person");

			f.WriteString("name", "Luke Skywalker");

			// Write a block that will be skipped.
			var optional = f.StartBlock("skip this");
			f.WriteDouble("age", 20);
			f.EndBlock(optional);

			f.EndBlock(person);

			var bytes = mem.ToArray();
			f.Close();

			f = OpenBinaryFormat.FromBytes(bytes);

			person = f.StartBlock("person");
			Assert.True(person != -1);

			var name = f.Seek<string>("name", null, person);
			var age = f.Seek<int>("age", 0, person);
			var notThere = f.Seek<string>("notThere", "not found", person);

			f.EndBlock(person);

			Assert.True(name == "Luke Skywalker");
			Assert.False(age == 20);
			Assert.True(notThere == "not found");

			f.Close();
		}
	}
}

