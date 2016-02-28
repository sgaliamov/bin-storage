using System;

namespace Zylab.Interview.BinStorage.Index.BTree {

	public class NodeData : IndexData, IComparable<NodeData>, IComparable {
		public string Key { get; set; }

		public int CompareTo(object other) {
			return CompareTo((NodeData)other);
		}

		public int CompareTo(NodeData other) {
			return string.Compare(Key, other.Key, StringComparison.InvariantCultureIgnoreCase);
		}
	}

}