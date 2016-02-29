using System.Collections.Generic;

namespace Zylab.Interview.BinStorage.Index.BTree.InMemory {

	public class InMemoryNodeStorage : INodeStorage {
		private readonly IDictionary<string, IndexDataKey> _keys;
		private readonly IDictionary<string, InMemoryNode> _nodes;
		private readonly int _t;
		private INode _root;

		public InMemoryNodeStorage(int t) {
			_t = t;
			_nodes = new Dictionary<string, InMemoryNode>();
			_keys = new Dictionary<string, IndexDataKey>();
		}

		public void Dispose() {
			_nodes.Clear();
		}

		public INode GetRoot() {
			return _root ?? (_root = NewNode());
		}

		public IndexDataKey NewKey(string key, IndexData data) {
			var node = new IndexDataKey {
				Key = key,
				Offset = data.Offset,
				Md5Hash = data.Md5Hash,
				Size = data.Size
			};
			_keys.Add(key, node);

			return node;
		}

		public INode NewNode() {
			return new InMemoryNode(_t);
		}

		public void SetRoot(INode node) {
			_root = node;
		}
	}

}