using System;
using System.IO;

namespace BinStorage.Storage {

	/// <summary>
	///     Fake stream to read large files from MemoryMappedFile
	/// </summary>
	public class FakeStream : Stream {
		private long _offset;
		private long _position;
		private readonly Func<long, long, byte[], int, int, int> _read;

		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="read">Data reader function</param>
		/// <param name="offset">Data offset</param>
		/// <param name="size">Data size</param>
		public FakeStream(Func<long, long, byte[], int, int, int> read, long offset, long size) {
			_read = read;
			_offset = offset;

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

			var read = _read(_offset, size, buffer, offset, count);
			_offset += read;
			_position += read;

			return read;
		}

		public override void Write(byte[] buffer, int offset, int count) {
			throw new NotImplementedException();
		}
	}

}