using System;

namespace GmX
{	
	public partial class VolumePopupSlider : Gtk.Window
	{

		private Mpg123Wrapper deamon = null;
		private bool muted = false;
		
		protected virtual void OnVscale1ValueChanged (object sender, System.EventArgs e)
		{
			if(this.deamon == null)
			{
				return;	
			}
			
			this.deamon.adjustVolume((int)this.vscale1.Adjustment.Value);
			if(this.vscale1.Adjustment.Value > 70)
			{
				this.image1.Pixbuf = new Gdk.Pixbuf("audio-volume-high.png");
			}
			else if(this.vscale1.Adjustment.Value > 35)
			{
				this.image1.Pixbuf = new Gdk.Pixbuf("audio-volume-medium.png");
			}
			else
			{
				this.image1.Pixbuf = new Gdk.Pixbuf("audio-volume-low.png");
			}	
						
		}		
		
		public VolumePopupSlider() : base(Gtk.WindowType.Popup)
		{
			//this.Build();
			//this.Resize(20,150);
			//this.vscale1.HeightRequest = 150;
		}
		
		public void setParent(Mpg123Wrapper deamon)
		{
			this.deamon = deamon;	
		}


		protected virtual void OnEventbox2ButtonReleaseEvent (object o, Gtk.ButtonReleaseEventArgs args)
		{
			
			if(this.deamon == null)
			{
				return;	
			}
			
			if(this.muted == false)
			{
				this.image1.Pixbuf = new Gdk.Pixbuf("audio-volume-muted.png");
				this.deamon.adjustVolume(0);
				this.muted = true;
				this.vscale2.Sensitive = false;
			}
			else
			{
				if(this.vscale2.Adjustment.Value > 70)
				{
					this.image1.Pixbuf = new Gdk.Pixbuf("audio-volume-high.png");
				}
				else if(this.vscale2.Adjustment.Value > 35)
				{
					this.image1.Pixbuf = new Gdk.Pixbuf("audio-volume-medium.png");
				}
				else
				{
					this.image1.Pixbuf = new Gdk.Pixbuf("audio-volume-low.png");
				}		
				this.deamon.adjustVolume((int)this.vscale2.Adjustment.Value);	
			
				this.muted = false;
				this.vscale2.Sensitive = true;
			}
		}			
	}
}
