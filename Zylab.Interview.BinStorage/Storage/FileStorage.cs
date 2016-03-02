using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using Zylab.Interview.BinStorage.Errors;
using Zylab.Interview.BinStorage.Index;

namespace Zylab.Interview.BinStorage.Storage {

	/// <summary>
	///     File data storage
	/// </summary>
	public class FileStorage : IStorage {
		private const int DefaultReadBufferSize = Sizes.Size4Kb;
		private const long DefaultCapacity = Sizes.Size1Gb;
		private const int CursorHolderSize = sizeof(long);

		private readonly ReaderWriterLockSlim _lock;
		private readonly int _readBufferSize;
		private readonly string _storageFilePath;
		private long _capacity;
		private long _cursor;
		private MemoryMappedFile _file;

		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="storageFilePath">Path to storage file</param>
		/// <param name="capacity">
		///     Initial capacity of the file
		///     Capacity automatically increased to file size, or twice on overflow
		/// </param>
		/// <param name="readBufferSize">Buffer size for reading from append stream</param>
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
			if(input == null) throw new ArgumentNullException(nameof(input));
			CheckDisposed();

			if(input.CanSeek) {
				return AppendSeekableStream(input);
			}

			try {
				_lock.EnterWriteLock();
				return AppendNonSeekableStream(input);
			}
			finally {
				_lock.ExitWriteLock();
			}
		}

		public Stream Get(IndexData indexData) {
			if(indexData == null) throw new ArgumentNullException(nameof(indexData));
			CheckDisposed();

			if(indexData.Size == 0) {
				return Stream.Null;
			}

			return new FakeStream(
				(dataOffset, dataSize, buffer, offset, count) => {
					try {
						_lock.EnterReadLock();
						using(var reader = _file.CreateViewStream(dataOffset, dataSize, MemoryMappedFileAccess.Read)) {
							return reader.Read(buffer, offset, count);
						}
					}
					finally {
						_lock.ExitReadLock();
					}
				},
				indexData.Offset,
				indexData.Size);
		}


		private IndexData AppendNonSeekableStream(Stream input) {
			var start = _cursor;
			var indexData = new IndexData {
				Offset = start
			};

			var buffer = new byte[_readBufferSize];
			var count = input.Read(buffer, 0, _readBufferSize);
			var prevCount = count;
			var prevBuffer = Interlocked.Exchange(ref buffer, new byte[_readBufferSize]);

			using(var hashAlgorithm = MD5.Create()) {
				do {
					if(prevCount > 0) {
						EnsureCapacity(_cursor, prevCount);
						using(var writer = _file.CreateViewStream(_cursor, prevCount, MemoryMappedFileAccess.Write)) {
							writer.Write(prevBuffer, 0, prevCount);
							_cursor += prevCount;
						}
					}

					count = input.Read(buffer, 0, _readBufferSize);
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

				indexData.Md5Hash = hashAlgorithm.Hash;
			}

			indexData.Size = _cursor - start;

			return indexData;
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
				do {
					_lock.EnterReadLock();
					try {
						if(prevCount > 0) {
							using(var writer = _file.CreateViewStream(cursor, prevCount, MemoryMappedFileAccess.Write)) {
								writer.Write(prevBuffer, 0, prevCount);
								cursor += prevCount;
							}
						}
					}
					finally {
						_lock.ExitReadLock();
					}

					count = input.Read(buffer, 0, _readBufferSize);
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

				indexData.Md5Hash = hashAlgorithm.Hash;
			}

			return indexData;
		}

		private void EnsureCapacity(long cursor, long length) {
			try {
				_lock.EnterWriteLock();

				CheckSpace(cursor, length);
				var required = cursor + length;
				if(required <= _capacity) return;

				ReleaseFile();
				_capacity <<= 1;
				if(required > _capacity) {
					_capacity = required;
				}
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