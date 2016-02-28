using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Zylab.Interview.BinStorage.Index.BTree {

	public class MemoryNodeStorage : INodeStorage {
		private readonly IDictionary<long, Node> _dictionary;

		public MemoryNodeStorage() {
			_dictionary = new ConcurrentDictionary<long, Node>();
		}

		public Node Create(long key, int degree) {
			var node = new Node(degree);
			_dictionary.Add(key, node);

			return node;
		}

		public Node Get(long key) {
			return _dictionary[key];
		}

		public void Dispose() {
			_dictionary.Clear();
		}
	}

}