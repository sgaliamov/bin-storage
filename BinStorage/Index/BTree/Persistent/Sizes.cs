namespace BinStorage.Index.BTree.Persistent {

	/// <summary>
	///     Sized and offsets
	/// </summary>
	public class Sizes {
		public const int Md5HashSize = 16;
		public const int IndexDataSize = Md5HashSize + sizeof(long) + sizeof(long);
		public const int CursorHolderOffset = 0;
		public const int CursorHolderSize = sizeof(long);
		public const long RootHolderOffset = CursorHolderSize;
		public const int RootHolderSize = sizeof(long);

		public Sizes(int degree) {
			var degree2 = degree * 2;

			ChildrensSize = degree2 * sizeof(long); // PersistentNode.Childrens:long[]
			KeysSize = (degree2 - 1) * (sizeof(int) + sizeof(long)); // PersistentNode.Keys:KeyInfo[]

			// PersistentNode.KeysCount:int + PersistentNode.ChildrensCount:int
			const int countFieldsSize = sizeof(int) + sizeof(int);

			NodeSize = ChildrensSize + KeysSize + countFieldsSize;
			ChildrensOffset = KeysSize + countFieldsSize;
		}

		public int ChildrensSize { get; }
		public int ChildrensOffset { get; }
		public int KeysSize { get; }
		public int NodeSize { get; }
	}

}