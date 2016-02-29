using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Zylab.Interview.BinStorage.Index.RedBlackTree {

	public class RedBlackTreeIndex : IIndex {
		private readonly string _indexFilePath;
		private readonly ReaderWriterLockSlim _locker;
		private readonly TimeSpan _timeout;
		private readonly SortedDictionary<string, IndexData> _tree;

		public RedBlackTreeIndex(string indexFilePath, TimeSpan timeout) {
			_indexFilePath = indexFilePath;
			_timeout = timeout;
			_locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

			_tree = GetOrCreateTree(_indexFilePath);
		}

		public void Add(string key, IndexData data) {
			try {
				if(!_locker.TryEnterWriteLock(_timeout))
					throw new TimeoutException($"Timeout {_timeout} expired to add key {key} to index");

				_tree.Add(key, data);
			}
			finally {
				_locker.ExitWriteLock();
			}
		}

		public IndexData Get(string key) {
			try {
				if(!_locker.TryEnterReadLock(_timeout))
					throw new TimeoutException($"Timeout {_timeout} expired to read index by key {key}");

				return _tree[key];
			}
			finally {
				_locker.ExitReadLock();
			}
		}

		public bool Contains(string key) {
			try {
				if(!_locker.TryEnterReadLock(_timeout))
					throw new TimeoutException($"Timeout {_timeout} expired to read index by key {key}");

				return _tree.ContainsKey(key);
			}
			finally {
				_locker.ExitReadLock();
			}
		}

		public void Dispose() {
			// todo: https://msdn.microsoft.com/en-us/library/system.idisposable(v=vs.110).aspx
			var formatter = new BinaryFormatter();
			using(var stream = new FileStream(_indexFilePath, FileMode.Create, FileAccess.Write, FileShare.None)) {
				formatter.Serialize(stream, _tree);
			}
		}

		private static SortedDictionary<string, IndexData> GetOrCreateTree(string indexFilePath) {
			SortedDictionary<string, IndexData> tree = null;

			if(File.Exists(indexFilePath)) {
				using(var stream = new FileStream(indexFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
					var formatter = new BinaryFormatter();
					tree = (SortedDictionary<string, IndexData>)formatter.Deserialize(stream);
				}
			}

			return tree ?? new SortedDictionary<string, IndexData>();
		}
	}

}