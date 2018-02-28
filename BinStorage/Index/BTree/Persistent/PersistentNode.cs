namespace Zylab.Interview.BinStorage.Index.BTree.Persistent {

	/// <summary>
	///     B-tree node
	/// </summary>
	public class PersistentNode {
		/// <summary>
		///     Node offset in file, data used for processing
		/// </summary>
		private readonly long _offset;

		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="offset">Node offset in bytes</param>
		/// <param name="degree">Degree of b-tree</param>
		public PersistentNode(long offset, int degree) {
			_offset = offset;
			var t2 = degree << 1;
			Childrens = new long[t2];
			Keys = new KeyInfo[t2 - 1];
			ChildrensCount = 0;
			KeysCount = 0;
		}

		/// <summary>
		///     Active keys count
		/// </summary>
		public int KeysCount { get; set; }

		/// <summary>
		///     Childrens count
		/// </summary>
		public int ChildrensCount { get; set; }

		/// <summary>
		///     Keys info
		/// </summary>
		public KeyInfo[] Keys { get; set; }

		/// <summary>
		///     Children offsets
		/// </summary>
		public long[] Childrens { get; set; }

		/// <summary>
		///     Get node offset
		/// </summary>
		/// <returns>Node offset in bytes</returns>
		public long GetOffset() {
			return _offset;
		}
	}

}