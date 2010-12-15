
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using ManagedNite;

namespace KinectWpfDemo
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			Loaded += MainWindow_Loaded;
			Closing += MainWindow_Closing;

			InitializeComponent();
		}

		private void MainWindow_Closing(object sender, CancelEventArgs e)
		{
			if (m_thread != null)
			{
				m_running = false;
				m_thread.Join();
			}
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			m_thread = new Thread(CreateAndRunKinect);
			m_running = true;
			m_thread.Start();
		}

		private void CreateAndRunKinect()
		{
			using (XnMOpenNIContext context = new XnMOpenNIContext())
			{
				context.Init();

				// TODO: why does it crash when we dispose session manager?
				XnMSessionManager sessionManager = new XnMSessionManager(context, "Wave", "RaiseHand");
				{
					BeginInvoke(() => { Status.Text = "Session created, wave to start session."; });

					sessionManager.SessionStarted += SessionManager_SessionStarted;
					sessionManager.SessionEnded += SessionManager_SessionEnded;

					while (m_running)
					{
						uint status = context.Update();
						if (status == 0)
							sessionManager.Update(context);
					}
				}
			}
		}

		private void SessionManager_SessionEnded(object sender, EventArgs e)
		{
			BeginInvoke(() => { Status.Text = "Session Ended, wave to start it again."; });
		}

		private void SessionManager_SessionStarted(object sender, PointEventArgs e)
		{
			BeginInvoke(() => { Status.Text = "Session Started"; });
		}

		private void BeginInvoke(Action action)
		{
			Dispatcher.BeginInvoke(DispatcherPriority.Background, action);			
		}

		bool m_running;
		Thread m_thread;
	}
}
