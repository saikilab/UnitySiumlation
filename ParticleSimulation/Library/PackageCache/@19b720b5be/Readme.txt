●●エクセル(.xls)ファイルのインポートテスト●●

【準備】
エクセルのファイルを扱うためのライブラリとしてNPOIを使用しています
ライブラリ自体はアップロードしていないので、下記からダウンロードしてください

http://npoi.codeplex.com/releases
(2.0 beta 1 のみ動作確認済み)

解凍後、dotnet3.5内のファイルを[Assets/Editor/dll/NPOI]に追加してください

【動作説明】
Editor/Data/CharaData.xlsをインポートしてAssets/Data/CharaData.assetsを生成・更新しています。
エクセルを編集・保存してUnityのエディタに戻ると自動的にReimportされて変更が反映されます。
ここに少し詳しく書いています↓
http://kzonag.blog.fc2.com/blog-entry-45.html

【参考】
以下の記事を参考にさせて頂きました
・[Unity]Excelでデータを管理してUnity iOS/Androidで使うワークフローをもう少し詳しく（修正版）
http://terasur.blog.fc2.com/blog-entry-511.html

・[Unity]NPOI2.0.1(beta 1)を使ってxlsとxlsxを読み込んでみた
http://caitsithware.sakura.ne.jp/wordpress/?p=108

・ScriptableObjectでのシリアライズ[Unity]
http://tasogare66.blog.fc2.com/blog-entry-61.html

・実行時のために最適なデータ構造を作成しよう
http://www.slideshare.net/pigeon6/ss-15740075

ーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーー

●●エクセル(.xls)ファイルのエクスポートテスト●●
エクセルへの書き出し機能を追加しました。
エクセルインポート時にデータに応じたオブジェクトがObjRootの子として生成されます。
Assets/Data/CharaData.assetsを選択した状態でメニューから[Assets/ExportExcel]を行うと、
ObjRootの子オブジェクトに基づいてCharaData.assetsが更新され、Editor/Data/CharaData.xlsが出力されます。
ここに少し詳しく書いています↓
http://kzonag.blog.fc2.com/blog-entry-46.html

【参考】
以下の記事を参考にさせて頂きました
・NPOI入門してみた
http://okazuki.hatenablog.com/entry/20091128/1259405232
