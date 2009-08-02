
using System;

namespace GmX
{		
	public class Song
	{	
		public string track;
		public string artist;
		public string album;
		public string location;
		public string enqueue;
		public string runtime;
		public string tracknum; 
		
		public Song (string location)
		{
			ID3 song = new ID3(location);
			
			this.enqueue = "";
			this.track = song.track +"";
			this.artist = song.artist +"";
			this.album = song.album +"";
			this.location = location +"";
			this.runtime = song.runtime;
			this.tracknum = "";
		} 
		
		public Song (string trackdb, string artistdb, string albumdb, string locationdb, string runtimedb)
		{
			this.enqueue = "";
			this.track = trackdb;
			this.artist = artistdb;
			this.album = albumdb;
			this.location = locationdb;
			this.runtime = runtimedb;
			this.tracknum = "";
		}
		
	}	
}
