namespace Zylab.Interview.BinStorage.Index.BTree.Persistent {

	public struct PersistentNode : INode {
		public PersistentNode(long offset, int degree) {
			var t2 = degree << 1;
			Childrens = new long[t2];
			Keys = new KeyData[t2];
			Offset = offset;
			ChildrensPosition = 0;
			KeysPosition = 0;
		}

		public long Offset;
		public int ChildrensPosition;
		public int KeysPosition;

		public long[] Childrens;
		public KeyData[] Keys;
	}

}