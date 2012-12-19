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
			f.ReadingField += delegate (object sender, ReadingFieldEventArgs evt){
				switch (evt.Name) {
				case "Age": {
					double age = f.Read<double>(evt);
					Assert.True(age == 80);
					evt.Handled = true;
				} break;
				}
			};
			f.ReadDocument();
		}

		[Test()]
		public void TestDoubleBlock()
		{
			// Write to memory.
			var mem = new System.IO.MemoryStream();
			var f = OpenBinaryFormat.ToMemory(mem);
			// Put the data in a block.
			var pos = f.StartBlock("v1.0");

			f.WriteDouble("Age", 80);
			var bytes = mem.ToArray();

			f.EndBlock(pos);
			f.Close();
			
			// Read from memory.
			f = OpenBinaryFormat.FromMemory(new System.IO.MemoryStream(bytes));
			f.ReadingField += delegate (object sender, ReadingFieldEventArgs evt){
				switch (evt.Name) {
				case "Age": {
					double age = f.Read<double>(evt);
					Assert.True(age == 80);
					evt.Handled = true;
				} break;
				}
			};
			f.ReadDocument();
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
			f.ReadingField += delegate (object sender, ReadingFieldEventArgs evt){
				switch (evt.Name) {
				case "Age": {
					float age = f.Read<float>(evt);
					Console.WriteLine(age.ToString());
					Assert.True(age == 80);
					evt.Handled = true;
				} break;
				}
			};
			f.ReadDocument();
		}
		
		[Test()]
		public void TestFloatBlock()
		{
			// Write to memory.
			var mem = new System.IO.MemoryStream();
			var f = OpenBinaryFormat.ToMemory(mem);
			// Put the data in a block.
			var pos = f.StartBlock("v1.0");
			
			f.WriteFloat("Age", 80);
			var bytes = mem.ToArray();
			
			f.EndBlock(pos);
			f.Close();
			
			// Read from memory.
			f = OpenBinaryFormat.FromMemory(new System.IO.MemoryStream(bytes));
			f.ReadingField += delegate (object sender, ReadingFieldEventArgs evt){
				switch (evt.Name) {
				case "Age": {
					float age = f.Read<float>(evt);
					Assert.True(age == 80);
					evt.Handled = true;
				} break;
				}
			};
			f.ReadDocument();
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
			f.ReadingField += delegate (object sender, ReadingFieldEventArgs evt){
				switch (evt.Name) {
				case "Age": {
					var age = f.Read<double>(evt);
					Console.WriteLine(age.ToString());
					Assert.True(age == 80);
					evt.Handled = true;
				} break;
				}
			};
			f.ReadDocument();
		}
		
		[Test()]
		public void TestFloatToDoubleBlock()
		{
			// Write to memory.
			var mem = new System.IO.MemoryStream();
			var f = OpenBinaryFormat.ToMemory(mem);
			// Put the data in a block.
			var pos = f.StartBlock("v1.0");
			
			f.WriteFloat("Age", 80);
			var bytes = mem.ToArray();
			
			f.EndBlock(pos);
			f.Close();
			
			// Read from memory.
			f = OpenBinaryFormat.FromMemory(new System.IO.MemoryStream(bytes));
			f.ReadingField += delegate (object sender, ReadingFieldEventArgs evt){
				switch (evt.Name) {
				case "Age": {
					var age = f.Read<double>(evt);
					Assert.True(age == 80);
					evt.Handled = true;
				} break;
				}
			};
			f.ReadDocument();
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
			f.ReadingField += delegate (object sender, ReadingFieldEventArgs evt){
				switch (evt.Name) {
				case "Age": {
					var age = f.Read<float>(evt);
					Assert.True(age == 80);
					evt.Handled = true;
				} break;
				}
			};
			f.ReadDocument();
		}
		
		[Test()]
		public void TestDoubleToFloatBlock()
		{
			// Write to memory.
			var mem = new System.IO.MemoryStream();
			var f = OpenBinaryFormat.ToMemory(mem);
			// Put the data in a block.
			var pos = f.StartBlock("v1.0");
			
			f.WriteDouble("Age", 80);
			var bytes = mem.ToArray();
			
			f.EndBlock(pos);
			f.Close();
			
			// Read from memory.
			f = OpenBinaryFormat.FromMemory(new System.IO.MemoryStream(bytes));
			f.ReadingField += delegate (object sender, ReadingFieldEventArgs evt){
				switch (evt.Name) {
				case "Age": {
					var age = f.Read<float>(evt);
					Assert.True(age == 80);
					evt.Handled = true;
				} break;
				}
			};
			f.ReadDocument();
		}

		[Test()]
		public void TestTripleBlock()
		{
			// The number of nested blocks does not matter.
			var mem = new System.IO.MemoryStream();
			var f = OpenBinaryFormat.ToMemory(mem);
			var block1 = f.StartBlock("v1.0");
			var block2 = f.StartBlock("v1.1");
			var block3 = f.StartBlock("v1.2");
			f.WriteString("Message", "hello");
			f.EndBlock(block3);
			f.EndBlock(block2);
			f.EndBlock(block1);
			var bytes = mem.ToArray();
			f.Close();

			f = OpenBinaryFormat.FromBytes(bytes);
			f.ReadingField += delegate (object sender, ReadingFieldEventArgs evt) {
				switch (evt.Name) {
				case "Message": {
					var msg = f.Read<string>(evt);
					Assert.True(msg == "hello");
					evt.Handled = true;
				} break;
				}
			};
			f.ReadDocument();
		}

		[Test()]
		public void TestTripleBlockIgnore()
		{
			// The number of nested blocks does not matter.
			var mem = new System.IO.MemoryStream();
			var f = OpenBinaryFormat.ToMemory(mem);
			var block1 = f.StartBlock("v1.0");
			var block2 = f.StartBlock("v1.1");
			var block3 = f.StartBlock("v1.2");
			f.WriteString("egasseM", "hello");
			f.EndBlock(block3);
			f.EndBlock(block2);
			f.EndBlock(block1);
			f.WriteString("Message", "goodbye");
			var bytes = mem.ToArray();
			f.Close();
			
			f = OpenBinaryFormat.FromBytes(bytes);
			f.ReadingField += delegate (object sender, ReadingFieldEventArgs evt) {
				switch (evt.Name) {
				case "Message": {
					var msg = f.Read<string>(evt);
					Assert.True(msg == "goodbye");
					evt.Handled = true;
				} break;
				}
			};
			f.ReadDocument();
		}

		[Test()]
		public void TestSequential()
		{
			var mem = new System.IO.MemoryStream();
			var f = OpenBinaryFormat.ToMemory(mem);
			f.WriteDouble("x", 1);
			f.WriteDouble("x", 2);
			f.WriteDouble("x", 3);
			f.WriteDouble("x", 4);
			var bytes = mem.ToArray();
			f.Close();
			
			f = OpenBinaryFormat.FromBytes(bytes);
			double res = 1;
			f.ReadingField += delegate (object sender, ReadingFieldEventArgs evt) {
				switch (evt.Name) {
				case "x": {
					double val = f.Read<double>(evt);
					Assert.True(val == res);
					res++;
					evt.Handled = true;
				} break;
				}
			};
			f.ReadDocument();
		}

		[Test()]
		public void TestIgnore()
		{
			// Write a block of unknown data and make sure it skips correctly.
			var mem = new System.IO.MemoryStream();
			var f = OpenBinaryFormat.ToMemory(mem);
			long pos = f.StartBlock("v1.0");
			f.WriteDouble("x", 1);
			f.WriteDouble("x", 2);
			f.WriteDouble("x", 3);
			f.WriteDouble("x", 4);
			f.EndBlock(pos);
			f.WriteDouble("y", 5);
			var bytes = mem.ToArray();
			f.Close();
			
			f = OpenBinaryFormat.FromBytes(bytes);
			f.ReadingField += delegate (object sender, ReadingFieldEventArgs evt) {
				switch (evt.Name) {
				case "y": {
					double val = f.Read<double>(evt);
					Assert.True(val == 5);
					evt.Handled = true;
				} break;
				}
			};
			f.ReadDocument();
		}

		[Test()]
		public void TestWrongCast()
		{
			// Cast between incompatible types and check that skipping block happens correctly.
			var mem = new System.IO.MemoryStream();
			var f = OpenBinaryFormat.ToMemory(mem);
			long pos = f.StartBlock("v1.0");
			f.WriteDouble("y", 5);
			f.EndBlock(pos);
			pos = f.StartBlock("v2.0");
			f.WriteDouble("y", 5);
			f.EndBlock(pos);
			var bytes = mem.ToArray();
			f.Close();

			int wasConverted = 0;
			int wasError = 0;
			f = OpenBinaryFormat.FromBytes(bytes);
			f.ReadingField += delegate (object sender, ReadingFieldEventArgs evt) {
				switch (evt.Name) {
				case "y": {
					try
					{
						f.Read<byte[]>(evt);
						evt.Handled = true;
						wasConverted++;
					} catch {
						wasError++;
					}
				} break;
				}
			};
			f.ReadDocument();

			Assert.True(wasConverted == 0);
			Assert.True(wasError == 2);
		}
	}
}

