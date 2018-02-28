using System;
using System.IO;
using BinStorage.Index;

namespace BinStorage.Storage {

	/// <summary>
	///     Data storage interface
	/// </summary>
	public interface IStorage : IDisposable {
		/// <summary>
		///     Append new data to storage
		/// </summary>
		/// <param name="input">Inpup stream</param>
		/// <returns>Index info of processed data</returns>
		IndexData Append(Stream input);

		/// <summary>
		///     Read data by index
		/// </summary>
		/// <param name="indexData">index info</param>
		/// <returns>Data stream</returns>
		Stream Get(IndexData indexData);
	}

}