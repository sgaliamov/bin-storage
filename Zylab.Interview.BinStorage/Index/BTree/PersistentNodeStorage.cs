//using System;
//using System.IO;
//using System.IO.MemoryMappedFiles;

//namespace Zylab.Interview.BinStorage.Index.BTree {

//	public class PersistentNodeStorage : INodeStorage {
//		private readonly long _capacity;
//		private readonly string _indexFilePath;
//		private readonly MemoryMappedFile _mappedFile;

//		public PersistentNodeStorage(string indexFilePath, long capacity) {
//			_indexFilePath = indexFilePath;
//			_capacity = capacity;
//			_mappedFile = MemoryMappedFile.CreateFromFile(
//				_indexFilePath,
//				FileMode.OpenOrCreate,
//				null,
//				_capacity,
//				MemoryMappedFileAccess.ReadWrite);
//		}

//		public Node NewKey(long key, int degree) {
//			throw new NotImplementedException();
//		}

//		public Node GetNode(long key) {
//			throw new NotImplementedException();
//		}
//	}

//}