using System;
using System.Collections.Generic;

namespace Zylab.Interview.BinStorage.Index.BTree {

	public class BTreeIndex : IIndex {
		private readonly INodeStorage _nodeStorage;
		private readonly int _t;
		private Node _root;

		public BTreeIndex(INodeStorage nodeStorage, int t) {
			_nodeStorage = nodeStorage;
			_t = t;
			_root = _nodeStorage.NewNode(_t);
		}

		public void Add(string key, IndexData indexData) {
			if(!_root.IsFull) {
				InsertNonFull(_root, key, indexData);
				return;
			}

			var oldRoot = _root;
			_root = _nodeStorage.NewNode(_t);
			_root.Childrens.Add(oldRoot);
			Split(_root, 0, oldRoot);
			InsertNonFull(_root, key, indexData);
		}

		public IndexData Get(string key) {
			return Search(_root, key);
		}

		public bool Contains(string key) {
			return Search(_root, key) != null;
		}

		public void Dispose() {
			_nodeStorage.Dispose();
		}

		private static IndexData Search(Node parent, string key) {
			while(true) {
				IndexDataKey found;
				var i = BinarySearch(parent.Keys, key, out found);

				if(found != null) {
					return found;
				}

				if(parent.IsLeaf) {
					return null;
				}

				parent = parent.Childrens[i];
			}
		}

		private void Split(Node parent, int position, Node fullNode) {
			var newNode = _nodeStorage.NewNode(_t);

			parent.Keys.Insert(position, fullNode.Keys[_t - 1]);
			parent.Childrens.Insert(position + 1, newNode);

			newNode.Keys.AddRange(fullNode.Keys.GetRange(_t, _t - 1));

			fullNode.Keys.RemoveRange(_t - 1, _t);

			if(!fullNode.IsLeaf) {
				newNode.Childrens.AddRange(fullNode.Childrens.GetRange(_t, _t));
				fullNode.Childrens.RemoveRange(_t, _t);
			}
		}

		private void InsertNonFull(Node parent, string key, IndexData data) {
			while(true) {
				IndexDataKey found;
				var positionToInsert = BinarySearch(parent.Keys, key, out found);

				if(parent.IsLeaf) {
					parent.Keys.Insert(positionToInsert, _nodeStorage.NewKey(key, data));
					return;
				}

				var child = parent.Childrens[positionToInsert];
				if(child.IsFull) {
					Split(parent, positionToInsert, child);
					if(string.Compare(key, parent.Keys[positionToInsert].Key, StringComparison.OrdinalIgnoreCase) > 0) {
						positionToInsert++;
					}
				}

				parent = parent.Childrens[positionToInsert];
			}
		}

		private static int BinarySearch(IList<IndexDataKey> items, string key, out IndexDataKey found) {
			var lo = 0;
			var hi = items.Count - 1;
			while(lo <= hi) {
				var i = lo + ((hi - lo) >> 1);

				var c = string.Compare(items[i].Key, key, StringComparison.OrdinalIgnoreCase);
				if(c == 0) {
					found = items[i];
					return i;
				}
				if(c < 0) {
					lo = i + 1;
				}
				else {
					hi = i - 1;
				}
			}

			found = null;
			return lo;
		}
	}

}