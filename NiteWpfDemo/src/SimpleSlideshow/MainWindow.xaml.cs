
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SimpleSlideshow
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			m_model = new MainWindowModel(Path.Combine(Directory.GetCurrentDirectory(), @"Images"));
			DataContext = m_model;

			InitializeComponent();

			CompositionTarget.Rendering += CompositionTarget_Rendering;
		}

		private void CompositionTarget_Rendering(object sender, System.EventArgs e)
		{
			DepthOutput.Source = m_model.Session.GetDepthImage();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (m_model != null)
			{
				m_model.Dispose();
				m_model = null;
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
				Close();
		}

		MainWindowModel m_model;
	}
}
