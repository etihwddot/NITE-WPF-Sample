
using System.Windows;
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
		}

		NuiSession m_session;
	}
}
