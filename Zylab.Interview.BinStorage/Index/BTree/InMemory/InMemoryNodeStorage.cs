namespace Zylab.Interview.BinStorage.Index.BTree.InMemory {

	public class InMemoryNodeStorage : INodeStorage {
		private INode _root;

		public InMemoryNodeStorage(int degree) {
			Degree = degree;
		}

		public int Degree { get; }

		public void Dispose() {
		}

		public INode GetRoot() {
			return _root ?? (_root = NewNode());
		}

		public IndexDataKey NewKey(string key, IndexData data) {
			return new IndexDataKey {
				Key = key,
				Offset = data.Offset,
				Md5Hash = data.Md5Hash,
				Size = data.Size
			};
		}

		public INode NewNode() {
			return new InMemoryNode(Degree);
		}

		public void SetRoot(INode node) {
			_root = node;
		}
	}

}