# MusicVideoPlayer
An IPA plugin for playing videos inside BeatSaber

# Usage

Select a song and difficulty and a Play icon will appear in the top right of the main screen. This takes you to the video detail view for the currently selected song. 

To begin adding a video, click search. A list of videos will appear, based on the chosen song's title and author. If these results aren't satisfactory use the refine button to type your own search terms. Select a video and press download. The video will be added to a queue and you can see the download progress in both the detail view and song list.

When the video has downloaded, the preview and offset buttons will allow you to fine tune the video offset to synchronise the beatmap audio to the downloaded video.

[Demo: Downloading a video](https://streamable.com/ayzjn)

In the settings menu you can enable automatic downloads, which will automatically download videos for new songs if the mapper specified a video. Download quality can be adjusted, but it is not recommended to go above Medium. You can also select a screen position from a set of presets.


**Install Instructions:**
Extract the contents of the release zip to your Beat Saber Directory, merge folders as necessary.
Requires BSUtils & BSML.

# Features:
**Play Videos in Beat Saber**
* A custom json file `video.json` can be used in a song directory to detail a video (details below)
* These are automatically created for downloaded files
* Compatible with a wide range of video formats (check VideoPlayer Unity docs)
* Supports manual sync, looping, thumbnails and metadata

**Download Videos in game**
* Use the provided keyboard or the shortcut buttons to fill in your search query
* Adjustable video quality settings
* `video.json` can be shared and used to download the same video as another user (mappers can share `video.json` and players can automatically download the correct video and information)
* This plugin uses the application youtube-dl to do the heavy lifting, and will keep it up to date every launch

**Sample video.json**
```json
{
	"title":"$100 Bills BEAT SABER Playthrough!",
	"author":"Ruirize",
	"description":"Come join me on https://twitch.tv/ruirize ! Mixed Reality filmed using LIV: https://liv.tv Played on the HTC Vive in Beat Saber. Twitch: ...",
	"duration":"2:36",
	"URL":"/watch?v=NHD1utOvak8",
	"thumbnailURL":"https://i.ytimg.com/vi/NHD1utOvak8/hqdefault.jpg?sqp=-oaymwEjCPYBEIoBSFryq4qpAxUIARUAAAAAGAElAADIQj0AgKJDeAE=&amp;rs=AOn4CLD48jnea5icsiDQiE0QL4tF8j2t7w",
	"loop":false,
	"offset":0,
	"videoPath":"$100 Bills BEAT SABER Playthrough!.mp4"
}
```
