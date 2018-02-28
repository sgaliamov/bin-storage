using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using BinStorage.Errors;

namespace BinStorage.Index.RedBlackTree {

	/// <summary>
	///     Red black tree based on SortedDictionary
	/// </summary>
	public class RedBlackTreeIndex : IIndex {
		private readonly string _indexFilePath;
		private readonly SortedDictionary<string, IndexData> _tree;

		public RedBlackTreeIndex(string indexFilePath) {
			_indexFilePath = indexFilePath;

			_tree = GetOrCreateTree(_indexFilePath);
		}

		public void Add(string key, IndexData data) {
			CheckDisposed();

			if(_tree.ContainsKey(key)) {
				throw new DuplicateException($"An entry with the same key ({key}) already exists.");
			}
			_tree.Add(key, data);
		}

		public bool Contains(string key) {
			CheckDisposed();

			return _tree.ContainsKey(key);
		}

		public IndexData Get(string key) {
			CheckDisposed();

			return _tree[key];
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

		private void SaveIndex() {
			var formatter = new BinaryFormatter();
			using(var stream = new FileStream(_indexFilePath, FileMode.Create, FileAccess.Write, FileShare.None)) {
				formatter.Serialize(stream, _tree);
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
				SaveIndex();
			}

			_disposed = true;
		}

		private bool _disposed;

		private void CheckDisposed() {
			if(_disposed) {
				throw new ObjectDisposedException("Index is disposed");
			}
		}

		~RedBlackTreeIndex() {
			Dispose(false);
		}

		#endregion
	}

}