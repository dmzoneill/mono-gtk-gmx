// MediaController.cs
// Andrés Villagrán Placencia - andres@villagranquiroz.cl
// [ Villagrán Quiroz ] Servicios informáticos
// http://www.villagranquiroz.cl
//

using System;
using System.IO;
using System.Collections;
using GmX.Player;
using Gtk;
using Gst;

namespace GmX {
	
	namespace Player {
	
		public class MediaController {
			private Controller controller;
			private TreeStore store;
			private int SongCount;
			
			
			public MediaController(Controller ctrl) {
				controller = ctrl;
				SongCount = 0;
			}
			private void CreateTreeStore() {
				try{ if(store.NColumns > 0) store.Clear(); store = null;} catch(Exception) {};
				
				store = new TreeStore(typeof(string), typeof(string), typeof(string), 
				                      typeof(string), typeof(string), typeof(string));
				
			}
			
			private void LoadDataBase() {
//				controller.database.Ejecutar("DELETE FROM Songs");
				//ArrayList lista = controller.mediaDataBase.SongList;
				
				//controller.mediaDataBase.FindMp3Files("/home/avillagran/Media");
				//controller.mediaDataBase.FindMp3Files("/media/cdrom0");

				//store.Clear();
				//foreach(string[] datos in lista) {
					//store.AppendValues(datos);
					//SongCount +=1;
				//}
			} 
			
			public void GetStore(out TreeStore store, out int SongCount) {
				CreateTreeStore();
				store = this.store;
				SongCount = this.SongCount;
			}
			public void GetSongs(out TreeStore store, out int SongCount) {
				//CreateTreeStore();
				LoadDataBase();
				store = this.store;
				SongCount = this.SongCount;
			}
		}
	}
}