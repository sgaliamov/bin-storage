using System.Collections.Generic;

namespace Zylab.Interview.BinStorage.Index.BTree.InMemory {

	public class InMemoryNode : INode {
		public InMemoryNode(int t) {
			var t2 = t << 1;
			Childrens = new List<INode>(t2);
			Keys = new List<IKey>(t2);
		}

		public List<INode> Childrens { get; }

		public List<IKey> Keys { get; }
	}

}