
using System;
using System.Threading;
using System.Windows.Threading;

namespace Nui.Utility.Windows
{
	public abstract class NotifyPropertyChangedDispatcherBase : NotifyPropertyChangedBase
	{
		protected NotifyPropertyChangedDispatcherBase()
		{
			if (m_dispatcher == null)
				throw new InvalidOperationException("This Dispatcher-affined object cannot be created on a thread that has no associated Dispatcher.");
		}

		public void VerifyAccess()
		{
			m_dispatcher.VerifyAccess();
		}

		protected bool CheckAccess()
		{
			return m_dispatcher.CheckAccess();
		}

		protected Dispatcher Dispatcher
		{
			get { return m_dispatcher; }
		}

		readonly Dispatcher m_dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
	}
}
