using System;
using System.Threading;
using System.Collections; 
using System.Timers;
using System.Text.RegularExpressions;
using Gtk;
using Gdk;
using System.Collections.Generic;
using System.IO;
using TagLib;
using GmX;


namespace GmX
{	
	public class PlayList
	{
		private MainWindow Owner;
		private ListStore musicListStore;
		private ListStore artistListStore;
		private ListStore albumListStore;
		private Thread musicLibraryLoader;
		private ArrayList tempMediaFolder = new ArrayList();
		private ArrayList tempMediaFile = new ArrayList();
		private int songCount;
		private int songsAdded;
		private bool playListLoaded = false;
		private string query = "select * from gmxdb order by artist ASC, album ASC, track ASC";
		private AudioDatabase db;
				
		
		public PlayList(MainWindow owner)
		{		
			this.artistListStore = new ListStore (typeof(Gdk.Pixbuf), typeof (string));
			this.albumListStore = new ListStore (typeof(Gdk.Pixbuf), typeof (string));
			this.musicListStore = new ListStore (typeof(string), typeof(string), typeof(string), typeof(string), typeof(int), typeof(int), typeof(int), typeof(string));
			this.Owner = owner;		
			this.db = new AudioDatabase();	
		}
		
		
		public ListStore getStore()
		{
			return this.musicListStore;	
		}
		
		
		public ListStore getArtistStore()
		{
			return this.artistListStore;	
		}
		
		public ListStore getAlbumStore()
		{
			return this.albumListStore;	
		}
		
		
		private void addFile(string artist, string track, string album, string genre, int runtime, int year, int filesize, string location)
		{
			if(artist.Length > 1 && track.Length > 1)
			{			
				Application.Invoke(delegate { this.musicListStore.AppendValues(artist, track, album, genre, runtime, year, filesize, location); });
						
				this.db.addSong(artist, track, album, genre, runtime, year, filesize, location);
			}
			
			string locactionAbrev = location;
					
			if(locactionAbrev.Length > 100)
			{
				int start = locactionAbrev.Length - 99;
				int end = locactionAbrev.Length - start;
				string abrev = locactionAbrev.Substring(start,end);
				locactionAbrev = " ..." + abrev;
			}
			
			Application.Invoke( delegate { this.Owner.updateMediaAddedInfo(this.songCount,this.songsAdded,artist,track,locactionAbrev); });
		}
	
		
		private void addToPlaylist(String directory)
		{
			this.songsAdded = 0;
			this.songCount = 0;
			this.getFolderSongCount(directory);
			this.DirSearch(directory);	
		}
		
		
		private void getFolderSongCount(string sDir)
		{
			try
       		{
           		foreach (string d in Directory.GetDirectories(sDir))
           		{
               		foreach (string f in Directory.GetFiles(d))
               		{						
						if(f.EndsWith(".mp3"))
						{
							this.songCount++;
						}
            	   }
						
            	   this.getFolderSongCount(d);
           		}				

       		}			
			catch (Exception t)	
			{
				ExceptionOutputHandler.handle(t);
			}			
		}
		
		
		private void DirSearch(string sDir)
		{					
       		try
       		{
           		foreach (string d in Directory.GetDirectories(sDir))
           		{
               		foreach (string f in Directory.GetFiles(d))
               		{						
						if(f.EndsWith(".mp3"))
						{
							ID3 id3 = new ID3(f);							
							this.addFile(id3.artist,id3.track,id3.album,id3.genre,id3.runtime,id3.year,id3.filesize,f);	
							this.songsAdded++;							
						}
            	   }
					
            	   DirSearch(d);
           		}
       		}			
			catch (Exception t)	
			{
				ExceptionOutputHandler.handle(t);
			}				
		}
		

		private void musicLibraryWorker()
    	{		
			if(this.tempMediaFolder.Count == 0 && this.tempMediaFile.Count == 0)
			{	
				if(this.playListLoaded == false)
				{									
					this.db.getSongs(this.query,this.musicListStore);
				}
				else 
				{
      				this.addToPlaylist("/home/dave/Music");
				}					
			}
			else 
			{				
				try
				{
					foreach(object j in this.tempMediaFile)
					{
						ID3 id3 = new ID3(j.ToString());							
						this.addFile(id3.artist,id3.track,id3.album,id3.genre,id3.runtime,id3.year,id3.filesize,j.ToString());
						Console.WriteLine("add file");
					}
			
					foreach(object k in this.tempMediaFolder)
					{
						String pop = k.ToString();
						this.addToPlaylist(pop);
					}
					
					Application.Invoke( delegate {  this.tempMediaFile.Clear(); } );
					Application.Invoke( delegate {  this.tempMediaFolder.Clear(); } );				
					
				}
				catch(Exception t)
				{
					ExceptionOutputHandler.handle(t);	
				}				
			}	
			
			Application.Invoke( delegate { this.Owner.updateNodeViewColumnStats(); } );	
			Application.Invoke( delegate { this.Owner.updateMediaAddedInfo(this.songCount,this.songCount,"","",""); });			
    	}
				
		
		public void addMedia (ArrayList item, Boolean directory)
		{
			this.songCount = 0;
			this.songsAdded = 0;	
										
			if(directory == true)
			{						
				this.tempMediaFolder = item;
			}
			else
			{
				this.tempMediaFile = item;
			}
					
			if(this.musicLibraryLoader != null)
			{
				if(this.musicLibraryLoader.IsAlive==false)
				{
					this.musicLibraryLoader = new Thread(new ThreadStart(musicLibraryWorker));
					this.musicLibraryLoader.Priority = ThreadPriority.BelowNormal;
					this.musicLibraryLoader.Start();
				}
			}
			else
			{
				this.musicLibraryLoader = new Thread(new ThreadStart(musicLibraryWorker));
				this.musicLibraryLoader.Priority = ThreadPriority.BelowNormal;
				this.musicLibraryLoader.Start();
			}		
		}
	
		
		public void addMedia (Boolean directory)
		{			
			this.songCount = 0;
			this.songsAdded = 0;
			
			Gtk.FileChooserDialog fc;
		
			if(directory == true)
			{
				fc = new Gtk.FileChooserDialog("Select Folders to add", this.Owner , FileChooserAction.SelectFolder, "Cancel",ResponseType.Cancel, "Add Folders",ResponseType.Accept);			
			}
			else
			{
				fc = new Gtk.FileChooserDialog("Select Files to add", this.Owner, FileChooserAction.Open, "Cancel",ResponseType.Cancel, "Add Files",ResponseType.Accept);			
			}
			
			fc.SelectMultiple = true;	
						
			if (fc.Run() == (int)ResponseType.Accept) 
			{
				foreach (string item in fc.Filenames)
				{
					try
					{
						System.Threading.Thread.Sleep(100);
						this.musicLibraryLoader.Abort();
					}
					catch(Exception t)	
					{
						ExceptionOutputHandler.handle(t);
					}
				
					if(directory == true)
					{						
						this.tempMediaFolder.Add(item);
					}
					else
					{
						this.tempMediaFile.Add(item);
					}
					
					if(this.musicLibraryLoader != null)
					{
						if(this.musicLibraryLoader.IsAlive==false)
						{
							this.musicLibraryLoader = new Thread(new ThreadStart(musicLibraryWorker));
							this.musicLibraryLoader.Priority = ThreadPriority.BelowNormal;
							this.musicLibraryLoader.Start();
						}
					}
					else
					{
						this.musicLibraryLoader = new Thread(new ThreadStart(musicLibraryWorker));
						this.musicLibraryLoader.Priority = ThreadPriority.BelowNormal;
						this.musicLibraryLoader.Start();
					}					
				}
			}
			
			fc.Destroy();
		}
		
		
		public void loadLibrary()
		{			
			// Start The Music Library Reader
			this.musicLibraryLoader = new Thread(new ThreadStart(musicLibraryWorker));
			this.musicLibraryLoader.Priority = ThreadPriority.BelowNormal;
			this.musicLibraryLoader.Start();	
		}
		
		
		public void abortMusicLibraryLoader()
		{
			try
			{
				this.musicLibraryLoader.Abort();	
			}
			catch (Exception t)	
			{
				ExceptionOutputHandler.handle(t);
			}
		}
		
		
		public int getSongCount()
		{
			return this.songCount;	
		}		
		
		
		public void reloadLibrary(string query)
		{
			this.query = query;
			this.musicListStore.Clear();
			this.playListLoaded = false;
			this.loadLibrary();
		}	
		
		public void addToLibrary(string query)
		{
			this.query = query;
			this.playListLoaded = false;
			this.loadLibrary();
		}	
				
	}
}
