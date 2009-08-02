using System;

namespace GmX
{	
	public class ExceptionOutputHandler
	{		
		public static void handle(Exception excp)
		{
			Console.WriteLine("");
			Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
			Console.WriteLine("Exception -> ");
			Console.WriteLine("");
			Console.WriteLine(excp.StackTrace.ToString());
			Console.WriteLine("");
			Console.WriteLine("Error Message -> ");
			Console.WriteLine("");
			Console.WriteLine("  " + excp.Message.ToString());
		}
	}
}
