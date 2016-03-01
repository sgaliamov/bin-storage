using System;

namespace Zylab.Interview.BinStorage.Index {

	[Serializable]
	public class IndexData {
		public byte[] Md5Hash { get; set; }
		public long Offset { get; set; }
		public long Size { get; set; }
	}

}