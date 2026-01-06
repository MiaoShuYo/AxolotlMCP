using BenchmarkDotNet.Running;

namespace AxolotlMCP.Benchmarks;

/// <summary>
/// 基准测试程序入口。
/// </summary>
public static class Program
{
    /// <summary>
    /// 应用程序入口点。使用 BenchmarkSwitcher 支持命令行选择要运行的基准测试。
    /// </summary>
    /// <param name="args">命令行参数</param>
    public static void Main(string[] args)
    {
        // 使用 BenchmarkSwitcher 允许用户选择运行特定的基准测试类
        var switcher = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly);
        switcher.Run(args);
    }
}


