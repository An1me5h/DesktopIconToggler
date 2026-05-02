; ============================================================
;  Desktop Icon Toggler — Windows Installer Script
;  Built with NSIS (Nullsoft Scriptable Install System)
; ============================================================

Unicode True

Name              "Desktop Icon Toggler"
OutFile           "DesktopIconToggler_Setup.exe"
InstallDir        "$PROGRAMFILES64\DesktopIconToggler"
InstallDirRegKey  HKCU "Software\DesktopIconToggler" "InstallDir"
RequestExecutionLevel admin
BrandingText      "Desktop Icon Toggler v1.0"

!include "MUI2.nsh"

!define MUI_ABORTWARNING
!define MUI_ICON   "${NSISDIR}\Contrib\Graphics\Icons\modern-install.ico"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

!define MUI_WELCOMEPAGE_TITLE "Welcome to Desktop Icon Toggler Setup"
!define MUI_WELCOMEPAGE_TEXT  "This will install Desktop Icon Toggler on your computer.$\r$\n$\r$\nFeatures:$\r$\n  • Toggle desktop icons with one click$\r$\n  • Auto transparent taskbar on desktop$\r$\n  • Runs silently in the system tray$\r$\n$\r$\nClick Next to continue."

!define MUI_FINISHPAGE_RUN      "$INSTDIR\DesktopIconToggler.exe"
!define MUI_FINISHPAGE_RUN_TEXT "Launch Desktop Icon Toggler now"
!define MUI_FINISHPAGE_TITLE    "Installation Complete!"
!define MUI_FINISHPAGE_TEXT     "Desktop Icon Toggler has been installed.$\r$\n$\r$\nIt will appear in your system tray (bottom-right near the clock).$\r$\n$\r$\nLeft-click the tray icon to toggle desktop icons."

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "License.txt"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "English"

Section "Desktop Icon Toggler" SecMain
  SectionIn RO
  SetOutPath "$INSTDIR"
  File "DesktopIconToggler.exe"
  File "License.txt"
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  WriteRegStr HKCU "Software\DesktopIconToggler" "InstallDir" "$INSTDIR"
  WriteRegStr   HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\DesktopIconToggler" "DisplayName"     "Desktop Icon Toggler"
  WriteRegStr   HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\DesktopIconToggler" "UninstallString" '"$INSTDIR\Uninstall.exe"'
  WriteRegStr   HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\DesktopIconToggler" "DisplayIcon"     "$INSTDIR\DesktopIconToggler.exe"
  WriteRegStr   HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\DesktopIconToggler" "Publisher"       "Desktop Icon Toggler"
  WriteRegStr   HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\DesktopIconToggler" "DisplayVersion"  "1.0.0"
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\DesktopIconToggler" "NoModify"        1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\DesktopIconToggler" "NoRepair"        1
  CreateDirectory "$SMPROGRAMS\Desktop Icon Toggler"
  CreateShortcut  "$SMPROGRAMS\Desktop Icon Toggler\Desktop Icon Toggler.lnk" "$INSTDIR\DesktopIconToggler.exe"
  CreateShortcut  "$SMPROGRAMS\Desktop Icon Toggler\Uninstall.lnk"            "$INSTDIR\Uninstall.exe"
  CreateShortcut  "$DESKTOP\Desktop Icon Toggler.lnk"                         "$INSTDIR\DesktopIconToggler.exe"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "DesktopIconToggler" '"$INSTDIR\DesktopIconToggler.exe"'
SectionEnd

Section "Uninstall"
  ExecWait 'taskkill /F /IM DesktopIconToggler.exe'
  Sleep 500
  Delete "$INSTDIR\DesktopIconToggler.exe"
  Delete "$INSTDIR\License.txt"
  Delete "$INSTDIR\Uninstall.exe"
  RMDir  "$INSTDIR"
  Delete "$SMPROGRAMS\Desktop Icon Toggler\Desktop Icon Toggler.lnk"
  Delete "$SMPROGRAMS\Desktop Icon Toggler\Uninstall.lnk"
  RMDir  "$SMPROGRAMS\Desktop Icon Toggler"
  Delete "$DESKTOP\Desktop Icon Toggler.lnk"
  DeleteRegKey HKCU "Software\DesktopIconToggler"
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\DesktopIconToggler"
  DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "DesktopIconToggler"
  MessageBox MB_OK "Desktop Icon Toggler has been uninstalled successfully."
SectionEnd
