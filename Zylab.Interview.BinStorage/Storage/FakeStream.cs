using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Zylab.Interview.BinStorage.Storage {

	public class FakeStream : Stream {
		private readonly MemoryMappedFile _file;
		private long _offset;

		public FakeStream(MemoryMappedFile file, long offset, long size) {
			_offset = offset;
			_file = file;

			Length = size;
		}

		public override bool CanRead {
			get { throw new NotImplementedException(); }
		}

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
			using(var reader = _file.CreateViewStream(_offset, count - offset, MemoryMappedFileAccess.Read)) {
				var read = reader.Read(buffer, offset, count);
				_offset += read;

				return read;
			}
		}

		public override void Write(byte[] buffer, int offset, int count) {
			throw new NotImplementedException();
		}
	}

}