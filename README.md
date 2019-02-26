# MusicVideoPlayer
An IPA plugin for playing videos inside BeatSaber

**Instructions:**
Extract the contents of the release zip to your Beat Saber Directory, merge folders as necessary.
Requires CustomUI, which can be obtained using the ModSaber Installer (https://github.com/lolPants/modsaber-installer/releases)

# Features:
**Play Videos in Beat Saber**
* A custom json file `video.json` can be used in a song directory to detail a video
* These are automatically created for downloaded files
* Compatible with a wide range of video formats (check VideoPlayer Unity docs)
* Supports manual sync, looping, thumbnails and metadata

**Download Videos in game**
* Use the provided keyboard or the shortcut buttons to fill in your search query
* Adjustable video quality settings
* `video.json` can be shared and used to download the same video as another user (mappers can share `video.json` and players can automatically download the correct video and information)
* This plugin uses the application youtube-dl to do the heavy lifting, and will keep it up to date every launch

**Re-positionable screen**
* Preset positions are available in the settings menu
* Use the laser and `grip` button to re-position and rotate the screen to a custom position
* Use `Up` and `Down` on the track-pad or joystick to move the screen towards or away from you
* Use `Left` and `Right` to grow or shrink the screen
* Screen glows and affects the background, just like the existing lights
