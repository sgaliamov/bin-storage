using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Zylab.Interview.BinStorage.Index.RedBlackTree {

	public class RedBlackTreeIndex : IIndex {
		private readonly string _indexFilePath;
		private readonly SortedDictionary<string, IndexData> _tree;

		public RedBlackTreeIndex(string indexFilePath) {
			_indexFilePath = indexFilePath;

			_tree = GetOrCreateTree(_indexFilePath);
		}

		public void Add(string key, IndexData data) {
			_tree.Add(key, data);
		}

		public IndexData Get(string key) {
			return _tree[key];
		}

		public bool Contains(string key) {
			return _tree.ContainsKey(key);
		}

		public void Dispose() {
			// todo: https://msdn.microsoft.com/en-us/library/system.idisposable(v=vs.110).aspx
			var formatter = new BinaryFormatter();
			using(var stream = new FileStream(_indexFilePath, FileMode.Create, FileAccess.Write, FileShare.None)) {
				formatter.Serialize(stream, _tree);
			}
		}

		private static SortedDictionary<string, IndexData> GetOrCreateTree(string indexFilePath) {
			SortedDictionary<string, IndexData> tree = null;

			if(File.Exists(indexFilePath)) {
				using(var stream = new FileStream(indexFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
					var formatter = new BinaryFormatter();
					tree = (SortedDictionary<string, IndexData>)formatter.Deserialize(stream);
				}
			}

			return tree ?? new SortedDictionary<string, IndexData>();
		}
	}

}