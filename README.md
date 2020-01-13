# MusicVideoPlayer
An IPA plugin for playing videos inside BeatSaber

# Usage
1) Download the video you wish to play with a particular song.
2) Add that video to the song folder.
3) Create the video.json using the sample provided below. 

Coming shortly will be the ability to download and search for videos. 

**Install Instructions:**
Extract the contents of the release zip to your Beat Saber Directory, merge folders as necessary.
Requires BSUtils & BSML.

# Features:
**Play Videos in Beat Saber**
* A custom json file `video.json` can be used in a song directory to detail a video (details below)
* These are automatically created for downloaded files
* Compatible with a wide range of video formats (check VideoPlayer Unity docs)
* Supports manual sync, looping, thumbnails and metadata

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
