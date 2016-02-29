using System;
using System.Collections.Generic;

namespace Zylab.Interview.BinStorage.Index.BTree {

	public interface INodeStorage : IDisposable {
		int Degree { get; }
		INode NewNode();
		INode GetRoot();
		void SetRoot(INode node);
		bool IsFull(INode node);
		bool IsLeaf(INode node);
		bool SearchPosition(INode node, string key, out IndexData found, out int position);
		int Compare(INode parent, int keyIndex, string key);

		IKey NewKey(string key, IndexData data);
		void InsertKey(INode node, int position, IKey key);
		IKey GetKey(INode node, int position);
		void AddRangeKeys(INode node, IEnumerable<IKey> keys);
		IEnumerable<IKey> GetRangeKeys(INode node, int index, int count);
		void RemoveRangeKeys(INode node, int index, int count);

		void AddChildren(INode node, INode children);
		void InsertChildren(INode node, int position, INode children);
		INode GetChildren(INode node, int position);
		void AddRangeChildrens(INode node, IEnumerable<INode> nodes);
		IEnumerable<INode> GetRangeChildrens(INode node, int index, int count);
		void RemoveRangeChildrens(INode node, int index, int count);

		void Commit(INode node);
	}

}