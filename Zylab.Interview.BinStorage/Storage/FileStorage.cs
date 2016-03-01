﻿using System;
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

		private readonly int _readBufferSize;
		private readonly string _storageFilePath;
		private long _capacity;
		private long _cursor;
		private MemoryMappedFile _mappedFile;
		private MemoryMappedViewAccessor _positionHolder;
		private MemoryMappedViewStream _writer;

		public FileStorage(
			string storageFilePath,
			long capacity = DefaultCapacity,
			int readBufferSize = DefaultReadBufferSize) {
			_storageFilePath = storageFilePath;
			_readBufferSize = readBufferSize;
			_capacity = capacity + CursorHolderSize;

			InitFile();
		}

		public IndexData Append(Stream data) {
			CheckDisposed();

			var indexData = new IndexData {
				Offset = _cursor
			};

			// todo: if input stream is seekable, calculate new _cursor to allow other threads write new data,
			// todo: write nonseekable input stream to separate queue and join it when finish
			var buffer = new byte[_readBufferSize];
			var count = data.Read(buffer, 0, _readBufferSize);
			var prevCount = count;
			var prevBuffer = Interlocked.Exchange(ref buffer, new byte[_readBufferSize]);
			using(var hashAlgorithm = MD5.Create()) {
				do {
					Write(prevBuffer, prevCount);

					count = data.Read(buffer, 0, _readBufferSize);
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
				_positionHolder.Write(0, _cursor);

				indexData.Md5Hash = hashAlgorithm.Hash;
			}
			indexData.Size = _cursor - indexData.Offset;

			return indexData;
		}

		public Stream Get(IndexData indexData) {
			CheckDisposed();

			if(indexData.Size == 0) {
				return Stream.Null;
			}

			return _mappedFile.CreateViewStream(indexData.Offset, indexData.Size);
		}

		private void Write(byte[] buffer, int count) {
			if(_cursor + count > _capacity) {
				ReleaseFile();
				_capacity <<= 1;
				InitFile();
			}

			_writer.Write(buffer, 0, count);
			_cursor += count;
		}

		private void InitFile() {
			long fileLength;
			_cursor = ReadPosition(out fileLength);
			if(_capacity < fileLength) {
				_capacity = fileLength;
			}

			_mappedFile = MemoryMappedFile.CreateFromFile(
				_storageFilePath,
				FileMode.OpenOrCreate,
				null,
				_capacity,
				MemoryMappedFileAccess.ReadWrite);

			_positionHolder = _mappedFile.CreateViewAccessor(0, CursorHolderSize);
			if(_cursor == 0) {
				_cursor = CursorHolderSize;
				_positionHolder.Write(0, _cursor);
			}
			_writer = _mappedFile.CreateViewStream(_cursor, _capacity - _cursor); // todo: read by parts
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
			_writer.Dispose();
			_positionHolder.Dispose();
			_mappedFile.Dispose();
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