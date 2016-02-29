using System.Collections.Generic;
using System.Linq;

namespace Zylab.Interview.BinStorage.Index.BTree.InMemory {

	public class InMemoryNode : INode {
		public InMemoryNode(int t) {
			var t2 = t << 1;
			Childrens = new List<INode>(t2);
			Keys = new List<IndexDataKey>(t2);
		}

		public List<INode> Childrens { get; }

		public List<IndexDataKey> Keys { get; }

		public override string ToString() {
			return string.Join(", ", Keys.Select(x => x.Key));
		}
	}

}