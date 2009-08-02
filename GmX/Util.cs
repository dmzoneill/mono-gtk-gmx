
using System;
using System.IO;

namespace GmX
{
	
	
	public class Util
	{
		
		public Util()
		{
			Util.recurseDirectory("/home/dave/Downloads");
		}
		
		
		public static void recurseDirectory(string dir)
		{		
			try	
			{
	   			foreach (string d in Directory.GetDirectories(dir)) 
	  			{
					foreach (string f in Directory.GetFiles(d)) 
					{
		   				Console.WriteLine(f);
					}
					recurseDirectory(d);
	  			}
			}
			catch (System.Exception excpt) 
			{
				Console.WriteLine(excpt.Message);
			}			
		}
		
	}
}
