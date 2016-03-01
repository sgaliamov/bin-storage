namespace Zylab.Interview.BinStorage.Index.BTree.Persistent {

	public class Sizes {
		public const int Md5HashSize = 16;
		public const int IndexDataSize = Md5HashSize + sizeof(long) + sizeof(long);
		public const int CursorHolderSize = sizeof(long);
		public const long RootHolderOffset = CursorHolderSize;
		public const int RootHolderSize = sizeof(long);

		public Sizes(int degree) {
			var degree2 = degree * 2;

			ChildrensSize = degree2 * sizeof(long);
			KeysSize = (degree2 - 1) * (sizeof(int) + sizeof(long)); // KeyInfo
			NodeSize = ChildrensSize + KeysSize;
			KeysOffset = sizeof(int) + sizeof(int); // PersistentNode.KeysCount + PersistentNode.ChildrensCount
			ChildrensOffset = KeysSize + KeysOffset;
		}

		public int ChildrensSize { get; }
		public int ChildrensOffset { get; }
		public int KeysSize { get; }
		public int KeysOffset { get; }
		public int NodeSize { get; }
	}

}