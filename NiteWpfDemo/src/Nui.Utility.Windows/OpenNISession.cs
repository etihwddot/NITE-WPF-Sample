
using System.ComponentModel;
using System.Threading;
using System.Windows;
using xn;
using xnv;

namespace Nui.Utility.Windows
{
	public sealed class NuiSession : NotifyPropertyChangedDispatcherBase
	{
		public NuiSession(Window window)
		{
			m_window = window;
			window.Loaded += Window_Loaded;
			window.Closing += Window_Closing;
		}

		public static readonly string StateProperty = "State";
		public SessionState State
		{
			get
			{
				VerifyAccess();
				return m_state;
			}
			private set
			{
				VerifyAccess();

				if (m_state != value)
				{
					m_state = value;
					RaisePropertyChanged(StateProperty);
				}
			}
		}

		public static readonly string PointProperty = "Point";
		public System.Windows.Media.Media3D.Point3D? Point
		{
			get
			{
				VerifyAccess();
				return m_point;
			}
			private set
			{
				VerifyAccess();

				if (m_point != value)
				{
					m_point = value;
					RaisePropertyChanged(PointProperty);
				}
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			// only listen to the first loaded
			m_window.Loaded -= Window_Loaded;

			// create a processing thread
			m_thread = new Thread(CreateAndRun);
			m_running = true;
			m_thread.Start();
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if (m_thread != null)
			{
				m_running = false;
				m_thread.Join();
			}
		}

		private void CreateAndRun()
		{
			using (Context context = new Context(@"data\openNI.xml"))
			{
				SessionManager sessionManager = new SessionManager(context, "Wave", "RaiseHand");

				// update the state
				Dispatcher.BeginInvoke(() => { State = SessionState.Idle; });

				sessionManager.SessionStart += SessionManager_SessionStart;
				sessionManager.SessionEnd += SessionManager_SessionEnd;

				PointControl pointControl = new PointControl();
				pointControl.PrimaryPointCreate += PointControl_PrimaryPointCreate;
				pointControl.PrimaryPointDestroy += PointControl_PrimaryPointDestroy;
				pointControl.PrimaryPointUpdate += PointControl_PrimaryPointUpdate;

				PointDenoiser denoiser = new PointDenoiser();
				denoiser.AddListener(pointControl);
				sessionManager.AddListener(denoiser);

				while (m_running)
				{
					context.WaitAndUpdateAll();
					sessionManager.Update(context);
				}
			}
		}

		private void PointControl_PrimaryPointUpdate(ref HandPointContext context)
		{
			System.Windows.Media.Media3D.Point3D? point = PointFromContext(context);
			Dispatcher.BeginInvoke(() => { Point = point; });
		}

		private void PointControl_PrimaryPointDestroy(uint id)
		{
			Dispatcher.BeginInvoke(() => { Point = null; });
		}

		private void PointControl_PrimaryPointCreate(ref HandPointContext context, ref Point3D ptfocus)
		{
			System.Windows.Media.Media3D.Point3D? point = PointFromContext(context);
			Dispatcher.BeginInvoke(() => { Point = point; });
		}

		private System.Windows.Media.Media3D.Point3D? PointFromContext(HandPointContext context)
		{
			return new System.Windows.Media.Media3D.Point3D(context.ptPosition.X, context.ptPosition.Y, context.ptPosition.Z);
		}

		private void SessionManager_SessionEnd()
		{
			// update the state
			Dispatcher.BeginInvoke(() => { State = SessionState.Idle; });
		}

		private void SessionManager_SessionStart(ref Point3D position)
		{
			// update the state
			Dispatcher.BeginInvoke(() => { State = SessionState.Running; });
		}

		readonly Window m_window;
		
		volatile bool m_running;
		Thread m_thread;
		SessionState m_state;
		private System.Windows.Media.Media3D.Point3D? m_point;
	}
}
