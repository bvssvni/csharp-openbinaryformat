using System;
using System.Collections.Generic;

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

		public delegate void ReadingFieldDelegate(object sender, IEnumerator<bool> iterator);
		
		public const int FORMAT_TYPE_BLOCK = -1;
		public const int FORMAT_TYPE_LONG = -100;
		public const int FORMAT_TYPE_INT = -101;
		public const int FORMAT_TYPE_DOUBLE = -200;
		public const int FORMAT_TYPE_FLOAT = -201;
		public const int FORMAT_TYPE_STRING = -300;
		public const int FORMAT_TYPE_BYTES = -400;
		
		public T Read<T>(Field evt)
		{
			switch (evt.Type) {
			case FORMAT_TYPE_STRING:
				evt.Handled = true;
				return (T)Convert.ChangeType(r.ReadString(), typeof(T));
			case FORMAT_TYPE_DOUBLE:
				evt.Handled = true;
				return (T)Convert.ChangeType(r.ReadDouble(), typeof(T));
			case FORMAT_TYPE_FLOAT:
				evt.Handled = true;
				return (T)Convert.ChangeType(r.ReadSingle(), typeof(T));
			case FORMAT_TYPE_INT:
				evt.Handled = true;
				return (T)Convert.ChangeType(r.ReadInt32(), typeof(T));
			case FORMAT_TYPE_LONG:
				evt.Handled = true;
				return (T)Convert.ChangeType(r.ReadInt64(), typeof(T));
			case FORMAT_TYPE_BYTES:
				evt.Handled = true;
				return (T)Convert.ChangeType(r.ReadBytes((int)r.ReadInt64()), typeof(T));
			}
			
			// Jump to the end of block.
			evt.Handled = false;
			return default(T);
		}

		private Field m_currentField;

		public Field CurrentField
		{
			get {
				return m_currentField;
			}
		}

		/// <summary>
		/// Returns an iterator that keeps track of the fields in the file or stream.
		/// This creates a "root block" iterator for nested blocks.
		/// </summary>
		private IEnumerator<bool> Fields()
		{
			long end = r.BaseStream.Length;
			m_currentField = new Field();
			while (r.BaseStream.Position < end) {
				m_currentField.Name = r.ReadString();
				m_currentField.Type = r.ReadInt32();
				m_currentField.Handled = false;
				m_currentField.ReadIt = false;

				yield return true;
			}
		}

		/// <summary>
		/// Constructs a field iterator for a block within the context of a parent field iterator.
		/// </summary>
		/// <returns>
		/// Returns an iterator for a block.
		/// </returns>
		/// <param name='fields'>
		/// The parent block to restrict the domain of the iterator.
		/// </param>
		/// <param name='blockEnd'>
		/// The end position of the block.
		/// </param>
		private IEnumerator<bool> BlockFields(IEnumerator<bool> fields, long blockEnd)
		{
			while (r.BaseStream.Position < blockEnd && fields.MoveNext()) {
				yield return true;
			}
		}

		public void ReadField(ReadingFieldDelegate func, IEnumerator<bool> fields = null)
		{
			if (fields == null) fields = Fields();
			if (m_currentField == null || m_currentField.Handled) fields.MoveNext();

			do {
				if (m_currentField.Type == FORMAT_TYPE_BLOCK) {
					// Start on a new block.
					var length = r.ReadInt64();
					var pos = r.BaseStream.Position;
					var blockFields = BlockFields(fields, pos + length);
					m_currentField.Handled = true;

					ReadField(func, blockFields);
					if (m_currentField.ReadIt) {
						Console.WriteLine("ReadIt");
						return;
					}

					if (!m_currentField.Handled) {
						// Jump to end of block.
						r.BaseStream.Position = pos + length;
						m_currentField.Handled = true;

						// Console.WriteLine("Jumping");
					}
				} else {
					// Deal with the field.
					func(this, fields);

					if (!m_currentField.Handled) {
						return;
					}
				}
			} while (fields.MoveNext());
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
			format.r = new System.IO.BinaryReader(mem, System.Text.Encoding.UTF8);

			return format;
		}
		
		public static OpenBinaryFormat ToFile(string file)
		{
			OpenBinaryFormat format = new OpenBinaryFormat();
			System.IO.FileStream f = new System.IO.FileStream(file, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
			format.w = new System.IO.BinaryWriter(f, System.Text.Encoding.UTF8);
			
			return format;
		}

		public static OpenBinaryFormat ToMemory(System.IO.MemoryStream mem)
		{
			OpenBinaryFormat format = new OpenBinaryFormat();
			format.w = new System.IO.BinaryWriter(mem, System.Text.Encoding.UTF8);

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

