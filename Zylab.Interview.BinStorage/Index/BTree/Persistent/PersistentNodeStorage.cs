using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace Zylab.Interview.BinStorage.Index.BTree.Persistent {

	public class PersistentNodeStorage : INodeStorage<PersistentNode, KeyInfo> {
		private const int DefaultDegre = 1024;
		private const long DefaultCapacity = 0x400000; // 4 MB		

		private readonly int _degree2;
		private readonly MemoryMappedFile _indexFile;
		private readonly Sizes _sizes;
		private long _cursor;
		private PersistentNode _root;

		public PersistentNodeStorage(string indexFilePath, long capacity = DefaultCapacity, int degree = DefaultDegre) {
			Degree = degree;
			_degree2 = 2 * degree;
			_sizes = new Sizes(degree);

			_indexFile = MemoryMappedFile.CreateFromFile(
				indexFilePath,
				FileMode.OpenOrCreate,
				null,
				capacity,
				MemoryMappedFileAccess.ReadWrite);

			InitRootAndCursor();
		}

		public void Dispose() {
			using(var accessor = _indexFile.CreateViewAccessor(0, Sizes.CursorHolderSize + Sizes.RootHolderSize)) {
				accessor.Write(0, _cursor);
				accessor.Write(Sizes.RootHolderOffset, _root.Offset);
			}
			Commit(_root);
			_indexFile.Dispose();
		}

		public void AddChildren(PersistentNode node, PersistentNode children) {
			node.Childrens[node.ChildrensCount++] = children.Offset;
		}

		public void Commit(PersistentNode node) {
			using(var writer = _indexFile.CreateViewStream(node.Offset, _sizes.NodeSize)) {
				WriteInt(writer, node.KeysCount);
				WriteInt(writer, node.ChildrensCount);
				for(var i = 0; i < node.KeysCount; i++) {
					WriteInt(writer, node.Keys[i].Size);
					WriteLong(writer, node.Keys[i].Offset);
				}
				writer.Position = _sizes.ChildrensOffset;
				for(var i = 0; i < node.ChildrensCount; i++) {
					WriteLong(writer, node.Childrens[i]);
				}
			}
		}

		public int Compare(PersistentNode node, int keyIndex, string key) {
			return string.Compare(key, node.Keys[keyIndex].Key, StringComparison.OrdinalIgnoreCase);
		}

		public PersistentNode GetChildren(PersistentNode node, int position) {
			var offset = node.Childrens[position];

			return ReadNode(offset);
		}

		public KeyInfo GetKey(PersistentNode node, int position) {
			return node.Keys[position];
		}

		public PersistentNode GetRoot() {
			return _root;
		}

		public void InsertChildren(PersistentNode node, int position, PersistentNode children) {
			if(position > node.ChildrensCount || position < 0) {
				throw new ArgumentOutOfRangeException(nameof(position));
			}

			Array.Copy(node.Childrens, position, node.Childrens, position + 1, node.ChildrensCount - position);
			node.Childrens[position] = children.Offset;
			node.ChildrensCount++;
		}

		public void InsertKey(PersistentNode node, int position, KeyInfo key) {
			if(position > node.KeysCount || position < 0) {
				throw new ArgumentOutOfRangeException(nameof(position));
			}

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

		public KeyInfo NewKey(string key, IndexData data) {
			var buffer = Encoding.UTF8.GetBytes(key);
			var newKey = new KeyInfo { Offset = _cursor, Key = key, Size = buffer.Length };
			var size = Sizes.IndexDataSize + buffer.Length;

			using(var writer = _indexFile.CreateViewStream(_cursor, size)) {
				writer.Write(buffer, 0, buffer.Length);
				writer.Write(data.Md5Hash, 0, Sizes.Md5HashSize);
				WriteLong(writer, data.Size);
				WriteLong(writer, data.Offset);
			}

			_cursor += size;
			return newKey;
		}

		public PersistentNode NewNode() {
			var node = new PersistentNode(_cursor, _degree2);
			_cursor += _sizes.NodeSize;

			return node;
		}

		public bool SearchPosition(PersistentNode node, string key, out IndexData found, out int position) {
			var lo = 0;
			var hi = node.KeysCount - 1;
			while(lo <= hi) {
				var i = lo + ((hi - lo) >> 1);

				var keyInfo = node.Keys[i];
				var c = string.Compare(keyInfo.Key, key, StringComparison.OrdinalIgnoreCase);
				if(c == 0) {
					using(var reader = _indexFile.CreateViewAccessor(keyInfo.Offset + keyInfo.Size, Sizes.IndexDataSize)) {
						reader.Read(0, out found);
					}
					position = i;
					return true;
				}
				if(c < 0) {
					lo = i + 1;
				}
				else {
					hi = i - 1;
				}
			}
			position = lo;
			found = default(IndexData);
			return false;
		}

		public void SetRoot(PersistentNode node) {
			_root = node;
		}

		public int Degree { get; }

		private PersistentNode ReadNode(long offset) {
			PersistentNode node;
			using(var reader = _indexFile.CreateViewStream(offset, _sizes.NodeSize)) {
				node = new PersistentNode(offset, _degree2) {
					KeysCount = ReadInt(reader),
					ChildrensCount = ReadInt(reader)
				};
				for(var i = 0; i < node.KeysCount; i++) {
					node.Keys[i].Size = ReadInt(reader);
					node.Keys[i].Offset = ReadLong(reader);
				}
				reader.Position = _sizes.ChildrensOffset;
				for(var i = 0; i < node.ChildrensCount; i++) {
					node.Childrens[i] = ReadLong(reader);
				}
			}

			for(var i = 0; i < node.KeysCount; i++) {
				using(var reader = _indexFile.CreateViewAccessor(node.Keys[i].Offset, node.Keys[i].Size)) {
					node.Keys[i].Key = ReadKey(reader, node.Keys[i].Size);
				}
			}

			return node;
		}

		private static int ReadInt(Stream reader) {
			var buffer = new byte[sizeof(int)];
			reader.Read(buffer, 0, buffer.Length);
			return BitConverter.ToInt32(buffer, 0);
		}

		private static long ReadLong(Stream reader) {
			var buffer = new byte[sizeof(long)];
			reader.Read(buffer, 0, buffer.Length);
			return BitConverter.ToInt64(buffer, 0);
		}

		public static string ReadKey(MemoryMappedViewAccessor reader, int size) {
			var buffer = new byte[size];
			reader.ReadArray(0, buffer, 0, size);

			return Encoding.UTF8.GetString(buffer);
		}

		private static void WriteInt(Stream writer, int value) {
			var buffer = BitConverter.GetBytes(value);
			writer.Write(buffer, 0, buffer.Length);
		}

		private static void WriteLong(Stream writer, long value) {
			var buffer = BitConverter.GetBytes(value);
			writer.Write(buffer, 0, buffer.Length);
		}

		private void InitRootAndCursor() {
			using(var accessor = _indexFile.CreateViewAccessor(0, Sizes.CursorHolderSize + Sizes.RootHolderSize)) {
				_cursor = accessor.ReadInt64(0);
				if(_cursor == 0) {
					_cursor = Sizes.RootHolderOffset + Sizes.RootHolderOffset;
					_root = NewNode();
				}
				else {
					var rootOffset = accessor.ReadInt64(Sizes.RootHolderOffset);
					_root = ReadNode(rootOffset);
				}
			}
		}
	}

}