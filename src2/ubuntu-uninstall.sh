#!/bin/bash

echo
echo Removing sources installation folder
echo
sudo rm -rf /usr/local/src/conference-audio-recorder

echo
echo Removing binaries installation folder
echo
sudo rm -rf /usr/local/conference-audio-recorder

desktopDir=$(xdg-user-dir DESKTOP)

echo
echo Removing shortcuts
echo
sudo rm -f "/usr/share/applications/Conference Audio Recorder.desktop"
rm -f "${desktopDir}/Conference Audio Recorder.desktop"

echo
echo Application was uninstalled
echo
echo Recordings and settings under ~/.local/share/ConferenceAudioRecorder are kept.
echo
sleep 2m
