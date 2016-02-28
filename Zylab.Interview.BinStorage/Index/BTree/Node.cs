namespace Zylab.Interview.BinStorage.Index.BTree {

	public class Node {
		private readonly int _degree;

		public Node(int degree) {
			_degree = degree;
			Childrens = new Node[2 * _degree];
			Keys = new NodeData[2 * _degree];
			Position = 0;
		}

		public Node[] Childrens { get; set; }

		public NodeData[] Keys { get; set; }

		public ushort Position { get; set; }

		public bool IsLeaf => Position == 0;

		public bool IsFull => Position == 2 * _degree - 1;
	}

}