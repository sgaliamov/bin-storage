using System;
using System.Collections.Generic;
using BinStorage.Errors;

namespace BinStorage.Index.BTree {

	/// <summary>
	///     Generic b-tree
	/// </summary>
	/// <typeparam name="TNode">Node type</typeparam>
	/// <typeparam name="TKey">Key type</typeparam>
	public class BTreeIndex<TNode, TKey> : IIndex {
		private INodeStorage<TNode, TKey> _storage;

		public BTreeIndex(INodeStorage<TNode, TKey> storage) {
			_storage = storage;
		}

		public void Add(string key, IndexData indexData) {
			if(key == null) throw new ArgumentNullException(nameof(key));
			if(indexData == null) throw new ArgumentNullException(nameof(indexData));
			CheckDisposed();
			if(Contains(key)) {
				throw new DuplicateException($"An entry with the same key ({key}) already exists.");
			}

			var root = _storage.GetRoot();
			if(!_storage.IsFull(root)) {
				InsertNonFull(root, key, indexData);
				return;
			}

			var oldRoot = root;
			root = _storage.NewNode();
			_storage.SetRoot(root);
			_storage.AddChildren(root, oldRoot);
			Split(root, 0, oldRoot);
			InsertNonFull(root, key, indexData);
		}

		public bool Contains(string key) {
			if(key == null) throw new ArgumentNullException(nameof(key));
			CheckDisposed();

			IndexData data;
			return Search(_storage.GetRoot(), key, out data);
		}

		public IndexData Get(string key) {
			if(key == null) throw new ArgumentNullException(nameof(key));
			CheckDisposed();

			IndexData data;
			if(!Search(_storage.GetRoot(), key, out data)) {
				throw new KeyNotFoundException($"The given key ({key}) was not present in the dictionary.");
			}

			return data;
		}

		private bool Search(TNode parent, string key, out IndexData found) {
			while(true) {
				int position;
				var isFound = _storage.SearchPosition(parent, key, out found, out position);

				if(isFound) {
					return true;
				}

				if(_storage.IsLeaf(parent)) {
					return false;
				}

				parent = _storage.GetChildren(parent, position);
			}
		}

		private void Split(TNode parent, int positionToInsert, TNode fullNode) {
			var newNode = _storage.NewNode();

			var midKey = _storage.GetKey(fullNode, _storage.Degree - 1);
			_storage.InsertKey(parent, positionToInsert, midKey);
			_storage.InsertChildren(parent, positionToInsert + 1, newNode);

			_storage.MoveRightHalfKeys(newNode, fullNode);
			if(!_storage.IsLeaf(fullNode)) {
				_storage.MoveRightHalfChildrens(newNode, fullNode);
			}

			_storage.Commit(parent);
			_storage.Commit(fullNode);
			_storage.Commit(newNode);
		}

		private void InsertNonFull(TNode parent, string key, IndexData data) {
			while(true) {
				IndexData found;
				int positionToInsert;
				_storage.SearchPosition(parent, key, out found, out positionToInsert);

				if(_storage.IsLeaf(parent)) {
					var newKey = _storage.NewKey(key, data);
					_storage.InsertKey(parent, positionToInsert, newKey);
					_storage.Commit(parent);
					return;
				}

				var child = _storage.GetChildren(parent, positionToInsert);
				if(_storage.IsFull(child)) {
					Split(parent, positionToInsert, child);
					if(_storage.Compare(parent, positionToInsert, key) > 0) {
						positionToInsert++;
						parent = _storage.GetChildren(parent, positionToInsert);
						continue;
					}
				}

				parent = child;
			}
		}

		#region IDisposable

		private void CheckDisposed() {
			if(_disposed) {
				throw new ObjectDisposedException("Index is disposed");
			}
		}

		private bool _disposed;

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if(_disposed)
				return;

			if(disposing) {
				_storage = null;
			}

			_disposed = true;
		}

		~BTreeIndex() {
			Dispose(false);
		}

		#endregion
	}

}