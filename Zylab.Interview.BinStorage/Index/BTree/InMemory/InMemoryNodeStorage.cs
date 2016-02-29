using System;
using System.Collections.Generic;

namespace Zylab.Interview.BinStorage.Index.BTree.InMemory {

	public class InMemoryNodeStorage : INodeStorage {
		private INode _root;

		public InMemoryNodeStorage(int degree) {
			Degree = degree;
		}

		public void Dispose() {
		}

		public void AddChildren(INode node, INode children) {
			((InMemoryNode)node).Childrens.Add(children);
		}

		public void AddRangeChildrens(INode node, IEnumerable<INode> nodes) {
			((InMemoryNode)node).Childrens.AddRange(nodes);
		}

		public void AddRangeKeys(INode node, IEnumerable<IKey> keys) {
			((InMemoryNode)node).Keys.AddRange(keys);
		}

		public void Commit(INode node) {
			// do nothing			
		}

		public int Compare(INode parent, int keyIndex, string key) {
			var indexDataKey = (IndexDataKey)GetKey(parent, keyIndex);
			return string.Compare(key, indexDataKey.Key, StringComparison.OrdinalIgnoreCase);
		}

		public INode GetChildren(INode node, int position) {
			return ((InMemoryNode)node).Childrens[position];
		}

		public IKey GetKey(INode node, int position) {
			return ((InMemoryNode)node).Keys[position];
		}

		public IEnumerable<INode> GetRangeChildrens(INode node, int index, int count) {
			return ((InMemoryNode)node).Childrens.GetRange(index, count);
		}

		public IEnumerable<IKey> GetRangeKeys(INode node, int index, int count) {
			return ((InMemoryNode)node).Keys.GetRange(index, count);
		}

		public INode GetRoot() {
			return _root ?? (_root = NewNode());
		}

		public void InsertChildren(INode node, int position, INode children) {
			((InMemoryNode)node).Childrens.Insert(position, children);
		}

		public void InsertKey(INode node, int position, IKey key) {
			((InMemoryNode)node).Keys.Insert(position, key);
		}

		public bool IsFull(INode node) {
			return ((InMemoryNode)node).Keys.Count == 2 * Degree - 1;
		}

		public bool IsLeaf(INode node) {
			return ((InMemoryNode)node).Childrens.Count == 0;
		}

		public IKey NewKey(string key, IndexData data) {
			return new IndexDataKey {
				Key = key,
				Data = data
			};
		}

		public INode NewNode() {
			return new InMemoryNode(Degree);
		}

		public void RemoveRangeChildrens(INode node, int index, int count) {
			((InMemoryNode)node).Childrens.RemoveRange(index, count);
		}

		public void RemoveRangeKeys(INode node, int index, int count) {
			((InMemoryNode)node).Keys.RemoveRange(index, count);
		}

		public bool SearchPosition(INode node, string key, out IndexData found, out  int position) {
			var lo = 0;
			var hi = ((InMemoryNode)node).Keys.Count - 1;
			while(lo <= hi) {
				var i = lo + ((hi - lo) >> 1);

				var indexDataKey = (IndexDataKey)((InMemoryNode)node).Keys[i];
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

		public void SetRoot(INode node) {
			_root = node;
		}

		public int Degree { get; }
	}

}