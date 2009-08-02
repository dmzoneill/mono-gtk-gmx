using System;
using System.Data;
using Mono.Data.SqliteClient;
using System.Collections; 
using System.Timers;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using GmX;
using Gtk;
using Gdk;


namespace GmX
{	
	public class AudioDatabase
	{
		private string connectionString = "URI=file:/home/dave/gmx.db,version=3";
		private IDbConnection dbcon;
		private IDbCommand dbcmd;
		private IDataReader reader;
		private string table = "gmx";
		private Pixbuf blankPixbuf = new Pixbuf("blank.png",40,30,true);
		
		
		public AudioDatabase()
		{
			this.openConnection();			
		}
			
	
		public void addSong(string artist, string track, string album, string genre, Int32 runtime, Int32 year, Int32 filesize, string location)
		{			
			bool exists = false;
			
			try
			{
				string check = "SELECT location FROM " + this.table + " WHERE location='" + Base64Encoder.Encode(location) + "';";
				this.dbcmd.CommandText = check;
				this.reader = dbcmd.ExecuteReader();
				this.reader.Read();
				this.reader.GetString(0);
				exists = true;
				
			}
			catch (Exception t)
			{
				ExceptionOutputHandler.handle(t);
				exists = false;			
			}	
			
			if(exists == false)
			{				
				string insert = "INSERT INTO " + this.table + " (artist,track,album,genre,runtime,year,filesize,location) VALUES('" + Base64Encoder.Encode(artist) + "','" + Base64Encoder.Encode(track) + "','" + Base64Encoder.Encode(album) + "','" + Base64Encoder.Encode(genre) + "','" + runtime  + "','" + year + "','" + filesize + "','" + Base64Encoder.Encode(location) +"')";
				this.dbcmd.CommandText = insert;
				this.reader = this.dbcmd.ExecuteReader();				
			}					
		}
		
		
		public int getArtistCount()
		{
			try
			{
				string sql = "SELECT COUNT(DISTINCT artist) FROM " + this.table + "";
       			this.dbcmd.CommandText = sql;
       			this.reader = this.dbcmd.ExecuteReader();
				this.reader.Read();
				return int.Parse(this.reader.GetString(0));
			}
			catch (Exception t)
			{				
				ExceptionOutputHandler.handle(t);	
				return 0;
			}
		}
		
		
		public int getAlbumCount()
		{
			try
			{
				string sql = "SELECT COUNT(DISTINCT album) FROM " + this.table + "";
       			this.dbcmd.CommandText = sql;
       			this.reader = this.dbcmd.ExecuteReader();
				this.reader.Read();
				return int.Parse(this.reader.GetString(0));				
			}
			catch (Exception t)
			{				
				ExceptionOutputHandler.handle(t);
				return 0;
			}
		}
		
		
		public int getTrackCount()
		{
			try
			{
				string sql = "SELECT COUNT(track) FROM " + this.table + "";
       			this.dbcmd.CommandText = sql;
       			this.reader = this.dbcmd.ExecuteReader();
				this.reader.Read();
				return int.Parse(this.reader.GetString(0));				
			}
			catch (Exception t)
			{				
				ExceptionOutputHandler.handle(t);
				return 0;
			}
		}
		
		
		public void getSongs(string query, ListStore list)
		{
			
			try
			{				
       			string sql = query;
       			this.dbcmd.CommandText = sql;
       			this.reader = this.dbcmd.ExecuteReader();
       		
				while(reader.Read()) 
				{
					try
					{
						string artist = Base64Decoder.Decode(this.reader.GetString(0));
						string track = Base64Decoder.Decode(this.reader.GetString(1));
						string album = Base64Decoder.Decode(this.reader.GetString(2));
						string genre = Base64Decoder.Decode(this.reader.GetString(3));
						Int32 runtime = this.reader.GetInt32(4);
						Int32 year = this.reader.GetInt32(5);
						Int32 filesize = this.reader.GetInt32(6);
						string location = Base64Decoder.Decode(this.reader.GetString(7));
						list.AppendValues(artist,track,album,genre,runtime,year,filesize,location);
					}
					catch (Exception t)
					{
						ExceptionOutputHandler.handle(t);
					}	
				}
				
			}
			catch(Exception t)
			{
				ExceptionOutputHandler.handle(t);
			}
			
		}
		
		
		public void getArtists(ListStore temp)
		{
			temp.Clear();
			temp.AppendValues(null,"*****");
			
			try
			{				
       			string sql = "SELECT DISTINCT artist FROM " + this.table + " order by artist ASC";
       			this.dbcmd.CommandText = sql;
       			this.reader = this.dbcmd.ExecuteReader();
       		
				while(this.reader.Read()) 
				{
					try
					{
						string artist = Base64Decoder.Decode(this.reader.GetString(0));		
						Application.Invoke( delegate { temp.AppendValues(this.blankPixbuf,artist); });	
						System.Threading.Thread.Sleep(10);
					}
					catch (Exception t)
					{
						ExceptionOutputHandler.handle(t);
					}
				}
				
			}
			catch(Exception t)
			{
				ExceptionOutputHandler.handle(t);
			}
		}
		
		
		
		public void getAlbums(ListStore temp)
		{
			temp.Clear();
			string album;
			string location;
			string folder;
			Pixbuf tempPixbuf;
			ArrayList fail = new ArrayList();
			int y = 0;
			
			temp.AppendValues(null,"*****");
			
			try
			{				
				//string sql = "SELECT album, location FROM " + this.table + " WHERE album IN (SELECT DISTINCT album FROM " + this.table + " order by album ASC)";
       			string sql = "SELECT album, location FROM " + this.table + " order by album ASC";
       			this.dbcmd.CommandText = sql;
       			this.reader = this.dbcmd.ExecuteReader();
       		
				while(this.reader.Read()) 
				{
					album = this.reader.GetString(0);					
					Console.WriteLine("gg" + album);						
						
					if(!fail.Contains(album))
					{
						fail.Add(album);
						location = Base64Decoder.Decode(this.reader.GetString(1));
						folder = System.IO.Path.GetDirectoryName(location);	
					
						if(File.Exists(folder + "/cover.png"))
						{
							location = 	folder +  "/cover.png";
							tempPixbuf = new Pixbuf(location,40,40,true);
						}	
						else
						{
							location = "blank.png";	
							tempPixbuf = this.blankPixbuf;
						}
						
						Application.Invoke( delegate { temp.AppendValues(tempPixbuf,album); });
						y++;
						System.Threading.Thread.Sleep(100);
					}
				}				
			}
			catch(Exception t)
			{
				ExceptionOutputHandler.handle(t);
			}
			
			Console.WriteLine("done " + y);
		}
		
		
		public void openConnection()
		{
			this.dbcon = (IDbConnection) new SqliteConnection(this.connectionString);
       		this.dbcon.Open();
			
       		this.dbcmd = this.dbcon.CreateCommand();	
			
			try				
			{
				string create = "CREATE TABLE " + this.table + " (track varchar(255), artist varchar(255), album varchar(255), genre varchar(255), runtime Int32(10), year Int32(5), filesize Int32(15), location varchar(255))";
				this.dbcmd.CommandText = create;
				this.reader = this.dbcmd.ExecuteReader();
			}
			catch (Exception t)
			{
				ExceptionOutputHandler.handle(t);
			}				
		}
		
		
		public string getAlbumArt(string album)
		{
			string location = null;
			
			try
			{				
       			string sql = "SELECT location FROM " + this.table + " where album='"+ Base64Encoder.Encode(album) +"' limit 0, 1";
       			this.dbcmd.CommandText = sql;
       			this.reader = this.dbcmd.ExecuteReader();
       		
				while(this.reader.Read()) 
				{
					try
					{
						location = Base64Decoder.Decode(this.reader.GetString(0));						
					}
					catch (Exception t)
					{
						ExceptionOutputHandler.handle(t);
					}	
				}
				
			}
			catch(Exception t)
			{
				ExceptionOutputHandler.handle(t);
			}
			
			if(location!=null)
			{
				string folder = System.IO.Path.GetDirectoryName(location);	
				
				if(File.Exists(folder + "/cover.png"))
				{
					location = 	folder +  "/cover.png";
				}	
				else
				{
					location = null;	
				}
			}			
			
			return location;			
		}
		
		
		public void closeConnection()
		{
			this.reader.Close();
       		this.reader = null;
       		this.dbcmd.Dispose();
       		this.dbcmd = null;
       		this.dbcon.Close();
       		this.dbcon = null;				
		}
	}
}
