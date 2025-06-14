using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;

namespace PureTonerPlugin
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_ContentRendered(object sender, EventArgs e)
		{
			string thispath = Environment.GetCommandLineArgs()[0];
			string thisdir = Path.GetDirectoryName(thispath);
			string txtpath = Path.Combine(thisdir, "engine.txt");
			//engine.txtが存在すればresampler情報を入力する
			if (File.Exists(txtpath))
			{
				using (StreamReader sr = new StreamReader(txtpath, Encoding.UTF8))
				{
					while (!sr.EndOfStream)
					{
						string line = sr.ReadLine();
						if (line != "") resamplerpath.Text = line;
					}
				}
			}
			//puretonerのパスを表示する
			puretonerpath.Text = Path.Combine(thisdir, "PureToner.exe");
			puretonerpath.IsReadOnly = true;
		}
		private void copybutton_Click(object sender, RoutedEventArgs e)
		{
			Clipboard.SetData(DataFormats.Text, (string)puretonerpath.Text);
		}

		private void browsebutton_Click(object sender, RoutedEventArgs e)
		{
			var open = new OpenFileDialog();

			open.Filter = "UTAU用エンジン(.exe)|*.exe";
			if (open.ShowDialog() == true)
			{
				// 選択されたファイル名 (ファイルパス) をメッセージボックスに表示
				resamplerpath.Text = open.FileName;
			}
		}

		private void button1_Click(object sender, RoutedEventArgs e)
		{
			string thispath = Environment.GetCommandLineArgs()[0];
			string thisdir = Path.GetDirectoryName(thispath);
			string txtpath = Path.Combine(thisdir, "engine.txt");

			//OK
			if (resamplerpath.Text == puretonerpath.Text || Path.GetFileName(resamplerpath.Text) == "PureToner.exe")
			{
				MessageBox.Show("ResamplerにPureTonerを指定できません！", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}else if (!File.Exists(resamplerpath.Text))
			{
				MessageBox.Show("Resamplerの指定が存在しません！", "エラー",MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			using (StreamWriter sr = new StreamWriter(txtpath, false, Encoding.UTF8))
			{
				sr.WriteLine(resamplerpath.Text);
			}

			this.Close();
		}
	}
}
