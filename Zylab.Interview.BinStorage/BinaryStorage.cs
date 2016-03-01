using System;
using System.IO;
using Zylab.Interview.BinStorage.Errors;
using Zylab.Interview.BinStorage.Index;
using Zylab.Interview.BinStorage.Index.RedBlackTree;
using Zylab.Interview.BinStorage.Storage;

namespace Zylab.Interview.BinStorage {

	public class BinaryStorage : IBinaryStorage {
		private readonly IIndex _index;
		private readonly IStorage _storage;

		public BinaryStorage(StorageConfiguration configuration) {
			_index = new ThreadSafeIndex(
				new RedBlackTreeIndex(
					Path.Combine(configuration.WorkingFolder, configuration.IndexFileName)),
				configuration.IndexTimeout);
			_storage = new FileStorage(Path.Combine(configuration.WorkingFolder, configuration.StorageFileName));
		}

		public void Add(string key, Stream data) {
			CheckDisposed();

			if(_index.Contains(key)) {
				throw new DuplicateException($"An entry with the same key ({key}) already exists.");
			}

			var indexData = _storage.Append(data);
			_index.Add(key, indexData);
		}

		public bool Contains(string key) {
			CheckDisposed();

			return _index.Contains(key);
		}

		public Stream Get(string key) {
			CheckDisposed();

			var indexData = _index.Get(key);

			return _storage.Get(indexData);
		}

		#region IDisposable

		private bool _disposed;

		private void CheckDisposed() {
			if(_disposed) {
				throw new ObjectDisposedException("Binary storage is disposed");
			}
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if(_disposed)
				return;

			_storage.Dispose();
			_index.Dispose();

			_disposed = true;
		}

		~BinaryStorage() {
			Dispose(false);
		}

		#endregion
	}

}