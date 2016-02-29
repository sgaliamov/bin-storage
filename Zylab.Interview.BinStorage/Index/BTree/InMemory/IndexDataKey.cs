namespace Zylab.Interview.BinStorage.Index.BTree.InMemory {

	public class IndexDataKey : IKey {
		public string Key { get; set; }
		public IndexData Data { get; set; }
	}

}