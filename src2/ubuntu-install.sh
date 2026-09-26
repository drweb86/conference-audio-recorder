#!/bin/bash

set -e

LATEST_SOURCES=false
for arg in "$@"; do
    if [[ "$arg" == "--latest" ]]; then
        echo "Using latest sources."
        LATEST_SOURCES=true
    fi
done

sourceCodeInstallationDirectory=/usr/local/src/conference-audio-recorder
binariesInstallationDirectory=/usr/local/conference-audio-recorder

if [ "$EUID" -eq 0 ]; then
  echo "Please do not run this script with sudo or as root."
  exit 1
fi

echo
echo Install .NET 10
echo
wget https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh
chmod +x /tmp/dotnet-install.sh
/tmp/dotnet-install.sh --channel 10.0
sudo apt install -y libasound2 || sudo apt install -y libasound2t64

echo
echo Cleaning installation directories
echo
sudo rm -rf ${sourceCodeInstallationDirectory}
sudo rm -rf ${binariesInstallationDirectory}

echo
echo Get source code
echo
version=$(wget -qO- https://api.github.com/repos/drweb86/conference-audio-recorder/releases/latest | grep '"tag_name"' | cut -d'"' -f4)
echo "Latest release: $version"

if [ "$LATEST_SOURCES" = true ]; then
    wget -O /tmp/conference-audio-recorder-src.zip https://github.com/drweb86/conference-audio-recorder/archive/refs/heads/main.zip
else
    wget -O /tmp/conference-audio-recorder-src.zip https://github.com/drweb86/conference-audio-recorder/archive/refs/tags/${version}.zip
fi
sudo unzip -q /tmp/conference-audio-recorder-src.zip -d /tmp/conference-audio-recorder-src-extracted
sudo mv /tmp/conference-audio-recorder-src-extracted/conference-audio-recorder-* ${sourceCodeInstallationDirectory}
rm -f /tmp/conference-audio-recorder-src.zip
sudo rm -rf /tmp/conference-audio-recorder-src-extracted
cd ${sourceCodeInstallationDirectory}

echo
echo Building
echo
cd ./src2
sudo /root/.dotnet/dotnet publish ConferenceAudioRecorder/ConferenceAudioRecorder.csproj /p:Version=${version} /p:AssemblyVersion=${version} -c Release --property:PublishDir=${binariesInstallationDirectory} --use-current-runtime --self-contained

echo
echo Prepare shortcut
echo
sudo cp "${sourceCodeInstallationDirectory}/src2/packaging/conference-audio-recorder.png" "${binariesInstallationDirectory}/conference-audio-recorder.png"

temporaryShortcut=/tmp/conference-audio-recorder.desktop
sudo rm -f ${temporaryShortcut}
cat > ${temporaryShortcut} << EOL
[Desktop Entry]
Encoding=UTF-8
Version=${version}
Name=Conference Audio Recorder
GenericName=Conference audio recorder
Categories=AudioVideo;Audio;Recorder;
Comment=Record microphone and speaker audio from conference calls.
Type=Application
Terminal=false
Exec=${binariesInstallationDirectory}/ConferenceAudioRecorder
Icon=${binariesInstallationDirectory}/conference-audio-recorder.png
StartupWMClass=ConferenceAudioRecorder
EOL
chmod 775 ${temporaryShortcut}

desktopDir=$(xdg-user-dir DESKTOP)
for shortcutLocation in "/usr/share/applications" "${desktopDir}"; do
    shortcutFile="${shortcutLocation}/Conference Audio Recorder.desktop"
    echo "Create shortcut in ${shortcutFile}"
    sudo cp ${temporaryShortcut} "${shortcutFile}"
    sudo chmod 775 "${shortcutFile}"
    gio set "${shortcutFile}" metadata::trusted true || true
done

echo
echo Installed.
echo Binaries: ${binariesInstallationDirectory}
echo Sources: ${sourceCodeInstallationDirectory}
echo
sleep 2m
