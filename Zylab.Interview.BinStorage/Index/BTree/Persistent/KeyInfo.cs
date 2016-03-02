namespace Zylab.Interview.BinStorage.Index.BTree.Persistent {

	/// <summary>
	///     Position only of a key in index file
	/// </summary>
	public struct KeyInfo {
		/// <summary>
		///     Length of key
		/// </summary>
		public int Size { get; set; }

		/// <summary>
		///     Position of key in index file
		/// </summary>
		public long Offset { get; set; }

		/// <summary>
		///     Key itself, data used for processing
		/// </summary>
		private string _key;

		/// <summary>
		///     Get key
		/// </summary>
		/// <returns>Key inself</returns>
		public string GetKey() {
			return _key;
		}

		/// <summary>
		///     Set key
		/// </summary>
		/// <param name="key">Key itself</param>
		public void SetKey(string key) {
			_key = key;
		}
	}

}