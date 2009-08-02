using System;
using System.Text;
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


public partial class MainWindow: Gtk.Window
{		
	// PlayList
	private PlayList playList;	
	// The Playback monitor
	private System.Timers.Timer playBackProgressTrackerTimer;	
	// GTk.window.poup volume slide popup
	private VolumeSliderPopup volSlider = null;	
	// the current selected node
	private TreeSelection selection = null;	
	// variables used interop between threads for playback progress
	// and media list builder
	private int runtime = 0;
	private int currenttime = -1;	
	// trayicon
	private StatusIcon trayIcon;	
    private SortType sortOrder; 
	private Thread nodeViewUpdater;
	
	// Database connection
	private AudioDatabase db;
	
	// Drag and drop type
	enum TargetType 
	{
         Text,
         Uri
    };
	private Gtk.TargetEntry[] targetEntries = 
	{
		new TargetEntry ("string", 0, (uint)TargetType.Text),
		new TargetEntry ("text/plain", 0, (uint)TargetType.Text),
		new TargetEntry ("text/uri-list", 0, (uint)TargetType.Uri)
    }; 
	

	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{			
		// start up mpg123 wrapper
		new Mpg123Wrapper();
		
		//set up database
		this.db = new AudioDatabase();	
		
		Build ();			
		
		// set up the initial playlist
		this.playList = new PlayList(this);
		
		// Setup Size
		//this.Resize(1000,550);
		
		// Initilize the trayicon
		this.trayIconSetup();		
		
		// Setup the NodeView ( modal , tree columns etc )
		this.nodeViewSetup();
		
		// Setup styles and events handlers for labels / buttons
		this.guiExtendedSetup();	
		
		// Initilaize the Equalizer
		this.simpleEqualizerValueChanged(this,new EventArgs());						
		
		// load the library
		//this.playList.loadLibrary();
		// load the library artists and albums	
		// Start The Music Library Reader
		this.nodeViewUpdater = new Thread(new ThreadStart(nodeViewWorker));
		this.nodeViewUpdater.Priority = ThreadPriority.BelowNormal;
		this.nodeViewUpdater.Start();		
		
		new GmX.Util();
		

		
	}
	
	
	public void nodeViewWorker()
	{
		this.updateArtistsNodeview();
		this.updateAlbumsNodeview();
	}
	
	
	public void guiExtendedSetup()
	{		
		// Label styles
		this.spacer1Label.Text = " ";
		this.spacer2Label.Text = "";		
		this.newSongLabel.ModifyFont(Pango.FontDescription.FromString("bold 7"));
		this.newSongLocationLabel.ModifyFont(Pango.FontDescription.FromString("normal 7"));
		this.countUpTimeLabel.ModifyFont(Pango.FontDescription.FromString("DS-Digital 18"));
		this.countDownTimeLabel.ModifyFont(Pango.FontDescription.FromString("DS-Digital 18"));	
		this.tracksAddedLabel.ModifyFont(Pango.FontDescription.FromString("bold 7"));
		this.countUpTimeLabel.WidthRequest = 80;
		this.countDownTimeLabel.WidthRequest = 80;		
		this.trackLabel.ModifyFont(Pango.FontDescription.FromString("bold 10"));
		this.trackLabel.Text = "";		
		this.artistLabel.ModifyFont(Pango.FontDescription.FromString("normal 9 bold"));
		this.artistLabel.Text = "";		
		this.albumLabel.ModifyFont(Pango.FontDescription.FromString("normal 7"));
		this.albumLabel.Text = "";
		
		this.Icon = new Pixbuf("music-note.png");
		this.button1.ModifyFont(Pango.FontDescription.FromString("normal 7"));
						
		// Setup visible and invisible hboxes;				
		this.hbox11.Visible = true;
		this.hbox9.Visible = false;
		this.hbox10.Visible = false;
		this.hbox6.Visible = false;
		this.hbox4.Visible = false;
		this.hbox1.Visible = false;
		
		
		// Menu for the Add media Button
		Menu addMediaMenu = new Menu();
		MenuItem addFile = new MenuItem("Add files");
		addFile.Activated += delegate {	this.playList.addMedia(false); };
		addMediaMenu.Append(addFile);
		MenuItem addFolder = new MenuItem("Add folders");
		addFolder.Activated += delegate {	this.playList.addMedia(true); };
		addMediaMenu.Append(addFolder);
		
		// Add Media Button Menu Event Handler		
		this.addMediaButton.Activated += delegate { addMediaMenu.ShowAll(); addMediaMenu.Popup(); };
		
		// Button sensitivity settings
		this.mediaPauseAction.Sensitive = false;
		this.mediaStopAction.Sensitive = false;	
	}	
	
	
	public void nodeViewSetup()
	{		
		// Enqueue Column
		Gtk.TreeViewColumn enqueueColumn = new Gtk.TreeViewColumn ();
		enqueueColumn.Sizing = TreeViewColumnSizing.Fixed;
		enqueueColumn.FixedWidth = 20;
		enqueueColumn.Resizable = true;
		enqueueColumn.Title = "*";
		Gtk.CellRendererText enqueueNameCell = new Gtk.CellRendererText ();
		enqueueColumn.PackStart (enqueueNameCell, true);
		enqueueColumn.SetCellDataFunc( enqueueNameCell, new Gtk.TreeCellDataFunc (RenderEnqueue));
				
		// Artist Column
		Gtk.TreeViewColumn artistColumn = new Gtk.TreeViewColumn ();
		artistColumn.Sizing = TreeViewColumnSizing.Fixed;
		artistColumn.FixedWidth = 100;
		artistColumn.Resizable = true;
		artistColumn.Title = "Artist";
		Gtk.CellRendererText artistNameCell = new Gtk.CellRendererText ();
		artistColumn.PackStart (artistNameCell, true);
		artistColumn.SetCellDataFunc( artistNameCell, new Gtk.TreeCellDataFunc (RenderArtist) );
		artistColumn.Clickable = true;
		artistColumn.Reorderable = true;
		artistColumn.SortIndicator = true;
		artistColumn.Clicked += new EventHandler(onTreeColumnClicked); 
		
 		// Track Column
		Gtk.TreeViewColumn songColumn = new Gtk.TreeViewColumn ();
		songColumn.Sizing = TreeViewColumnSizing.Fixed;
		songColumn.FixedWidth = 200;
		songColumn.Resizable = true;
		songColumn.Title = "Track";
		Gtk.CellRendererText songTitleCell = new Gtk.CellRendererText ();
		songColumn.PackStart (songTitleCell, true);
		songColumn.SetCellDataFunc( songTitleCell, new Gtk.TreeCellDataFunc (RenderTrack));
		songColumn.Clickable = true;
		songColumn.Reorderable = true;
		songColumn.SortIndicator = true;
		songColumn.Clicked += new EventHandler(onTreeColumnClicked); 
		
		// Album column
		Gtk.TreeViewColumn albumColumn = new Gtk.TreeViewColumn ();
		albumColumn.Sizing = TreeViewColumnSizing.Fixed;
		albumColumn.FixedWidth = 150;
		albumColumn.Resizable = true;
		albumColumn.Title = "Album";
		Gtk.CellRendererText albumTitleCell = new Gtk.CellRendererText ();
		albumColumn.PackStart (albumTitleCell, true);
		albumColumn.Visible = true;	
		albumColumn.SetCellDataFunc( albumTitleCell, new Gtk.TreeCellDataFunc (RenderAlbum));
		albumColumn.Clickable = true;
		albumColumn.Reorderable = true;
		albumColumn.SortIndicator = true;
		albumColumn.Clicked += new EventHandler(onTreeColumnClicked); 
		
		// Location Column
		Gtk.TreeViewColumn locationColumn = new Gtk.TreeViewColumn ();
		locationColumn.Sizing = TreeViewColumnSizing.Fixed;
		locationColumn.FixedWidth = 150;
		locationColumn.Resizable = true;
		locationColumn.Title = "Location";
		Gtk.CellRendererText locationTitleCell = new Gtk.CellRendererText ();
		locationColumn.PackStart (locationTitleCell, true);
		locationColumn.Visible = false;
		locationColumn.SetCellDataFunc( locationTitleCell, new Gtk.TreeCellDataFunc (RenderLocation));
		locationColumn.Clickable = true;
		locationColumn.Reorderable = true;
		locationColumn.SortIndicator = true;
		
		// Runtime Column
		Gtk.TreeViewColumn runtimeColumn = new Gtk.TreeViewColumn ();
		runtimeColumn.Sizing = TreeViewColumnSizing.Fixed;
		runtimeColumn.FixedWidth = 30;
		runtimeColumn.Resizable = true;
		runtimeColumn.Title = "Runtime";
		Gtk.CellRendererText runtimeTitleCell = new Gtk.CellRendererText ();
		runtimeColumn.PackStart (runtimeTitleCell, true);
		runtimeColumn.Visible = true;
		runtimeColumn.SetCellDataFunc( runtimeTitleCell, new Gtk.TreeCellDataFunc (RenderRuntime));
		runtimeColumn.Clickable = true;
		runtimeColumn.Reorderable = true;
		runtimeColumn.SortIndicator = true;
		runtimeColumn.Clicked += new EventHandler(onTreeColumnClicked); 
		
		// Tracknum Column
		Gtk.TreeViewColumn tracknumColumn = new Gtk.TreeViewColumn ();
		tracknumColumn.Sizing = TreeViewColumnSizing.Fixed;
		tracknumColumn.FixedWidth = 40;
		tracknumColumn.Resizable = true;
		tracknumColumn.Title = "#";
		Gtk.CellRendererText tracknumTitleCell = new Gtk.CellRendererText ();
		tracknumColumn.PackStart (tracknumTitleCell, true);
		tracknumColumn.Visible = true;
		tracknumColumn.SetCellDataFunc( tracknumTitleCell, new Gtk.TreeCellDataFunc (RenderTracknum));	
				
		// Append the Columns
		this.nodeview1.AppendColumn (tracknumColumn);
		this.nodeview1.AppendColumn (enqueueColumn);
		this.nodeview1.AppendColumn (artistColumn);
		this.nodeview1.AppendColumn (songColumn);
		this.nodeview1.AppendColumn (albumColumn);
		this.nodeview1.AppendColumn (locationColumn);
		this.nodeview1.AppendColumn (runtimeColumn);
		
		// Nodeview Settings		
		this.nodeview1.EnableTreeLines = false;
		this.nodeview1.HeadersClickable = true;
		this.nodeview1.HeadersVisible = true;
		this.nodeview1.HoverSelection = false;
		this.nodeview1.Selection.Changed += nodeViewSelectionChanged;
		this.nodeview1.ModifyFont(Pango.FontDescription.FromString("normal 8"));
		this.nodeview1.ExpanderColumn = songColumn;
		
		this.nodeview2.EnableTreeLines = false;
		this.nodeview2.HeadersClickable = true;
		this.nodeview2.HeadersVisible = true;
		this.nodeview2.HoverSelection = false;
		this.nodeview2.Selection.Changed += artistNodeViewSelectionChanged;
		this.nodeview2.ModifyFont(Pango.FontDescription.FromString("normal 8"));
		
		this.nodeview3.EnableTreeLines = false;
		this.nodeview3.HeadersClickable = true;
		this.nodeview3.HeadersVisible = true;
		this.nodeview3.HoverSelection = false;
		//this.nodeview1.Selection.Changed += nodeViewSelectionChanged;
		this.nodeview3.ModifyFont(Pango.FontDescription.FromString("normal 8"));
		
		// artists node view nodeview 2
		
		// artists Icon Column
		Gtk.TreeViewColumn artistsIconHeader = new Gtk.TreeViewColumn ();
		artistsIconHeader.MinWidth = 50;
		artistsIconHeader.Spacing = 50;
		artistsIconHeader.Sizing = TreeViewColumnSizing.Fixed;
		Gtk.CellRendererPixbuf artistsIconCell = new Gtk.CellRendererPixbuf ();
		artistsIconHeader.PackStart (artistsIconCell, true);
		artistsIconHeader.SetCellDataFunc( artistsIconCell, new Gtk.TreeCellDataFunc (RenderArtistsIcon));
		this.nodeview2.AppendColumn (artistsIconHeader);
		
		// artists Column
		Gtk.TreeViewColumn artistsHeader = new Gtk.TreeViewColumn ();
		artistsHeader.Sizing = TreeViewColumnSizing.Fixed;
		Gtk.CellRendererText artistsCell = new Gtk.CellRendererText ();
		artistsHeader.PackStart (artistsCell, true);
		artistsHeader.SetCellDataFunc( artistsCell, new Gtk.TreeCellDataFunc (RenderArtists));
		this.nodeview2.AppendColumn (artistsHeader);
		
		// albums node view nodeview 3
		
		// albums ICON Column
		Gtk.TreeViewColumn albumsIcon = new Gtk.TreeViewColumn ();
		albumsIcon.MinWidth = 50;
		albumsIcon.Spacing = 50;		
		albumsIcon.Sizing = TreeViewColumnSizing.Fixed;
		Gtk.CellRendererPixbuf albumsIconCell = new Gtk.CellRendererPixbuf();
		albumsIcon.PackStart (albumsIconCell, true);
		albumsIcon.SetCellDataFunc( albumsIconCell, new Gtk.TreeCellDataFunc (RenderAlbumsIcon));
		this.nodeview3.AppendColumn(albumsIcon);
		
		// albums Column
		Gtk.TreeViewColumn albumsHeader = new Gtk.TreeViewColumn ();
		albumsHeader.Sizing = TreeViewColumnSizing.Fixed;
		Gtk.CellRendererText albumsCell = new Gtk.CellRendererText ();
		albumsHeader.PackStart (albumsCell, true);
		albumsHeader.SetCellDataFunc( albumsCell, new Gtk.TreeCellDataFunc (RenderAlbums));
		this.nodeview3.AppendColumn (albumsHeader);
		
		this.nodeview3.HeadersVisible = false;
		this.nodeview2.HeadersVisible = false;
		
		// Model for the (GTK.Nodeview)
		this.nodeview1.Model = this.playList.getStore();
		this.nodeview2.Model = this.playList.getArtistStore();
		this.nodeview3.Model = this.playList.getAlbumStore();
		
		//drag and drop
		Gtk.Drag.DestSet(this, DestDefaults.All, targetEntries, Gdk.DragAction.Copy);
        this.DragDataReceived += new DragDataReceivedHandler(OnDragDataReceived); 

		// Show The Nodeview
		this.nodeview1.ShowAll();	
		this.nodeview2.ShowAll();
		this.nodeview3.ShowAll();

	}
	
	
	public void onTreeColumnClicked(object sender, EventArgs args)
	{
		try
		{
			TreeViewColumn column;
        
			if (sender is TreeViewColumn)
			{
				column = (sender as TreeViewColumn);
				string st;
				
				if (this.sortOrder == SortType.Ascending) 
				{
					this.sortOrder = SortType.Descending;
					st = "DESC";
					
				}
				else 
				{
					this.sortOrder = SortType.Ascending;
					st = "ASC";
				}
				
				column.SortOrder = this.sortOrder;
                column.SortIndicator = true;	
				
				if(column.Title.Contains("Artist"))	
				{
					this.playList.reloadLibrary("select * from gmxdb order by artist " + st + ", track ASC");
				}
				else if(column.Title.Contains("Track"))	
				{
					this.playList.reloadLibrary("select * from gmxdb order by track " + st + ", artist ASC");
				}
				
				else if(column.Title.Contains("Album"))	
				{
					this.playList.reloadLibrary("select * from gmxdb order by album " + st + ", track ASC, artist ASC");
				
				}
				else if(column.Title.Contains("Runtime"))	
				{
					this.playList.reloadLibrary("select * from gmxdb order by runtime " + st + ", artist ASC, track ASC");
				}             
			} 			
		}
		catch (Exception t)
		{
			ExceptionOutputHandler.handle(t);
		}
	}
	
	
	protected void OnDragDataReceived (object o, DragDataReceivedArgs args)
    { 
		switch ((TargetType)args.Info)
		{
			case TargetType.Text:
			
				ArrayList files = new ArrayList();
				ArrayList folders = new ArrayList();
				
				foreach (string uri in Encoding.UTF8.GetString(args.SelectionData.Data).Trim().Split(System.Environment.NewLine.ToCharArray()))
				{			
					string text = uri;
					text = text.Substring(7,text.Length -7);
			    	text = text.Replace("%20"," ");
					text = text.Replace("%5B","[");
					text = text.Replace("%5D","]");
					text = text.Trim();
				
					if(System.IO.File.Exists(text)==true)
			   		{		
						files.Add(text);					
					}
					else if (System.IO.Directory.Exists(text)==true)
			   		{
						folders.Add(text);
					}
					else
					{
					}
				}
						
				this.playList.addMedia(folders,true);
				this.playList.addMedia(files,false);			

			break;

			case TargetType.Uri:
				
				foreach (string uri in Encoding.UTF8.GetString(args.SelectionData.Data).Trim().Split('\n'))
				{
					Console.WriteLine("Uri: {0}", Uri.UnescapeDataString(uri));
				}
			
			break;
		}
	}	                 
		                 
		                 
	public void trayIconSetup()
	{
		this.trayIcon = new StatusIcon(new Pixbuf ("/home/dave/workspace/GmX/GmX/bin/Debug/music-note.png"));
		this.trayIcon.Visible = true;
		this.trayIcon.Activate += delegate { this.Visible = !this.Visible; };
		this.trayIcon.PopupMenu += OnTrayIconPopup;
		this.trayIcon.Tooltip = ".............";			
	}
	
	
	public void updateNodeViewColumnStats()
	{
		/*
		int tsecs = 0;
		int mins = 0;
		int hours = 0;
		int count = 0;
		
		ArrayList distinctArtists = new ArrayList();
		ArrayList distinctAlbums = new ArrayList();
		
		foreach(object[] row in this.playList.getStore())
		{
			Song t = (Song) row[0];
			tsecs = tsecs + int.Parse(t.runtime);	
			if(!distinctArtists.Contains(t.artist))
			{
				distinctArtists.Add(t.artist);
			}
			if(!distinctAlbums.Contains(t.album))
			{
				distinctAlbums.Add(t.album);
			}
			
			count++;
		}
			
		mins = (tsecs / 60)  % 60;		
		hours = tsecs / 60 / 60;
		
		TreeViewColumn[] cols =  this.nodeview1.Columns;
		cols[6].Title = "Runtime [" + hours.ToString() + ":" + mins.ToString() + "] ";
		cols[3].Title = "Track [ " + count.ToString() + "] ";
		cols[2].Title = "Artist [ " + distinctArtists.Count.ToString() + "] ";
		cols[4].Title = "Album [ " + distinctAlbums.Count.ToString() + "] ";
		*/
	}
	
	
	public void OnTrayIconPopup (object o, EventArgs args) 
	{
		Menu popupMenu = new Menu();
		ImageMenuItem menuItemQuit = new ImageMenuItem ("Quit");
		Gtk.Image appimg = new Gtk.Image(Stock.Quit, IconSize.Menu);
		
		menuItemQuit.Image = appimg;
		popupMenu.Add(menuItemQuit);

		menuItemQuit.Activated += delegate {
			this.OnDeleteEvent(this,new DeleteEventArgs());
		};
		
		popupMenu.ShowAll();
		popupMenu.Popup();
	}
	
	
	private void RenderArtists (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{		
		
		try
		{
			string artist = model.GetValue(iter,1).ToString();
			
			if(artist.CompareTo("*****")!=0)
			{
				(cell as Gtk.CellRendererText).Foreground = "black";
				(cell as Gtk.CellRendererText).Background = "white";
				(cell as Gtk.CellRendererText).Text = artist;
			}
			else
			{
				(cell as Gtk.CellRendererText).Background = "Light Slate Gray";
				(cell as Gtk.CellRendererText).Foreground = "white";
				(cell as Gtk.CellRendererText).Text = "All Artists";
			}	
		}
		catch(Exception t)	
		{
			ExceptionOutputHandler.handle(t);
		}
	}
	
	
	
	private void RenderAlbums (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{		
		
		try
		{
			string album = model.GetValue(iter,1).ToString();
			
			if(album.CompareTo("*****")!=0)
			{
				(cell as Gtk.CellRendererText).Foreground = "black";
				(cell as Gtk.CellRendererText).Background = "white";
				(cell as Gtk.CellRendererText).Text = album + "";
			}
			else
			{
				(cell as Gtk.CellRendererText).Background = "Light Slate Gray";
				(cell as Gtk.CellRendererText).Foreground = "white";
				(cell as Gtk.CellRendererText).Text = "All Albums";
			}	
		}
		catch(Exception t)	
		{
			ExceptionOutputHandler.handle(t);
		}
	}
	
	
	private void RenderAlbumsIcon (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{		
		
		try
		{
			string location = this.db.getAlbumArt(model.GetValue(iter,1).ToString());
			
			if(location!=null)
			{
				(cell as Gtk.CellRendererPixbuf).Pixbuf = new Pixbuf(location,40,40,true);
			}
			else
			{
				if(model.GetValue(iter,1).ToString().CompareTo("*****") != 0)
				{
					(cell as Gtk.CellRendererPixbuf).Pixbuf = new Pixbuf("cd-rom-48x48.png",40,40,true);				
				}
				else
				{
					(cell as Gtk.CellRendererPixbuf).Pixbuf = new Pixbuf("blank.png",40,40, true);
				}
			}	
		}
		catch(Exception t)	
		{
			ExceptionOutputHandler.handle(t);
		}
	}
	
	
	
	private void RenderArtistsIcon (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{		
		
		try
		{			
			(cell as Gtk.CellRendererPixbuf).Pixbuf = new Pixbuf("blank.png",40,20,true);				
		}
		catch(Exception t)	
		{
			ExceptionOutputHandler.handle(t);
		}
	}
	
	
	
	private void RenderTracknum (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		try
		{
			int num = int.Parse(model.GetPath(iter).ToString()) + 1;
			(cell as Gtk.CellRendererText).Text = (num).ToString();
		}
		catch(Exception t)	
		{
			ExceptionOutputHandler.handle(t);
		}
	}
	
	
	private void RenderArtist (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		try
		{
			string artist = (string)  model.GetValue(iter,0);
			if(artist =="")
				artist = "unknown";
			
			(cell as Gtk.CellRendererText).Text = artist;
		}
		catch(Exception t)	
		{
			ExceptionOutputHandler.handle(t);
		}
	}
	
	
	private void RenderRuntime (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		try
		{
			int runtime = (int) model.GetValue(iter,4);	
			int seconds = runtime;
			string sSecs;
			int mins;
			
			if(seconds!=0)
			{			
				int secsMod = seconds % 60;
				if(secsMod < 10)
					sSecs = "0" + secsMod.ToString();
				else
					sSecs = secsMod.ToString();
				
				mins = seconds / 60;				
			}
			else
			{
				mins = 0;
				sSecs = "00";	
			}
			
			(cell as Gtk.CellRendererText).Text = "" + mins.ToString() + ":" + sSecs;
		}
		catch(Exception t)	
		{
			ExceptionOutputHandler.handle(t);
		}
	}
	
	
	private void RenderTrack (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		try
		{
			string track = (string) model.GetValue(iter,1);
			if(track =="")
				track = "unknown";
			
			(cell as Gtk.CellRendererText).Text = track;
		}
		catch(Exception t)	
		{
			ExceptionOutputHandler.handle(t);
		}
	}
	
	
	private void RenderEnqueue (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		try
		{
			string enqueue = "";
				
			if(enqueue.Length == 0)
			{
				(cell as Gtk.CellRendererText).Text = enqueue;
			}
			else
			{
				(cell as Gtk.CellRendererText).Foreground = "red";
				(cell as Gtk.CellRendererText).Text = enqueue;
			}
		}
		catch(Exception t)	
		{
			ExceptionOutputHandler.handle(t);
		}
	}
	
	
 	private void RenderAlbum (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		try
		{
			string album = (string) model.GetValue(iter,2);
			if(album =="")
				album = "unknown";

			(cell as Gtk.CellRendererText).Text = album;			
		}
		catch(Exception t)	
		{
			ExceptionOutputHandler.handle(t);
		}
	}
	
	
	private void RenderLocation (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		try
		{
			string location = (string) model.GetValue(iter,7);
			(cell as Gtk.CellRendererText).Text = location;
		}
		catch(Exception t)	
		{
			ExceptionOutputHandler.handle(t);
		}
	}
		

	public void updateMediaAddedInfo(int upper,int current,string artist,string track,string location)
	{
		if(this.hbox6.Visible == false)
		{
			Application.Invoke(delegate  {	this.hbox6.Visible = true;	});
		}
		
		Console.WriteLine(upper + " " + current);
		
		Application.Invoke(delegate  {
			this.newSongLabel.Text = artist + " - " + track;			
			this.songsAddedProgressBar.Adjustment.Upper = upper;
			this.songsAddedProgressBar.Adjustment.Value = current;
			this.newSongLocationLabel.Text = location.ToString();
			this.tracksAddedLabel.Text = "[ " + current + " / " + upper + " ]";			
			}
		);
		
		if(upper == current)
		{			
			Application.Invoke(delegate  {	this.hbox6.Visible = false;	});				
		}		
	}
	
	
	public void updateMediaPlayingInfo(String artist, String track, String album)
	{
		// label 4 5 6
		this.trackLabel.Text = track;
		this.artistLabel.Text = artist;
		this.albumLabel.Text = album;	
		this.AlbumArtIcon.Pixbuf = new Pixbuf(this.db.getAlbumArt(album),180,180,true);
	}
	
	
	public void progressChecker(object sender, EventArgs e)
	{		
		if(this.currenttime < this.runtime)
		{
			int rSecs, rMins, cPosSecs, cPosMins;
			string timenote, timenote1;			

			cPosSecs = (((int) songProgressBar.Adjustment.Value + 1) % 60);
			cPosMins = ((int) songProgressBar.Adjustment.Value + 1) / 60;	
			String secs = cPosSecs.ToString();
			if(cPosSecs < 10) secs = "0" + cPosSecs;
			
			timenote = cPosMins + ":" + secs;

			int timeleft = this.runtime - (int) songProgressBar.Adjustment.Value;
			rSecs = (int) timeleft % 60;
			rMins = (int) timeleft / 60;
			secs = rSecs.ToString();
			if(rSecs < 10) secs = "0" + rSecs;
			
			timenote1 = rMins + ":" + secs;

			this.currenttime += 1;
			Application.Invoke(delegate { songProgressBar.Adjustment.Value +=1; } );
			Application.Invoke(delegate { countUpTimeLabel.Text = timenote; countDownTimeLabel.Text = timenote1; });
			Application.Invoke(delegate { this.Title = this.artistLabel.Text + " - "  + this.trackLabel.Text; });
		}
		else
		{			
			this.playNext();
		}		
	}
	
	
	public void playNext()		
	{		
		this.currenttime = -1;
		this.songProgressBar.Adjustment.Value = 0;
		TreeModel model;
		TreeIter iter;
		String songLocation;
		this.hbox1.Visible = true;
		this.hbox4.Visible = true;
				
		try
		{			
			((TreeSelection)this.selection).GetSelected (out model, out iter);
			
			TreePath path =  model.GetPath(iter);
			path.Next();
			
			TreeIter next;
			model.GetIter(out next, path);
			
			songLocation = (string) this.nodeview1.Model.GetValue(next, 7);
			this.selection.TreeView.Selection.SelectIter(next);
			
		}		
		catch (Exception t)
		{			
			this.playList.getStore().GetIterFirst(out iter);
			songLocation = (string) this.nodeview1.Model.GetValue(iter, 7);			
			TreePath myPath = new TreePath("0");
			this.nodeview1.Selection.SelectPath(myPath); 		
			ExceptionOutputHandler.handle(t);
		}	
			
		
		try
		{
   			ID3 file = new ID3 (songLocation);
			this.updateMediaPlayingInfo(file.artist,file.track,file.album);
			this.runtime = file.runtime;
			this.songProgressBar.Adjustment.Lower = 0;
			this.songProgressBar.Adjustment.Upper = this.runtime;
			
		}
		catch(Exception t)	
		{
			ExceptionOutputHandler.handle(t);
		}
		
		
		try	
		{ 
			this.playBackProgressTrackerTimer.Stop();
		}	
		catch(Exception t)	
		{
			ExceptionOutputHandler.handle(t);
		}
		
		
		this.playBackProgressTrackerTimer = new System.Timers.Timer();
	    this.playBackProgressTrackerTimer.Elapsed += new ElapsedEventHandler(progressChecker);
		this.playBackProgressTrackerTimer.Interval = 1000;
		this.playBackProgressTrackerTimer.Start();
		
		Mpg123Wrapper.mp3Play(songLocation);
	}
	
	
	public void nodeViewSelectionChanged(object sender, EventArgs e)
	{				
		TreeModel model;
		TreeIter iterSelected;
  		if(((TreeSelection)sender).GetSelected (out model, out iterSelected)) 
		{
			this.selection = (TreeSelection) sender;
  		} 	
	}	
	
	
	protected virtual void OnNodeview1RowActivated (object sender, Gtk.RowActivatedArgs args)
	{	
		this.mediaPlayAction.Sensitive = false;
		this.mediaPauseAction.Sensitive = true;
		this.mediaStopAction.Sensitive = true;		
		this.hbox1.Visible = true;
		this.hbox4.Visible = true;
		this.songProgressBar.Visible = true;		
		this.currenttime = -1;
				
		try
		{
			try
			{
				this.playBackProgressTrackerTimer.Stop();	
			}
			catch(Exception t)	
			{
				ExceptionOutputHandler.handle(t);
			}
			
			TreeIter iter;
			this.playList.getStore().GetIter(out iter,args.Path);
			String songLocation = (string) this.nodeview1.Model.GetValue(iter, 7);	
			
   			ID3 file = new ID3 (songLocation);
			this.updateMediaPlayingInfo(file.artist,file.track,file.album);
			this.runtime = file.runtime;
			this.songProgressBar.Adjustment.Lower = 0;
			this.songProgressBar.Adjustment.Upper = this.runtime;	
			this.songProgressBar.Adjustment.Value = 0;
						
			
			this.playBackProgressTrackerTimer = new System.Timers.Timer();
	    	this.playBackProgressTrackerTimer.Elapsed += new ElapsedEventHandler(progressChecker);
			this.playBackProgressTrackerTimer.Interval = 1000;
			this.playBackProgressTrackerTimer.Start();
		
			Mpg123Wrapper.mp3Play(songLocation);
			
		}
		catch (Exception t)
		{
			ExceptionOutputHandler.handle(t);			
		}		
	}
	

	protected virtual void OnMediaStopActionActivated (object sender, System.EventArgs e)
	{		
		this.mediaPlayAction.Sensitive = true;
		this.mediaPauseAction.Sensitive = false;
		this.mediaStopAction.Sensitive = false;
		Mpg123Wrapper.mp3Stop();
		this.playBackProgressTrackerTimer.Stop();
		this.songProgressBar.Value = 0;
		this.hbox1.Visible = false;
		this.hbox4.Visible = false;
		this.songProgressBar.Visible = false;		
	}
	
	
	protected virtual void OnMediaPlayActionActivated (object sender, System.EventArgs e)
	{
		TreeModel model;
		TreeIter iterSelected;
		TreeIter iter;
		String songLocation;
		
		this.currenttime = -1;
		
		this.hbox1.Visible = true;
		this.hbox4.Visible = true;
		this.songProgressBar.Visible = true;
		
		if(this.selection!=null) 
		{
			((TreeSelection)this.selection).GetSelected (out model, out iterSelected);
			songLocation = (string) this.nodeview1.Model.GetValue(iterSelected, 7);					
		}		
		else
		{			
			this.playList.getStore().GetIterFirst(out iter);
			songLocation = (string) this.nodeview1.Model.GetValue(iter, 7);	
			TreePath myPath = new TreePath("0");
			this.nodeview1.Selection.SelectPath(myPath); 	
		}

		this.mediaPlayAction.Sensitive = false;
		this.mediaPauseAction.Sensitive = true;
		this.mediaStopAction.Sensitive = true;		
		this.songProgressBar.Visible = true;
		
		try
		{
   			ID3 file = new ID3 (songLocation);
			this.updateMediaPlayingInfo(file.artist,file.track,file.album);
			this.runtime = file.runtime;
			this.songProgressBar.Adjustment.Lower = 0;
			this.songProgressBar.Adjustment.Upper = this.runtime;
			
		}
		catch(Exception t)	
		{
			ExceptionOutputHandler.handle(t);
		}
		
		this.playBackProgressTrackerTimer = new System.Timers.Timer();
	    this.playBackProgressTrackerTimer.Elapsed += new ElapsedEventHandler(progressChecker);
		this.playBackProgressTrackerTimer.Interval = 1000;
		this.playBackProgressTrackerTimer.Start();
		
		Mpg123Wrapper.mp3Play(songLocation);		
	}
	
	
	protected virtual void OnMediaPauseActionActivated (object sender, System.EventArgs e)
	{
		Mpg123Wrapper.mp3Pause();
		if(this.mediaPauseAction.Active == true)
		{
			this.playBackProgressTrackerTimer.Stop();	
		}
		else
		{
			this.playBackProgressTrackerTimer.Start();
		}
	}
	
	
	protected virtual void OnAudioSettingsActionToggled (object sender, System.EventArgs e)
	{
		if(hbox10.Visible == false)
		{
			hbox10.Visible = true;
		}
		else 
		{
			hbox10.Visible = false;
		}
	}
	
	
	protected virtual void OnMediaPreviousActionActivated (object sender, System.EventArgs e)
	{
		TreeModel model;
		TreeIter iterSelected;
		TreeIter iter;
		String songLocation;
		
		this.currenttime = -1;
		
		if(this.selection!=null) 
		{
			((TreeSelection)this.selection).GetSelected (out model, out iterSelected);
			
			TreePath path =  model.GetPath(iterSelected);
			path.Prev();
			
			TreeIter prev;
			model.GetIter(out prev, path);
			
			songLocation = (string) this.nodeview1.Model.GetValue(prev, 7);
			this.selection.TreeView.Selection.SelectIter(prev);
		}		
		else
		{			
			this.playList.getStore().GetIterFirst(out iter);
			songLocation = (string) this.nodeview1.Model.GetValue(iter, 7);		
		}
		
		this.mediaPlayAction.Sensitive = false;
		this.mediaPauseAction.Sensitive = true;
		this.mediaStopAction.Sensitive = true;		
		this.hbox1.Visible = true;
		this.hbox4.Visible = true;
		this.songProgressBar.Visible = true;
		
		
		try
		{
   			ID3 file = new ID3 (songLocation);
			this.updateMediaPlayingInfo(file.artist,file.track,file.album);
			this.runtime = file.runtime;
			this.songProgressBar.Adjustment.Lower = 0;
			this.songProgressBar.Adjustment.Upper = this.runtime;
			this.songProgressBar.Adjustment.Value = 0;
			
		}
		catch(Exception t)	
		{
			ExceptionOutputHandler.handle(t);
		}
		
		
		try
		{
			this.playBackProgressTrackerTimer.Stop();	
		}
		catch(Exception t)	
		{
			ExceptionOutputHandler.handle(t);
		}
		
		
		this.playBackProgressTrackerTimer = new System.Timers.Timer();
	    this.playBackProgressTrackerTimer.Elapsed += new ElapsedEventHandler(progressChecker);
		this.playBackProgressTrackerTimer.Interval = 1000;
		this.playBackProgressTrackerTimer.Start();
		
		Mpg123Wrapper.mp3Play(songLocation);
		
	}
	
	
	protected virtual void OnMediaNextActionActivated (object sender, System.EventArgs e)
	{
		TreeModel model;
		TreeIter iterSelected;
		TreeIter iter;
		String songLocation;
		
		this.currenttime = -1;
		
		if(this.selection!=null) 
		{
			((TreeSelection)this.selection).GetSelected (out model, out iterSelected);
			
			TreePath path =  model.GetPath(iterSelected);
			path.Next();
			
			TreeIter next;
			model.GetIter(out next, path);
			
			songLocation = (string) this.nodeview1.Model.GetValue(next, 7);	
			this.selection.TreeView.Selection.SelectIter(next);
		}		
		else
		{			
			this.playList.getStore().GetIterFirst(out iter);
			songLocation = (string) this.nodeview1.Model.GetValue(iter, 7);		
		}
		
		this.mediaPlayAction.Sensitive = false;
		this.mediaPauseAction.Sensitive = true;
		this.mediaStopAction.Sensitive = true;		
		this.hbox1.Visible = true;
		this.hbox4.Visible = true;
		this.songProgressBar.Visible = true;
		
		try
		{
   			ID3 file = new ID3 (songLocation);
			this.updateMediaPlayingInfo(file.artist,file.track,file.album);
			this.runtime = file.runtime;
			this.songProgressBar.Adjustment.Lower = 0;
			this.songProgressBar.Adjustment.Upper = this.runtime;
			this.songProgressBar.Adjustment.Value = 0;
			
		}
		catch(Exception t)	
		{
			ExceptionOutputHandler.handle(t);
		}
		
		try
		{
			this.playBackProgressTrackerTimer.Stop();	
		}
		catch(Exception t)	
		{
			ExceptionOutputHandler.handle(t);
		}
		
		this.playBackProgressTrackerTimer = new System.Timers.Timer();
	    this.playBackProgressTrackerTimer.Elapsed += new ElapsedEventHandler(progressChecker);
		this.playBackProgressTrackerTimer.Interval = 1000;
		this.playBackProgressTrackerTimer.Start();
		
		Mpg123Wrapper.mp3Play(songLocation);
		
	}

	
	protected virtual void OnHscale1ChangeValue (object o, Gtk.ChangeValueArgs args)
	{
		Mpg123Wrapper.seek((int)this.songProgressBar.Adjustment.Value);
		this.currenttime = (int) this.songProgressBar.Adjustment.Value -1;
	}	
	
	
	protected virtual void OnVscale2ChangeValue (object o, Gtk.ChangeValueArgs args)
	{
		Mpg123Wrapper.equalizer((int)this.vscale2.Adjustment.Value,1);
	}	
	
	protected virtual void OnVscale3ChangeValue (object o, Gtk.ChangeValueArgs args)
	{
		Mpg123Wrapper.equalizer((int)this.vscale3.Adjustment.Value,5);
	}	
	
	protected virtual void OnVscale4ChangeValue (object o, Gtk.ChangeValueArgs args)
	{
		Mpg123Wrapper.equalizer((int)this.vscale4.Adjustment.Value,9);
	}	
	
	protected virtual void OnVscale5ChangeValue (object o, Gtk.ChangeValueArgs args)
	{		
		Mpg123Wrapper.equalizer((int)this.vscale5.Adjustment.Value,13);
	}	
	
	protected virtual void OnVscale6ChangeValue (object o, Gtk.ChangeValueArgs args)
	{
		Mpg123Wrapper.equalizer((int)this.vscale6.Adjustment.Value,17);
	}	
	
	protected virtual void OnVscale7ChangeValue (object o, Gtk.ChangeValueArgs args)
	{
		Mpg123Wrapper.equalizer((int)this.vscale7.Adjustment.Value,21);
	}
	
	protected virtual void OnVscale8ChangeValue (object o, Gtk.ChangeValueArgs args)
	{
		Mpg123Wrapper.equalizer((int)this.vscale8.Adjustment.Value,25);
	}
	
	protected virtual void OnVscale9ChangeValue (object o, Gtk.ChangeValueArgs args)
	{
		Mpg123Wrapper.equalizer((int)this.vscale9.Adjustment.Value,29);
	}
	

	
	protected virtual void simpleEqualizerValueChanged (object sender, System.EventArgs e)
	{
		if(checkbutton2.Active == true)
		{
			double bass = Math.Round(this.hscale2.Adjustment.Value,2);
			double mid = Math.Round(this.hscale3.Adjustment.Value,2);
			double treble = Math.Round(this.hscale4.Adjustment.Value,2);
			Mpg123Wrapper.simple_equalizer(bass,mid,treble);	
		}
		else
		{
			double bass = Math.Round(1.0,2);
			double mid = Math.Round(1.0,2);
			double treble = Math.Round(1.0,2);
			Mpg123Wrapper.simple_equalizer(bass,mid,treble);	
		}
	}
	
	
	protected virtual void OnCheckbutton2Toggled (object sender, System.EventArgs e)
	{
		this.simpleEqualizerValueChanged(this,new EventArgs());
	}
	
	
	protected virtual void OnDeleteEvent (object o, Gtk.DeleteEventArgs args)
	{
		try
		{
			this.playList.abortMusicLibraryLoader();
			Mpg123Wrapper.stop();
			System.Threading.Thread.Sleep(100);
			Console.WriteLine("Exit");
			Application.Quit();
		}
		catch(Exception t)	
		{
			ExceptionOutputHandler.handle(t);
		}
	}
	

	protected virtual void VolumeButtonActivatedHandler (object sender, System.EventArgs e)
	{
		if(this.volSlider != null)
		{		
			this.volSlider.Visible = false;
			this.volSlider.Dispose();
			this.volSlider.Destroy();
			this.volSlider = null;
			return;
		}
		
		int x, y;
		ModifierType mask;
		this.RootWindow.GetPointer(out x, out y,out mask);
		
		this.volSlider = new VolumeSliderPopup();		
		this.volSlider.Visible = true;
		this.volSlider.Modal = false;
		this.volSlider.Move(x +15, y-170);		
	}

	
	protected virtual void MainWindowFocusChangedButtonReleaseHandler (object o, Gtk.ButtonReleaseEventArgs args)
	{		
		if(this.volSlider != null)
		{		
			this.volSlider.Visible = false;
			this.volSlider.Dispose();
			this.volSlider.Destroy();
			this.volSlider = null;
		}
	}


	[GLib.ConnectBeforeAttribute]
	protected virtual void MusicNodeViewSelectionHandler (object o, Gtk.ButtonPressEventArgs args)
	{
		if(args.Event.Button == 3)
		{
			Menu popupMenu = new Menu();
			MenuItem enqueue = new ImageMenuItem ("Enqueue");
			enqueue.Activated += EnqueueSelectionHandler;
			popupMenu.Add(enqueue);
			MenuItem properties = new ImageMenuItem ("Properties");
			popupMenu.Add(properties);
			popupMenu.ShowAll();
			popupMenu.Popup();
		}
		
	}

	
	public void EnqueueSelectionHandler(object sender, EventArgs e)
	{
		TreeModel model;
		TreeIter iterSelected;
		((TreeSelection)this.selection).GetSelected (out model, out iterSelected);		
		//Song row = (Song) model.GetValue (iterSelected, 0);
		//row.enqueue = "1";
	}
	

	protected virtual void OnClearActionActivated (object sender, System.EventArgs e)
	{
		
			
	}
		
	
	public void setNodeViewSensitivity(bool sense)
	{
		this.nodeview1.Sensitive = sense;	
	}
	
	
	public void updateArtistsNodeview()
	{	
		this.db.getArtists(this.playList.getArtistStore());
	}
	
	
	public void updateAlbumsNodeview()
	{	
		this.db.getAlbums(this.playList.getAlbumStore());
	}
	
	
	
	public void artistNodeViewSelectionChanged(object sender, EventArgs e)
	{				
		TreeModel model;
		TreeIter iterSelected;
  		if(((TreeSelection)sender).GetSelected (out model, out iterSelected)) 
		{
			string artist = model.GetValue(iterSelected,1).ToString();
			this.playList.addToLibrary("select * from gmx where artist='" + Base64Encoder.Encode(artist) + "' order by album ASC, track ASC");
  		} 	
	}	
	

	protected virtual void OnButton1Clicked (object sender, System.EventArgs e)
	{
		this.playList.abortMusicLibraryLoader();
		Application.Invoke(delegate  {	this.hbox6.Visible = false;	});	
	}
	
}