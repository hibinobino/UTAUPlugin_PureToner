using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System.Reflection;
using System.IO;

//※エンジン部分
//これをUTAUエンジンに指定することでこの中でtフラグを付与し、
//engine.txtで指定されたUTAUエンジンに渡して疑似的に純正律を出力させる。
namespace PureTonerConsole
{
	class Program
	{
		static void Main(string[] args)
		{
			string runtimePath = '"' + Assembly.GetEntryAssembly().Location + '"';
            string cmdLineArg = Environment.CommandLine.Replace(runtimePath + " ", "");

            //engine.txtから出力エンジンのパスを取得する
            string engine = getEnginePathFromTxt(runtimePath);

            //音階
            string cmdArgsTone = args[2];
            Console.WriteLine("Original Tone = " + args[2]);
            //フラグ
            string cmdArgsFlag = args[4];
            Console.WriteLine("Original flag = " + cmdArgsFlag);

			double heikinRatio = Math.Pow(2.0, 1.0 / 12.0);		//12乗根（平均律における半音階のHz倍率）
			double centRatio = Math.Pow(2.0, 1.0 / 1200.0);				//1200乗根（平均律における1cent音階（半音の1000分の1）のHz倍率）

			//各種長調、短調、クロマティックスケールの種類を羅列
			string[] majScales = new string[12] { "Cmaj", "C#maj", "Dmaj", "D#maj", "Emaj", "Fmaj", "F#maj", "Gmaj", "G#maj", "Amaj", "A#maj", "Bmaj" };
            string[] minScales = new string[12] { "Cmin", "C#min", "Dmin", "D#min", "Emin", "Fmin", "F#min", "Gmin", "G#min", "Amin", "A#min", "Bmin" };
			string[] allScales = majScales.Concat(minScales).ToArray();
            string[] chromScales = new string[12] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

            //C4～B4　平均律の周波数
            double[] evenfreq = getTemperateHzArray();

            //純正律の時の長調、短調の基音からのHz倍率
            double[] majPureRate = new double[12] { 1, 16.0/15.0, 9.0/8.0, 6.0/5.0, 5.0/4.0, 4.0/3.0, 45.0/32.0, 3.0/2.0, 8.0/5.0, 5.0/3.0, 9.0/5.0, 15.0/8.0 };
			double[] minPureRate = new double[12] { 1, 16.0/15.0, 9.0/8.0, 6.0 / 5.0, 32/25, 4.0/3.0, 64.0/45.0, 3.0/2.0, 8.0 / 5.0, 128.0/75.0, 9.0/5.0, 48.0/25.0 };

			//UTAUはA4=440Hz
			Console.WriteLine("UTAU Tuning = A4 440Hz");


            //フラグから調を判定する
            //scaleIndex = -1　フラグ指定なし：平均律として扱う
            //0 <= scaleIndex < 12　長調
            //12 <= scaleIndex < 24　短調
			//basekey　C～B(0-11)の基音のインデックス
            int scaleIndex = getScaleBaseKey(cmdArgsFlag, allScales);
            bool isMajor = scaleIndex < 12;        //長調かどうか判定
            int basekey = scaleIndex % 12;


			if (basekey != -1)
			{
				Console.WriteLine($"Base key is {basekey}/{chromScales[basekey]}");
				Console.WriteLine("Generate as pure intonation.");

				//C4台の純正律の周波数を格納する配列
				double[] purefreq = new double[12];

				//基音の周波数を一致させる（基準とする）
				purefreq[basekey] = evenfreq[basekey];

				//C4台の残りの周波数を決定する（基音はi=0から）
				for (int i = 0; i < 12; i++)
				{
					purefreq[(i + basekey) % 12] = purefreq[basekey] * (isMajor ? majPureRate[i] : minPureRate[i]);	//長調または短調の周波数倍率をかける
					if ((i + basekey) >= 12) purefreq[(i + basekey) % 12] /= 2;		//オクターブ高くなった場合は2で割る
				}

				//指定の音階の純正律の周波数を求める
				int height = int.Parse(cmdArgsTone.Substring(cmdArgsTone.Length - 1, 1));	//音階の最後の一文字
				string name = cmdArgsTone.Replace(height.ToString(), "");		//音階の音名の部分
				int tvalue = 0;		//tフラグの設定値
				for (int i = 0; i < 12; i++)
				{
					if (chromScales[i] == name)
					{
						//平均律と純正律の周波数差
						tvalue = (int)Math.Log(Math.Abs(purefreq[i] / evenfreq[i]), centRatio);		//平均律から純正律まで何セント分離れているか
						break;
					}
				}

				Console.WriteLine("Added t" + tvalue.ToString());

				//調フラグ削除してtフラグ追加
				string newFlag = cmdArgsFlag.Replace(majScales[basekey], "").Replace(minScales[basekey], "") + "t" + tvalue.ToString();

				//コマンドライン引数を新しいものに置き換え
                cmdLineArg = cmdLineArg.Replace('"' +cmdArgsFlag + '"', '"' + newFlag + '"');
			}
			else
			{
				//平均律で出力する場合は特に何もしない
				Console.WriteLine("No scale flag detected: Generate as even temperature.");
			}

			//engine.txt指定のエンジンに引数を渡して実行する
			var p = new ProcessStartInfo();
			p.FileName = engine;
			p.Arguments = cmdLineArg;
			p.UseShellExecute = false;
			p.CreateNoWindow = true;
			Process.Start(p).WaitForExit();

		}

		static string getEnginePathFromTxt(string runtimePath)
        {
            //同じディレクトリのengine.txtを読み取って
            //出力先のUTAUエンジンのパスを取得する
            string engine = "";
            try
            {
                using (var sr = new StreamReader(Path.Combine(Path.GetDirectoryName(runtimePath), "engine.txt"), Encoding.UTF8))
                {
                    engine = sr.ReadLine().Replace("\"", "");       //パスがダブルクオートで囲われているので取りのぞいてengineに代入
                    Console.WriteLine(engine);
                }
            }
            catch
            {
                Console.WriteLine("engine.txtが見つかりません。");
            }
            return engine;
        }

		static int getScaleBaseKey(string cmdArgsFlag, string[] scales)
		{
			int scaleIndex = -1;
            for (int i = 0; i < scales.Length; i++)
            {
                if (cmdArgsFlag.Contains(scales[i]))
                {
					//調がフラグに含まれている場合
                    if (scaleIndex == -1) scaleIndex = i;		//scaleIndexが未設定であればiを代入する
                    else
                    {
						//ほかの調が既に存在するので、フラグ上の記載順が後のほうを採用する
						scaleIndex = cmdArgsFlag.IndexOf(scales[i]) < cmdArgsFlag.IndexOf(scales[scaleIndex]) ? scaleIndex : i;
                    }
                }
            }
			return scaleIndex;
        }

		static double[] getTemperateHzArray()
		{
            double[] evenfreq = new double[12];
            for (int i = 0; i < 12; i++)
            {
                evenfreq[i] = 440.0 * Math.Pow(2.0, (i - 9) / 12.0);
            }
			return evenfreq;
        }
	}
}
