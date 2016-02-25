using System;

namespace Zylab.Interview.BinStorage {

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

		public string StorageFileName { get; set; }

		public string IndexFileName { get; set; }

		public TimeSpan IndexTimeout { get; set; }
	}

}