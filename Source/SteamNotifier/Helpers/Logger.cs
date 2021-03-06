﻿using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text;

namespace SteamNotifier.Helpers
{
    /// <summary>
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// </summary>
        Invalid = 0,

        /// <summary>
        ///     Verbose messages (enter/exit subroutine, buffer contents, etc.)
        /// </summary>
        Verbose = 1,

        /// <summary>
        ///     Debug messages, to help in diagnosing a problem
        /// </summary>
        Debug = 2,

        /// <summary>
        ///     Informational messages, showing completion, progress, etc.
        /// </summary>
        Info = 3,

        /// <summary>
        ///     Warning error messages which do not cause a functional failure
        /// </summary>
        Warn = 4,

        /// <summary>
        ///     Major error messages, some lost functionality
        /// </summary>
        Error = 5,

        /// <summary>
        ///     Critical error messages, aborts the subsystem
        /// </summary>
        Fatal = 6
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    internal sealed class Logger : IDisposable
    {
        /// <summary>
        /// </summary>
        private static readonly ConcurrentQueue<string> LogQueue = new ConcurrentQueue<string>();

        /// <summary>
        /// </summary>
        private static volatile Logger _instance;

        /// <summary>
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// </summary>
        private static DateTime _lastFlushed = DateTime.Now;

        /// <summary>
        /// </summary>
        private FileStream _outputStream;

        /// <summary>
        /// </summary>
        private Logger()
        {
            Info("Logger Instance Initialized");
            _outputStream = new FileStream(ActiveLogFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        }

        /// <summary>
        /// </summary>
        public static string ActiveLogFile => Path.Combine(Location.LogPath, LogFile);

        /// <summary>
        /// </summary>
        public static Logger Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                lock (SyncRoot)
                {
                    if (_instance == null)
                    {
                        _instance = new Logger();
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// </summary>
        public static string LogFile
        {
            get
            {
                string date = DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                return string.Format(CultureInfo.InvariantCulture, "SteamNotifier-({0}).log", date);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            lock (SyncRoot)
            {
                FlushLogger();

                byte[] output = new UTF8Encoding().GetBytes(Environment.NewLine);
                _outputStream.Write(output, 0, output.Length);

                _outputStream.Flush();
                _outputStream.Flush(true);
                _outputStream.Dispose();
                _outputStream.Close();
                _outputStream = null;
                _instance = null;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="logMessage"></param>
        public void Debug(string logMessage)
        {
            Write(LogLevel.Debug, logMessage);
        }

        /// <summary>
        /// </summary>
        /// <param name="logMessage"></param>
        /// <param name="args"></param>
        public void Debug(string logMessage, params object[] args)
        {
            Write(LogLevel.Debug, logMessage, args);
        }

        /// <summary>
        /// </summary>
        /// <param name="logMessage"></param>
        public void Error(string logMessage)
        {
            Write(LogLevel.Error, logMessage);
        }

        /// <summary>
        /// </summary>
        /// <param name="logMessage"></param>
        /// <param name="args"></param>
        public void Error(string logMessage, params object[] args)
        {
            Write(LogLevel.Error, logMessage, args);
        }

        /// <summary>
        /// </summary>
        /// <param name="exception"></param>
        public void Exception(Exception exception)
        {
            Write(LogLevel.Fatal, exception.ToString());
        }

        /// <summary>
        /// </summary>
        /// TODO: Handle Exception
        public void FlushLogger()
        {
            lock (SyncRoot)
            {
                try
                {
                    while (LogQueue.Count > 0)
                    {
                        LogQueue.TryDequeue(out string logEntry);
                        byte[] output = new UTF8Encoding().GetBytes(logEntry + Environment.NewLine);
                        _outputStream.Write(output, 0, output.Length);
                    }
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine(exception);
                    throw;
                }

                _lastFlushed = DateTime.Now;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="logMessage"></param>
        public void Info(string logMessage)
        {
            Write(LogLevel.Info, logMessage);
        }

        /// <summary>
        /// </summary>
        /// <param name="logMessage"></param>
        /// <param name="args"></param>
        public void Info(string logMessage, params object[] args)
        {
            Write(LogLevel.Info, logMessage, args);
        }

        /// <summary>
        /// </summary>
        /// <param name="logMessage"></param>
        public void Verbose(string logMessage)
        {
            Write(LogLevel.Verbose, logMessage);
        }

        /// <summary>
        /// </summary>
        /// <param name="logMessage"></param>
        /// <param name="args"></param>
        public void Verbose(string logMessage, params object[] args)
        {
            Write(LogLevel.Verbose, logMessage, args);
        }

        /// <summary>
        /// </summary>
        /// <param name="logMessage"></param>
        public void Warn(string logMessage)
        {
            Write(LogLevel.Warn, logMessage);
        }

        /// <summary>
        /// </summary>
        /// <param name="logMessage"></param>
        /// <param name="args"></param>
        public void Warn(string logMessage, params object[] args)
        {
            Write(LogLevel.Warn, logMessage, args);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static bool DoPeriodicFlush()
        {
            TimeSpan logAge = DateTime.Now - _lastFlushed;
            return logAge.TotalSeconds >= 60;
        }

        /// <summary>
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="logMessage"></param>
        /// <param name="args"></param>
        private void Write(LogLevel logLevel, string logMessage, params object[] args)
        {
            Write(logLevel, string.Format(CultureInfo.InvariantCulture, logMessage, args));
        }

        /// <summary>
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="logMessage"></param>
        private void Write(LogLevel logLevel, string logMessage)
        {
            lock (SyncRoot)
            {
                string logEntry = string.Format(CultureInfo.InvariantCulture, "{0} {1,-7} | {2}", DateTime.Now, logLevel, logMessage);
                System.Diagnostics.Debug.WriteLine(logEntry);
                LogQueue.Enqueue(logEntry);

                if (LogQueue.Count >= 100 || DoPeriodicFlush())
                {
                    FlushLogger();
                }
            }
        }
    }
}