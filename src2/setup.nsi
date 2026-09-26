; Compression
SetCompressor /FINAL /SOLID lzma
SetCompressorDictSize 64

Unicode true

!define PRODUCT_NAME "Conference Audio Recorder"
!define PRODUCT_REG "ConferenceAudioRecorder"
!ifndef PRODUCT_VERSION
  !define PRODUCT_VERSION "0.0.0"
!endif
!define PRODUCT_PUBLISHER "Siarhei Kuchuk"
!define PRODUCT_WEB_SITE "https://github.com/drweb86/conference-audio-recorder"
!define START_YEAR "2022"
!define CURRENT_YEAR "$%DATE:~-4%"

!include "MUI2.nsh"
!include "x64.nsh"
!include "WinVer.nsh"
!include "FileFunc.nsh"
!include "nsDialogs.nsh"
!include "LogicLib.nsh"

!insertmacro GetParameters
!insertmacro GetOptions

!define MUI_ABORTWARNING
!define MUI_ICON "ConferenceAudioRecorder\Assets\app.ico"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

!define MUI_LANGDLL_ALLLANGUAGES
!define MUI_LANGDLL_REGISTRY_ROOT "HKCU"
!define MUI_LANGDLL_REGISTRY_KEY "Software\${PRODUCT_REG}"
!define MUI_LANGDLL_REGISTRY_VALUENAME "Installer Language"

!define MUI_WELCOMEPAGE_TITLE_3LINES
!insertmacro MUI_PAGE_WELCOME

Page custom InstallModePageCreate InstallModePageLeave

!define MUI_PAGE_CUSTOMFUNCTION_PRE SkipDirectoryPage
!insertmacro MUI_PAGE_DIRECTORY

Var StartMenuFolder
!define MUI_PAGE_CUSTOMFUNCTION_PRE SkipStartMenuPage
!define MUI_STARTMENUPAGE_DEFAULTFOLDER "${PRODUCT_NAME}"
!define MUI_STARTMENUPAGE_REGISTRY_ROOT "HKCU"
!define MUI_STARTMENUPAGE_REGISTRY_KEY "Software\${PRODUCT_REG}"
!define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "Start Menu Folder"
!insertmacro MUI_PAGE_STARTMENU Application $StartMenuFolder

!insertmacro MUI_PAGE_INSTFILES

!define MUI_FINISHPAGE_RUN "$INSTDIR\bin\ConferenceAudioRecorder.exe"
!define MUI_FINISHPAGE_RUN_TEXT "$(Installer_LaunchApp)"
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_LANGUAGE "French"
!insertmacro MUI_LANGUAGE "German"
!insertmacro MUI_LANGUAGE "SpanishInternational"
!insertmacro MUI_LANGUAGE "Italian"
!insertmacro MUI_LANGUAGE "Portuguese"
!insertmacro MUI_LANGUAGE "PortugueseBR"
!insertmacro MUI_LANGUAGE "Russian"
!insertmacro MUI_LANGUAGE "SimpChinese"
!insertmacro MUI_LANGUAGE "TradChinese"
!insertmacro MUI_LANGUAGE "Japanese"
!insertmacro MUI_LANGUAGE "Korean"
!insertmacro MUI_LANGUAGE "Dutch"
!insertmacro MUI_LANGUAGE "Polish"
!insertmacro MUI_LANGUAGE "Czech"
!insertmacro MUI_LANGUAGE "Swedish"
!insertmacro MUI_LANGUAGE "Danish"
!insertmacro MUI_LANGUAGE "Norwegian"
!insertmacro MUI_LANGUAGE "Finnish"
!insertmacro MUI_LANGUAGE "Greek"
!insertmacro MUI_LANGUAGE "Hungarian"
!insertmacro MUI_LANGUAGE "Romanian"
!insertmacro MUI_LANGUAGE "Turkish"
!insertmacro MUI_LANGUAGE "Ukrainian"
!insertmacro MUI_LANGUAGE "Arabic"
!insertmacro MUI_LANGUAGE "Hebrew"
!insertmacro MUI_LANGUAGE "Thai"
!insertmacro MUI_LANGUAGE "Vietnamese"
!insertmacro MUI_LANGUAGE "Indonesian"
!insertmacro MUI_LANGUAGE "Bulgarian"
!insertmacro MUI_LANGUAGE "Croatian"
!insertmacro MUI_LANGUAGE "Slovak"
!insertmacro MUI_LANGUAGE "Slovenian"

!include "setup-strings.nsh"

Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "..\Output\conference-audio-recorder_${PRODUCT_VERSION}_windows_setup.exe"
InstallDir "$LOCALAPPDATA\Programs\${PRODUCT_NAME}"
ShowInstDetails show
ShowUnInstDetails show
RequestExecutionLevel user

VIProductVersion "${PRODUCT_VERSION}.0"
VIAddVersionKey "ProductName" "${PRODUCT_NAME}"
VIAddVersionKey "ProductVersion" "${PRODUCT_VERSION}"
VIAddVersionKey "CompanyName" "${PRODUCT_PUBLISHER}"
VIAddVersionKey "LegalCopyright" "CC0 ${START_YEAR}-${CURRENT_YEAR} ${PRODUCT_PUBLISHER}"
VIAddVersionKey "FileDescription" "${PRODUCT_NAME} installer"
VIAddVersionKey "FileVersion" "${PRODUCT_VERSION}"

Var InstallMode
Var MultiUser
Var InstallModeDialog
Var InstallModeRadio1
Var InstallModeRadio2
Var InstallModeLabel
Var CmdLineMode

Function .onInit
  !insertmacro MUI_LANGDLL_DISPLAY

  StrCpy $InstallMode "CurrentUser"
  StrCpy $MultiUser "0"
  StrCpy $CmdLineMode "0"

  ${GetParameters} $0
  ${GetOptions} $0 "/ALLUSERS" $1
  ${IfNot} ${Errors}
    StrCpy $InstallMode "AllUsers"
    StrCpy $MultiUser "1"
    StrCpy $CmdLineMode "1"
  ${EndIf}

  ${GetOptions} $0 "/AllUsers" $1
  ${IfNot} ${Errors}
    StrCpy $InstallMode "AllUsers"
    StrCpy $MultiUser "1"
    StrCpy $CmdLineMode "1"
  ${EndIf}

  ${GetOptions} $0 "/CURRENTUSER" $1
  ${IfNot} ${Errors}
    StrCpy $InstallMode "CurrentUser"
    StrCpy $MultiUser "0"
    StrCpy $CmdLineMode "1"
  ${EndIf}

  ${If} ${RunningX64}
    SetRegView 64
    ${If} $MultiUser == "1"
      StrCpy $INSTDIR "$PROGRAMFILES64\${PRODUCT_NAME}"
    ${Else}
      StrCpy $INSTDIR "$LOCALAPPDATA\Programs\${PRODUCT_NAME}"
    ${EndIf}
  ${Else}
    MessageBox MB_OK|MB_ICONSTOP "$(Installer_ArchRequired)"
    Abort
  ${EndIf}
FunctionEnd

Function InstallModePageCreate
  ${If} $CmdLineMode == "1"
    Abort
  ${EndIf}

  !insertmacro MUI_HEADER_TEXT "$(Installer_InstallModeTitle)" "$(Installer_InstallModeSubtitle)"

  nsDialogs::Create 1018
  Pop $InstallModeDialog
  ${If} $InstallModeDialog == error
    Abort
  ${EndIf}

  ${NSD_CreateLabel} 0 0 100% 24u "$(Installer_InstallModeDescription)"
  Pop $InstallModeLabel

  ${NSD_CreateRadioButton} 10u 30u 100% 12u "$(Installer_CurrentUserOption)"
  Pop $InstallModeRadio1
  ${NSD_OnClick} $InstallModeRadio1 InstallModeRadio1Click

  ${NSD_CreateLabel} 20u 45u 95% 16u "$(Installer_CurrentUserDescription)"
  Pop $0

  ${NSD_CreateRadioButton} 10u 65u 100% 12u "$(Installer_AllUsersOption)"
  Pop $InstallModeRadio2
  ${NSD_OnClick} $InstallModeRadio2 InstallModeRadio2Click

  ${NSD_CreateLabel} 20u 80u 95% 16u "$(Installer_AllUsersDescription)"
  Pop $0

  ${If} $MultiUser == "1"
    ${NSD_Check} $InstallModeRadio2
  ${Else}
    ${NSD_Check} $InstallModeRadio1
  ${EndIf}

  nsDialogs::Show
FunctionEnd

Function InstallModeRadio1Click
  Pop $0
  StrCpy $InstallMode "CurrentUser"
  StrCpy $MultiUser "0"
FunctionEnd

Function InstallModeRadio2Click
  Pop $0
  StrCpy $InstallMode "AllUsers"
  StrCpy $MultiUser "1"
FunctionEnd

Function InstallModePageLeave
  ${If} $MultiUser == "1"
    UserInfo::GetAccountType
    Pop $0
    ${If} $0 != "admin"
      MessageBox MB_OK|MB_ICONSTOP "$(Installer_AdminRequiredDetailed)"
      Abort
    ${EndIf}
    StrCpy $INSTDIR "$PROGRAMFILES64\${PRODUCT_NAME}"
  ${Else}
    StrCpy $INSTDIR "$LOCALAPPDATA\Programs\${PRODUCT_NAME}"
  ${EndIf}
FunctionEnd

Function SkipDirectoryPage
  Abort
FunctionEnd

Function SkipStartMenuPage
  Abort
FunctionEnd

Section "MainSection" SEC01
  SetOutPath "$INSTDIR\bin"
  SetOverwrite on

  ${If} $MultiUser == "1"
    SetShellVarContext all
  ${Else}
    SetShellVarContext current
  ${EndIf}

  ${If} ${IsNativeARM64}
    File /r "..\Output\publish\arm64\*.*"
  ${Else}
    File /r "..\Output\publish\x64\*.*"
  ${EndIf}

  SetOutPath "$INSTDIR"
  File "..\LICENSE"
  File "..\THIRD-PARTY-NOTICES.md"

  WriteUninstaller "$INSTDIR\uninst.exe"

  ${If} $MultiUser == "1"
    WriteRegStr HKLM "Software\${PRODUCT_REG}" "" $INSTDIR
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_REG}" "DisplayName" "${PRODUCT_NAME}"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_REG}" "UninstallString" "$INSTDIR\uninst.exe"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_REG}" "DisplayIcon" "$INSTDIR\bin\ConferenceAudioRecorder.exe"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_REG}" "DisplayVersion" "${PRODUCT_VERSION}"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_REG}" "Publisher" "${PRODUCT_PUBLISHER}"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_REG}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  ${Else}
    WriteRegStr HKCU "Software\${PRODUCT_REG}" "" $INSTDIR
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_REG}" "DisplayName" "${PRODUCT_NAME}"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_REG}" "UninstallString" "$INSTDIR\uninst.exe"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_REG}" "DisplayIcon" "$INSTDIR\bin\ConferenceAudioRecorder.exe"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_REG}" "DisplayVersion" "${PRODUCT_VERSION}"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_REG}" "Publisher" "${PRODUCT_PUBLISHER}"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_REG}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  ${EndIf}

  !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    CreateDirectory "$SMPROGRAMS\$StartMenuFolder"
    CreateShortcut "$SMPROGRAMS\$StartMenuFolder\${PRODUCT_NAME}.lnk" "$INSTDIR\bin\ConferenceAudioRecorder.exe"
  !insertmacro MUI_STARTMENU_WRITE_END

  CreateShortcut "$DESKTOP\${PRODUCT_NAME}.lnk" "$INSTDIR\bin\ConferenceAudioRecorder.exe"
SectionEnd

Function un.onInit
  !insertmacro MUI_UNGETLANGUAGE
FunctionEnd

Section Uninstall
  !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuFolder

  Delete "$SMPROGRAMS\$StartMenuFolder\${PRODUCT_NAME}.lnk"
  RMDir "$SMPROGRAMS\$StartMenuFolder"
  Delete "$DESKTOP\${PRODUCT_NAME}.lnk"

  RMDir /r "$INSTDIR\bin"
  Delete "$INSTDIR\LICENSE"
  Delete "$INSTDIR\THIRD-PARTY-NOTICES.md"
  Delete "$INSTDIR\uninst.exe"
  RMDir "$INSTDIR"

  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_REG}"
  DeleteRegKey HKLM "Software\${PRODUCT_REG}"
  DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_REG}"
  DeleteRegKey HKCU "Software\${PRODUCT_REG}"

  SetAutoClose true
SectionEnd
