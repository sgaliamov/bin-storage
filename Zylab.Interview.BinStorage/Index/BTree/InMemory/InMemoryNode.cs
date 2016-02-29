using System;
using System.Collections.Generic;
using System.Linq;

namespace Zylab.Interview.BinStorage.Index.BTree.InMemory {

	public class InMemoryNode : INode {
		// ReSharper disable once InconsistentNaming
		private readonly int _2t;
		private readonly List<INode> _childrens;
		private readonly List<IndexDataKey> _keys;

		public InMemoryNode(int t) {
			_2t = t << 1;
			_childrens = new List<INode>(_2t);
			_keys = new List<IndexDataKey>(_2t);
		}

		public void AddChildren(INode node) {
			_childrens.Add(node);
		}

		public void AddRangeChildrens(IEnumerable<INode> nodes) {
			_childrens.AddRange(nodes);
		}

		public void AddRangeKeys(IEnumerable<IndexDataKey> keys) {
			_keys.AddRange(keys);
		}

		public INode GetChildren(int position) {
			return _childrens[position];
		}

		public IndexDataKey GetKey(int position) {
			return _keys[position];
		}

		public IEnumerable<INode> GetRangeChildrens(int index, int count) {
			return _childrens.GetRange(index, count);
		}

		public IEnumerable<IndexDataKey> GetRangeKeys(int index, int count) {
			return _keys.GetRange(index, count);
		}

		public void InsertChildren(int position, INode node) {
			_childrens.Insert(position, node);
		}

		public void InsertKey(int position, IndexDataKey key) {
			_keys.Insert(position, key);
		}

		public void RemoveRangeChildrens(int index, int count) {
			_childrens.RemoveRange(index, count);
		}

		public void RemoveRangeKeys(int index, int count) {
			_keys.RemoveRange(index, count);
		}

		public int SearchPosition(string key, out IndexDataKey found) {
			var lo = 0;
			var hi = _keys.Count - 1;
			while(lo <= hi) {
				var i = lo + ((hi - lo) >> 1);

				var c = string.Compare(_keys[i].Key, key, StringComparison.OrdinalIgnoreCase);
				if(c == 0) {
					found = _keys[i];
					return i;
				}
				if(c < 0) {
					lo = i + 1;
				}
				else {
					hi = i - 1;
				}
			}

			found = null;
			return lo;
		}

		public bool IsFull => _keys.Count == _2t - 1;

		public bool IsLeaf => _childrens.Count == 0;

		public override string ToString() {
			return string.Join(", ", _keys.Select(x => x.Key));
		}
	}

}