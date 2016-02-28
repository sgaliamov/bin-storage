using System;
using System.Collections.Generic;
using System.Linq;

namespace Zylab.Interview.BinStorage.Index.BTree {

	public class BTreeIndex : IIndex {
		private readonly int _degree;
		private readonly INodeStorage _nodeStorage;
		private Node _root;

		public BTreeIndex(INodeStorage nodeStorage, int degree) {
			_nodeStorage = nodeStorage;

			if(degree < 2) {
				throw new ArgumentException("BTree degree must be at least 2", nameof(degree));
			}

			_root = _nodeStorage.Create(0, degree);
			_degree = degree;
		}

		public void Add(string key, IndexData indexData) {
			if(!_root.IsFull) {
				InsertNonFull(_root, key, indexData);
				return;
			}

			var oldRoot = _root;
			_root = new Node(_degree);
			_root.Childrens.Add(oldRoot);
			SplitChild(_root, 0, oldRoot);
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
			if(parent.IsLeaf) return null;

			while(true) {
				NodeData found;
				var i = BinarySearch(parent.Keys, key, out found);

				if(found != null) {
					return found;
				}

				parent = parent.Childrens[i];
			}
		}

		

		private void SplitChild(Node parentNode, int nodeToBeSplitIndex, Node nodeToBeSplit) {
			var newNode = new Node(_degree);

			parentNode.Keys.Insert(nodeToBeSplitIndex, nodeToBeSplit.Keys[_degree - 1]);
			parentNode.Childrens.Insert(nodeToBeSplitIndex + 1, newNode);

			newNode.Keys.AddRange(nodeToBeSplit.Keys.GetRange(_degree, _degree - 1));

			nodeToBeSplit.Keys.RemoveRange(_degree - 1, _degree);

			if(!nodeToBeSplit.IsLeaf) {
				newNode.Childrens.AddRange(nodeToBeSplit.Childrens.GetRange(_degree, _degree));
				nodeToBeSplit.Childrens.RemoveRange(_degree, _degree);
			}
		}

		private void InsertNonFull(Node parent, string key, IndexData newPointer) {
			NodeData found;
			var positionToInsert = BinarySearch(parent.Keys, key, out found);

			if(parent.IsLeaf) {
				parent.Keys.Insert(positionToInsert, new IndexData { Key = key, Pointer = newPointer });
				return;
			}

			var child = parent.Childrens[positionToInsert];
			if(child.IsFull) {
				SplitChild(parent, positionToInsert, child);
				if(key.CompareTo(parent.Keys[positionToInsert].Key) > 0) {
					positionToInsert++;
				}
			}

			InsertNonFull(parent.Childrens[positionToInsert], key, newPointer);
		}

		private static int BinarySearch(IList<NodeData> items, string key, out NodeData found) {
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
			return ~lo;
		}
	}

}