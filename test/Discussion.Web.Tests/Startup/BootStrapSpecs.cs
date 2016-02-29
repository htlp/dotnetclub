﻿using System.Net;
using System.Diagnostics;
using System.IO;
using Xunit;
using System;
using System.Linq;
using Microsoft.AspNet.Testing;

namespace Discussion.Web.Tests.Startup
{
    public class BootStrapSpecs
    {
        [Fact]
        public void should_bootstrap_success()
        {
            const int httpListenPort = 5000;
            var testCompleted = false;
            HttpWebResponse response = null;

            StartWebApp(httpListenPort, (dnxWebServer) =>
            {
                try
                {
                    var httpWebRequest = WebRequest.CreateHttp("http://localhost:" + httpListenPort.ToString());
                    response = httpWebRequest.GetResponse() as HttpWebResponse;
                }
                catch(WebException ex)
                {
                    response = ex.Response as HttpWebResponse;
                }
                finally
                {
                    testCompleted = true;
                    dnxWebServer.Kill();
                }
            }, () => testCompleted);

            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        private void StartWebApp(int port, Action<Process> onServerReady, Func<bool> testSuccessed)
        {
            var args = Environment.GetCommandLineArgs();

            var dnxPath = DnxPath();
            var appBaseIndex = Array.IndexOf(args, "--appbase");
            var testPath = appBaseIndex >= 0 ? args[appBaseIndex + 1] : Environment.CurrentDirectory;
            var webProject = Path.Combine(testPath, "../../src/Discussion.Web").NormalizeSeparatorChars();

            var dnxWeb = new ProcessStartInfo
            {
                FileName = dnxPath,
                Arguments = "Microsoft.AspNet.Server.Kestrel --server.urls http://localhost:" + port.ToString(),
                WorkingDirectory = webProject,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                LoadUserProfile = true,
                UseShellExecute = false
            };
            Console.WriteLine($"dnx command is: {dnxPath}{Environment.NewLine}Try to start web site from directory {webProject}");

            string outputData = string.Empty, errorOutput = string.Empty;
            var startedSuccessfully = false;
            var dnxWebServer = new Process { StartInfo = dnxWeb };


            dnxWebServer.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                if (startedSuccessfully)
                {
                    return;
                }

                outputData += e.Data;
                if (outputData.Contains("Now listening on") && outputData.Contains("Application started."))
                {
                    startedSuccessfully = true;
                    onServerReady.BeginInvoke(dnxWebServer, null, null);
                };
            };
            dnxWebServer.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                errorOutput += e.Data;
            };


            dnxWebServer.EnableRaisingEvents = true;
            dnxWebServer.Exited += (object sender, EventArgs e) =>
            {
                if (!testSuccessed())
                    throw new Exception("Server is down unexpectedly.");
            };

            dnxWebServer.Start();
            dnxWebServer.BeginErrorReadLine();
            dnxWebServer.BeginOutputReadLine();
            dnxWebServer.WaitForExit(20 * 1000);
        }

        private static string DnxPath()
        {
            var dnxPathFragment = string.Concat(".dnx", Path.DirectorySeparatorChar, "runtimes");
            var dnxCommand = TestPlatformHelper.IsWindows ? "dnx.exe" : "dnx";
            var envPathSeparator = TestPlatformHelper.IsWindows ? ';' : ':';

            var envPath = Environment.GetEnvironmentVariable("PATH");            
            var runtimeBin = envPath.Split(new char[] { envPathSeparator }, StringSplitOptions.RemoveEmptyEntries)
                .Where(c => c.Contains(dnxPathFragment)).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(runtimeBin))
            {
                throw new Exception("Runtime not detected on the machine.");
            }

            return Path.Combine(runtimeBin, dnxCommand);
        }
    }

    static class StringPathExtensions
    {
        public static string NormalizeSeparatorChars(this string path)
        {
            return path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        }
    }
}