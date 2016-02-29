using System;

namespace Zylab.Interview.BinStorage.Index.BTree.InMemory {

	public class InMemoryNodeStorage : INodeStorage<InMemoryNode, IndexDataKey> {
		private InMemoryNode _root;

		public InMemoryNodeStorage(int degree) {
			Degree = degree;
		}

		public void Dispose() {
			// do nothing
		}

		public void AddChildren(InMemoryNode node, InMemoryNode children) {
			node.Childrens.Add(children);
		}

		public void Commit(InMemoryNode node) {
			// do nothing
		}

		public int Compare(InMemoryNode node, int keyIndex, string key) {
			return string.Compare(key, node.Keys[keyIndex].Key, StringComparison.OrdinalIgnoreCase);
		}

		public InMemoryNode GetChildren(InMemoryNode node, int position) {
			return node.Childrens[position];
		}

		public IndexDataKey GetKey(InMemoryNode node, int position) {
			return node.Keys[position];
		}

		public InMemoryNode GetRoot() {
			return _root ?? (_root = NewNode());
		}

		public void InsertChildren(InMemoryNode node, int position, InMemoryNode children) {
			node.Childrens.Insert(position, children);
		}

		public void InsertKey(InMemoryNode node, int position, IndexDataKey key) {
			node.Keys.Insert(position, key);
		}

		public bool IsFull(InMemoryNode node) {
			return node.Keys.Count == 2 * Degree - 1;
		}

		public bool IsLeaf(InMemoryNode node) {
			return node.Childrens.Count == 0;
		}

		public void MoveRightHalfChildrens(InMemoryNode node, InMemoryNode source) {
			var range = source.Childrens.GetRange(Degree, Degree);
			node.Childrens.AddRange(range);
			source.Childrens.RemoveRange(Degree, Degree);
		}

		public void MoveRightHalfKeys(InMemoryNode node, InMemoryNode source) {
			var rangeKeys = source.Keys.GetRange(Degree, Degree - 1);
			node.Keys.AddRange(rangeKeys);
			source.Keys.RemoveRange(Degree - 1, Degree);
		}

		public IndexDataKey NewKey(string key, IndexData data) {
			return new IndexDataKey {
				Key = key,
				Data = data
			};
		}

		public InMemoryNode NewNode() {
			return new InMemoryNode(Degree);
		}

		public bool SearchPosition(InMemoryNode node, string key, out IndexData found, out int position) {
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
			_root = node;
		}

		public int Degree { get; }
	}

}