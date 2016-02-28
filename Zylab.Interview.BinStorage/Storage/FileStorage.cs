using System.IO;
using System.IO.MemoryMappedFiles;
using System.Security.Cryptography;
using System.Threading;
using Zylab.Interview.BinStorage.Index;

namespace Zylab.Interview.BinStorage.Storage {

	public class FileStorage : IStorage {
		private const int DefaultReadBufferSize = 0x2800; // 10 KB
		private const long DefaultCapacity = 0x100000; // 256 MB
		private const int PositionHolderSize = sizeof(long);

		private readonly int _readBufferSize;
		private readonly HashAlgorithm _hashAlgorithm;
		private readonly MemoryMappedFile _mappedFile;
		private readonly MemoryMappedViewAccessor _positionHolder;
		private long _position;
		private long _capacity;

		public FileStorage(string storageFilePath, long capacity = DefaultCapacity, int readBufferSize = DefaultReadBufferSize) {
			_capacity = capacity;
			_readBufferSize = readBufferSize;
			_mappedFile = MemoryMappedFile.CreateFromFile(
				storageFilePath,
				FileMode.OpenOrCreate,
				null,
				capacity + PositionHolderSize,
				MemoryMappedFileAccess.ReadWrite);

			_positionHolder = _mappedFile.CreateViewAccessor(0, PositionHolderSize);
			_position = InitPosition(_positionHolder);

			_hashAlgorithm = MD5.Create();
		}

		public IndexData Append(Stream data) {
			var indexData = new IndexData {
				Offset = _position
			};

			var buffer = new byte[_readBufferSize];
			var count = data.Read(buffer, 0, _readBufferSize);
			if(count == 0) {
				return null;
			}

			var prevCount = count;
			var prevBuffer = Interlocked.Exchange(ref buffer, new byte[_readBufferSize]);
			_hashAlgorithm.Initialize();
			using(var writer = _mappedFile.CreateViewStream(_position, 0)) {
				do {
					writer.Write(prevBuffer, 0, prevCount);
					_position += prevCount;

					count = data.Read(buffer, 0, _readBufferSize);
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
				writer.Flush();
			}
			_positionHolder.Write(0, _position);

			indexData.Md5Hash = _hashAlgorithm.Hash;
			indexData.Size = _position - indexData.Offset;

			return indexData;
		}

		public Stream Get(IndexData indexData) {
			return _mappedFile.CreateViewStream(indexData.Offset, indexData.Size);
		}

		public void Dispose() {
			// todo: https://msdn.microsoft.com/en-us/library/system.idisposable(v=vs.110).aspx
			_positionHolder.Dispose();
			_mappedFile.Dispose();
			_hashAlgorithm.Dispose();
		}

		private static long InitPosition(UnmanagedMemoryAccessor viewAccessor) {
			var position = viewAccessor.ReadInt64(0);
			if(position != 0) return position;

			position = PositionHolderSize;
			viewAccessor.Write(0, position);

			return position;
		}
	}

}