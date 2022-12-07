using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using StaticExtensions;

namespace DiagramNet.Actions {


	internal class UndoManager {
		protected MemoryStream?[] list;
		protected int currPos = -1;
		protected int lastPos = -1;
		protected bool canUndo = false;
		protected bool canRedo = false;
		protected int capacity;
		protected bool enabled = true;


		public UndoManager(int capacity) {
			list = new MemoryStream[capacity];
			this.capacity = capacity;
		}

		public bool CanUndo {
			get {
				return (currPos != -1);
			}
		}

		public bool CanRedo {
			get {
				return (currPos != lastPos);
			}
		}

		public bool Enabled {
			get {
				return enabled;
			}
			set {
				enabled = value;
			}
		}

		public void AddUndo(Document o) {
			if (!enabled) return;

			currPos++;
			if (currPos >= capacity)
				currPos--;

			ClearList(currPos);

			PushList();

			list[currPos] = SerializeObject(o);
			lastPos = currPos;
		}

		public Document? Undo() {
			if (!CanUndo)
				throw new ApplicationException("Can't Undo.");
			MemoryStream? aMS = list[currPos];
			if (aMS is null) return null; 

			Document? ret = DeserializeObject(aMS);
			currPos--;
			return ret;			
		}

		public Document? Redo() {
			if (!CanRedo)
				throw new ApplicationException("Can't Undo.");

			currPos++;
			MemoryStream? aMS = list[currPos];
			if (aMS is null) return null;
			Document? ret = DeserializeObject(aMS);
			return ret;
		}

		private static MemoryStream? SerializeObject(Document o) {
			string? documentJson = o.AsJsonString();
			if (documentJson == null) { return null;}
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(documentJson);
			MemoryStream mem = new(buffer);
			return mem;
		}

		private static Document? DeserializeObject(MemoryStream mem) {
			byte[] buffer = mem.ToArray();
			string documentJson = System.Text.Encoding.UTF8.GetString(buffer);
			return (Document?)documentJson.AsFromJsonString();
		}

		private void ClearList(int p) {
			if (currPos >= capacity - 1)
				return;

			for (int i = p; i < capacity; i++) {
				MemoryStream? aMS = list[i];
				if (aMS is not null) aMS.Close();
				list[i] = null;
			}
		}

		private void PushList() {
			if (currPos >= capacity - 1) { 
				MemoryStream? aMS = list[currPos];
				if (aMS != null) {
					aMS.Close();
					for (int i = 1; i <= currPos; i++) {
						list[i - 1] = list[i];
					}
				}
			}
		}
	}
}
