// playing_info.cs
// Andrés Villagrán Placencia - andres@villagranquiroz.cl
// [ Villagrán Quiroz ] Servicios informáticos
// http://www.villagranquiroz.cl
//

using System;
using GmX.Player;
using Gtk;
using TagLib;
using GLib;
using Gst;

namespace GmX
{
	namespace Player {
	
		public class Controller
		{
			private string song;
			private string album;
			private string length;
			private string author;
			private string track;
			private int duration_min, duration_sec;
			private int cTime, min, sec;
			// Monoplayer internal objects
			private MediaPlayer player;
			private MainWindow mainWindow;

			private MediaController mediaController;

			
			public Controller() {
				cTime = 0; min = 0; sec = 0;
				// Creating Monoplayer modules
				player = new MediaPlayer(this);				
				mediaController = new MediaController(this);
								
			}
			public MediaPlayer GetPlayer() {
				return player;
			}
			
			public MainWindow mainwindow {
				get {
					return mainWindow;
				}
			}
			
			public bool MediaPlayer_Play() {
				if(player.Play()) {
					return true;
				}
				else {
					return false;
				}
			}
			public bool MediaPlayer_Pause() {
				if(player.Pause()) {
					return true;
				} else {
					return false;
				}
			}
			public bool MediaPlayer_LoadFile(string file) {
				if(player.LoadFile(file)) {		 
					return true;
				} else
					return false;
			}
			public void MediaPlayer_GetTags() {
				TagLib.File file = player.GetTags();
				song = file.Tag.Title;
				album = file.Tag.Album;
				author = file.Tag.FirstArtist;
				track = file.Tag.Track+"/"+file.Tag.TrackCount;
				duration_sec = file.Properties.Duration.Seconds;
				duration_min = file.Properties.Duration.Minutes;
				if(song == null) song = "[Desconocido]";
				if(album == null) album = "[Desconocido]";
				if(author == null) author = "[Desconocido]";
			}
			public TagLib.File MediaPlayer_GetTagFile(string File) {
				return player.GetTagFile(File);
			}
			public void MediaController_GetStore(out TreeStore store, out int SongCount) {
				TreeStore instore;
				int inSongCount;
				mediaController.GetStore(out instore, out inSongCount);
				store = instore;
				SongCount = inSongCount; 
			}
			public void MediaController_GetSongs(out TreeStore store, out int SongCount) {
				TreeStore instore;
				int inSongCount;
				mediaController.GetSongs(out instore, out inSongCount);
				store = instore;
				SongCount = inSongCount; 
			}
			private bool TimeHandler() {
				if(player.isPlaying() == true) {
					cTime += 1;
					
					if(cTime > 59) {
						min +=1;
						cTime = 0;
						sec = 0;
					}
					else {
						sec += 1;
					}
					//mainWindow.UpdateTime(String.Format("{0:00}:{1:00}", min, sec), 
					             //         String.Format("{0:00}:{1:00}", duration_min, duration_sec));
				}
				return true;
			}
			private bool ProgressHandler() {
				if(player.isPlaying() == true) {
					int cur = (min*60)+sec; // Calculating current time in seconds
					int tot = (duration_min*60)+duration_sec; // Calculating duration (song) time in seconds
					float progress = ((cur*100)/tot);
					progress = progress/100;
					//mainWindow.UpdateProgress(progress);
				}
				return true;
			}
			public void ResetTimer() {
				cTime = 0;
				min = 0;
				sec = 0;
			}
			public void ViewsController(string LoadView) {
				/*try { centro.Children[1].Destroy(); } catch(Exception){};
				switch(LoadView) {
					case "list_view":
						view = new list_view(this);
						centro.Add(view);
						break;
				}*/
			}
			public void MainQuit() {
				player.QuitLoop();
				//dataBase.Cerrar();
				//Application.Quit ();			
			}
		}
	}
}
