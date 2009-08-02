// Player.cs
// Andrés Villagrán Placencia - andres@villagranquiroz.cl
// [ Villagrán Quiroz ] Servicios informáticos
// http://www.villagranquiroz.cl
//

using System;
using System.Threading;
using Gst;
using GLib;
using System.Runtime.InteropServices;
using TagLib;
using GmX.Player;

namespace GmX {
	namespace Player {
		public class MediaPlayer {
			private Controller controller;
			private static Gst.Bin pipeline = null;
			private System.Threading.Thread thread;
			private Element filesrc;
			private Element mad;
			private Element osssink;
			private static MainLoop loop;
			private State state;
			private string file;
			
			public MediaPlayer(Controller ctrl) {
				this.controller = ctrl;
				//Inicializar la Interfaz Multimedia
				Gst.Application.Init();
				loop = new MainLoop ();			
			}
			/// <summary>
			/// Parse messages and states of gstreamer pipeline
			/// </summary>
			/// <param name="bus">
			/// A <see cref="Bus"/>
			/// </param>
			/// <param name="message">
			/// A <see cref="Message"/>
			/// </param>
			/// <returns>
			/// A <see cref="System.Boolean"/>
			/// </returns>
			private bool BusCb (Bus bus, Message message) {
				switch (message.Type) {
					case MessageType.Error:
						string err = String.Empty;
						message.ParseError (out err);
						Console.WriteLine ("Gstreamer error: {0}", err);
						loop.Quit ();
						break;
					case MessageType.Eos:
						pipeline.SetState(State.Null);
						state = State.Null;
						//controller.View_NextSong();
						//pipeline.SetState(State.Playing);
						break;
				}
				
				return true;
			}
			/// <summary>
			/// Create a pipeline use gstreamer
			/// </summary>
			/// <returns>
			/// A <see cref="System.Boolean"/>
			/// </returns>
			private bool Make_Pipeline() {
				try{
					pipeline = new Pipeline("pipeline");
					
					// crear un elemento filesrc para leer el Archivo MP3
					filesrc = ElementFactory.Make ("filesrc", "filesrc");
					
					// Crear un Mad Decodificador
					mad = ElementFactory.Make ("mad", "mad");
					
					// Crear el Audio Sink
					osssink = ElementFactory.Make ("alsasink", "alsasink");
					
					// Agregar los elementos al pipeline principal
					pipeline.Add (filesrc);
					pipeline.Add (mad);
					pipeline.Add (osssink);
					
					// Conectar los elementos
					filesrc.Link (mad);
					mad.Link (osssink);
					
				}catch(Exception e){ Console.WriteLine(e); }
				
				return true;
			}
			/// <summary>
			/// This function reads the file to play
			/// </summary>
			public bool LoadFile(string file) {
				// The file to play exists
				if(System.IO.File.Exists(file)) {
					if(loop.IsRunning) loop.Quit();
					if(pipeline != null) {
						pipeline.SetState(State.Null);
						pipeline = null;
					}
					Make_Pipeline();
					controller.ResetTimer();
					pipeline.Bus.AddWatch (new BusFunc (BusCb));
					filesrc.SetProperty("location", file);
					this.file = file;					
					state = State.Ready;
					pipeline.SetState(state);
					controller.MediaPlayer_GetTags();
					//controller.buttons_SetMediaInfo();
					return true;
				} else { // The file dont exists
					filesrc.SetProperty("location", null);
					state = State.Ready;
					pipeline.SetState(state);
					return false;
				}
			}
			
			public bool Play() {	
				
				if(state == State.Paused) {
					Console.WriteLine("Playing");
					state = State.Playing;
					pipeline.SetState(state);
					return true;
				} else if(state == State.Ready) {
					state = State.Playing;
					pipeline.SetState(state);
					if(loop.IsRunning == false)
						loop.Run ();
					return true;
				} else {
					this.Pause();
					return false;
				}
			}
			public bool Pause() {
				if(state == State.Playing) {
					state = State.Paused;
					pipeline.SetState(state);
					return true;			
				} else 
					return false;
			}
			public void QuitLoop() {
				loop.Quit();
			}
			public File GetTags() {
				TagLib.File tagfile = TagLib.File.Create(this.file);
				return tagfile;
			}
			public File GetTagFile(string file) {
				TagLib.File tagfile = TagLib.File.Create(file);
				return tagfile;
			}
			public bool isPlaying() {
				if(state == State.Playing)
					return true;
				else 
					return false;
			}
		}
	}
}