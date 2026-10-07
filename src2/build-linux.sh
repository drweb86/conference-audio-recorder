#!/bin/bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
OUTPUT_DIR="$REPO_ROOT/Output"

cd "$SCRIPT_DIR"

for tool in fpm rpmbuild bsdtar tar zstd; do
    if ! command -v "$tool" >/dev/null 2>&1; then
        echo "Missing required tool: $tool" >&2
        echo "Ubuntu: sudo apt-get install -y ruby ruby-dev build-essential rpm libarchive-tools zstd && sudo gem install --no-document fpm -v 1.18.0" >&2
        exit 1
    fi
done

version=$(head -1 "$REPO_ROOT/CHANGELOG.md" | sed 's/^\xEF\xBB\xBF//' | sed 's/^# //')
echo "Building Conference Audio Recorder v$version Linux packages"
echo ""

rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

script_dir="$OUTPUT_DIR/staging/scripts"
mkdir -p "$script_dir"

cat > "$script_dir/postinst" << 'POSTINST'
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

cat > "$script_dir/postrm" << 'POSTRM'
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
chmod 755 "$script_dir/postinst" "$script_dir/postrm"

description="Record conference calls from the microphone and speakers"
maintainer="Siarhei Kuchuk <https://github.com/drweb86>"
url="https://github.com/drweb86/conference-audio-recorder"

for rid in linux-x64 linux-arm64; do
    case "$rid" in
        linux-x64) deb_arch=amd64; pkg_arch=x86_64 ;;
        linux-arm64) deb_arch=arm64; pkg_arch=aarch64 ;;
    esac

    publish_dir="$OUTPUT_DIR/staging/$deb_arch/publish"
    pkg_root="$OUTPUT_DIR/staging/$deb_arch/pkg"

    echo "========================================="
    echo "  Publishing $rid"
    echo "========================================="
    echo ""

    rm -rf "$OUTPUT_DIR/staging/$deb_arch"

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

    echo "Creating package root..."
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

    find "$pkg_root/usr" -type d -exec chmod 755 {} \;
    find "$pkg_root/usr/lib/conference-audio-recorder" -type f -exec chmod 644 {} \;
    chmod 755 "$pkg_root/usr/lib/conference-audio-recorder/ConferenceAudioRecorder"
    find "$pkg_root/usr/lib/conference-audio-recorder" \( -name "*.so" -o -name "*.so.*" \) -exec chmod 755 {} \;
    chmod 644 "$pkg_root/usr/share/applications/conference-audio-recorder.desktop"
    chmod 644 "$pkg_root/usr/share/pixmaps/conference-audio-recorder.png"
    chmod 644 "$pkg_root/usr/share/doc/conference-audio-recorder/copyright"
    chmod 644 "$pkg_root/usr/lib/conference-audio-recorder/THIRD-PARTY-NOTICES.md"
    chmod 644 "$pkg_root/usr/share/doc/conference-audio-recorder/THIRD-PARTY-NOTICES.md"

    common=(
        -s dir
        -n conference-audio-recorder
        -v "$version"
        --description "$description"
        --maintainer "$maintainer"
        --url "$url"
        --license "CC0-1.0"
        --category sound
        -C "$pkg_root"
        --force
    )
    scripts=(
        --after-install "$script_dir/postinst"
        --after-remove "$script_dir/postrm"
    )

    echo "Building deb ($deb_arch)..."
    fpm "${common[@]}" "${scripts[@]}" \
        -t deb \
        -a "$deb_arch" \
        --deb-priority optional \
        --deb-compression xz \
        --depends libc6 \
        --depends libgcc-s1 \
        --depends libstdc++6 \
        --depends libx11-6 \
        --depends libfontconfig1 \
        --depends "libasound2 | libasound2t64" \
        --depends libsndfile1 \
        -p "$OUTPUT_DIR/conference-audio-recorder_${version}_linux_${deb_arch}.deb" \
        usr

    echo "Building rpm ($pkg_arch)..."
    fpm "${common[@]}" "${scripts[@]}" \
        -t rpm \
        -a "$pkg_arch" \
        --rpm-os linux \
        --rpm-compression xz \
        --rpm-auto-add-directories \
        --rpm-summary "$description" \
        --depends "libasound.so.2()(64bit)" \
        --depends "libsndfile.so.1()(64bit)" \
        -p "$OUTPUT_DIR/conference-audio-recorder_${version}_linux_${pkg_arch}.rpm" \
        usr

    echo "Building pacman ($pkg_arch)..."
    fpm "${common[@]}" "${scripts[@]}" \
        -t pacman \
        -a "$pkg_arch" \
        --depends glibc \
        --depends gcc-libs \
        --depends libx11 \
        --depends fontconfig \
        --depends alsa-lib \
        --depends libsndfile \
        -p "$OUTPUT_DIR/conference-audio-recorder_${version}_linux_${pkg_arch}.pkg.tar.zst" \
        usr

    echo "Building tarball ($deb_arch)..."
    fpm "${common[@]}" \
        -t tar \
        -a "$deb_arch" \
        -p "$OUTPUT_DIR/conference-audio-recorder_${version}_linux_${deb_arch}.tar.gz" \
        usr

    echo ""
done

rm -rf "$OUTPUT_DIR/staging"

echo "========================================="
echo "  Build complete"
echo "========================================="
ls -lh \
    "$OUTPUT_DIR"/*.deb \
    "$OUTPUT_DIR"/*.rpm \
    "$OUTPUT_DIR"/*.pkg.tar.zst \
    "$OUTPUT_DIR"/*.tar.gz
