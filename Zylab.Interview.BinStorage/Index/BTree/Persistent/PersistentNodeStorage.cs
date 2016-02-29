using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Zylab.Interview.BinStorage.Index.BTree.Persistent {

	public class PersistentNodeStorage : INodeStorage<PersistentNode, KeyData> {
		private const int CursorHolderSize = sizeof(long);
		private const int RootHolderOffset = CursorHolderSize;
		private const int RootHolderSize = sizeof(long);
		private const int DefaultDegre = 1024;
		private const long DefaultCapacity = 0x400000; // 4 MB
		private const int IndexDataSize = 16 + sizeof(long) + sizeof(long); // md5 hash + offset + size
		private readonly int _degree2;

		private readonly MemoryMappedFile _indexFile;
		private readonly int _nodeSize;
		private long _cursor;
		private PersistentNode _root;

		public PersistentNodeStorage(string indexFilePath, long capacity = DefaultCapacity, int degree = DefaultDegre) {
			Degree = degree;
			_degree2 = 2 * degree;
			_nodeSize = _degree2 * sizeof(long) + (_degree2 - 1) * Marshal.SizeOf(typeof(KeyData)); // childs + keys

			_indexFile = MemoryMappedFile.CreateFromFile(
				indexFilePath,
				FileMode.OpenOrCreate,
				null,
				capacity,
				MemoryMappedFileAccess.ReadWrite);

			InitRootAndCursor();
		}

		public void Dispose() {
			using(var accessor = _indexFile.CreateViewAccessor(0, CursorHolderSize)) {
				accessor.Write(0, _cursor);
			}
			Commit(_root);
			_indexFile.Dispose();
		}

		public void AddChildren(PersistentNode node, PersistentNode children) {
			node.Childrens[node.ChildrensCount++] = children.Offset;
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
			return _root;
		}

		public void InsertChildren(PersistentNode node, int position, PersistentNode children) {
			Array.Copy(node.Childrens, position, node.Childrens, position + 1, node.ChildrensCount - position);
			node.Childrens[position] = children.Offset;
			node.ChildrensCount++;
		}

		public void InsertKey(PersistentNode node, int position, KeyData key) {
			Array.Copy(node.Keys, position, node.Keys, position + 1, node.KeysCount - position);
			node.Keys[position] = key;
			node.KeysCount++;
		}

		public bool IsFull(PersistentNode node) {
			return node.KeysCount == _degree2 - 1;
		}

		public bool IsLeaf(PersistentNode node) {
			return node.ChildrensCount == 0;
		}

		public void MoveRightHalfChildrens(PersistentNode newNode, PersistentNode fullNode) {
			Array.Copy(fullNode.Childrens, Degree, newNode.Childrens, 0, Degree);
			newNode.ChildrensCount = Degree;

			Array.Clear(fullNode.Childrens, Degree, Degree);
			fullNode.ChildrensCount = Degree;
		}

		public void MoveRightHalfKeys(PersistentNode newNode, PersistentNode fullNode) {
			Array.Copy(fullNode.Keys, Degree, newNode.Keys, 0, Degree - 1);
			newNode.KeysCount = Degree - 1;

			Array.Clear(fullNode.Keys, Degree - 1, Degree);
			fullNode.KeysCount = Degree - 1;
		}

		public KeyData NewKey(string key, IndexData data) {
			var newKey = new KeyData { Offset = _cursor };

			using(var accessor = _indexFile.CreateViewAccessor(_cursor, IndexDataSize)) {
				WriteKey(accessor, key);

				accessor.Write(_cursor, ref data);
				_cursor += IndexDataSize;
			}

			newKey.Size = _cursor = newKey.Offset;
			return newKey;
		}

		public PersistentNode NewNode() {
			var node = new PersistentNode(_cursor, _degree2);
			_cursor += _nodeSize;

			return node;
		}

		public bool SearchPosition(PersistentNode node, string key, out IndexData found, out int position) {
			throw new NotImplementedException();
		}

		public void SetRoot(PersistentNode node) {
			_root = node;
		}

		public int Degree { get; }

		private void InitRootAndCursor() {
			using(var accessor = _indexFile.CreateViewAccessor(0, CursorHolderSize)) {
				_cursor = accessor.ReadInt64(0);
				if(_cursor == 0) {
					_cursor = CursorHolderSize + RootHolderSize;
					_root = NewNode();
				}
				else {
					var rootOffset = accessor.ReadInt64(RootHolderOffset);
					using(var reader = _indexFile.CreateViewStream(rootOffset, _nodeSize)) {
						var formatter = new BinaryFormatter();
						_root = (PersistentNode)formatter.Deserialize(reader);
						_root.Offset = RootHolderOffset;
					}
				}
			}
		}

		private void WriteKey(UnmanagedMemoryAccessor accessor, string key) {
			var keyBytes = Encoding.UTF8.GetBytes(key);
			accessor.Write(_cursor, keyBytes.LongLength);
			_cursor += sizeof(long);
			accessor.WriteArray(_cursor, keyBytes, 0, keyBytes.Length);
			_cursor += keyBytes.Length;
		}
	}

}