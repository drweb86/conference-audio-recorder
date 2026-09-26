#!/bin/bash

# Builds Conference Audio Recorder .deb packages for amd64 and arm64.
# Run from the src2/ directory on Ubuntu with .NET 10 SDK installed.

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
OUTPUT_DIR="$REPO_ROOT/Output"

cd "$SCRIPT_DIR"

version=$(head -1 "$REPO_ROOT/CHANGELOG.md" | sed 's/^\xEF\xBB\xBF//' | sed 's/^# //')
echo "Building Conference Audio Recorder v$version deb packages"
echo ""

rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

declare -A ARCH_MAP
ARCH_MAP["linux-x64"]="amd64"
ARCH_MAP["linux-arm64"]="arm64"

for rid in "${!ARCH_MAP[@]}"; do
    deb_arch="${ARCH_MAP[$rid]}"
    publish_dir="$OUTPUT_DIR/staging/$deb_arch/publish"
    pkg_root="$OUTPUT_DIR/staging/$deb_arch/pkg"
    deb_file="$OUTPUT_DIR/conference-audio-recorder_${version}_linux_${deb_arch}.deb"

    echo "========================================="
    echo "  Building $rid ($deb_arch)"
    echo "========================================="
    echo ""

    rm -rf "$OUTPUT_DIR/staging/$deb_arch"

    echo "Publishing..."
    dotnet publish ConferenceAudioRecorder/ConferenceAudioRecorder.csproj \
        "/p:InformationalVersion=$version" \
        "/p:VersionPrefix=$version" \
        "/p:Version=$version" \
        "/p:AssemblyVersion=$version" \
        "--runtime=$rid" \
        -c Release \
        "/p:PublishDir=$publish_dir" \
        /p:PublishReadyToRun=false \
        /p:RunAnalyzersDuringBuild=False \
        --self-contained true \
        --property WarningLevel=0

    echo "Creating package structure..."
    mkdir -p "$pkg_root/DEBIAN"
    mkdir -p "$pkg_root/usr/lib/conference-audio-recorder"
    mkdir -p "$pkg_root/usr/bin"
    mkdir -p "$pkg_root/usr/share/applications"
    mkdir -p "$pkg_root/usr/share/pixmaps"
    mkdir -p "$pkg_root/usr/share/doc/conference-audio-recorder"

    cp -a "$publish_dir/"* "$pkg_root/usr/lib/conference-audio-recorder/"
    cp "$REPO_ROOT/LICENSE" "$pkg_root/usr/share/doc/conference-audio-recorder/copyright"
    cp "$REPO_ROOT/THIRD-PARTY-NOTICES.md" "$pkg_root/usr/lib/conference-audio-recorder/THIRD-PARTY-NOTICES.md"
    cp "$REPO_ROOT/THIRD-PARTY-NOTICES.md" "$pkg_root/usr/share/doc/conference-audio-recorder/THIRD-PARTY-NOTICES.md"
    cp "$SCRIPT_DIR/packaging/conference-audio-recorder.png" "$pkg_root/usr/share/pixmaps/conference-audio-recorder.png"

    ln -sf ../lib/conference-audio-recorder/ConferenceAudioRecorder "$pkg_root/usr/bin/conference-audio-recorder"

    cat > "$pkg_root/usr/share/applications/conference-audio-recorder.desktop" << 'DESKTOP'
[Desktop Entry]
Version=1.0
Name=Conference Audio Recorder
GenericName=Conference audio recorder
Comment=Record microphone and speaker audio from conference calls
Categories=AudioVideo;Audio;Recorder;
Type=Application
Terminal=false
Exec=conference-audio-recorder
Icon=conference-audio-recorder
StartupWMClass=ConferenceAudioRecorder
DESKTOP

    installed_size=$(du -sk "$pkg_root" | cut -f1)

    cat > "$pkg_root/DEBIAN/control" << CONTROL
Package: conference-audio-recorder
Version: $version
Section: sound
Priority: optional
Architecture: $deb_arch
Installed-Size: $installed_size
Depends: libc6, libgcc-s1, libstdc++6, libx11-6, libfontconfig1, libasound2 | libasound2t64
Maintainer: Siarhei Kuchuk <https://github.com/drweb86>
Homepage: https://github.com/drweb86/conference-audio-recorder
Description: Record conference calls from the microphone and speakers
 Conference Audio Recorder saves microphone and speaker audio as MP3 files.
 Separate tracks are kept beside the mixed recording.
 .
 License: CC0 1.0 Universal.
CONTROL

    cat > "$pkg_root/DEBIAN/postinst" << 'POSTINST'
#!/bin/bash
set -e
chmod +x /usr/lib/conference-audio-recorder/ConferenceAudioRecorder
if command -v update-desktop-database > /dev/null 2>&1; then
    update-desktop-database -q /usr/share/applications || true
fi
if [ -n "$SUDO_USER" ]; then
    DESKTOP_DIR=$(su - "$SUDO_USER" -c 'xdg-user-dir DESKTOP' 2>/dev/null) || true
    if [ -n "$DESKTOP_DIR" ] && [ -d "$DESKTOP_DIR" ]; then
        cp /usr/share/applications/conference-audio-recorder.desktop "$DESKTOP_DIR/Conference Audio Recorder.desktop"
        chown "$SUDO_USER":"$SUDO_USER" "$DESKTOP_DIR/Conference Audio Recorder.desktop"
        chmod 755 "$DESKTOP_DIR/Conference Audio Recorder.desktop"
        su - "$SUDO_USER" -c "gio set '$DESKTOP_DIR/Conference Audio Recorder.desktop' metadata::trusted true" 2>/dev/null || true
    fi
fi
POSTINST
    chmod 755 "$pkg_root/DEBIAN/postinst"

    cat > "$pkg_root/DEBIAN/postrm" << 'POSTRM'
#!/bin/bash
set -e
if command -v update-desktop-database > /dev/null 2>&1; then
    update-desktop-database -q /usr/share/applications || true
fi
if [ -n "$SUDO_USER" ]; then
    DESKTOP_DIR=$(su - "$SUDO_USER" -c 'xdg-user-dir DESKTOP' 2>/dev/null) || true
    if [ -n "$DESKTOP_DIR" ]; then
        rm -f "$DESKTOP_DIR/Conference Audio Recorder.desktop"
    fi
fi
POSTRM
    chmod 755 "$pkg_root/DEBIAN/postrm"

    find "$pkg_root/usr" -type d -exec chmod 755 {} \;
    find "$pkg_root/usr/lib/conference-audio-recorder" -type f -exec chmod 644 {} \;
    chmod 755 "$pkg_root/usr/lib/conference-audio-recorder/ConferenceAudioRecorder"
    find "$pkg_root/usr/lib/conference-audio-recorder" \( -name "*.so" -o -name "*.so.*" \) -exec chmod 755 {} \;
    chmod 644 "$pkg_root/usr/share/applications/conference-audio-recorder.desktop"
    chmod 644 "$pkg_root/usr/share/pixmaps/conference-audio-recorder.png"
    chmod 644 "$pkg_root/usr/share/doc/conference-audio-recorder/copyright"

    echo "Building .deb..."
    dpkg-deb --build --root-owner-group "$pkg_root" "$deb_file"
    echo "Created: $deb_file"
    echo ""
done

rm -rf "$OUTPUT_DIR/staging"
ls -lh "$OUTPUT_DIR"/*.deb
