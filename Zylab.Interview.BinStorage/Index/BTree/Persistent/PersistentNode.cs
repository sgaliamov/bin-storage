using System;

namespace Zylab.Interview.BinStorage.Index.BTree.Persistent {

	[Serializable]
	public class PersistentNode  {
		public long[] Childrens;
		public int ChildrensCount;
		public KeyData[] Keys;
		public int KeysCount;

		[NonSerialized] public long Offset;

		public PersistentNode(long offset, int t2) {
			Childrens = new long[t2];
			Keys = new KeyData[t2 - 1];
			Offset = offset;
			ChildrensCount = 0;
			KeysCount = 0;
		}
	}

}