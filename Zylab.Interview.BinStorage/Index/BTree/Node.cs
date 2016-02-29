using System.Collections.Generic;
using System.Linq;

namespace Zylab.Interview.BinStorage.Index.BTree {

	public class Node {
		// ReSharper disable once InconsistentNaming
		private readonly int _2t;

		public Node(int t) {
			_2t = t << 1;
			Childrens = new List<Node>(_2t);
			Keys = new List<IndexDataKey>(_2t);
		}

		public List<Node> Childrens { get; set; }

		public List<IndexDataKey> Keys { get; set; }

		public bool IsLeaf => Childrens.Count == 0;

		public bool IsFull => Keys.Count == _2t - 1;

		public override string ToString() {
			return string.Join(", ", Keys.Select(x => x.Key)) + (IsFull ? "; full" : "; " + Childrens.Count);
		}
	}

}