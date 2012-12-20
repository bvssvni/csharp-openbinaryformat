using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Obf
{
	[TestFixture()]
	public class Test
	{
		// This is a test class for saving and reading data.
		// A person is a parent if it has sub nodes.
		private class Person : List<Person>
		{
			public string Name;

			public void Read(OpenBinaryFormat f, IEnumerator<bool> state = null)
			{
				// We need to keep track of which fields we have read to avoid duplicates.
				var readName = false;
				var readKids = false;

				f.ReadField(delegate(object sender, IEnumerator<bool> iterator) {
					// Get the current field.
					var field = f.CurrentField;

					Assert.True(field.Type < 0);

					switch (field.Name) {
					case "name":
						if (readName) {
							Console.WriteLine("Already read name: " + this.Name);
							
							// We already read name.
							field.ReadIt = true;
							return;
						}

						this.Name = f.Read<string>(field);
						readName = true;
					
						Console.WriteLine(this.Name);
						
						break;
					case "count":
						if (readKids) {
							Console.WriteLine("Already read count: " + this.Name);
							
							// We already read kids.
							field.ReadIt = true;
							return;
						}

						int count = f.Read<int>(field);
						for (int i = 0; i < count; i++) {
							// Create a new kid and use the iterator as new state.
							Person kid = new Person();
							kid.Read(f, iterator);
							this.Add(kid);
						}
						
						Console.WriteLine("End " + this.Name);
					
						readKids = true;

						break;
					}
				}, state);
			}

			public void Save(OpenBinaryFormat f, bool usePersonBlock, bool useNameBlock, bool useKidsBlocks)
			{
				long personBlock = 0, nameBlock = 0, kidsBlock = 0;
				if (usePersonBlock)
					personBlock = f.StartBlock("personBlock");
				if (useNameBlock)
					nameBlock = f.StartBlock("nameBlock");
				f.WriteString("name", this.Name);
				if (useNameBlock) {
					// UNCOMMENT NEXT LINE TO PROVOKE BUG.
					// f.WriteDouble("nameBlock extensions", 42);
					f.EndBlock(nameBlock);
				} 
				if (useKidsBlocks)
					kidsBlock = f.StartBlock("kidsBlock");
				f.WriteInt("count", this.Count);
				for (int i = 0; i < this.Count; i++) {
					this [i].Save(f, usePersonBlock, useNameBlock, useKidsBlocks);
				}
				if (useKidsBlocks) {
					f.WriteString("kidsBlock extensions", "Turn over to the dark side");
					f.EndBlock(kidsBlock);
				} 
				if (usePersonBlock) {
					f.WriteDouble("personBlock extensions", 2.0);
					f.EndBlock(personBlock);
				}
			}

			public static Person DarthVader()
			{
				var darthVader = new Person();
				darthVader.Name = "Darth Vader";
				var luke = new Person();
				luke.Name = "Luke Skywalker";
				darthVader.Add(luke);
				var princessLeia = new Person();
				princessLeia.Name = "Princess Leia";
				darthVader.Add(princessLeia);
				var gandalf = new Person();
				gandalf.Name = "Han Solo Junior";
				princessLeia.Add(gandalf);

				return darthVader;
			}

			public void CheckDarthVader()
			{
				var p = this;
				Assert.True(p.Name == "Darth Vader");
				Assert.True(p.Count == 2);
				Assert.True(p [0].Name == "Luke Skywalker");
				Assert.True(p [0].Count == 0);
				Assert.True(p [1].Name == "Princess Leia");
				Assert.True(p [1].Count == 1);
				Assert.True(p [1] [0].Name == "Han Solo Junior");
			}
		}

		private void SubTest(bool usePersonBlock, bool useNameBlock, bool useKidsBlock)
		{
			var darthVader = Person.DarthVader();
			var mem = new System.IO.MemoryStream();
			var f = OpenBinaryFormat.ToMemory(mem);
			// Save data to memory stream.
			darthVader.Save(f, usePersonBlock, useNameBlock, useKidsBlock);
			// Extract the bytes before closing.
			var bytes = mem.ToArray();
			f.Close();
			
			// Read the data from memory stream.
			f = OpenBinaryFormat.FromBytes(bytes);
			Person p = new Person();
			p.Read(f);
			f.Close();
			
			// Check that the structure is correct.
			p.CheckDarthVader();
		}

		[Test()]
		public void TestPerson()
		{
			// SubTest(false, false, false);
			// SubTest(true, false, false);
			SubTest(false, true, false);

			/*
			// Test 3 different block combinations to make sure they all are read in exact same way.
			bool[] states = {false, true};
			for (int i = 0; i < states.Length; i++) {
				for (int j = 0; j < states.Length; j++) {
					for (int k = 0; k < states.Length; k++) {
						var darthVader = Person.DarthVader();
						var mem = new System.IO.MemoryStream();
						var f = OpenBinaryFormat.ToMemory(mem);
						// Save data to memory stream.
						darthVader.Save(f, states[i], states[j], states[k]);
						// Extract the bytes before closing.
						var bytes = mem.ToArray();
						f.Close();

						// Read the data from memory stream.
						f = OpenBinaryFormat.FromBytes(bytes);
						Person p = new Person();
						p.Read(f);
						f.Close();

						// Check that the structure is correct.
						p.CheckDarthVader();
					}
				}
			}
			*/


		}
	}
}

