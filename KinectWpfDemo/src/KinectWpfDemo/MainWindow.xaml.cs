
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using xn;
using xnv;

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
			using (Context context = new Context(@"data\openNI.xml"))
			{
				SessionManager sessionManager = new SessionManager(context, "Wave", "RaiseHand");
				BeginInvoke(() => { Status.Text = "Session created, wave to start session."; });

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
			HandPointContext contextCopy = context;
			BeginInvoke(() => { Status.Text = string.Format("Point updated\nX: {0:#.##}\nY: {1:#.##}\nZ: {2:#.##}.", contextCopy.ptPosition.X, contextCopy.ptPosition.Y, contextCopy.ptPosition.Z); });
		}

		private void PointControl_PrimaryPointDestroy(uint id)
		{
			BeginInvoke(() => { Status.Text = "Point destroyed."; });
		}

		private void  PointControl_PrimaryPointCreate(ref HandPointContext context, ref Point3D ptFocus)
		{
			BeginInvoke(() => { Status.Text = "Point created."; });
		}

		private void SessionManager_SessionStart(ref Point3D position)
		{
			BeginInvoke(() => { Status.Text = "Session Started"; });
		}

		private void SessionManager_SessionEnd()
		{
			BeginInvoke(() => { Status.Text = "Session Ended, wave to start it again."; });
		}

		private void BeginInvoke(Action action)
		{
			Dispatcher.BeginInvoke(DispatcherPriority.Background, action);			
		}

		volatile bool m_running;
		Thread m_thread;
	}
}
