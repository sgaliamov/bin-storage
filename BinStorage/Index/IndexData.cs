using System;

namespace Zylab.Interview.BinStorage.Index {

	/// <summary>
	///     Index data
	/// </summary>
	[Serializable]
	public class IndexData {
		/// <summary>
		///     Data size in bytes
		/// </summary>
		public long Size { get; set; }

		/// <summary>
		///     Position of data in storage file
		/// </summary>
		public long Offset { get; set; }

		/// <summary>
		///     MD5 hash of data
		/// </summary>
		public byte[] Md5Hash { get; set; }
	}

}