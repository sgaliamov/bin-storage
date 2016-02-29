using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Zylab.Interview.BinStorage.Index.BTree.Persistent {

	public class PersistentNodeStorage : INodeStorage<PersistentNode, KeyData> {
		private const int PositionHolderSize = sizeof(long);
		private const int RootHolderOffset = PositionHolderSize;
		private const int RootHolderSize = sizeof(long);
		private const int DefaultDegre = 1024;
		private const long DefaultCapacity = 0x400000; // 4 MB
		private const int IndexDataSize = 16 + sizeof(long) + sizeof(long); // md5 hash + offset + size

		private readonly long _capacity;
		private readonly MemoryMappedFile _indexFile;
		private readonly string _indexFilePath;
		private readonly int _nodeSize;
		private long _position;
		private PersistentNode _root;

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
					_position = PositionHolderSize + RootHolderSize;
				}
			}

			_nodeSize = degree * (sizeof(long) + Marshal.SizeOf(typeof(KeyData))); // childs + keys
		}

		public void Dispose() {
			using(var accessor = _indexFile.CreateViewAccessor(0, PositionHolderSize)) {
				accessor.Write(0, _position);
			}
			_indexFile.Dispose();
		}

		public void AddChildren(PersistentNode node, PersistentNode children) {
			node.Childrens[node.ChildrensPosition++] = children.Offset;
		}

		public void Commit(PersistentNode node) {
			using(var writer = _indexFile.CreateViewStream(node.Offset, _nodeSize)) {
				var formatter = new BinaryFormatter();
				formatter.Serialize(writer, node);
			}
		}

		public int Compare(PersistentNode node, int keyIndex, string key) {
			throw new NotImplementedException();
		}

		public PersistentNode GetChildren(PersistentNode node, int position) {
			var offset = node.Childrens[position];

			using(var reader = _indexFile.CreateViewStream(offset, _nodeSize)) {
				var formatter = new BinaryFormatter();
				var children = (PersistentNode)formatter.Deserialize(reader);
				children.Offset = offset;

				return children;
			}
		}

		public KeyData GetKey(PersistentNode node, int position) {
			return node.Keys[position];
		}

		public PersistentNode GetRoot() {
			return _root ?? (_root = NewNode());
		}

		public void InsertChildren(PersistentNode node, int position, PersistentNode children) {
			throw new NotImplementedException();
		}

		public void InsertKey(PersistentNode node, int position, KeyData key) {
			throw new NotImplementedException();
		}

		public bool IsFull(PersistentNode node) {
			return node.KeysPosition == 2 * Degree - 1;
		}

		public bool IsLeaf(PersistentNode node) {
			return node.ChildrensPosition == 0;
		}

		public void MoveRightHalfChildrens(PersistentNode node, PersistentNode source) {
			for(var i = Degree; i < Degree + Degree; i++) {
				node.Childrens[node.ChildrensPosition++] = source.Childrens[i];
				source.Childrens[i] = 0;
			}
			source.ChildrensPosition = Degree - 1;
		}

		public void MoveRightHalfKeys(PersistentNode node, PersistentNode source) {
			throw new NotImplementedException();
		}

		public KeyData NewKey(string key, IndexData data) {
			var newKey = new KeyData { Offset = _position };

			using(var accessor = _indexFile.CreateViewAccessor(_position, IndexDataSize)) {
				WriteKey(accessor, key);

				accessor.Write(_position, ref data);
				_position += IndexDataSize;
			}

			newKey.Size = _position = newKey.Offset;
			return newKey;
		}

		public PersistentNode NewNode() {
			var node = new PersistentNode(_position, Degree);
			_position += _nodeSize;

			return node;
		}

		public bool SearchPosition(PersistentNode node, string key, out IndexData found, out int position) {
			throw new NotImplementedException();
		}

		public void SetRoot(PersistentNode node) {
			_root = node;
		}

		public int Degree { get; }

		private void WriteKey(UnmanagedMemoryAccessor accessor, string key) {
			var keyBytes = Encoding.UTF8.GetBytes(key);
			accessor.Write(_position, keyBytes.LongLength);
			_position += sizeof(long);
			accessor.WriteArray(_position, keyBytes, 0, keyBytes.Length);
			_position += keyBytes.Length;
		}
	}

}