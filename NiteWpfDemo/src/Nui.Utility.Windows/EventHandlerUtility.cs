
using System;

namespace Nui.Utility.Windows
{
	public static class EventHandlerUtility
	{
		public static void RaiseEvent(this EventHandler handler, object sender)
		{
			if (handler != null)
				handler(sender, new EventArgs());
		}
	}
}
