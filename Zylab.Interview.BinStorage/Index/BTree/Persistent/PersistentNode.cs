using System.Linq;

namespace Zylab.Interview.BinStorage.Index.BTree.Persistent {

	public struct PersistentNode : INode {
		public PersistentNode(int t) {
			var t2 = t << 1;
			Childrens = new PersistentNode[t2];
			Keys = new IndexDataKey[t2];
			Offset = 0;
		}

		public long Offset;

		public PersistentNode[] Childrens;

		public IndexDataKey[] Keys;

		public override string ToString() {
			return string.Join(", ", Keys.Select(x => x.Key));
		}
	}

}