using System;

namespace Zylab.Interview.BinStorage.Index.BTree {

	public class IndexDataKey : IndexData, IComparable<IndexDataKey>, IComparable {
		public string Key { get; set; }

		public int CompareTo(object other) {
			return CompareTo((IndexDataKey)other);
		}

		public int CompareTo(IndexDataKey other) {
			return string.Compare(Key, other.Key, StringComparison.InvariantCultureIgnoreCase);
		}

		public override string ToString() {
			return Key;
		}
	}

}