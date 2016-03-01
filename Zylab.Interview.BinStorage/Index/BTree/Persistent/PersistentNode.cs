namespace Zylab.Interview.BinStorage.Index.BTree.Persistent {

	public class PersistentNode {
		public PersistentNode(long offset, int t2) {
			Childrens = new long[t2];
			Keys = new KeyInfo[t2 - 1];
			Offset = offset;
			ChildrensCount = 0;
			KeysCount = 0;
		}

		public long Offset { get; set; }
		public long[] Childrens { get; set; }
		public int ChildrensCount { get; set; }
		public KeyInfo[] Keys { get; set; }
		public int KeysCount { get; set; }
	}

}