
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

					using (XnMPointFilter filter = new XnMPointFilter())
					using (XnMPointDenoiser denoiser = new XnMPointDenoiser())
					{
						filter.PrimaryPointCreate += Filter_PrimaryPointCreate;
						filter.PrimaryPointDestroy += Filter_PrimaryPointDestroy;
						filter.PrimaryPointUpdate += Filter_PrimaryPointUpdate;

						denoiser.AddListener(filter);
						sessionManager.AddListener(denoiser);

						while (m_running)
						{
							uint status = context.Update();
							if (status == 0)
								sessionManager.Update(context);
						}
					}
				}
			}
		}

		private void Filter_PrimaryPointUpdate(object sender, HandPointContextEventArgs e)
		{
			BeginInvoke(() => { Status.Text = string.Format("Point updated\nX: {0:#.##}\nY: {1:#.##}\nZ: {2:#.##}.", e.HPC.Position.X, e.HPC.Position.Y, e.HPC.Position.Z); });
		}

		private void Filter_PrimaryPointDestroy(object sender, PointDestroyEventArgs e)
		{
			BeginInvoke(() => { Status.Text = "Point destroyed."; });
		}

		private void Filter_PrimaryPointCreate(object sender, PrimaryPointCreateEventArgs e)
		{
			BeginInvoke(() => { Status.Text = "Point created."; });
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
