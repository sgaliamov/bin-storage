using System.Collections.Generic;

namespace Zylab.Interview.BinStorage.Index.BTree.InMemory {

	public class InMemoryNode {
		public InMemoryNode(int t) {
			var t2 = t << 1;
			Childrens = new List<InMemoryNode>(t2);
			Keys = new List<IndexDataKey>(t2);
		}

		public List<InMemoryNode> Childrens { get; }

		public List<IndexDataKey> Keys { get; }
	}

}