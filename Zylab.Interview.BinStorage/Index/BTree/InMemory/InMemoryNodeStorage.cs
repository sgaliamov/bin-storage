using System;
using System.Collections.Generic;

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

		public void AddRangeChildrens(InMemoryNode node, InMemoryNode source, int position, int count) {
			var range = source.Childrens.GetRange(position, count);
			node.Childrens.AddRange(range);
		}

		public void AddRangeKeys(InMemoryNode node, IEnumerable<IndexDataKey> keys) {
			node.Keys.AddRange(keys);
		}

		public void Commit(InMemoryNode node) {
			// do nothing
		}

		public int Compare(InMemoryNode parent, int keyIndex, string key) {
			var indexDataKey = GetKey(parent, keyIndex);
			return string.Compare(key, indexDataKey.Key, StringComparison.OrdinalIgnoreCase);
		}

		public InMemoryNode GetChildren(InMemoryNode node, int position) {
			return node.Childrens[position];
		}

		public IndexDataKey GetKey(InMemoryNode node, int position) {
			return node.Keys[position];
		}

		public IEnumerable<IndexDataKey> GetRangeKeys(InMemoryNode node, int index, int count) {
			return node.Keys.GetRange(index, count);
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

		public IndexDataKey NewKey(string key, IndexData data) {
			return new IndexDataKey {
				Key = key,
				Data = data
			};
		}

		public InMemoryNode NewNode() {
			return new InMemoryNode(Degree);
		}

		public void RemoveRangeChildrens(InMemoryNode node, int index, int count) {
			node.Childrens.RemoveRange(index, count);
		}

		public void RemoveRangeKeys(InMemoryNode node, int index, int count) {
			node.Keys.RemoveRange(index, count);
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

		public void AddRangeChildrens(InMemoryNode node, IEnumerable<InMemoryNode> nodes) {
		}
	}

}