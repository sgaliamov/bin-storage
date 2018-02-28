using System;

namespace Zylab.Interview.BinStorage.Index.BTree {

	/// <summary>
	///     Abstract node storage for b-tree
	/// </summary>
	/// <typeparam name="TNode">Node type</typeparam>
	/// <typeparam name="TKey">Key type</typeparam>
	public interface INodeStorage<TNode, TKey> : IDisposable {
		/// <summary>
		///     B-tree dergree
		/// </summary>
		int Degree { get; }

		/// <summary>
		///     Allocate new node
		/// </summary>
		/// <returns>New node</returns>
		TNode NewNode();

		/// <summary>
		///     Return root of the tree
		/// </summary>
		/// <returns>Tree root</returns>
		TNode GetRoot();

		/// <summary>
		///     Define new root
		/// </summary>
		/// <param name="node">Node</param>
		void SetRoot(TNode node);

		/// <summary>
		///     Checks whether node is full
		/// </summary>
		/// <param name="node">Node</param>
		/// <returns>true, if keys count equals 2 * degree - 1, else false</returns>
		bool IsFull(TNode node);

		/// <summary>
		///     Checks whether node contains child nodes
		/// </summary>
		/// <param name="node">Node</param>
		/// <returns>true, if node does not contain childs, else false</returns>
		bool IsLeaf(TNode node);

		/// <summary>
		///     Search position of the key in the node
		/// </summary>
		/// <param name="node">Node</param>
		/// <param name="key">Key</param>
		/// <param name="found">If key is found, contains index data of the key</param>
		/// <param name="position">
		///     If key is found, contains position of the key
		///     if key is not found, returns position to insert
		/// </param>
		/// <returns>true, if key is presented in node, else false</returns>
		bool SearchPosition(TNode node, string key, out IndexData found, out int position);

		/// <summary>
		///     Compare given key with key in node
		/// </summary>
		/// <param name="node">Node</param>
		/// <param name="keyIndex">Index of a key in node</param>
		/// <param name="key">Key to compare</param>
		/// <returns>
		///     Above zero - if given key greater a key in node
		///     zero - if given key equals a key in node
		///     Less than zero - if given key lesser a key in node
		/// </returns>
		int Compare(TNode node, int keyIndex, string key);

		/// <summary>
		///     Creates new key
		/// </summary>
		/// <param name="key">Original key</param>
		/// <param name="data">Index data</param>
		/// <returns>New key info</returns>
		TKey NewKey(string key, IndexData data);

		/// <summary>
		///     Inset key to node to position
		/// </summary>
		/// <param name="node">Node</param>
		/// <param name="position">Position to insert</param>
		/// <param name="key">key</param>
		void InsertKey(TNode node, int position, TKey key);

		/// <summary>
		///     Get key at position
		/// </summary>
		/// <param name="node">Node</param>
		/// <param name="position">Position</param>
		/// <returns>Key at position</returns>
		TKey GetKey(TNode node, int position);

		/// <summary>
		///     Move rigth half of keys from full node to new node
		/// </summary>
		/// <param name="newNode">New node</param>
		/// <param name="fullNode">Full node</param>
		void MoveRightHalfKeys(TNode newNode, TNode fullNode);

		/// <summary>
		///     Add children to node
		/// </summary>
		/// <param name="node">Node</param>
		/// <param name="children">Children</param>
		void AddChildren(TNode node, TNode children);

		/// <summary>
		///     Inset child to node to position
		/// </summary>
		/// <param name="node">Node</param>
		/// <param name="position">Position to insert</param>
		/// <param name="children">Child node</param>
		void InsertChildren(TNode node, int position, TNode children);

		/// <summary>
		///     Get child of node at position
		/// </summary>
		/// <param name="node">Node</param>
		/// <param name="position">Position</param>
		/// <returns>Child node</returns>
		TNode GetChildren(TNode node, int position);

		/// <summary>
		///     Move rigth half of childs from full node to new node
		/// </summary>
		/// <param name="newNode">New node</param>
		/// <param name="fullNode">Full node</param>
		void MoveRightHalfChildrens(TNode newNode, TNode fullNode);

		/// <summary>
		///     Save node into storage
		/// </summary>
		/// <param name="node">Node</param>
		void Commit(TNode node);
	}

}