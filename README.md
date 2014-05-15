cAudiobook
==========
Windows-based open source audiobook player for drm-free audiobooks. In its current status the app is useable but missing some features, see To-Do-list.
To use it place your audiobooks in a folder and set it in the applications configuration file. Every folder in the audiobook folder is treated as a seperate audiobook. Folders are scanned recursively so that an audiobook may consist of several sub-folders.

The program depends on two external assemblies:
* ![NAudio](NAudio "https://naudio.codeplex.com/")
* ![google-api-for-dotnet](google-api-for-dotnet "https://code.google.com/p/google-api-for-dotnet/")

Working
-------
* Continiouos playback over multiple files.
* Reading audiobooks and automatically fetching cover art from the web.

To Do
-----
* Control to pause playback.
* Implement settings dialog (settings can currently be modified by editing a settings file).
* Sleep timer.
* A more accurate monitoring of the position in the current audio file.
* Cover art selection dialog.
* Read cover art from folder.
