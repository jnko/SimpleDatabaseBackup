cd c:\SimpleDatabaseBackup\SimpleDatabaseBackup\SimpleDatabaseBackup

del bin\Release\KeePass.exe
del bin\Release\KeePass.XmlSerializers.dll
del bin\Release\SimpleDatabaseBackup.pdb
del obj\*.* /Q /S

c:\Users\injk\ownCloud\KeePass\KeePass.exe --plgx-create "c:\SimpleDatabaseBackup\SimpleDatabaseBackup\SimpleDatabaseBackup"

