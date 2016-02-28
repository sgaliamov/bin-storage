using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Zylab.Interview.BinStorage.Index.BTree {

	public class MemoryNodeStorage : INodeStorage {
		private readonly IDictionary<string, Node> _nodes;
		private readonly IDictionary<string, IndexDataKey> _keys;

		public MemoryNodeStorage() {
			_nodes = new ConcurrentDictionary<string, Node>();
			_keys = new ConcurrentDictionary<string, IndexDataKey>();
		}

		public IndexDataKey NewKey(string key, IndexData data) {
			var node = new IndexDataKey() {
				Key = key,
				Offset = data.Offset,
				Md5Hash = data.Md5Hash,
				Size = data.Size
			};
			_keys.Add(key, node);

			return node;
		}

		public Node NewNode(int t) {
			return new Node(t);
		}

		public Node GetNode(string key) {
			return _nodes[key];
		}

		public void Dispose() {
			_nodes.Clear();
		}
	}

}