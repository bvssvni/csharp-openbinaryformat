using System;
using NUnit.Framework;

namespace Obf
{
	[TestFixture()]
	public class Test
	{
		[Test()]
		public void TestDouble()
		{
			// Write to memory.
			var mem = new System.IO.MemoryStream();
			var f = OpenBinaryFormat.ToMemory(mem);
			f.WriteDouble("Age", 80);
			var bytes = mem.ToArray();
			f.Close();

			// Read from memory.
			f = OpenBinaryFormat.FromMemory(new System.IO.MemoryStream(bytes));
			f.ReadDocument(delegate (object sender, ReadingFieldEventArgs evt){
				switch (evt.Name) {
				case "Age": {
					double age = f.Read<double>(evt);
					Assert.True(age == 80);
				} break;
				}
			});
		}

		[Test()]
		public void TestDoubleBlock()
		{
			// Write to memory.
			var mem = new System.IO.MemoryStream();
			var f = OpenBinaryFormat.ToMemory(mem);
			// Put the data in a block.
			var pos = f.WriteBeginBlock();

			f.WriteDouble("Age", 80);
			var bytes = mem.ToArray();

			f.WriteEndBlock(pos);
			f.Close();
			
			// Read from memory.
			f = OpenBinaryFormat.FromMemory(new System.IO.MemoryStream(bytes));
			f.ReadDocument(delegate (object sender, ReadingFieldEventArgs evt){
				switch (evt.Name) {
				case "Age": {
					double age = f.Read<double>(evt);
					Assert.True(age == 80);
				} break;
				}
			});
		}

		[Test()]
		public void TestFloat()
		{
			// Write to memory.
			var mem = new System.IO.MemoryStream();
			var f = OpenBinaryFormat.ToMemory(mem);
			f.WriteFloat("Age", 80);
			var bytes = mem.ToArray();
			f.Close();
			
			// Read from memory.
			f = OpenBinaryFormat.FromMemory(new System.IO.MemoryStream(bytes));
			f.ReadDocument(delegate (object sender, ReadingFieldEventArgs evt){
				switch (evt.Name) {
				case "Age": {
					float age = f.Read<float>(evt);
					Console.WriteLine(age.ToString());
					Assert.True(age == 80);
				} break;
				}
			});
		}
		
		[Test()]
		public void TestFloatBlock()
		{
			// Write to memory.
			var mem = new System.IO.MemoryStream();
			var f = OpenBinaryFormat.ToMemory(mem);
			// Put the data in a block.
			var pos = f.WriteBeginBlock();
			
			f.WriteFloat("Age", 80);
			var bytes = mem.ToArray();
			
			f.WriteEndBlock(pos);
			f.Close();
			
			// Read from memory.
			f = OpenBinaryFormat.FromMemory(new System.IO.MemoryStream(bytes));
			f.ReadDocument(delegate (object sender, ReadingFieldEventArgs evt){
				switch (evt.Name) {
				case "Age": {
					float age = f.Read<float>(evt);
					Assert.True(age == 80);
				} break;
				}
			});
		}

		[Test()]
		public void TestFloatToDouble()
		{
			// Write to memory.
			var mem = new System.IO.MemoryStream();
			var f = OpenBinaryFormat.ToMemory(mem);
			f.WriteFloat("Age", 80);
			var bytes = mem.ToArray();
			f.Close();
			
			// Read from memory.
			f = OpenBinaryFormat.FromMemory(new System.IO.MemoryStream(bytes));
			f.ReadDocument(delegate (object sender, ReadingFieldEventArgs evt){
				switch (evt.Name) {
				case "Age": {
					var age = f.Read<double>(evt);
					Console.WriteLine(age.ToString());
					Assert.True(age == 80);
				} break;
				}
			});
		}
		
		[Test()]
		public void TestFloatToDoubleBlock()
		{
			// Write to memory.
			var mem = new System.IO.MemoryStream();
			var f = OpenBinaryFormat.ToMemory(mem);
			// Put the data in a block.
			var pos = f.WriteBeginBlock();
			
			f.WriteFloat("Age", 80);
			var bytes = mem.ToArray();
			
			f.WriteEndBlock(pos);
			f.Close();
			
			// Read from memory.
			f = OpenBinaryFormat.FromMemory(new System.IO.MemoryStream(bytes));
			f.ReadDocument(delegate (object sender, ReadingFieldEventArgs evt){
				switch (evt.Name) {
				case "Age": {
					var age = f.Read<double>(evt);
					Assert.True(age == 80);
				} break;
				}
			});
		}

		[Test()]
		public void TestDoubleToFloat()
		{
			// Write to memory.
			var mem = new System.IO.MemoryStream();
			var f = OpenBinaryFormat.ToMemory(mem);
			f.WriteDouble("Age", 80);
			var bytes = mem.ToArray();
			f.Close();
			
			// Read from memory.
			f = OpenBinaryFormat.FromMemory(new System.IO.MemoryStream(bytes));
			f.ReadDocument(delegate (object sender, ReadingFieldEventArgs evt){
				switch (evt.Name) {
				case "Age": {
					var age = f.Read<float>(evt);
					Assert.True(age == 80);
				} break;
				}
			});
		}
		
		[Test()]
		public void TestDoubleToFloatBlock()
		{
			// Write to memory.
			var mem = new System.IO.MemoryStream();
			var f = OpenBinaryFormat.ToMemory(mem);
			// Put the data in a block.
			var pos = f.WriteBeginBlock();
			
			f.WriteDouble("Age", 80);
			var bytes = mem.ToArray();
			
			f.WriteEndBlock(pos);
			f.Close();
			
			// Read from memory.
			f = OpenBinaryFormat.FromMemory(new System.IO.MemoryStream(bytes));
			f.ReadDocument(delegate (object sender, ReadingFieldEventArgs evt){
				switch (evt.Name) {
				case "Age": {
					var age = f.Read<float>(evt);
					Assert.True(age == 80);
				} break;
				}
			});
		}

		[Test()]
		public void TestTripleBlock()
		{
			// The number of nested blocks does not matter.
			var mem = new System.IO.MemoryStream();
			var f = OpenBinaryFormat.ToMemory(mem);
			var block1 = f.WriteBeginBlock();
			var block2 = f.WriteBeginBlock();
			var block3 = f.WriteBeginBlock();
			f.WriteString("Message", "hello");
			f.WriteEndBlock(block3);
			f.WriteEndBlock(block2);
			f.WriteEndBlock(block1);
			var bytes = mem.ToArray();
			f.Close();

			f = OpenBinaryFormat.FromBytes(bytes);
			f.ReadDocument(delegate (object sender, ReadingFieldEventArgs evt) {
				switch (evt.Name) {
				case "Message": {
					var msg = f.Read<string>(evt);
					Assert.True(msg == "hello");
				} break;
				}
			});
		}
	}
}

