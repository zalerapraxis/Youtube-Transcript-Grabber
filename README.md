# Youtube Transcript Grabber

It's pretty much a local clone of https://nobinge.watch/ I wrote super quickly with the help of ChatGPT. It takes a Youtube URL as input (not as a CLI argument but as an input to an interactive prompt), grabs the subtitles for the video, strings them together with a generic prompt, and inserts the whole thing into the clipboard.

Take the clipboard output and paste it into ChatGPT, and it'll give you a summary of the video. 

Uses:
- https://github.com/BobLd/youtube-transcript-api-sharp (for getting subtitles)
- https://github.com/zzzprojects/html-agility-pack (for getting video title)
