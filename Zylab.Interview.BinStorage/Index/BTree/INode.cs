using System.Collections.Generic;

namespace Zylab.Interview.BinStorage.Index.BTree {

	public interface INode {
		bool IsFull { get; }
		bool IsLeaf { get; }
		int SearchPosition(string key, out IndexDataKey found);

		void InsertKey(int position, IndexDataKey key);
		IndexDataKey GetKey(int position);
		void AddRangeKeys(IEnumerable<IndexDataKey> keys);
		IEnumerable<IndexDataKey> GetRangeKeys(int index, int count);
		void RemoveRangeKeys(int index, int count);

		void AddChildren(INode node);
		void InsertChildren(int position, INode node);
		INode GetChildren(int position);
		void AddRangeChildrens(IEnumerable<INode> nodes);
		IEnumerable<INode> GetRangeChildrens(int index, int count);
		void RemoveRangeChildrens(int index, int count);
	}

}