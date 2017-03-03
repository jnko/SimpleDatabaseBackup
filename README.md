# SimpleDatabaseBackup
SimpleDatabseBackup is a KeePass plugin which was created for simpleness. There is no setup, no GUI and no configuration. Just install and go. That's all.

# What is this plugin for?
It will create a backup for every opened KeePass database (.kdbx) every time a database with changes is saved.
The backups will be placed in the databases source folder, with a maximum of 5 backups. The backups will be rotated. A logfile keeps track of the backups.
SimpleDatabaseBackup is fully cross-platform compatible.

# Download
The compiled plugin can be downloaded here: 

https://github.com/jnko/SimpleDatabaseBackup/raw/master/SimpleDatabaseBackup.plgx

# Installing SimpleDatabaseBackup plugin
Please follow the KeePass documentation:

http://keepass.info/help/v2/plugins.html

In short unzip into the KeePass.exe folder.

# Updating the SimpleDatabaseBackup plugin

When updating the plugin please always manually remove the backup databases

# Can I change the number of backups ?
At the time of writing this it is hard-coded in the sources, but you may follow the excellent documentation on the KeePass website and build your own customized plugin. On the other hand you can create an issue here and if there is some need from the users I will add this freature request.

# Further information
This plugin is heavily based on the DatabaseBackup plugin written by Francis Noël which you can find here: 

http://keepass.info/plugins.html#databasebackup

There is no license or copyright information left in the sources so I hope Franics allows me to re-use his code. If not, please leave me a note.

I created this small plugin because I use KeePass daily with one personal and one shared business database. 
Even not enough on I need to do this on cross-platform operating systems every day. People tend to make mistakes from time to time. That is the right moment for backups.
At the time I only used Windows with Francis DatabaseBackup plugin everything was fine, but since I need to switch between Windows, macOS and Linux I realized that Francis plugin doesn't play nicely. It has some serious bugs when using multiple databases and the path names were not cross-platform compatible. So I forked DatabaseBackup, removed the GUI, stripped out everything unneeded and modified it for my needs - cross-platform, simple, silent ==> SimpleDatabaseBackup

# History
* 1.0.7 Major bugfix. Rewritten the complete backup logic. Now SDB never would forget to delete old backups - even when stored on shared cloud folders.
* 1.0.6 Minor bugfix. Removed some unused references to increase compatibility (Yes, it's true - Someone is still using WindowsXP)
* 1.0.5 Fixed a bug which caused an unhandled exception when saving a new database.
* 1.0.4 Fixed a bug that prevents the entrie backup.
* 1.0.3 Added KeePass update notifications; A database will be backed-up only when modified.
* 1.0.1 First public release.
