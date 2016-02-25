using System.IO;
using System.Security.Cryptography;
using System.Threading;
using Zylab.Interview.BinStorage.Index;

namespace Zylab.Interview.BinStorage.Storage {

	public class FileStorage : IStorage {
		private const int DefaultBufferSize = 4096;
		private readonly int _bufferSize;
		private readonly HashAlgorithm _hashAlgorithm;
		private readonly Stream _reader;
		private readonly FileStream _writer;

		public FileStorage(string storageFilePath, int bufferSize = DefaultBufferSize) {
			_bufferSize = bufferSize;
			_writer = new FileStream(storageFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
			_reader = new FileStream(storageFilePath, FileMode.Open, FileAccess.Read, FileShare.Write);
			_hashAlgorithm = MD5.Create();
		}

		public IndexData Append(Stream data) {
			var indexData = new IndexData {
				Offset = _writer.Position
			};

			var buffer = new byte[_bufferSize];
			var count = data.Read(buffer, 0, _bufferSize);
			if(count == 0) {
				return null;
			}

			var prevCount = count;
			var prevBuffer = Interlocked.Exchange(ref buffer, new byte[_bufferSize]);
			_hashAlgorithm.Initialize();

			do {
				_writer.Write(prevBuffer, 0, prevCount);
				count = data.Read(buffer, 0, _bufferSize);
				if(count > 0) {
					_hashAlgorithm.TransformBlock(prevBuffer, 0, prevCount, null, 0);
					prevCount = count;
					prevBuffer = Interlocked.Exchange(ref buffer, prevBuffer);
				}
				else {
					_hashAlgorithm.TransformFinalBlock(prevBuffer, 0, prevCount);
					break;
				}
			} while(true);

			indexData.Md5Hash = _hashAlgorithm.Hash;
			indexData.Size = _writer.Position - indexData.Offset;

			_writer.Flush(true);

			return indexData;
		}

		public Stream Get(IndexData indexData) {
			_reader.Seek(indexData.Offset, SeekOrigin.Begin);

			return new BufferedStream(_reader, _bufferSize);
		}

		public void Dispose() {
			// todo: https://msdn.microsoft.com/en-us/library/system.idisposable(v=vs.110).aspx
			_writer.Dispose();
			_reader.Dispose();
			_hashAlgorithm.Dispose();
		}
	}

}