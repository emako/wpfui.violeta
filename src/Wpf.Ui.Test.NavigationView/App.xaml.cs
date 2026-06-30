using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Wpf.Ui.Test.NavigationView;

public partial class App : System.Windows.Application
{
	public App()
	{
		DispatcherUnhandledException += OnDispatcherUnhandledException;
		AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
		TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
	}

	private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
	{
		WriteExceptionLog("DispatcherUnhandledException", e.Exception);
	}

	private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		WriteExceptionLog("CurrentDomainUnhandledException", e.ExceptionObject as Exception);
	}

	private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
	{
		WriteExceptionLog("UnobservedTaskException", e.Exception);
	}

	private static void WriteExceptionLog(string source, Exception? exception)
	{
		try
		{
			string path = Path.Combine(Path.GetTempPath(), "wpfui-violeta-navigationview-crash.txt");
			var builder = new StringBuilder();
			builder.AppendLine($"Timestamp: {DateTime.Now:O}");
			builder.AppendLine($"Source: {source}");
			builder.AppendLine(exception?.ToString() ?? "<null>");
			File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
		}
		catch
		{
		}
	}
}