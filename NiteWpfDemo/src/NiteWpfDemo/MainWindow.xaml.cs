
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Nui.Utility.Windows;

namespace NiteWpfDemo
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			m_session = new NuiSession();
			DataContext = m_session;

			InitializeComponent();

			CompositionTarget.Rendering += CompositionTarget_Rendering;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (m_session != null)
			{
				m_session.Dispose();
				m_session = null;
			}
		}

		private void CompositionTarget_Rendering(object sender, EventArgs e)
		{
			ImageOutput.Source = m_session.GetColorImage();
			DepthOutput.Source = m_session.GetDepthImage();
		}

		NuiSession m_session;
	}
}
