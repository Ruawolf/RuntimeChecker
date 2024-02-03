using RuntimeChecker.Utility;
using System.Text;

namespace RuntimeChecker;

internal class Program
{
    static readonly StringBuilder sb = new();

    [STAThread]
    static void Main()
    {
        if (OperatingSystem.IsWindows())
        {
            // Information
            var is64bit = Environment.Is64BitProcess;
            Output($"Application Mode : {(Environment.Is64BitProcess ? "64bit" : "32bit")}");
            Output($"System Architecture : {(Environment.Is64BitOperatingSystem ? "64bit OS" : "32bit OS")}");

            var (edition, displayVersion, buildVersion) = SystemInfo.WindowsInfo();
            Output($"Windows Edition : {edition}");
            Output($"Windows Version : {displayVersion}({buildVersion})");


            // .NET Framework
            var dfRuntime14 = DotnetFramework.Check1To4(is64bit);
            var dfRuntime45 = DotnetFramework.CheckAfter4d5(is64bit);
            var dfClr = DotnetFramework.CheckCLR();

            Output("\n.NET Framework:");
            RuntimeInfo.Merge(dfRuntime14, dfRuntime45).ForEach(Output);

            Output("\n.NET Framework CLR:");
            dfClr.ForEach(Output);

            // .NET
            var (dSdk, dFramework) = Dotnet.Check();

            Output("\n.NET SDK:");
            dSdk.ForEach(Output);

            Output("\n.NET:");
            dFramework.ForEach(Output);

            // ファイル出力
            var exeDirPath = Path.GetDirectoryName(AppContext.BaseDirectory);
            if (exeDirPath != null && Directory.Exists(exeDirPath))
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                File.WriteAllText(Path.Combine(exeDirPath, $"RuntimeCheckerResult_{timestamp}.txt"), sb.ToString());
            }
        }
        else
        {
            Console.WriteLine("Supported only on Windows.");
        }

        Console.WriteLine("\nPress any key to continue・・・");
        Console.ReadKey();
    }

    static void Output(RuntimeInfo x) => Output($"  {x.ToString()[(x.RuntimeName.Length + 1)..]}");
    static void Output() => Output(string.Empty);
    static void Output(string value)
    {
        sb.AppendLine(value);
        Console.WriteLine(value);
    }
}