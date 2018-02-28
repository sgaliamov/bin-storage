﻿using System;
using System.IO;

namespace Zylab.Interview.BinStorage {

	public interface IBinaryStorage : IDisposable {
		/// <summary>
		///     Add data to the storage
		/// </summary>
		/// <param name="key">Unique identifier of the stream, cannot be null or empty</param>
		/// <param name="data">Non empty stream with data, cannot be null or empty </param>
		/// <exception cref="System.ArgumentException">
		///     An element with the same key already exists
		/// </exception>
		/// <exception cref="System.ArgumentNullException">
		///     key is null or data is null
		/// </exception>
		/// <exception cref="System.IO.IOException">
		///     I/O exception occurred during persisting data
		/// </exception>
		void Add(string key, Stream data);

		/// <summary>
		///     Get stream with data from the storage
		/// </summary>
		/// <param name="key">Unique identifier of the stream</param>
		/// <returns>Stream with data</returns>
		/// <exception cref="System.ArgumentNullException">
		///     key is null.
		/// </exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">
		///     key does not exist
		/// </exception>
		/// <exception cref="System.IO.IOException">
		///     I/O exception occurred during read
		/// </exception>
		Stream Get(string key);

		/// <summary>
		///     Check if key is present in the storage
		/// </summary>
		/// <param name="key">Unique identifier of the stream</param>
		/// <returns>true if key is present and false otherwise</returns>
		bool Contains(string key);
	}

}