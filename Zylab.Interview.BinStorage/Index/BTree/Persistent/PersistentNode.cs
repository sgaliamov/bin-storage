namespace Zylab.Interview.BinStorage.Index.BTree.Persistent {

	public struct PersistentNode : INode {
		public PersistentNode(int t) {
			var t2 = t << 1;
			Childrens = new long[t2];
			Keys = new IKey[t2];
			Offset = 0;
			Size = 0;
		}

		public long Offset;
		public long Size;
		public long[] Childrens;
		public IKey[] Keys;
	}

}