using System;
using System.Text;
using System.Windows;
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
			string runtimePath = Environment.GetCommandLineArgs()[0];
			string runtimeDir = Path.GetDirectoryName(runtimePath);
			string txtPath = Path.Combine(runtimeDir, "engine.txt");

			//engine.txtが存在すればresampler情報を入力する
			if (File.Exists(txtPath))
			{
				using (StreamReader sr = new StreamReader(txtPath, Encoding.UTF8))
				{
					while (!sr.EndOfStream)
					{
						string line = sr.ReadLine();
						if (line != "") textboxResamplerPath.Text = line;	//情報があればテキストボックスに表示する
					}
				}
			}
			//puretonerのパスを表示する
			textboxPureTonerPath.Text = Path.Combine(runtimeDir, "PureToner.exe");		//コンソールのパスをテキストボックスに表示する
			textboxPureTonerPath.IsReadOnly = true;		//コピー専用なので読み取り専用にする
		}
		private void copybutton_Click(object sender, RoutedEventArgs e)
		{
			Clipboard.SetData(DataFormats.Text, (string)textboxPureTonerPath.Text);		//コンソールのパスをコピー
		}

		private void browsebutton_Click(object sender, RoutedEventArgs e)
		{
			var open = new OpenFileDialog();

			open.Filter = "UTAU用エンジン(.exe)|*.exe";
			if (open.ShowDialog() == true)
			{
				// 選択されたファイル名 (ファイルパス) をテキストボックスに表示
				textboxResamplerPath.Text = open.FileName;
			}
		}

		private void buttonOK_Click(object sender, RoutedEventArgs e)
		{
			string runtimePath = Environment.GetCommandLineArgs()[0];
			string runtimeDir = Path.GetDirectoryName(runtimePath);
			string txtPath = Path.Combine(runtimeDir, "engine.txt");

			//入力確認
			if (textboxResamplerPath.Text == textboxPureTonerPath.Text || Path.GetFileName(textboxResamplerPath.Text) == "PureToner.exe")
			{
				//コンソールをResamplerに指定禁止
				MessageBox.Show("ResamplerにPureTonerを指定できません！", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}else if (!File.Exists(textboxResamplerPath.Text))
			{
				//ファイルが存在しない場合
				MessageBox.Show("Resamplerの指定が存在しません！", "エラー",MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			//入力確認終わり、engine.txtに書き込む
			using (StreamWriter sr = new StreamWriter(txtPath, false, Encoding.UTF8))
			{
				sr.WriteLine(textboxResamplerPath.Text);
			}

			Close();
		}
	}
}
