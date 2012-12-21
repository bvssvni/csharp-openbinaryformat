using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Obf
{
	[TestFixture()]
	public class Tests
	{
		[Test()]
		public void TestRead1()
		{
			var mem = new System.IO.MemoryStream();
			var f = OpenBinaryFormat.ToMemory(mem);

			var person = f.StartBlock("person");

			f.WriteString("name", "Luke Skywalker");
			var optional = f.StartBlock("optional");
			f.WriteDouble("age", 20);
			f.EndBlock(optional);

			f.EndBlock(person);

			var bytes = mem.ToArray();
			f.Close();

			f = OpenBinaryFormat.FromBytes(bytes);

			person = f.StartBlock("person");
			var name = f.Read<string>("name", null, person);
			var age = f.Read<int>("age", 0, person);
			var notThere = f.Read<string>("notThere", "not found", person);
			f.EndBlock(person);

			Assert.True(name == "Luke Skywalker");
			Assert.True(age == 20);
			Assert.True(notThere == "not found");

			f.Close();
		}

		[Test()]
		public void TestRead2()
		{
			var mem = new System.IO.MemoryStream();
			var f = OpenBinaryFormat.ToMemory(mem);
			
			var person = f.StartBlock("person");
			
			f.EndBlock(person);
			
			var bytes = mem.ToArray();
			f.Close();
			
			f = OpenBinaryFormat.FromBytes(bytes);

			
			f.Close();
		}
	}
}

