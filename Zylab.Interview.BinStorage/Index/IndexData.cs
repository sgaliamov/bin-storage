using System;

namespace Zylab.Interview.BinStorage.Index {

	[Serializable]
	public struct IndexData {
		public byte[] Md5Hash;
		public long Offset;
		public long Size;
	}

}