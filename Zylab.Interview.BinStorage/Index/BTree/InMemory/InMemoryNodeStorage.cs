using System;

namespace Zylab.Interview.BinStorage.Index.BTree.InMemory {

	/// <summary>
	///     Stores nodes in memory
	///     All data stored in simple Lists
	/// </summary>
	public class InMemoryNodeStorage : INodeStorage<InMemoryNode, IndexDataKey> {
		private InMemoryNode _root;

		public InMemoryNodeStorage(int degree) {
			Degree = degree;
		}

		public void AddChildren(InMemoryNode node, InMemoryNode children) {
			CheckDisposed();

			node.Childrens.Add(children);
		}

		public void Commit(InMemoryNode node) {
			CheckDisposed();
		}

		public int Compare(InMemoryNode node, int keyIndex, string key) {
			CheckDisposed();

			return string.Compare(key, node.Keys[keyIndex].Key, StringComparison.OrdinalIgnoreCase);
		}

		public InMemoryNode GetChildren(InMemoryNode node, int position) {
			CheckDisposed();

			return node.Childrens[position];
		}

		public IndexDataKey GetKey(InMemoryNode node, int position) {
			CheckDisposed();

			return node.Keys[position];
		}

		public InMemoryNode GetRoot() {
			CheckDisposed();

			return _root ?? (_root = NewNode());
		}

		public void InsertChildren(InMemoryNode node, int position, InMemoryNode children) {
			CheckDisposed();

			node.Childrens.Insert(position, children);
		}

		public void InsertKey(InMemoryNode node, int position, IndexDataKey key) {
			CheckDisposed();

			node.Keys.Insert(position, key);
		}

		public bool IsFull(InMemoryNode node) {
			CheckDisposed();

			return node.Keys.Count == 2 * Degree - 1;
		}

		public bool IsLeaf(InMemoryNode node) {
			CheckDisposed();

			return node.Childrens.Count == 0;
		}

		public void MoveRightHalfChildrens(InMemoryNode newNode, InMemoryNode fullNode) {
			CheckDisposed();

			var range = fullNode.Childrens.GetRange(Degree, Degree);
			newNode.Childrens.AddRange(range);
			fullNode.Childrens.RemoveRange(Degree, Degree);
		}

		public void MoveRightHalfKeys(InMemoryNode newNode, InMemoryNode fullNode) {
			CheckDisposed();

			var rangeKeys = fullNode.Keys.GetRange(Degree, Degree - 1);
			newNode.Keys.AddRange(rangeKeys);
			fullNode.Keys.RemoveRange(Degree - 1, Degree);
		}

		public IndexDataKey NewKey(string key, IndexData data) {
			CheckDisposed();

			return new IndexDataKey {
				Key = key,
				Data = data
			};
		}

		public InMemoryNode NewNode() {
			CheckDisposed();

			return new InMemoryNode(Degree);
		}

		public bool SearchPosition(InMemoryNode node, string key, out IndexData found, out int position) {
			CheckDisposed();

			var lo = 0;
			var hi = node.Keys.Count - 1;
			while(lo <= hi) {
				var i = lo + ((hi - lo) >> 1);

				var indexDataKey = node.Keys[i];
				var c = string.Compare(indexDataKey.Key, key, StringComparison.OrdinalIgnoreCase);
				if(c == 0) {
					found = indexDataKey.Data;
					position = i;
					return true;
				}
				if(c < 0) {
					lo = i + 1;
				}
				else {
					hi = i - 1;
				}
			}

			found = default(IndexData);
			position = lo;
			return false;
		}

		public void SetRoot(InMemoryNode node) {
			CheckDisposed();

			_root = node;
		}

		public int Degree { get; }

		#region IDisposable

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if(_disposed)
				return;

			if(disposing) {
				_root = null;
			}

			_disposed = true;
		}

		private bool _disposed;

		private void CheckDisposed() {
			if(_disposed) {
				throw new ObjectDisposedException("Node storage is disposed");
			}
		}

		~InMemoryNodeStorage() {
			Dispose(false);
		}

		#endregion
	}

}