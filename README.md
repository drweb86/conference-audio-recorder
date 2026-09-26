# Conference Audio Recorder

> Records the microphone and the speakers during a conference call, including Viber, Skype, and Teams, and saves MP3 files.

> The mixed recording is saved together with the separate microphone and speaker files. Those file names end with ` - microphone` and ` - speaker`.

[![Image Gallery](./help/Assets/Image%20Gallery.gif)](./help/Screenshots.md)

[Screenshots](./help/Screenshots.md)

<details>
<summary>🌐 Supported languages (70+)</summary>

Afrikaans, Albanian, Arabic, Armenian, Asturian, Basque, Belarusian,
Bengali, Bosnian, Breton, Bulgarian, Catalan, Chinese, Croatian, Czech,
Danish, Dutch, Efik, Esperanto, Estonian, Farsi, Finnish, French,
Galician, Georgian, German, Greek, Hebrew, Hindi, Hungarian, Icelandic,
Indonesian, Irish, Italian, Japanese, Korean, Kurdish, Latvian,
Lithuanian, Luxembourgish, Macedonian, Malay, Marathi, Mongolian,
Nigerian Pidgin, Norwegian, Pashto, Polish, Portuguese, Romanian,
Russian, Scottish Gaelic, Serbian, Slovak, Slovenian, Spanish, Swedish,
Tamil, Tatar, Telugu, Thai, Traditional Chinese, Turkish, Ukrainian,
Urdu, Uzbek, Vietnamese, Welsh, Yue Chinese, English

</details>

## Features

- Records the microphone and the speakers at the same time
- Keeps each track, then writes the mixed MP3
- Dark theme by default, with light and system themes in Settings

## Requirements

**Windows 11 x64, ARM64** or **Ubuntu 24+**.

## 📦 Installation Options

<details>

<summary>📦 Installation for Windows</summary>

A. [Microsoft Store](https://apps.microsoft.com/detail/9p1gzl37n0mt)

Best option. Store will keep application up to date.

B. WinGet

```
winget install --id SiarheiKuchuk.ConferenceAudioRecorder
```

C. Setup [look for asset **windows_setup.exe**](https://github.com/drweb86/conference-audio-recorder/releases/latest)

The installer can install for the current user or for all users. You update the application yourself.

D. Binaries [look for windows_archive.7z](https://github.com/drweb86/conference-audio-recorder/releases/latest)

A self-contained build. Run `x64\ConferenceAudioRecorder.exe` or `arm64\ConferenceAudioRecorder.exe`. You update the application yourself.

</details>

<details>

<summary>📦 Installation for Linux</summary>

A. Installation via APT Repository

Best option. System will keep application updated.

One-Time Setup - add repository

```
curl -fsSL https://drweb86.github.io/conference-audio-recorder/gpg-key.pub | sudo gpg --dearmor -o /usr/share/keyrings/conference-audio-recorder.gpg
echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/conference-audio-recorder.gpg] https://drweb86.github.io/conference-audio-recorder stable main" | sudo tee /etc/apt/sources.list.d/conference-audio-recorder.list > /dev/null
```

Install

```
sudo apt update && sudo apt install conference-audio-recorder
```

Update

```
sudo apt update && sudo apt upgrade conference-audio-recorder
```

Uninstall

```
sudo apt remove conference-audio-recorder
sudo rm /etc/apt/sources.list.d/conference-audio-recorder.list /usr/share/keyrings/conference-audio-recorder.gpg
```

Recordings and settings in `~/.local/share/ConferenceAudioRecorder` are kept.

B. DEB [look for asset linux_arm64.deb and linux_amd64.deb](https://github.com/drweb86/conference-audio-recorder/releases/latest)

For amd64:

```
sudo dpkg -i conference-audio-recorder_*_linux_amd64.deb
sudo apt-get install -f
```

For ARM64:

```
sudo dpkg -i conference-audio-recorder_*_linux_arm64.deb
sudo apt-get install -f
```

Uninstall

```
sudo apt remove conference-audio-recorder
```

C. Bash script

Installation:

`wget -O - https://raw.githubusercontent.com/drweb86/conference-audio-recorder/main/src2/ubuntu-install.sh | bash`

Installation of preview:

`wget -O - https://raw.githubusercontent.com/drweb86/conference-audio-recorder/main/src2/ubuntu-install.sh | bash -s -- --latest`

Uninstallation (source install only):

`wget -O - https://raw.githubusercontent.com/drweb86/conference-audio-recorder/main/src2/ubuntu-uninstall.sh | bash`

After installation (APT or .deb), the command is `conference-audio-recorder`.

</details>

## Documents

- [License](licenses/README.md) (CC0 1.0 Universal)
- [Privacy policy](privacy/desktop/README.md)
- [Third-party notices](notices/README.md)
- [Changelog](CHANGELOG.md)

<sub>This software is crafted with the help of Command Code — an AI pair programmer.</sub>
