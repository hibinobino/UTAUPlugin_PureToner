### PureTonerとは
疑似的に純正律で音声を生成できるようにtフラグを自動的に付与するプラグインです。

### 構成と仕組み
疑似エンジンとしてのPureToner.exeとプラグインとしてのPureTonerPlugin.exeが含まれます。
- **PureTonerPlugin.exe** ... プラグイン一覧に出てきます。ここで最終的に使いたいresamplerエンジンを指定することができ、またPureToner.exeのファイルパスをコピーすることができます。コピーしたファイルパスはUTAU本体のresampler欄に貼り付けてください。
- **PureToner.exe** ... UTAU本体のresamplerに指定することでノートにtフラグを付与してから任意のresamplerに渡します。これ自体に音声ファイルに出力する機能はありません。

### 実行ファイル(.uar)の配布場所
[ウェブサイト](https://hibinobino.main.jp/utau/plugins/#puretoner)にて配布しています。
