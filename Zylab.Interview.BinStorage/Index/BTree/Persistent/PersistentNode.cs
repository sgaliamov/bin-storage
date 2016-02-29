using System;

namespace Zylab.Interview.BinStorage.Index.BTree.Persistent {

	[Serializable]
	public class PersistentNode  {
		public long[] Childrens;
		public int ChildrensPosition;
		public KeyData[] Keys;
		public int KeysPosition;

		[NonSerialized] public long Offset;

		public PersistentNode(long offset, int degree) {
			var t2 = degree << 1;
			Childrens = new long[t2];
			Keys = new KeyData[t2];
			Offset = offset;
			ChildrensPosition = 0;
			KeysPosition = 0;
		}
	}

}