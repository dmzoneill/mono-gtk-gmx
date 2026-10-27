# GmX

<p align="center">
  <img src=".github/logo.png" alt="mono-gtk-gmx logo" width="128" />
</p>

A Mono GTK# media player for MP3 files.

## What it does

- Plays MP3 files through `mpg123` (and a `libwmp3` wrapper), with seeking,
  volume control and an equalizer.
- Reads ID3 tags and keeps an audio database and playlists.
- Includes a volume slider popup and a playlist viewer.

`Gnome Media Player` is an earlier project in the same solution.

## Tech

- C# with Mono and GTK#
- mpg123

## Building

Open the `.sln` file in MonoDevelop, or build it with Mono and run the resulting
`.exe` with `mono`. Requires GTK# 2.

## License

Licensed under the Apache License 2.0. See [LICENSE](LICENSE).
