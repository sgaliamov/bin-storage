using System;
using System.Collections.Generic;

namespace Zylab.Interview.BinStorage.Index.BTree {

	public class BTreeIndex : IIndex {
		private readonly INodeStorage _storage;
		private readonly int _t;

		public BTreeIndex(INodeStorage storage, int t) {
			_storage = storage;
			_t = t;
		}

		public void Add(string key, IndexData indexData) {
			var root = _storage.GetRoot();
			if(!root.IsFull) {
				InsertNonFull(root, key, indexData);
				return;
			}

			var oldRoot = root;
			root = _storage.NewNode();
			root.AddChildren(oldRoot);
			Split(root, 0, oldRoot);
			InsertNonFull(root, key, indexData);
		}

		public IndexData Get(string key) {
			var data = Search(_storage.GetRoot(), key);

			if(data == null) {
				throw new KeyNotFoundException($"The given key ({key}) was not present in the dictionary.");
			}

			return data;
		}

		public bool Contains(string key) {
			return Search(_storage.GetRoot(), key) != null;
		}

		public void Dispose() {
			_storage.Dispose();
		}

		private static IndexData Search(INode parent, string key) {
			while(true) {
				IndexDataKey found;
				var i = parent.SearchPosition(key, out found);

				if(found != null) {
					return found;
				}

				if(parent.IsLeaf) {
					return null;
				}

				parent = parent.GetChildren(i);
			}
		}

		private void Split(INode parent, int position, INode fullNode) {
			var newNode = _storage.NewNode();

			parent.InsertKey(position, fullNode.GetKey(_t - 1));
			parent.InsertChildren(position + 1, newNode);

			newNode.AddRangeKeys(fullNode.GetRangeKeys(_t, _t - 1));

			fullNode.RemoveRangeKeys(_t - 1, _t);

			if(!fullNode.IsLeaf) {
				newNode.AddRangeChildrens(fullNode.GetRangeChildrens(_t, _t));
				fullNode.RemoveRangeChildrens(_t, _t);
			}
		}

		private void InsertNonFull(INode parent, string key, IndexData data) {
			while(true) {
				IndexDataKey found;
				var positionToInsert = parent.SearchPosition(key, out found);

				if(parent.IsLeaf) {
					parent.InsertKey(positionToInsert, _storage.NewKey(key, data));
					return;
				}

				var child = parent.GetChildren(positionToInsert);
				if(child.IsFull) {
					Split(parent, positionToInsert, child);
					if(string.Compare(key, parent.GetKey(positionToInsert).Key, StringComparison.OrdinalIgnoreCase) > 0) {
						positionToInsert++;
					}
				}

				parent = parent.GetChildren(positionToInsert);
			}
		}

		
	}

}