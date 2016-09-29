# SimpleDatabaseBackup
SimpleDatabseBackup is a KeePass plugin which was created for simpleness. There is no setup, no GUI and no configuration. Just copy over the plugin into KeePess plugin folder and you are done.

# What is this plugin for?
It will create a backup for every opened KeePass database (.kdbx) every time a database with changes is saved.
The backups will be placed in the databases source folder, with a maximum of 5 backups. The backups will be rotated. A logfile keeps track of the backups.
SimpleDatabaseBackup is fully cross-platform compatible.

# Can I change the number of backups ?
At the moment it it hard-coded in the sources, but you may follow the excellent documentation on the KeePass website and build your own customized plugin. On the other hand you can create an issue here and I probably add this freature request.

# Further information
This plugin is heavily based on the DatabaseBackup plugin from Francis Noël which you can find here: http://keepass.info/plugins.html#databasebackup
There is no license or copyright information left in the sources so I hope Franics allows me to re-use his code. If not, please leave me a note.

I created this small plugin because I use KeePass daily with one personal and one shared business database. 
Even not enough on I need to do this on cross-platform operating systems every day. People tend to make mistakes from time to time. That is the right moment for backups.
At the time I only used Windows with Francis DatabaseBackup plugin everything was fine, but since I need to switch between Win, macOS and Linux I realized that Francis plugin doesn't play nicely in this scenario. It even has some serious bugs when using multiple databases.
