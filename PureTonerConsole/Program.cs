using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System.Reflection;
using System.IO;

namespace PureTonerConsole
{
	class Program
	{
		static void Main(string[] args)
		{
			bool ismajor = false;
			string engine = "";

			try
			{
				using (var sr = new StreamReader(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),"engine.txt"), Encoding.UTF8))
				{
					engine = sr.ReadLine().Replace("\"", "");
					Console.WriteLine(engine);
				}
			}
			catch
			{
				Console.WriteLine("engine.txtが見つかりません。");
			}

			string path = '"' + Assembly.GetEntryAssembly().Location + '"';
			string cmd = Environment.CommandLine.Replace(path + " ", "");

			double heikin = Math.Pow(2.0, 1.0 / 12.0);
			double cent = Math.Pow(2.0, 1.0 / 1200.0);

			string[] majscale = new string[12] { "Cmaj", "C#maj", "Dmaj", "D#maj", "Emaj", "Fmaj", "F#maj", "Gmaj", "G#maj", "Amaj", "A#maj", "Bmaj" };
			string[] chromscale = new string[12] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
			string[] minscale = new string[12] { "Cmin", "C#min", "Dmin", "D#min", "Emin", "Fmin", "F#min", "Gmin", "G#min","Amin", "A#min", "Bmin" };
			double[] majpurerate = new double[12] { 1, 16.0/15.0, 9.0/8.0, 6.0/5.0, 5.0/4.0, 4.0/3.0, 45.0/32.0, 3.0/2.0, 8.0/5.0, 5.0/3.0, 9.0/5.0, 15.0/8.0 };
			double[] minpurerate = new double[12] { 1, 16.0/15.0, 9.0/8.0, 6.0 / 5.0, 32/25, 4.0/3.0, 64.0/45.0, 3.0/2.0, 8.0 / 5.0, 128.0/75.0, 9.0/5.0, 48.0/25.0 };

			Console.WriteLine("UTAU Tuning = A4 440Hz");

			//音階
			string tone = args[2];
			Console.WriteLine("Original Tone = " + args[2]);
			//フラグ
			string flag = args[4];
			Console.WriteLine("Original flag = " + flag);

			#region フラグから調を判定/なければ平均律
			int basekey = -1;


			for (int i = 0; i < 12; i++)
			{
				if (flag.Contains(majscale[i]))
				{
					if (basekey == -1) basekey = i;
					else
					{
						//ほかの調が既に存在する場合、インデクスを比較
						if (flag.IndexOf(majscale[i]) < flag.IndexOf(majscale[basekey])) basekey = i;
					}
					ismajor = true;
				}
			}
			for (int i = 0; i < 12; i++)
			{
				if (flag.Contains(minscale[i]))
				{
					if (basekey == -1) basekey = i;
					else
					{
						//ほかの調が既に存在する場合、インデクスを比較
						if (ismajor && flag.IndexOf(minscale[i]) < flag.IndexOf(majscale[basekey])) basekey = i;
						if (!ismajor && flag.IndexOf(minscale[i]) < flag.IndexOf(minscale[basekey])) basekey = i;
					}
					ismajor = false;
				}
			}
			#endregion

			if (basekey != -1)
			{
				Console.WriteLine($"Base key is {basekey}/{chromscale[basekey]}");
				Console.WriteLine("Generate as pure intonation.");

				//平均律の周波数を求める
				#region
				double[] evenfreq = new double[12];
				for (int i = 0; i < 12; i++)
				{
					evenfreq[i] = 440.0 * Math.Pow(2.0, (i - 9) / 12.0);
				}
				#endregion

				//C4台の純正律の周波数を求める
				#region
				double[] purefreq = new double[12];

				//音の距離を確認
				int dis = (basekey < 9) ? 9 - basekey : (9 - (basekey - 12)) % 12;

				//purefreq[basekey] = 440.0 / (ismajor? majpurerate[dis] : minpurerate[dis]);
				purefreq[basekey] = evenfreq[basekey];

				for (int i = 0; i < 12; i++)
				{
					purefreq[(i + basekey) % 12] = purefreq[basekey] * (ismajor ? majpurerate[i] : minpurerate[i]);
					if ((i + basekey) >= 12) purefreq[(i + basekey) % 12] /= 2;
				}
				for (int i = 0; i < 12; i++)
				{

					//Console.WriteLine($"{chromscale[i]}4={evenfreq[i]}");

					//Console.WriteLine($"{chromscale[i]}4={purefreq[i]}");
				}
				#endregion

				//指定の音階の純正律の周波数を求める
				int height = int.Parse(tone.Substring(tone.Length - 1, 1));
				string name = tone.Replace(height.ToString(), "");
				int tvalue = 0;
				for (int i = 0; i < 12; i++)
				{
					if (chromscale[i] == name)
					{
						//平均律と純正律の周波数差
						double dist = purefreq[i] / evenfreq[i];
						tvalue = (int)Math.Log(Math.Abs(dist), cent);
						break;
					}
				}

				Console.WriteLine("Added t" + tvalue.ToString());

				//調フラグ削除
				//tフラグ追加
				string newflag = flag.Replace(majscale[basekey], "").Replace(minscale[basekey], "") + "t" + tvalue.ToString();
				cmd = cmd.Replace('"' + flag + '"', '"' + newflag + '"');


			}
			else
			{
				Console.WriteLine("No scale flag detected: Generate as even temperature.");
			}
			//Console.WriteLine(cmd);

			//var p = Process.Start(@"C:\Program Files (x86)\UTAU\resampler.exe", cmd);
			var p = new ProcessStartInfo();
			p.FileName = engine;
			p.Arguments = cmd;
			p.UseShellExecute = false;
			p.CreateNoWindow = true;
			Process.Start(p).WaitForExit();


		}

	}
}
