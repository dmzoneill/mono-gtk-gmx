using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;


namespace GmX
{			
	public class Mpg123Wrapper
	{		
		private static string processName = "mpg123";
		private static string processArguments = "-R"; // interactive
		private static Boolean isActive = false;
		private static ThreadStart job = null;
		private static Thread jobRunner = null;
		private static int timeStarted = 0;
		private static System.Diagnostics.Process processInstance;
		private static StreamWriter mpg123streamWriter;
		
		
		public Mpg123Wrapper()
		{
			Mpg123Wrapper.start();
		}					
		
		
		public static void mp3Play(String fileLocation)
		{
			Mpg123Wrapper.mpg123streamWriter.Write("stop" + Environment.NewLine);	
			Mpg123Wrapper.mpg123streamWriter.Write("load " + fileLocation + Environment.NewLine);
		}
		
		
		public static void mp3Stop()
		{
			Mpg123Wrapper.mpg123streamWriter.Write("stop" + Environment.NewLine);
		}
		
		
		public static void mp3Pause()
		{
			Mpg123Wrapper.mpg123streamWriter.Write("pause" + Environment.NewLine);
		}
			
		
		public static void adjustVolume(int vol)
		{
			Mpg123Wrapper.mpg123streamWriter.Write("volume " + vol.ToString() +Environment.NewLine);
		}
		
		
		public static void seek(int position)
		{
			Mpg123Wrapper.mpg123streamWriter.Write("jump " + position.ToString() + "s" +Environment.NewLine);
		}
		
		
		public static void equalizer(int db, int band)
		{			
			Mpg123Wrapper.mpg123streamWriter.Write("eq 1 " + band + " " + db + Environment.NewLine);
			Mpg123Wrapper.mpg123streamWriter.Write("eq 2 " + band + " " + db + Environment.NewLine);
			band += 1;
			Mpg123Wrapper.mpg123streamWriter.Write("eq 1 " + band + " " + db + Environment.NewLine);
			Mpg123Wrapper.mpg123streamWriter.Write("eq 2 " + band + " " + db + Environment.NewLine);
			band += 1;
			Mpg123Wrapper.mpg123streamWriter.Write("eq 1 " + band + " " + db + Environment.NewLine);
			Mpg123Wrapper.mpg123streamWriter.Write("eq 2 " + band + " " + db + Environment.NewLine);
			band += 1;
			Mpg123Wrapper.mpg123streamWriter.Write("eq 1 " + band + " " + db + Environment.NewLine);
			Mpg123Wrapper.mpg123streamWriter.Write("eq 2 " + band + " " + db + Environment.NewLine);
		}
		
		
		public static void simple_equalizer(double bass, double mid, double high)
		{			
			Mpg123Wrapper.mpg123streamWriter.Write("seq " + bass + " " + mid + " " + high + " " + Environment.NewLine);
		}
		
		
		public static void start()
		{			
			try
			{
				if(Mpg123Wrapper.is_active()==false)
				{
					Mpg123Wrapper.timeStarted = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
					Mpg123Wrapper.isActive = true;
					Mpg123Wrapper.job = new ThreadStart(process);
        			Mpg123Wrapper.jobRunner = new Thread(Mpg123Wrapper.job);
        			Mpg123Wrapper.jobRunner.Start();						
				}										
			}
			catch(Exception t)
			{
				ExceptionOutputHandler.handle(t);			
			}
		}		
		
		
		public static void stop()
		{
			try
			{
				if(Mpg123Wrapper.isActive==true)
				{
					Mpg123Wrapper.processInstance.CloseMainWindow();
					Mpg123Wrapper.processInstance.Dispose();
					Mpg123Wrapper.processInstance.Close();		
					Mpg123Wrapper.isActive = false;
					Mpg123Wrapper.jobRunner.Abort();
				}
			}
			catch(Exception t)
			{
				ExceptionOutputHandler.handle(t);			
			}
		}
		
		
		public static Boolean is_active()
		{
			return Mpg123Wrapper.isActive;	
		}
		
		
		public static int getTimeStarted()
		{
			return Mpg123Wrapper.timeStarted;	
		}
		
		
		public static String getOutput()
		{
			return "";	
		}
		
				
		private static void process()
		{						
			if(Mpg123Wrapper.isActive==true)
			{		
				Mpg123Wrapper.processInstance = new Process();
        		Mpg123Wrapper.processInstance.StartInfo.FileName = Mpg123Wrapper.processName;
       			Mpg123Wrapper.processInstance.StartInfo.Arguments = Mpg123Wrapper.processArguments;				
       			Mpg123Wrapper.processInstance.StartInfo.UseShellExecute = false;
        		Mpg123Wrapper.processInstance.StartInfo.RedirectStandardOutput = true;
				Mpg123Wrapper.processInstance.OutputDataReceived += HandleOutputDataReceived;
				Mpg123Wrapper.processInstance.StartInfo.RedirectStandardInput = true;
				Mpg123Wrapper.processInstance.Start();
				Mpg123Wrapper.mpg123streamWriter = Mpg123Wrapper.processInstance.StandardInput;
				Mpg123Wrapper.mpg123streamWriter.AutoFlush = true;
				Mpg123Wrapper.mpg123streamWriter.Write("volume " + 100 + Environment.NewLine);
				Mpg123Wrapper.processInstance.BeginOutputReadLine();				   		
				Mpg123Wrapper.processInstance.WaitForExit();
				Mpg123Wrapper.isActive = false;
				Console.WriteLine("Mpg123 Stopped");
			}								
		}

		protected static void HandleOutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			
		}			
	}
}
