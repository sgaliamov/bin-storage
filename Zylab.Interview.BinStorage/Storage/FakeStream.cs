using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Zylab.Interview.BinStorage.Storage {

	/// <summary>
	///     Fake stream to read large files from MemoryMappedFile
	/// </summary>
	public class FakeStream : Stream {
		private readonly MemoryMappedFile _file;
		private long _offset;
		private long _position;

		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="file">Memory mapped file</param>
		/// <param name="offset">Data offset</param>
		/// <param name="size">Data size</param>
		public FakeStream(MemoryMappedFile file, long offset, long size) {
			_offset = offset;
			_file = file;

			Length = size;
		}

		public override bool CanRead => true;

		public override bool CanSeek {
			get { throw new NotImplementedException(); }
		}

		public override bool CanWrite {
			get { throw new NotImplementedException(); }
		}

		public override long Length { get; }

		public override long Position {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public override void Flush() {
			throw new NotImplementedException();
		}

		public override long Seek(long offset, SeekOrigin origin) {
			throw new NotImplementedException();
		}

		public override void SetLength(long value) {
			throw new NotImplementedException();
		}

		public override int Read(byte[] buffer, int offset, int count) {
			if(_position >= Length) {
				return 0;
			}

			var size = (long)(count - offset);
			var rest = Length - _position;
			if(rest < size) {
				size = rest;
			}

			using(var reader = _file.CreateViewStream(_offset, size, MemoryMappedFileAccess.Read)) {
				var read = reader.Read(buffer, offset, count);
				_offset += read;
				_position += read;

				return read;
			}
		}

		public override void Write(byte[] buffer, int offset, int count) {
			throw new NotImplementedException();
		}
	}

}