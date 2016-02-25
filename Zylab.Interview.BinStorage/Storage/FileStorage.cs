using System.IO;
using Zylab.Interview.BinStorage.Index;

namespace Zylab.Interview.BinStorage.Storage {

	public class FileStorage : IStorage {
		private readonly string _storageFilePath;

		public FileStorage(string storageFilePath) {
			_storageFilePath = storageFilePath;
		}

		public void Dispose() {
			throw new System.NotImplementedException();
		}

		public IndexData Append(Stream data) {
			throw new System.NotImplementedException();
		}

		public Stream Get(IndexData indexData) {
			throw new System.NotImplementedException();
		}
	}

}