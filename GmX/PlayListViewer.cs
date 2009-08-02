
using System;

namespace GmX
{	
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlayListViewer : Gtk.Bin
	{
		
		public PlayListViewer()
		{
			this.Build();
			
			Gtk.TreeViewColumn col = new Gtk.TreeViewColumn();
			col.Title = "fail";
			this.songNodeView.AppendColumn(col);
			this.songNodeView.Model = new Gtk.ListStore(typeof(String));
			
		}
		
		
	}
}
