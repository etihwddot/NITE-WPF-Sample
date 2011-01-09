
using System;
using System.Windows.Threading;

namespace Nui.Utility.Windows
{
	public static class DispatcherUtility
	{
		public static void BeginInvoke(this Dispatcher dispatcher, Action action)
		{
			dispatcher.BeginInvoke(DispatcherPriority.Background, action);
		}
	}
}
