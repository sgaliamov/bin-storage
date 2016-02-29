using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Zylab.Interview.BinStorage.Index.BTree.Persistent {

	public class PersistentNodeStorage : INodeStorage {
		private readonly long _capacity;
		private readonly string _indexFilePath;
		private readonly MemoryMappedFile _nodesFile;
		private INode _root;

		public PersistentNodeStorage(string indexFilePath, long capacity, int degree = 1024) {
			Degree = degree;
			_indexFilePath = indexFilePath;
			_capacity = capacity;
			_nodesFile = MemoryMappedFile.CreateFromFile(
				_indexFilePath,
				FileMode.OpenOrCreate,
				null,
				_capacity,
				MemoryMappedFileAccess.ReadWrite);
		}


		public void Dispose() {
			_nodesFile.Dispose();
		}

		public void AddChildren(INode node, INode children) {
			throw new NotImplementedException();
		}

		public void AddRangeChildrens(INode node, IEnumerable<INode> nodes) {
			throw new NotImplementedException();
		}

		public void AddRangeKeys(INode node, IEnumerable<IKey> keys) {
			throw new NotImplementedException();
		}

		public void Commit(INode node) {
			var persistentNode = (PersistentNode)node;

			using(var accessor = _nodesFile.CreateViewAccessor(persistentNode.Offset, persistentNode.Size)) {
				accessor.Write(0, ref persistentNode);
			}
		}

		public INode GetChildren(INode node, int position) {
			var persistentNode = (PersistentNode)node;
			using(var accessor = _nodesFile.CreateViewAccessor(persistentNode.Offset, 100)) {
				PersistentNode childred;
				accessor.Read(0, out childred);
				return childred;
			}
		}

		public IKey GetKey(INode node, int position) {
			throw new NotImplementedException();
		}

		public IEnumerable<INode> GetRangeChildrens(INode node, int index, int count) {
			throw new NotImplementedException();
		}

		public IEnumerable<IKey> GetRangeKeys(INode node, int index, int count) {
			throw new NotImplementedException();
		}

		public INode GetRoot() {
			return _root ?? (_root = NewNode());
		}

		public void InsertChildren(INode node, int position, INode children) {
			throw new NotImplementedException();
		}

		public int Compare(INode parent, int keyIndex, string key) {
			throw new NotImplementedException();
		}

		public void InsertKey(INode node, int position, IKey key) {
			throw new NotImplementedException();
		}

		public bool IsFull(INode node) {
			throw new NotImplementedException();
		}

		public bool IsLeaf(INode node) {
			throw new NotImplementedException();
		}

		public IKey NewKey(string key, IndexData data) {
			return null;
		}

		public INode NewNode() {
			return new PersistentNode();
		}

		public void RemoveRangeChildrens(INode node, int index, int count) {
			throw new NotImplementedException();
		}

		public void RemoveRangeKeys(INode node, int index, int count) {
			throw new NotImplementedException();
		}

		public int SearchPosition(INode node, string key, out IndexData found) {
			throw new NotImplementedException();
		}

		public void SetRoot(INode node) {
			_root = node;
		}

		public int Degree { get; }
	}

}