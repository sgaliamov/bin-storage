using System;

namespace Zylab.Interview.BinStorage.Index {

	/// <summary>
	///     Index interface
	/// </summary>
	public interface IIndex : IDisposable {
		/// <summary>
		///     Add new key and index data
		/// </summary>
		/// <param name="key">Key</param>
		/// <param name="indexData">Index data</param>
		void Add(string key, IndexData indexData);

		/// <summary>
		///     Get index data by key
		/// </summary>
		/// <param name="key">Key</param>
		/// <returns>Index data</returns>
		IndexData Get(string key);

		/// <summary>
		///     Checks whether key is presented in index
		/// </summary>
		/// <param name="key">Key</param>
		/// <returns> true - if contains, else - false</returns>
		bool Contains(string key);
	}

}