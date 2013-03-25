/*

OpenBinaryFormat - A binary format that is version-neutral, flexible and fault-tolerant.
BSD license.
by Sven Nilsen, 2012
http://www.cutoutpro.com
Version: 0.001 in angular degrees version notation
http://isprogrammingeasy.blogspot.no/2012/08/angular-degrees-versioning-notation.html

0.001	Added in-memory gzip compression on files that ends with ".gz".

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
1. Redistributions of source code must retain the above copyright notice, this
list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
this list of conditions and the following disclaimer in the documentation
and/or other materials provided with the distribution.
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies,
either expressed or implied, of the FreeBSD Project.

*/

using System;
using System.Collections.Generic;
using System.IO.Compression;

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
		// This field is set when writing to a compressed file ending with '.gz'.
		private string m_gzFile = null;
		private System.IO.BinaryWriter w;
		private System.IO.BinaryReader r;
		
		public const int FORMAT_TYPE_BLOCK = -1;
		public const int FORMAT_TYPE_LONG = -100;
		public const int FORMAT_TYPE_INT = -101;
		public const int FORMAT_TYPE_DOUBLE = -200;
		public const int FORMAT_TYPE_FLOAT = -201;
		public const int FORMAT_TYPE_STRING = -300;
		public const int FORMAT_TYPE_BYTES = -400;
		
		public static OpenBinaryFormat FromFile(string file)
		{
			OpenBinaryFormat format = new OpenBinaryFormat();
			System.IO.FileStream f = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read);
			if (file.EndsWith(".gz")) {
				var mem = new System.IO.MemoryStream();
				using (GZipStream gzip = new GZipStream(f, CompressionMode.Decompress))
				{
					// Use 4 KiB as buffer when decompressing.
					var bufferSize = 1024 << 2;
					var buffer = new byte[bufferSize];
					var readBytes = bufferSize;
					while (readBytes == bufferSize) {
						readBytes = gzip.Read(buffer, 0, buffer.Length);
						mem.Write(buffer, 0, readBytes);
					}
				}
				var bytes = mem.ToArray();
				f.Close();
				format.r = new System.IO.BinaryReader(new System.IO.MemoryStream(bytes), System.Text.Encoding.UTF8);
			} else {
				format.r = new System.IO.BinaryReader(f);
			}
			
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
		
		public static OpenBinaryFormat FromStream(System.IO.Stream stream)
		{
			OpenBinaryFormat format = new OpenBinaryFormat();
			format.r = new System.IO.BinaryReader(stream, System.Text.Encoding.UTF8);
			
			return format;
		}
		
		public static OpenBinaryFormat ToFile(string file)
		{
			OpenBinaryFormat format = new OpenBinaryFormat();
			if (file.EndsWith(".gz")) {
				var mem = new System.IO.MemoryStream();
				format.w = new System.IO.BinaryWriter(mem, System.Text.Encoding.UTF8);
				format.m_gzFile = file;
			} else {
				System.IO.FileStream f = new System.IO.FileStream(file, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
				format.w = new System.IO.BinaryWriter(f, System.Text.Encoding.UTF8);
			}
			
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
				
				if (m_gzFile != null) {
					// Save the file compressed to disc.
					System.IO.FileStream f = new System.IO.FileStream(m_gzFile, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
					GZipStream gzip = new GZipStream(f, CompressionMode.Compress);
					var bytes = ((System.IO.MemoryStream)w.BaseStream).ToArray();
					gzip.Write(bytes, 0, bytes.Length);
					gzip.Close();
				}
				
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
		public long StartBlock(string id)
		{
			if (w != null) {
				w.Write(id);
				w.Write(FORMAT_TYPE_BLOCK);
				long pos = w.BaseStream.Position;
				w.Write((long)-1);
				return pos;
			} else {
				var rId = r.ReadString();
				if (rId != id) throw new Exception("Expected '" + id + "' got '" + rId + "'.");
				
				var rType = r.ReadInt32();
				if (rType != FORMAT_TYPE_BLOCK) throw new Exception("Not a block type.");
				
				return r.BaseStream.Position + r.ReadInt64();
			}
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
			if (w != null) {
				long endPos = w.BaseStream.Position;
				w.BaseStream.Position = pos;
				w.Write(endPos - pos);
				w.BaseStream.Position = endPos;
			} else {
				r.BaseStream.Position = pos;
				
				// Remove read values.
				m_readValues.Clear();
				m_errors.Clear();
			}
		}
		
		private Dictionary<string, object> m_readValues = new Dictionary<string, object>();
		private Dictionary<string, string> m_errors = new Dictionary<string, string>();
		
		public Dictionary<string, string> Errors
		{
			get
			{return m_errors;}
		}
		
		public T Read<T>(string id, T defaultValue, long blockEnd = -1)
		{
			if (blockEnd == -1) blockEnd = r.BaseStream.Length;
			
			while (!m_readValues.ContainsKey(id) && r.BaseStream.Position < blockEnd) {
				var rId = r.ReadString();
				var rType = r.ReadInt32();
				object val = null;
				
				switch (rType) {
					case FORMAT_TYPE_INT:
						val = r.ReadInt32();
						break;
					case FORMAT_TYPE_LONG:
						val = r.ReadInt64();
						break;
					case FORMAT_TYPE_DOUBLE:
						val = r.ReadDouble();
						break;
					case FORMAT_TYPE_FLOAT:
						val = r.ReadSingle();
						break;
					case FORMAT_TYPE_STRING:
						val = r.ReadString();
						break;
					case FORMAT_TYPE_BYTES:
						val = r.ReadBytes((int)r.ReadInt64());
						break;
					case FORMAT_TYPE_BLOCK:
						// Read the block and look for the value.
						var subVal = Read<T>(id, defaultValue, r.BaseStream.Position + r.ReadInt64());
						if (m_readValues.ContainsKey(id)) return subVal;
						
						// If it does not exists, continue searching within this block.
						continue;
					default:
						// Unknown type.
						// Jump to end of block.
						m_errors.Add(id, "Unknown type: " + rType);
						r.BaseStream.Position = blockEnd;
						break;
				}
				
				m_readValues.Add(rId, val);
			}
			
			if (!m_readValues.ContainsKey(id)) return defaultValue;
			
			var objVal = m_readValues[id];
			return (T)Convert.ChangeType(objVal, typeof(T));
		}
	}
}

