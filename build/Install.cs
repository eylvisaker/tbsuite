using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Installation
{
	class Installer
	{
		public void Run()
		{
			Console.WriteLine("Tight Binding Suite Installer");
			Console.WriteLine();

			if (System.Environment.OSVersion.Platform == PlatformID.Unix)
			{
				DoUnixInstall();
			}
			else
			{
				Console.WriteLine("This script is only for installing on unix operating systems.");
				Console.WriteLine("Press any key to exit.");
				Console.ReadKey(true);
			}
		}

		private void DoUnixInstall()
		{
			string bindir = GetInstallDir();
			string assemblyDir = bindir + "/tbsuite";

			try
			{
				Console.WriteLine();
				if (Directory.Exists(assemblyDir))
				{
					Console.WriteLine("Removing old installation.");
					Directory.Delete(assemblyDir, true);
				}

				Console.WriteLine("Creating directory " + assemblyDir);
				Directory.CreateDirectory(assemblyDir);
				Console.WriteLine();

				Console.WriteLine("Installing scripts to {0}", bindir);
				List<string> scripts = new List<string>();

				List<string> filesToCopy = new List<string>();

				filesToCopy.AddRange(Directory.GetFiles("artifacts", "*.exe"));
				filesToCopy.AddRange(Directory.GetFiles("artifacts", "*.dll"));
				filesToCopy.AddRange(Directory.GetFiles("artifacts", "*.dll.config"));

				if (filesToCopy.Count == 0) 
					throw new InvalidOperationException("TBSuite has not been built. Please run make or build.sh to build the project.");

				foreach (string script in Directory.GetFiles("build/script"))
				{
					string filename = Path.GetFileName(script);
					string destfile = bindir + "/" + filename;

					string scriptContents = File.ReadAllText(script).Replace("\r\n", "\n");

					using (var w = new StreamWriter(destfile, false, Encoding.ASCII))
					{
						w.WriteLine("#!/bin/bash");
						w.WriteLine();
						w.WriteLine("INSTALL_DIR='" + assemblyDir + "'");
						w.WriteLine();

						w.WriteLine(scriptContents);
					}

					scripts.Add(filename);

					SetExecutePermission(destfile);
				}

				Console.WriteLine("Installing assemblies to {0}", assemblyDir);

				for (int i = 0; i < filesToCopy.Count; i++)
				{
					bool remove = false;

					if (filesToCopy[i].Contains("clapack.dll")) remove = true;
					else if (filesToCopy[i].Contains(".vshost.exe")) remove = true;
					else if (filesToCopy[i].Contains("Install.exe")) remove = true;

					if (remove)
					{
						filesToCopy.RemoveAt(i);
						i--;
					}
				}
				foreach (string file in filesToCopy)
				{
					string filename = Path.GetFileName(file);
					string destfile = assemblyDir + "/" + filename;

					File.Copy(file, destfile);

					//Console.WriteLine("Installing " + filename);
				}

				Console.WriteLine("Completed installing files.");
				Console.WriteLine();
				
				TestLapackPresence(assemblyDir);

				Console.WriteLine();
				Console.WriteLine("Installation has completed.");
				Console.WriteLine();
			}
			catch (UnauthorizedAccessException)
			{
				Console.WriteLine();
				Console.WriteLine("You do not have permissions to write to " + bindir);
				Console.WriteLine("To install in this directory, run install with elevated permissions, like:");
				Console.WriteLine("    sudo ./build.sh install");

				return;
			}
		}

		public void TestLapackPresence(string assemblyDir)
		{
			Assembly ass = Assembly.LoadFrom(assemblyDir + "/ErikMath.dll");
			if (ass == null)
			{
				Console.WriteLine("Could not find ErikMath.dll assembly.  Something is wrong!");
			}

			Type tp = ass.GetType("ERY.EMath.MatrixDiagonalizers.DiagonalizerFactory");
			if (tp == null)
			{
				Console.WriteLine("Could not load diagonalizer factory.  It seems ErikMath.dll is too new.");
			}

			PropertyInfo p = tp.GetProperty("PrimaryDiagonalizer",
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			if (p == null)
			{
				Console.WriteLine("The PrimaryDiagonalizer property could not be found.");
			}

			object value = p.GetValue(null, null);
			string diag = value.ToString();

			Console.WriteLine("Using the {0} matrix diagonalizer.", diag);

			if (diag.ToLowerInvariant() != "lapack")
			{
				Console.WriteLine();
				Console.WriteLine("Could not find LAPACK installation.");
				Console.WriteLine("If your installation of LAPACK is not linked as liblapack.so,");
				Console.WriteLine("either install LAPACK or add an absolute path to ErikMath.dll.config ");
				Console.WriteLine("file in the {0} directory.", assemblyDir);
			}

		}

		private void SetExecutePermission(string destfile)
		{
			// equivalent to chmod 755
			int permission = 7 << 6 | 5 << 3 | 5;

			chmod(destfile, permission);
		}

		[DllImport("libc")]
		extern static void chmod(string path, int value);

		private static string GetInstallDir()
		{
			const string defaultInstallDir = "/usr/local/bin";

			Console.Write($"Enter install directory ({defaultInstallDir}): ");
			string installdir = Console.ReadLine();

			if (string.IsNullOrEmpty(installdir))
				installdir = defaultInstallDir;

			while (installdir.EndsWith("/"))
				installdir = installdir.Substring(installdir.Length - 1);

			return installdir;
		}
	}
}
