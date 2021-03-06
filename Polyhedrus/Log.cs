﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;

namespace Polyhedrus
{
	public class Log
	{
		private enum LogType
		{
			Info,
			Warning,
			Error,
			Exception
		}

		[DllImport("kernel32")]
		private static extern bool AllocConsole();

		public static bool IsInitialized { get; private set; }
		public static bool EnableConsoleLog { get; private set; }
		public static bool EnableFileLog { get; private set; }

		private static object lockObject = new object();
		private static Timer timer;
		private static FileStream fileStream;
		private static StreamWriter streamWriter;
		private static DateTime lastFlushTime;
		private static int flushIntervalMillis;

		public static void InitLogging(bool enableConsoleLog, bool enableFileLog)
		{
			EnableConsoleLog = enableConsoleLog;
			EnableFileLog = enableFileLog;

			if (IsInitialized)
				throw new Exception("Logging already initialized");

			if (Application.Current != null)
				Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

			if (EnableFileLog)
			{
				var filename = string.Format("Polyhedrus-{0:yyyy-MM-dd-HHmmss}.log", DateTime.Now);
				var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				var filepath = Path.Combine(dir, "Logs", filename);
				Directory.CreateDirectory(Path.GetDirectoryName(filepath));
				fileStream = new FileStream(filepath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
				streamWriter = new StreamWriter(fileStream);
				flushIntervalMillis = 1000;
				timer = new Timer(e =>
				{
					lock (lockObject)
					{
						streamWriter.Flush();
						lastFlushTime = DateTime.Now;
					}

				}, null, flushIntervalMillis, flushIntervalMillis);
			}

			if (EnableConsoleLog)
			{
				AllocConsole();
			}
		}

		public static void InfoDialog(string message, string title = null)
		{
			Info(message + ((title != null) ? " - " + title : ""));
			title = title ?? "Information";
			MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
		}

		public static void WarnDialog(string message, string title = null)
		{
			Warn(message + ((title != null) ? " - " + title : ""));
			title = title ?? "Warning";
			MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
		}

		public static void ErrorDialog(string message, string title = null)
		{
			Error(message + ((title != null) ? " - " + title : ""));
			title = title ?? "Error";
			MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
		}

		public static void Info(string message)
		{
			LogLine(LogType.Info, message);
		}

		public static void InfoFormat(string message, params object[] parameters)
		{
			LogLineFormat(LogType.Info, message, parameters);
		}

		public static void Warn(string message)
		{
			LogLine(LogType.Warning, message);
		}

		public static void WarnFormat(string message, params object[] parameters)
		{
			LogLineFormat(LogType.Warning, message, parameters);
		}

		public static void Error(string message)
		{
			LogLine(LogType.Error, message);
		}

		public static void ErrorFormat(string message, params object[] parameters)
		{
			LogLineFormat(LogType.Error, message, parameters);
		}

		public static void Exception(Exception e)
		{
			lock (lockObject)
			{
				LogLine(LogType.Exception, e.Message);
				var ex = e;
				while (ex != null)
				{
					if (ex != e)
						WriteRaw("==== " + ex.Message + " ====");
					WriteRaw(ex.StackTrace);
					ex = e.InnerException;
				}
				WriteRaw("");
			}
		}

		private static void LogLineFormat(LogType logType, string message, object[] parameters)
		{
			string msg = string.Format(message, parameters);
			LogLine(logType, msg);
		}

		private static void LogLine(LogType logType, string message)
		{
			string data = string.Format("[Log.{0} at {1:HH:mm:ss.fff}] {2}", logType.ToString(), DateTime.Now, message);
			WriteRaw(data);
		}

		private static void WriteRaw(string message)
		{
			lock (lockObject)
			{
				if (EnableConsoleLog)
					Console.WriteLine(message);

				if (EnableFileLog)
				{
					streamWriter.WriteLine(message);
					if ((DateTime.Now - lastFlushTime).TotalMilliseconds > flushIntervalMillis)
					{
						lastFlushTime = DateTime.Now;
						streamWriter.Flush();
					}
				}
			}
		}

		static void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			Exception(e.Exception);
			e.Handled = true;
		}
	}
}
