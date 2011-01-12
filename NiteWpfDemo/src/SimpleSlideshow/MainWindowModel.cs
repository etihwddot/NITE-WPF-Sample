
using System;
using System.Collections.ObjectModel;
using System.IO;
using Nui.Utility.Windows;

namespace SimpleSlideshow
{
	public sealed class MainWindowModel : NotifyPropertyChangedDispatcherBase, IDisposable
	{
		public MainWindowModel(string imagesFolder)
		{
			m_images = new ReadOnlyCollection<string>(Directory.GetFiles(imagesFolder, "*.jpg"));
			m_session = new NuiSession();
			m_session.SwipeLeft += Session_SwipeLeft;
			m_session.SwipeRight += Session_SwipeRight;
		}

		public string CurrentImageProperty = "CurrentImage";
		public string CurrentImage
		{
			get { return m_images[m_imageIndex]; }
		}

		public string CurrentImageNameProperty = "CurrentImageName";
		public string CurrentImageName
		{
			get { return Path.GetFileName(m_images[m_imageIndex]); }
		}

		public NuiSession Session
		{
			get { return m_session; }
		}

		private void Session_SwipeRight(object sender, EventArgs e)
		{
			PreviousImage();
		}

		private void Session_SwipeLeft(object sender, EventArgs e)
		{
			NextImage();
		}

		private void NextImage()
		{
			m_imageIndex++;
			if (m_imageIndex >= m_images.Count)
				m_imageIndex = m_images.Count - 1;
			RaisePropertyChanged(CurrentImageProperty);
		}

		private void PreviousImage()
		{
			m_imageIndex--;
			if (m_imageIndex < 0)
				m_imageIndex = 0;
			RaisePropertyChanged(CurrentImageProperty);
		}

		public void Dispose()
		{
			if (m_session != null)
			{
				m_session.Dispose();
				m_session = null;
			}
		}

		readonly ReadOnlyCollection<string> m_images;
		int m_imageIndex;
		NuiSession m_session;
	}
}
