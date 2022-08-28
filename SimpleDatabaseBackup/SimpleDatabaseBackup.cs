﻿/*
  SimpleDatabaseBackup KeePass plugin
  
  Copyright (C) 2016,2017 Joern Koerner <joern.koerner@gmail.com>

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
    using System.Collections.Generic;

    using KeePass.Plugins;
    using KeePass.Forms;
    
    public sealed class SimpleDatabaseBackupExt : Plugin
    {
        private int NumberOfBackups = 5;          // This is the maximum number of backups to create for each opened database.
        private string backupPostfix = "_sdbackup";  // Somethig that is being added behind the filename "somefile_sdbackup" but before the extension "somefile_sdbackup.kdbx"
        private string backupExt = ".kdbx";
        private IPluginHost m_host = null;

        public override string UpdateUrl
        {
            get { return @"https://raw.githubusercontent.com/jnko/SimpleDatabaseBackup/master/SimpleDatabaseBackupVERSION.txt"; }
        }

        public override bool Initialize(IPluginHost host)
        {
            m_host = host;

            //I want a notification when the user saved the current database
            //m_host.MainWindow.FileSaved += this.OnFileSaved;
            //up to 1.0.3: I made a logical mistake.
            //The FileSaved event event here notifes us when the file has been saved. So this makes absolutely no sense to test for Database.Modified inside because the file 
            //has been saved already and thus the Modified property must ALWAYS be false.

            //I want a notification before the file is being saved. This way the Database.Modified property is either true or false.
            //The Backup will be made before the databse is saved.
            m_host.MainWindow.FileSaving += this.OnFileSaving;


            return true;
        }

        public override void Terminate()
        {
            // Important! Remove event handlers!
            this.m_host.MainWindow.FileSaving -= this.OnFileSaving;
        }

        public static string GetSourceFileName(FileSavingEventArgs e)
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
                //MessageBox.Show(e.Database.IOConnectionInfo.Path);  //Complete URI
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
            //MessageBox.Show(SourceFileName);    //Filename w/o ext
            return SourceFileName;
        }

        private void OnFileSaving(object sender, FileSavingEventArgs e)
        {
            //Thats the way we do it!
            if (e.Database.IsOpen)
            {
                //MessageBox.Show("Database is open.");
                //MessageBox.Show("Database mod: " + e.Database.Modified);
                if (e.Database.Modified) {
                    //MessageBox.Show("Database is modified. " + e.Database.Modified);
                    string SourceFile = string.Empty;
                    string SourceFileName = string.Empty;
                    string DateAppend = "_"+DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                    string BackupFile = string.Empty;     //Just the backup file filename (without path)
                    string BackupFilePath = string.Empty; //The complete path and filename
                    SourceFileName = GetSourceFileName(e);
                  //MessageBox.Show("SFN: " + SourceFileName);  //Filename Only
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
                        //MessageBox.Show("Downloaded_from:"+ e.Database.IOConnectionInfo.Path+ " To:"+ SourceFile+ " UN:PW"+ e.Database.IOConnectionInfo.UserName+ e.Database.IOConnectionInfo.Password);
                        wc.Dispose();
                        wc = null;
                    }

                    
                    string destPath = Path.GetDirectoryName(e.Database.IOConnectionInfo.GetDisplayName()); // Destination backup folder is KeePass.exe directory

                    if (Directory.Exists(destPath))
                    {
                        if (File.Exists(SourceFile))
                        {
                            // create file
                            BackupFile = SourceFileName + backupPostfix + DateAppend + backupExt;
                            BackupFilePath = destPath + "/" + BackupFile;
                            // MessageBox.Show("Creating backup " + BackupFilePath);
                            File.Copy(SourceFile, BackupFilePath, true);

                            //Get all backup databases in path
                            List<string> bakFiles = new List<string>();
                            foreach (string file in Directory.GetFiles(destPath, SourceFileName + backupPostfix + "*" + backupExt))
                            {
                                bakFiles.Add(file);
                            }

                            //Clean up (delete) all old database backups
                            bakFiles.Reverse();             //descending sort - newest on top

                            /* Debug only
                            string bf = "bakFiles: ";
                            foreach (string bakFile in bakFiles)
                            {
                                bf = bf + " " + bakFile;
                            }
                            MessageBox.Show(bf);
                            */

                            int cnt = 0;
                            foreach (string bakFile in bakFiles)
                            {
                                cnt++;
                                if (cnt > NumberOfBackups)
                                {
                                    if (File.Exists(bakFile))
                                    {
                                        File.Delete(bakFile);
                                    }
                                }
                            }

                        }
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
