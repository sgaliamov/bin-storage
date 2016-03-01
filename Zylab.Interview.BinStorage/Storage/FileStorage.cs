using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Security.Cryptography;
using System.Threading;
using Zylab.Interview.BinStorage.Index;

namespace Zylab.Interview.BinStorage.Storage {

	public class FileStorage : IStorage {
		private const int DefaultReadBufferSize = Constants.Size4Kb;
		private const long DefaultCapacity = Constants.Size1Gb;
		private const int CursorHolderSize = sizeof(long);

		private readonly object _lock = new object();

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

			InitFile();
		}

		public IndexData Append(Stream input) {
			CheckDisposed();

			if(input.CanSeek) {
				return AppendSeekableStream(input);
			}

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

			// todo: write nonseekable input stream to separate queue and join it when finish
			var buffer = new byte[_readBufferSize];
			var count = input.Read(buffer, 0, _readBufferSize);
			var prevCount = count;
			var prevBuffer = Interlocked.Exchange(ref buffer, new byte[_readBufferSize]);

			using(var writer = _file.CreateViewStream(cursor, length, MemoryMappedFileAccess.Write)) // todo: read by parts
			using(var hashAlgorithm = MD5.Create()) {
				do {
					writer.Write(prevBuffer, 0, prevCount);

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

		private void EnsureCapacity(long cursor, long count) {
			lock(_lock) {
				if(cursor + count <= _capacity) return;

				ReleaseFile();
				_capacity <<= 1;
				InitFile();
			}
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