using System.Collections.Generic;

namespace Zylab.Interview.BinStorage.Index.BTree.InMemory {

	/// <summary>
	///     B-tree node for in memory storage
	/// </summary>
	public class InMemoryNode {
		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="degree">B-tree degree</param>
		public InMemoryNode(int degree) {
			var t2 = degree << 1;
			Childrens = new List<InMemoryNode>(t2);
			Keys = new List<IndexDataKey>(t2 - 1);
		}

		/// <summary>
		///     Children nodes
		/// </summary>
		public List<InMemoryNode> Childrens { get; }

		/// <summary>
		///     Keys
		/// </summary>
		public List<IndexDataKey> Keys { get; }
	}

}