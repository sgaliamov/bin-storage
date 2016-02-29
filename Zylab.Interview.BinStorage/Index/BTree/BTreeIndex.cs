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
			IndexData data;
			return Search(_storage.GetRoot(), key, out data);
		}

		public IndexData Get(string key) {
			IndexData data;
			if(!Search(_storage.GetRoot(), key, out data)) {
				throw new KeyNotFoundException($"The given key ({key}) was not present in the dictionary.");
			}

			return data;
		}

		private bool Search(INode parent, string key, out IndexData found) {
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

		private void Split(INode parent, int position, INode fullNode) {
			var newNode = _storage.NewNode();

			_storage.InsertKey(parent, position, _storage.GetKey(fullNode, _storage.Degree - 1));
			_storage.InsertChildren(parent, position + 1, newNode);

			_storage.AddRangeKeys(newNode, _storage.GetRangeKeys(fullNode, _storage.Degree, _storage.Degree - 1));

			_storage.RemoveRangeKeys(fullNode, _storage.Degree - 1, _storage.Degree);

			if(!_storage.IsLeaf(fullNode)) {
				_storage.AddRangeChildrens(newNode, _storage.GetRangeChildrens(fullNode, _storage.Degree, _storage.Degree));
				_storage.RemoveRangeChildrens(fullNode, _storage.Degree, _storage.Degree);
			}

			_storage.Commit(parent);
			_storage.Commit(fullNode);
			_storage.Commit(newNode);
		}

		private void InsertNonFull(INode parent, string key, IndexData data) {
			while(true) {
				IndexData found;
				int positionToInsert;
				_storage.SearchPosition(parent, key, out found, out positionToInsert);

				if(_storage.IsLeaf(parent)) {
					_storage.InsertKey(parent, positionToInsert, _storage.NewKey(key, data));
					_storage.Commit(parent);
					return;
				}

				var child = _storage.GetChildren(parent, positionToInsert);
				if(_storage.IsFull(child)) {
					Split(parent, positionToInsert, child);
					if(_storage.Compare(parent, positionToInsert, key) > 0) {
						positionToInsert++;
					}
				}

				parent = _storage.GetChildren(parent, positionToInsert);
			}
		}
	}

}