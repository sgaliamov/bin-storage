using System.IO;
using Zylab.Interview.BinStorage.Index;
using Zylab.Interview.BinStorage.Storage;

namespace Zylab.Interview.BinStorage {

	public class BinaryStorage : IBinaryStorage {
		private readonly IIndex _index;
		private readonly IStorage _storage;

		public BinaryStorage(StorageConfiguration configuration) {
			_index = new RedBlackTreeIndex(
				Path.Combine(configuration.WorkingFolder, configuration.IndexFileName),
				configuration.IndexTimeout);
			_storage = new FileStorage(Path.Combine(configuration.WorkingFolder, configuration.StorageFileName));
		}

		public void Add(string key, Stream data) {
			var indexData = _storage.Append(data);
			_index.Add(key, indexData);
		}

		public Stream Get(string key) {
			var indexData = _index.Get(key);

			return _storage.Get(indexData);
		}

		public bool Contains(string key) {
			return _index.Contains(key);
		}

		public void Dispose() {
			// todo: https://msdn.microsoft.com/en-us/library/system.idisposable(v=vs.110).aspx
			_storage.Dispose();
			_index.Dispose();
		}
	}

}