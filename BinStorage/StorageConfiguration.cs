using System;

namespace BinStorage {

	/// <summary>
	/// Configuration
	/// </summary>
	public class StorageConfiguration {
		public StorageConfiguration() {
			StorageFileName = "storage.bin";
			IndexFileName = "index.bin";
			IndexTimeout = TimeSpan.FromSeconds(90);
		}

		/// <summary>
		///     Folder where implementation should store Index and FileStorage File
		/// </summary>
		public string WorkingFolder { get; set; }

		/// <summary>
		///     Name for storage file
		/// </summary>
		public string StorageFileName { get; set; }

		/// <summary>
		///     Name for index file
		/// </summary>
		public string IndexFileName { get; set; }

		/// <summary>
		///     Index lock timeout
		/// </summary>
		public TimeSpan IndexTimeout { get; set; }
	}

}