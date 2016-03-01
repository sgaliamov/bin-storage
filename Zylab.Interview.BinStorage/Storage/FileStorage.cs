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
		private MemoryMappedViewAccessor _cursorHolder;
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

			return _file.CreateViewStream(indexData.Offset, indexData.Size);
		}

		private IndexData AppendSeekableStream(Stream input) {
			var length = input.Length;
			var cursor = Interlocked.Add(ref _cursor, length) - length;
			EnsureCapacity(length);

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

		private void EnsureCapacity(long count) {
			lock(_lock) {
				if(_cursor + count <= _capacity) return;

				ReleaseFile();
				_capacity <<= 1;
				InitFile();
			}
		}

		private void InitFile() {
			long fileLength;
			_cursor = ReadPosition(out fileLength);
			if(_capacity < fileLength) {
				_capacity = fileLength;
			}

			_file = MemoryMappedFile.CreateFromFile(
				_storageFilePath,
				FileMode.OpenOrCreate,
				null,
				_capacity,
				MemoryMappedFileAccess.ReadWrite);

			_cursorHolder = _file.CreateViewAccessor(0, CursorHolderSize);
			if(_cursor == 0) {
				_cursor = CursorHolderSize;
				_cursorHolder.Write(0, _cursor);
			}
		}

		private long ReadPosition(out long fileLength) {
			using(var file = File.Open(_storageFilePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None)) {
				var buffer = new byte[CursorHolderSize];
				file.Read(buffer, 0, CursorHolderSize);
				fileLength = file.Length;

				return BitConverter.ToInt64(buffer, 0);
			}
		}

		private void ReleaseFile() {
			_cursorHolder.Write(0, _cursor);
			_cursorHolder.Dispose();
			_file.Dispose();
		}

		#region IDisposable

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if(_disposed)
				return;

			ReleaseFile();

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