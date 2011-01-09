
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

		public BitmapSource GetColorImage()
		{
			if (m_imageGenerator == null)
				return null;

			// calculate the core metadata
			ImageMetaData metadata = m_imageGenerator.GetMetaData();
			const int bytesPerPixel = 3;
			int totalPixels = metadata.XRes * metadata.YRes;

			// copy the image
			IntPtr imageMapPtr = m_imageGenerator.GetImageMapPtr();
			byte[] bytes = new byte[totalPixels * bytesPerPixel];
			Marshal.Copy(imageMapPtr, bytes, 0, bytes.Length);

			// create a bitmap
			return BitmapSource.Create(metadata.XRes, metadata.YRes, 96, 96, PixelFormats.Rgb24, null, bytes, metadata.XRes * bytesPerPixel);
		}

		public BitmapSource GetDepthImage()
		{
			int xResolution;
			int yResolution;
			ushort[] depths = GetDepths(out xResolution, out yResolution);
			if (depths == null)
				return null;

			const int bytesPerPixel = 3;

			// convert the depths to a grayscale image
			byte[] bytes = new byte[depths.Length * bytesPerPixel];
			for (int depthIndex = 0; depthIndex < depths.Length; depthIndex++)
			{
				int pixelIndex = depthIndex * bytesPerPixel;
				ushort depth = depths[depthIndex];

				byte gray = depth == 0 ? (byte) 0x00 : (byte) (0xFF - (depth >> 4));
				bytes[pixelIndex] = gray;
				bytes[pixelIndex + 1] = gray;
				bytes[pixelIndex + 2] = gray;
			}

			// create a bitmap
			return BitmapSource.Create(xResolution, yResolution, 96, 96, PixelFormats.Rgb24, null, bytes, xResolution * bytesPerPixel);
		}

		public ushort[] GetDepths(out int xResolution, out int yResolution)
		{
			xResolution = 0;
			yResolution = 0;

			if (m_depthGenerator == null)
				return null;

			// calculate the core metadata
			DepthMetaData metadata = m_depthGenerator.GetMetaData();
			xResolution = metadata.XRes;
			yResolution = metadata.YRes;
			int totalDepths = metadata.XRes * metadata.YRes;

			// copy the depths
			// TODO: Is there a better way to marshal ushorts from an IntPtr?
			IntPtr depthMapPtr = m_depthGenerator.GetDepthMapPtr();
			short[] depthsTemp = new short[totalDepths];
			Marshal.Copy(depthMapPtr, depthsTemp, 0, depthsTemp.Length);
			ushort[] depths = new ushort[totalDepths];
			Buffer.BlockCopy(depthsTemp, 0, depths, 0, totalDepths * metadata.BytesPerPixel);
			return depths;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			// only listen to the first loaded
			m_window.Loaded -= Window_Loaded;

			Debug.Assert(m_thread == null, "Can only run one instance of the processing thread.");

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
				m_context.Dispose();
				m_thread = null;
				m_context = null;
			}
		}

		private void CreateAndRun()
		{
			m_context = new Context(@"data\openNI.xml");
			m_imageGenerator = new ImageGenerator(m_context);
			m_depthGenerator = new DepthGenerator(m_context);

			SessionManager sessionManager = new SessionManager(m_context, "Wave", "RaiseHand");

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
				m_context.WaitAndUpdateAll();
				sessionManager.Update(m_context);
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

		Context m_context;
		ImageGenerator m_imageGenerator;
		DepthGenerator m_depthGenerator;
	
		volatile bool m_running;
		Thread m_thread;
		SessionState m_state;
		System.Windows.Media.Media3D.Point3D? m_point;
	}
}
