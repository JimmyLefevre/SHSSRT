# SHSSRT
## aka. the Super Hyper Secret Santa Rescue Team

What happens when somebody doesn't get their Secret Santa on a composition server? People make music! And games, apparently!

This is a small, but complete game, made in two weeks with FNA. It only supports keyboard input, and does not support resizing the window, but otherwise, it is a full game.

There are some stupid limitations with the original XNA API that really don't help when making a music-based game:
- MediaPlayer only supports playing one Song at a time, so we can't have nice crossfades.
- MediaPlayer has a gap when changing songs, and also has a gap when repeating a song, which is particularly bad for music that was made to loop cleanly.
- SoundEffect supports clean looping, and you can have as many of them as you want, but it only supports uncompressed, or not-very-compressed, formats like WAV. So, we use WAVs for everything, and we try to reduce our footprint by using ADPCM when we don't hear any objectionable artifacts. The quality of ADPCM-encoded WAVs varies a lot from file to file, so checking them is an entirely manual process.

Beyond that, the game was made in a short time frame. Not game jam-short, mind you; we still had time to implement niceties like interactive volume configuration, intro and completion "cutscenes", etc. But the content is, hopefully, more interesting than it is plentiful, and some features we implemented are under-explored.

Overall, though, most of the code we wrote ended up being useful in some way, and the whole thing tries to be very direct in what it does.

