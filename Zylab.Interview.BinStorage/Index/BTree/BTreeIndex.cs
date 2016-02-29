using System;
using System.Collections.Generic;

namespace Zylab.Interview.BinStorage.Index.BTree {

	public class BTreeIndex : IIndex {
		private readonly INodeStorage _storage;

		public BTreeIndex(INodeStorage storage) {
			_storage = storage;
		}

		public void Dispose() {
			_storage.Dispose();
		}

		public void Add(string key, IndexData indexData) {
			var root = _storage.GetRoot();
			if(!root.IsFull) {
				InsertNonFull(root, key, indexData);
				return;
			}

			var oldRoot = root;
			root = _storage.NewNode();
			_storage.SetRoot(root);
			root.AddChildren(oldRoot);
			Split(root, 0, oldRoot);
			InsertNonFull(root, key, indexData);
		}

		public bool Contains(string key) {
			return Search(_storage.GetRoot(), key) != null;
		}

		public IndexData Get(string key) {
			var data = Search(_storage.GetRoot(), key);

			if(data == null) {
				throw new KeyNotFoundException($"The given key ({key}) was not present in the dictionary.");
			}

			return data;
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

			parent.InsertKey(position, fullNode.GetKey(_storage.Degree - 1));
			parent.InsertChildren(position + 1, newNode);

			newNode.AddRangeKeys(fullNode.GetRangeKeys(_storage.Degree, _storage.Degree - 1));

			fullNode.RemoveRangeKeys(_storage.Degree - 1, _storage.Degree);

			if(!fullNode.IsLeaf) {
				newNode.AddRangeChildrens(fullNode.GetRangeChildrens(_storage.Degree, _storage.Degree));
				fullNode.RemoveRangeChildrens(_storage.Degree, _storage.Degree);
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