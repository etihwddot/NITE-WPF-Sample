
using System;
using System.Windows;
using System.Windows.Media;
using Nui.Utility.Windows;

namespace NiteWpfDemo
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			m_session = new NuiSession(this);
			DataContext = m_session;

			InitializeComponent();

			CompositionTarget.Rendering += CompositionTarget_Rendering;
		}

		private void CompositionTarget_Rendering(object sender, EventArgs e)
		{
			ImageOutput.Source = m_session.GetColorImage();
		}

		readonly NuiSession m_session;
	}
}
