using System;

namespace Obf
{
	/// <summary>
	/// A binary format that is version-neutral, flexible and fault-tolerant.
	/// 
	/// Each field got a name and type id.
	/// The name is used by the host to determine what kind of type is expected.
	/// 
	/// A negative type id is used internally to automatically convert between common data types.
	/// Negative type ids are reserved for primitive types.
	/// 
	/// The data is divided into block hierarchy that tells where to go when
	/// an unknown type id is read or when data is corrupt.
	/// 
	/// When an error or compability conflict occurs, the rest of the block is not read.
	/// To keep backward compability, enclose extensions to a file format within a block.
	/// </summary>
	public class OpenBinaryFormat
	{
		private System.IO.BinaryWriter w;
		private System.IO.BinaryReader r;
		
		public delegate void ReadingFieldEventHandler(object sender, ReadingFieldEventArgs evt);
		public event ReadingFieldEventHandler ReadingField;
		
		public const int FORMAT_TYPE_BLOCK = -1;
		public const int FORMAT_TYPE_LONG = -100;
		public const int FORMAT_TYPE_INT = -101;
		public const int FORMAT_TYPE_DOUBLE = -200;
		public const int FORMAT_TYPE_FLOAT = -201;
		public const int FORMAT_TYPE_STRING = -300;
		public const int FORMAT_TYPE_BYTES = -400;
		
		public T Read<T>(ReadingFieldEventArgs evt)
		{
			switch (evt.Type) {
			case FORMAT_TYPE_STRING:
				return (T)Convert.ChangeType(r.ReadString(), typeof(T));
			case FORMAT_TYPE_DOUBLE:
				return (T)Convert.ChangeType(r.ReadDouble(), typeof(T));
			case FORMAT_TYPE_FLOAT:
				return (T)Convert.ChangeType(r.ReadSingle(), typeof(T));
			case FORMAT_TYPE_INT:
				return (T)Convert.ChangeType(r.ReadInt32(), typeof(T));
			case FORMAT_TYPE_LONG:
				return (T)Convert.ChangeType(r.ReadInt64(), typeof(T));
			case FORMAT_TYPE_BYTES:
				return (T)Convert.ChangeType(r.ReadBytes((int)r.ReadInt64()), typeof(T));
			}
			
			// Jump to the end of block.
			r.BaseStream.Position = evt.BlockEnd;
			return default(T);
		}

		/// <summary>
		/// Reads document and calls event ReadingField.
		/// You can call this recursively in the event using
		/// 
		/// 	f.ReadDocument(evt.BlockEnd);
		/// 
		/// </summary>
		/// <param name='blockEnd'>
		/// Block end.
		/// </param>
		public void ReadDocument(long blockEnd = -1)
		{
			if (blockEnd < 0) blockEnd = r.BaseStream.Length;

			var evt = new ReadingFieldEventArgs();
			evt.BlockEnd = blockEnd;
			while (r.BaseStream.Position < blockEnd) {
				evt.Name = r.ReadString();
				evt.Type = r.ReadInt32();
				if (evt.Type == FORMAT_TYPE_BLOCK) {
					// Call itself recursively to skip block.
					long blockSize = r.ReadInt64();
					long posEnd = r.BaseStream.Position + blockSize;
					ReadDocument(posEnd);            
				} else {
					// Read field and break if anything wrong happened.
					evt.Handled = false;
					if (this.ReadingField != null) this.ReadingField(this, evt);
					if (!evt.Handled) {
						// Jump to end of block.
						if (r.BaseStream.Position < blockEnd) r.BaseStream.Position = blockEnd;
						if (r.BaseStream.Position > blockEnd) 
							throw new Exception("Read beyond end of block '" + 
							                    evt.Name + "' at position " + evt.BlockEnd);
						break;
					}
				}
				
			}
		}
		
		public static OpenBinaryFormat FromFile(string file)
		{
			OpenBinaryFormat format = new OpenBinaryFormat();
			System.IO.FileStream f = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read);
			format.r = new System.IO.BinaryReader(f);
			
			return format;
		}

		public static OpenBinaryFormat FromBytes(byte[] bytes)
		{
			return FromMemory(new System.IO.MemoryStream(bytes));
		}

		public static OpenBinaryFormat FromMemory(System.IO.MemoryStream mem)
		{
			OpenBinaryFormat format = new OpenBinaryFormat();
			format.r = new System.IO.BinaryReader(mem);

			return format;
		}
		
		public static OpenBinaryFormat ToFile(string file)
		{
			OpenBinaryFormat format = new OpenBinaryFormat();
			System.IO.FileStream f = new System.IO.FileStream(file, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
			format.w = new System.IO.BinaryWriter(f);
			
			return format;
		}

		public static OpenBinaryFormat ToMemory(System.IO.MemoryStream mem)
		{
			OpenBinaryFormat format = new OpenBinaryFormat();
			format.w = new System.IO.BinaryWriter(mem);

			return format;
		}
		
		public void Close()
		{
			if (w != null) {
				// Set the length of file.
				w.BaseStream.SetLength(w.BaseStream.Position);
				w.Close();
			}
			if (r != null) r.Close();
		}
		
		public void WriteDouble(string name, double val)
		{
			w.Write(name);
			w.Write(FORMAT_TYPE_DOUBLE);
			w.Write(val);
		}

		public void WriteFloat(string name, float val)
		{
			w.Write(name);
			w.Write(FORMAT_TYPE_FLOAT);
			w.Write(val);
		}
		
		public void WriteInt(string name, int val)
		{
			w.Write(name);
			w.Write(FORMAT_TYPE_INT);
			w.Write(val);
		}
		
		public void WriteLong(string name, long val)
		{
			w.Write(name);
			w.Write(FORMAT_TYPE_LONG);
			w.Write(val);
		}
		
		public void WriteString(string name, string val)
		{
			w.Write(name);
			w.Write(FORMAT_TYPE_STRING);
			w.Write(val);
		}
		
		public void WriteBytes(string name, byte[] data)
		{
			w.Write(name);
			w.Write(FORMAT_TYPE_BYTES);
			w.Write(data.LongLength);
			w.Write(data);
		}
		
		/// <summary>
		/// Starts a new block of data.
		/// A block is a piece of data that can be skipped if reading an unsupported type.
		/// All potentially unsupported types should be enclosed with a block or else the file will be closed.
		/// </summary>
		/// <returns>
		/// Returns the position of the start of block.
		/// </returns>
		/// <param name='w'>
		/// A binary writer that writes to a file or a stream.
		/// </param>
		public long StartBlock(string name)
		{
			w.Write(name);
			w.Write(FORMAT_TYPE_BLOCK);
			long pos = w.BaseStream.Position;
			w.Write((long)-1);
			
			return pos;
		}
		
		/// <summary>
		/// Ends a block by going back to the start and write the size of the block.
		/// </summary>
		/// <param name='w'>
		/// A binary writer that writes to a file or a stream.
		/// </param>
		/// <param name='pos'>
		/// The position of the start of the block.
		/// </param>
		public void EndBlock(long pos)
		{
			// Write the size of the block.
			long endPos = w.BaseStream.Position;
			w.BaseStream.Position = pos;
			w.Write(endPos-pos-8);
			w.BaseStream.Position = endPos;
		}
		
	}
}

