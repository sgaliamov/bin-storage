namespace Zylab.Interview.BinStorage.Index.BTree.InMemory {

	/// <summary>
	///     In memory key info
	/// </summary>
	public class IndexDataKey {
		/// <summary>
		///     Key itself
		/// </summary>
		public string Key { get; set; }

		/// <summary>
		///     Index data
		/// </summary>
		public IndexData Data { get; set; }
	}

}