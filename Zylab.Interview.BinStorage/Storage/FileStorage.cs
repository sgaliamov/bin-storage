using System.IO;
using System.Security.Cryptography;
using Zylab.Interview.BinStorage.Index;

namespace Zylab.Interview.BinStorage.Storage {

	public class FileStorage : IStorage {
		private const int DefaultBufferSize = 4096;
		private readonly Stream _reader;
		private readonly Stream _writer;
		public FileStorage(string storageFilePath) {
			_writer = new FileStream(storageFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
			_reader = new FileStream(storageFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		}


		public IndexData Append(Stream data) {
			var indexData = new IndexData {
				Offset = _writer.Position
			};

			var buffer = new byte[DefaultBufferSize];
			using(var md5 = MD5.Create()) {
				int read;
				while((read = data.Read(buffer, 0, buffer.Length)) > 0) {
					_writer.Write(buffer, 0, read);

					md5.TransformBlock(buffer, 0, read, buffer, 0);
				}
				md5.TransformFinalBlock(buffer, 0, buffer.Length);
				indexData.Md5Hash = md5.Hash;
			}

			indexData.Size = _writer.Position - indexData.Offset;

			return indexData;
		}

		public Stream Get(IndexData indexData) {
			_reader.Seek(indexData.Offset, SeekOrigin.Begin);

			return new BufferedStream(_reader);
		}

		public void Dispose() {
			// todo: https://msdn.microsoft.com/en-us/library/system.idisposable(v=vs.110).aspx
			_writer.Dispose();
			_reader.Dispose();
		}
	}

}