using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Zylab.Interview.BinStorage.Index {

	public class RedBlackTreeIndex : IIndex {
		private readonly string _indexFilePath;
		private readonly ReaderWriterLockSlim _locker;
		private readonly TimeSpan _timeout;
		private readonly SortedDictionary<string, IndexData> _tree;

		public RedBlackTreeIndex(string indexFilePath, TimeSpan timeout) {
			_indexFilePath = indexFilePath;
			_timeout = timeout;
			_locker = new ReaderWriterLockSlim();

			_tree = GetOrCreateTree(_indexFilePath);
		}

		public void Add(string key, IndexData data) {
			if(!_locker.TryEnterWriteLock(_timeout))
				throw new TimeoutException($"Timeout {_timeout} expired to add key {key} to index");

			_tree.Add(key, data);
			_locker.ExitWriteLock();
		}

		public IndexData Get(string key) {
			if(!_locker.TryEnterReadLock(_timeout))
				throw new TimeoutException($"Timeout {_timeout} expired to read index by key {key}");

			var indexData = _tree[key];
			_locker.ExitReadLock();

			return indexData;
		}

		public bool Contains(string key) {
			if(!_locker.TryEnterReadLock(_timeout))
				throw new TimeoutException($"Timeout {_timeout} expired to read index by key {key}");

			var contains = _tree.ContainsKey(key);
			_locker.ExitReadLock();

			return contains;
		}

		public void Dispose() {
			var formatter = new BinaryFormatter();
			using(var stream = new FileStream(_indexFilePath, FileMode.Create, FileAccess.Write, FileShare.None)) {
				formatter.Serialize(stream, _tree);
			}
		}

		private static SortedDictionary<string, IndexData> GetOrCreateTree(string indexFilePath) {
			if(File.Exists(indexFilePath)) {
				var formatter = new BinaryFormatter();
				using(var stream = new FileStream(indexFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
					try {
						return (SortedDictionary<string, IndexData>)formatter.Deserialize(stream);
					}
					catch(SerializationException e) {
						// todo: log
						Console.WriteLine(e);
					}
				}
			}

			return new SortedDictionary<string, IndexData>();
		}
	}

}