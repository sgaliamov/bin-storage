using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;

namespace Zylab.Interview.BinStorage.Index.BTree.Persistent {

	public class PersistentNodeStorage : INodeStorage {
		private const int PositionHolderSize = sizeof(long);
		private const int DefaultDegre = 1024;
		private const long DefaultCapacity = 0x400000; // 4 MB

		private readonly long _capacity;
		private readonly string _indexFilePath;
		private readonly MemoryMappedFile _indexFile;
		private readonly int _nodeSize;
		private long _position;
		private INode _root;
		private int _indexDataSize;

		public PersistentNodeStorage(string indexFilePath, long capacity = DefaultCapacity, int degree = DefaultDegre) {
			Degree = degree;
			_indexFilePath = indexFilePath;
			_capacity = capacity;
			_indexFile = MemoryMappedFile.CreateFromFile(
				_indexFilePath,
				FileMode.OpenOrCreate,
				null,
				_capacity,
				MemoryMappedFileAccess.ReadWrite);

			using(var accessor = _indexFile.CreateViewAccessor(0, PositionHolderSize)) {
				_position = accessor.ReadInt64(0);
				if(_position == 0) {
					_position = PositionHolderSize;
				}
			}

			_nodeSize = degree * (sizeof(long) + Marshal.SizeOf(typeof(KeyData))); // childs + keys
			_indexDataSize = 16 + sizeof(long) + sizeof(long); // md5 hash + offset + size
		}

		public void Dispose() {
			using(var accessor = _indexFile.CreateViewAccessor(0, PositionHolderSize)) {
				accessor.Write(0, _position);
			}
			_indexFile.Dispose();
		}

		public void AddChildren(INode node, INode children) {
			var parentNode = (PersistentNode)node;
			var childrenNode = (PersistentNode)children;

			parentNode.Childrens[parentNode.ChildrensPosition++] = childrenNode.Offset;
		}

		public void AddRangeChildrens(INode node, IEnumerable<INode> nodes) {
		}

		public void AddRangeKeys(INode node, IEnumerable<IKey> keys) {
			throw new NotImplementedException();
		}

		public void Commit(INode node) {
			var persistentNode = (PersistentNode)node;

			using(var accessor = _indexFile.CreateViewAccessor(persistentNode.Offset, _nodeSize)) {
				accessor.WriteArray(0, persistentNode.Keys, 0, Degree);
				accessor.WriteArray(0, persistentNode.Childrens, 0, Degree);
			}
		}

		public int Compare(INode parent, int keyIndex, string key) {
			throw new NotImplementedException();
		}

		public INode GetChildren(INode node, int position) {
			var persistentNode = (PersistentNode)node;
			var offset = persistentNode.Childrens[position];

			using(var accessor = _indexFile.CreateViewAccessor(offset, _nodeSize)) {
				var childred = new PersistentNode(offset, Degree);
				var count = accessor.ReadArray(0, childred.Keys, 0, Degree);
				accessor.ReadArray(count, childred.Childrens, 0, Degree);

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
			using(var accessor = _indexFile.CreateViewAccessor(_position, _indexDataSize)) {
				WriteKey(accessor, key);

				//accessor.Write(_position, data.Offset);
				//_position += sizeof(long);
			}

			return new KeyData();
		}

		private void WriteKey(MemoryMappedViewAccessor accessor, string key) {
			var keyBytes = Encoding.UTF8.GetBytes(key);
			accessor.Write(_position, keyBytes.LongLength);
			_position += sizeof(long);
			accessor.WriteArray(_position, keyBytes, 0, keyBytes.Length);
			_position += keyBytes.Length;
		}

		public INode NewNode() {
			var node = new PersistentNode(_position, Degree);
			_position += _nodeSize;

			return node;
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