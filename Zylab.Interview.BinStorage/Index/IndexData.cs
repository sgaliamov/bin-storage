using System;

namespace Zylab.Interview.BinStorage.Index {

	[Serializable]
	public class IndexData {
		public byte[] Md5Hash { get; set; }
		public ulong Start { get; set; }
		public ulong End { get; set; }
	}

}