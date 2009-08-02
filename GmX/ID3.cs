using System;
using GmX;

namespace GmX
{		
	public class ID3
	{		
		
		public string artist = "";
		public string track = "";
		public string album = "";
		public string location = "";
		public string genre;
		public int runtime;
		public int year;
		public int filesize;
		
		public ID3(String f)
		{
			try
			{
				TagLib.File file = TagLib.File.Create (f);
			 	string title = file.Tag.Title + "";
				string[] artists = file.Tag.AlbumArtists;
				string salbum = file.Tag.Album + "";				
				string[] performers = file.Tag.Performers;
				string cgenre;
				
				String bloke = "";
				
				if(performers.Length > 0)
				{
					bloke = performers[0];
				}
				else if(artists.Length > 0 )
				{
					bloke = artists[0];
				}
				else
				{
					bloke = "";
				}
				
				
				
				string[] genres = file.Tag.Genres;
				
				if(genres.Length > 0)
				{
					cgenre = genres[0];
				}
				
				else
				{
					cgenre = "";
				}
				
						
				this.artist = bloke;
				this.track = title;
				this.album = salbum;
				this.location = f;
				this.genre = cgenre;
				this.runtime = (int) file.Properties.Duration.TotalSeconds;
				this.year = (int) file.Tag.Year;
				this.filesize = (int) file.Length;
				
			}
			catch (Exception t)
			{
				ExceptionOutputHandler.handle(t);
			}				
		}	
	}
}
