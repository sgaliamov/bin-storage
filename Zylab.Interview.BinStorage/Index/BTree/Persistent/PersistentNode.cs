using System;

namespace Zylab.Interview.BinStorage.Index.BTree.Persistent {

	[Serializable]
	public class PersistentNode {
		[NonSerialized] private long _offset;

		public long[] Childrens;
		public int ChildrensCount;
		public KeyData[] Keys;
		public int KeysCount;

		public PersistentNode(long offset, int t2) {
			Childrens = new long[t2];
			Keys = new KeyData[t2 - 1];
			_offset = offset;
			ChildrensCount = 0;
			KeysCount = 0;
		}

		public long Offset {
			get { return _offset; }
			set { _offset = value; }
		}
	}

}