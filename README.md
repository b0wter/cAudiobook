![logo](https://raw.githubusercontent.com/b0wter/cAudiobook/master/AudiobookPlayer/res/audiobook_small.png)cAudiobook
==========
Windows-based open source audiobook player for drm-free audiobooks. In its current status the app is useable but missing some features, see To-Do-list.
To use it place your audiobooks in a folder and set it in the applications configuration file. Every folder in the audiobook folder is treated as a seperate audiobook. Folders are scanned recursively so that an audiobook may consist of several sub-folders.

The program currently requires .Net framework version 4.5 and depends on two external assemblies:
* [NAudio](https://naudio.codeplex.com/) for playback
* [google-api-for-dotnet](https://code.google.com/p/google-api-for-dotnet/) to grab book covers from Google 

Working
-------
* Continiouos playback over multiple files.
* Reading audiobooks and automatically fetching cover art from the web.
* Mini player window
* Cover art selection dialog.
* Settings dialog. (one caveat: if invalid input is given the user can still click ok but no changes will be saved)

To Do
-----
* Smooth validation of settings dialog
* Sleep timer.
* A more accurate monitoring of the position in the current audio file.
* Read cover art from folder.
* If the audio path is given relative and then changed to another path the state files contain wrong paths -> use absolute pathes only as workaround
* Implemention of the mini player in a more WPF-like way
* Target a lower .Net version than 4.5

Changelog
---------
* Settings dialog is now useable
* Select covers for your audiobooks by right-clicking the audiobooks

* New application icon
* Mini player
* Better playback controls
