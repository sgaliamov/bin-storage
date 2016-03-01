using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using Zylab.Interview.BinStorage.Errors;
using Zylab.Interview.BinStorage.Index;

namespace Zylab.Interview.BinStorage.Storage {

	public class FileStorage : IStorage {
		private const int DefaultReadBufferSize = Constants.Size4Kb;
		private const int BigReadBufferSize = Constants.Size16Mb;
		private const long DefaultCapacity = Constants.Size1Gb;
		private const int CursorHolderSize = sizeof(long);

		private readonly ReaderWriterLockSlim _lock;
		private readonly int _readBufferSize;
		private readonly string _storageFilePath;
		private long _capacity;
		private long _cursor;
		private MemoryMappedFile _file;

		public FileStorage(
			string storageFilePath,
			long capacity = DefaultCapacity,
			int readBufferSize = DefaultReadBufferSize) {
			_storageFilePath = storageFilePath;
			_readBufferSize = readBufferSize;
			_capacity = capacity + CursorHolderSize;
			_lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

			InitFile();
		}

		public IndexData Append(Stream input) {
			CheckDisposed();

			if(input.CanSeek) {
				return AppendSeekableStream(input);
			}

			// todo: write nonseekable input stream to separate queue and join it when finish
			throw new NotImplementedException();
		}

		public Stream Get(IndexData indexData) {
			CheckDisposed();

			if(indexData.Size == 0) {
				return Stream.Null;
			}

			return _file.CreateViewStream(indexData.Offset, indexData.Size, MemoryMappedFileAccess.Read);
		}

		private IndexData AppendSeekableStream(Stream input) {
			var length = input.Length;
			var cursor = Interlocked.Add(ref _cursor, length) - length;
			EnsureCapacity(cursor, length);

			var indexData = new IndexData {
				Offset = cursor,
				Size = length
			};

			var buffer = new byte[_readBufferSize];
			var count = input.Read(buffer, 0, _readBufferSize);
			var prevCount = count;
			var prevBuffer = Interlocked.Exchange(ref buffer, new byte[_readBufferSize]);

			using(var hashAlgorithm = MD5.Create()) {
				//if(length > BigReadBufferSize) {
				//	while(cursor < length) {
				//		Write(input, cursor, BigReadBufferSize, length, buffer, prevBuffer, prevCount, hashAlgorithm);
				//		cursor += BigReadBufferSize;
				//	}
				//	if(cursor > length) {
				//		var size = cursor - length;
				//		cursor = BigReadBufferSize - size;
				//		Write(input, cursor, size, length, buffer, prevBuffer, prevCount, hashAlgorithm);
				//	}
				//}
				//else {
					Write(input, cursor, length, buffer, prevBuffer, prevCount, hashAlgorithm);
				//}

				indexData.Md5Hash = hashAlgorithm.Hash;
			}

			return indexData;
		}

		private void Write(
			Stream input,
			long cursor,
			long length,
			byte[] buffer,
			byte[] prevBuffer,
			int prevCount,
			ICryptoTransform hashAlgorithm) {
			_lock.EnterReadLock();
			try {
				using(var writer = _file.CreateViewStream(cursor, length, MemoryMappedFileAccess.Write)) {
					do {
						writer.Write(prevBuffer, 0, prevCount);
						var count = input.Read(buffer, 0, _readBufferSize);
						cursor += count;

						if(count > 0) {
							hashAlgorithm.TransformBlock(prevBuffer, 0, prevCount, null, 0);
							prevCount = count;
							prevBuffer = Interlocked.Exchange(ref buffer, prevBuffer);
						}
						else {
							hashAlgorithm.TransformFinalBlock(prevBuffer, 0, prevCount);
							break;
						}
					} while(true);
				}
			}
			finally {
				_lock.ExitReadLock();
			}
		}

		private void EnsureCapacity(long cursor, long length) {
			try {
				_lock.EnterWriteLock();

				CheckSpace(cursor, length);
				if(cursor + length <= _capacity) return;

				ReleaseFile();
				_capacity <<= 1;
				InitFile();
			}
			finally {
				_lock.ExitWriteLock();
			}
		}

		private void CheckSpace(long cursor, long length) {
			if(GetFreeSpace() >= cursor + length) return;

			ReleaseFile();
			throw new NotEnoughDiskSpaceException("There is not enough space on the disk.");
		}

		private long GetFreeSpace() {
			var pathRoot = Path.GetPathRoot(_storageFilePath);

			return DriveInfo.GetDrives()
				.Where(drive => drive.IsReady && string.Equals(drive.Name, pathRoot, StringComparison.InvariantCultureIgnoreCase))
				.Select(x => x.TotalFreeSpace)
				.First();
		}

		private void InitFile() {
			if(File.Exists(_storageFilePath)) {
				var length = new FileInfo(_storageFilePath).Length;
				if(_capacity < length) {
					_capacity = length;
				}
			}

			_file = MemoryMappedFile.CreateFromFile(
				_storageFilePath,
				FileMode.OpenOrCreate,
				null,
				_capacity,
				MemoryMappedFileAccess.ReadWrite);

			using(var cursorHolder = _file.CreateViewAccessor(0, CursorHolderSize, MemoryMappedFileAccess.Read)) {
				_cursor = cursorHolder.ReadInt64(0);
			}
			if(_cursor == 0) {
				_cursor = CursorHolderSize;
			}
		}

		private void ReleaseFile(bool disposeFile = true) {
			using(var cursorHolder = _file.CreateViewAccessor(0, CursorHolderSize, MemoryMappedFileAccess.Write)) {
				cursorHolder.Write(0, _cursor);
			}

			if(disposeFile) {
				_file.Dispose();
			}
		}

		#region IDisposable

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if(_disposed)
				return;

			if(disposing) {
				ReleaseFile(false);
			}
			_file.Dispose();

			_disposed = true;
		}

		private bool _disposed;

		private void CheckDisposed() {
			if(_disposed) {
				throw new ObjectDisposedException("Binary storage is disposed");
			}
		}

		~FileStorage() {
			Dispose(false);
		}

		#endregion
	}

}