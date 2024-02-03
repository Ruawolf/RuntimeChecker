## .NET Framework

レジストリ：`HKEY_LOCAL_MACHINE\SOFTWARE\[WOW6432Node\]Microsoft\NET Framework Setup\NDP`

`C:\Windows\Microsoft.NET\Framework[64]<version>`にCLRと標準ライブラリがある。下位バージョンのファイルが少ないが、上位CLRに差分を足して動いているっぽい？（未調査）

[.NET Framework のバージョン確認方法](https://learn.microsoft.com/ja-jp/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed)

[.NET Framework のサービスパックレベル](https://learn.microsoft.com/ja-jp/troubleshoot/developer/dotnet/framework/general/determine-dotnet-versions-service-pack-levels)

### CLI

[CLRホストAPIリファレンス](https://learn.microsoft.com/ja-jp/dotnet/framework/unmanaged-api/hosting/clrcreateinstance-function)

[VBからCLR(.NET)を利用する その1[準備編]](https://www.pg-fl.jp/program/tips/vb2clr1.htm)

#### 取得手順

1. `mscoree.dll(metahost.h)`から`CLRCreateInstance()`を取得
2. `CLRCreateInstance()`で`ICLRMetaHost`インスタンス（COM）を取得
3. `ICLRMetaHost::EnumerateInstalledRuntimes()`で`ICLRRuntimeInfo `の列挙体を取得
4. `ICLRRuntimeInfo::GetVersionString()`でバージョン取得



## .NET

レジストリ：`HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions`

.NETは`<dotnetRoot>\shared\Microsoft.NETCore.App\<version>`にCLRと標準ライブラリが含まれている。

### 取得手順

1. `nethost.dll(nethost.h)`から`get_hostfxr_path()`を取得[^ nethost]
1. `get_hostfxr_path()`で`hostfxr.dll`のパスを取得
1. `hostfxr.dll(hostfxr.h)`から`hostfxr_get_dotnet_environment_info()`を取得
1. `hostfxr_get_dotnet_environment_info()`にコールバック関数を渡す
1. コールバック関数内で`hostfxr_dotnet_environment_info`構造体からバージョン取得

[^ nethost]: nethost.dllはランタイムインストールで配置されません。<br>通常はapphost(dotnet.exeやアプリビルド時のexe)に静的リンクされています。<br>C#で使用したい場合、外部DLLとして一緒に配布するか、C#上で挙動を再現する必要があります。

### get_hostfxr_path

`hostfxr.dll`のパスを取得するコード

https://github.com/dotnet/runtime/blob/5535e31a712343a63f5d7d796cd874e563e5ac14/src/native/corehost/nethost/nethost.cpp#L56



[.NET6 が起動するまでのコードを追ってみよう](https://qiita.com/up-hash/items/87e98261bb026298f207#hostfxr_resolver_t%E3%81%A7hostfxrdll%E3%82%92%E8%A7%A3%E6%B1%BA%E3%81%99%E3%82%8B)に`hostfxr_resolver`の解説がある。

`hostfxr_resolver`も`get_hostfxr_path`も内部で`try_get_path`を使っているので参考までに。



.NETのディレクトリレイアウト : https://learn.microsoft.com/ja-jp/dotnet/core/distribution-packaging

## Native AOTにおけるDLL、COMの使用方法（参考記事）

[.NET 8のCOM Interop用SourceGeneratorを導入する](https://www.sysnet.pe.kr/2/0/13470)

[LibraryImportAttributeが.NET 7で追加されたので触ってみました](https://tan.hatenadiary.jp/entry/2022/12/16/002739)