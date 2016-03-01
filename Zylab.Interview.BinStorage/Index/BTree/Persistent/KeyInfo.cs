namespace Zylab.Interview.BinStorage.Index.BTree.Persistent {

	public struct KeyInfo {
		public int Size { get; set; }
		public long Offset { get; set; }

		public string Key { get; set; }
	}

}