/*
  SimpleDatabaseBackup KeePass plugin
  
  Copyright (C) 2016 Joern Koerner <joern.koerner@gmail.com>

  This program is free software; you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation; either version 2 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

namespace SimpleDatabaseBackup
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Cache;
    using System.Windows.Forms;

    using KeePass.Plugins;
    using KeePass.Forms;

    public sealed class SimpleDatabaseBackupExt : Plugin
    {
        private int NumberOfBackups = 5;    // This is the maximum number of backups to create for each opened database.
        private IPluginHost m_host = null;

        public override string UpdateUrl
        {
            get { return "https://github.com/jnko/SimpleDatabaseBackup/SimpleDatabaseBackupVERSION.txt"; }
        }

        public override bool Initialize(IPluginHost host)
        {
            m_host = host;

            // We want a notification when the user tried to save the
            // current database
           m_host.MainWindow.FileSaved += this.OnFileSaved;

            return true;
        }

        public override void Terminate()
        {
            // Important! Remove event handlers!
            this.m_host.MainWindow.FileSaved -= this.OnFileSaved;
        }


        public static string GetSourceFileName(FileSavedEventArgs e)
        {
            string SourceFileName = string.Empty;
            if (e.Database.IOConnectionInfo.IsLocalFile())
            {
                // local file
                var f = new FileInfo(e.Database.IOConnectionInfo.Path);
                SourceFileName = f.Name;

                // remove file extension
                if (!string.IsNullOrEmpty(f.Extension))
                {
                    SourceFileName = SourceFileName.Substring(0, SourceFileName.Length - f.Extension.Length);
                }

                f = null;
            }
            else
            {
                // remote file
                SourceFileName = e.Database.IOConnectionInfo.Path;

                int lastPosBack = SourceFileName.LastIndexOf("/");
                if (lastPosBack > 0 && lastPosBack < SourceFileName.Length)
                {
                    SourceFileName = SourceFileName.Substring(lastPosBack + 1);
                }

                int lastPosSlash = SourceFileName.LastIndexOf(@"\");
                if (lastPosSlash > 0 && lastPosSlash < SourceFileName.Length)
                {
                    SourceFileName = SourceFileName.Substring(lastPosSlash + 1);
                }

                // remove file extension
                int lastPoitDot = SourceFileName.LastIndexOf(".");
                int lenghtWithoutDot = SourceFileName.Length - lastPoitDot;
                if (lenghtWithoutDot == 3 || lenghtWithoutDot == 4 || lenghtWithoutDot == 5)
                {
                    SourceFileName = SourceFileName.Substring(0, lastPoitDot);
                }
            }

            return SourceFileName;
        }

        public static string GetLogFileName(FileSavedEventArgs e)
        {
            string SourceFileName = string.Empty;
            if (e.Database.IOConnectionInfo.IsLocalFile())
            {
                // local file
                var f = new FileInfo(e.Database.IOConnectionInfo.Path);
                SourceFileName = f.Name;

                f = null;
            }
            else
            {
                // remote file
                SourceFileName = e.Database.IOConnectionInfo.Path;

                int lastPosBack = SourceFileName.LastIndexOf("/");
                if (lastPosBack > 0 && lastPosBack < SourceFileName.Length)
                {
                    SourceFileName = SourceFileName.Substring(lastPosBack + 1);
                }

                int lastPosSlash = SourceFileName.LastIndexOf(@"\");
                if (lastPosSlash > 0 && lastPosSlash < SourceFileName.Length)
                {
                    SourceFileName = SourceFileName.Substring(lastPosSlash + 1);
                }
            }

            return SourceFileName + "_log";
        }

        private void OnFileSaved(object sender, FileSavedEventArgs e)
        {
            //Thats the way we do it!
            if (e.Database.IsOpen)
            {
                if (e.Database.Modified) { 
                    string SourceFile = string.Empty;
                    string SourceFileName = string.Empty;
                    string BackupFile = string.Empty;
                    SourceFileName = GetSourceFileName(e);
                    if (e.Database.IOConnectionInfo.IsLocalFile())
                    {
                        SourceFile = e.Database.IOConnectionInfo.Path;
                    }
                    else
                    {
                        // remote file
                        SourceFile = Path.GetTempFileName();

                        var wc = new WebClient();

                        wc.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

                        if ((e.Database.IOConnectionInfo.UserName.Length > 0) || (e.Database.IOConnectionInfo.Password.Length > 0))
                        {
                            wc.Credentials = new NetworkCredential(e.Database.IOConnectionInfo.UserName, e.Database.IOConnectionInfo.Password);
                        }

                        wc.DownloadFile(e.Database.IOConnectionInfo.Path, SourceFile);
                        wc.Dispose();
                        wc = null;
                    }

                    // read log file
                    string destPath = Path.GetDirectoryName(e.Database.IOConnectionInfo.GetDisplayName()); // Destination backup folder is KeePass.exe directory
                    string BackupLogFile = destPath + "/" + GetLogFileName(e);
                    string[] LogFile = null;
                    if (File.Exists(BackupLogFile))
                    {
                        LogFile = File.ReadAllLines(BackupLogFile);
                    }

                    if (Directory.Exists(destPath))
                    {
                        // create file
                        BackupFile = destPath + "/" + SourceFileName + "_" + DateTime.Now.ToString("dd-MMMM-yyyy-hh-mm-ss") + ".kdbx";
                        File.Copy(SourceFile, BackupFile, true);

                        // delete extra file
                        if (LogFile != null)
                        {
                            if (LogFile.Length + 1 > NumberOfBackups)
                            {
                                for (int LoopDelete = NumberOfBackups - 1; LoopDelete < LogFile.Length; LoopDelete++)
                                {
                                    if (File.Exists(LogFile[LoopDelete]))
                                    {
                                        File.Delete(LogFile[LoopDelete]);
                                    }
                                }
                            }
                        }

                        // write log file
                        TextWriter fLog = new StreamWriter(BackupLogFile, false);
                        fLog.WriteLine(BackupFile);
                        if (LogFile != null)
                        {
                            var LoopMax = (uint)LogFile.Length;
                            if (LoopMax > NumberOfBackups)
                            {
                                LoopMax = (uint)NumberOfBackups;
                            }

                            for (uint i = 0; i < LoopMax; i++)
                            {
                                fLog.WriteLine(LogFile[i]);
                            }
                        }

                        fLog.Close();
                        fLog.Dispose();
                        fLog = null;
                    }
                    else
                    {
                        MessageBox.Show("Backupfolder not found  : " + destPath);
                    }

                    // delete temp remote file
                    if (!e.Database.IOConnectionInfo.IsLocalFile())
                    {
                        File.Delete(SourceFile);
                    }
                }
            }
            else
            {
                MessageBox.Show("Database is not open.");
            }

        }
    }
}
