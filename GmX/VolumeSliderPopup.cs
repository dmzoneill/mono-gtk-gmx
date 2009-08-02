using System;

namespace GmX
{		
	public partial class VolumeSliderPopup : Gtk.Window
	{
		private bool muted = false;		
		
		public VolumeSliderPopup() : base(Gtk.WindowType.Popup)
		{
			this.Build();
			this.Resize(20,150);
			this.vscale1.HeightRequest = 150;
		}
		

		protected virtual void OnVscale1ValueChanged (object sender, System.EventArgs e)
		{
			if(Mpg123Wrapper.is_active() == false)
			{
				return;	
			}
			
			Mpg123Wrapper.adjustVolume((int)this.vscale1.Adjustment.Value);
			
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
		
		
		protected virtual void OnEventbox1ButtonReleaseEvent (object o, Gtk.ButtonReleaseEventArgs args)
		{
			
			if(Mpg123Wrapper.is_active() == false)
			{
				return;	
			}
						
			if(this.muted == false)
			{
				this.image1.Pixbuf = new Gdk.Pixbuf("audio-volume-muted.png");
				Mpg123Wrapper.adjustVolume(0);
				this.muted = true;
				this.vscale1.Sensitive = false;
			}
			else
			{
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
				
				Mpg123Wrapper.adjustVolume((int)this.vscale1.Adjustment.Value);	
			
				this.muted = false;
				this.vscale1.Sensitive = true;
				
			}
		}			
	}
}
