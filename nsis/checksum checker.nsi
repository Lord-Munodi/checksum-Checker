; Checksum Checker installer script, installs per-user
; Written based on the examples by written by Joost Verburg
;--------------------------------
;Include Modern UI
;!verbose 2
  !include "MUI2.nsh"
  !include "FileFunc.nsh"

;--------------------------------
;General

  ;Name and file
  !define APPNAME "Checksum Checker"
  !define ContextMenuString "Open with Checksum Checker"
  !define Version "1.0"
  Name "${APPNAME} ${Version}"
  OutFile "${APPNAME} installer.exe"

  ;Default installation folder
  InstallDir "$LOCALAPPDATA\Checksum Checker"
  
  ;Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\Checksum Checker" ""

  ;Request application privileges for Windows Vista
  RequestExecutionLevel user

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_WELCOME
  ;!insertmacro MUI_PAGE_LICENSE "../LICENCE"
  !insertmacro MUI_PAGE_COMPONENTS
  ;!insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  !insertmacro MUI_PAGE_FINISH
  ;!insertmacro MUI_FINISHPAGE_RUN "$INSTDIR\Checksum Checker.exe"
  
  ;!insertmacro MUI_UNPAGE_WELCOME
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  ;!insertmacro MUI_UNPAGE_FINISH
  
;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Installer Sections

Section "Install executable" BaseInstall

  SetOutPath "$INSTDIR"
  
  ;ADD YOUR OWN FILES HERE...
  File "..\Checksum Checker\bin\Release\Checksum Checker.exe"

  
  ;Store installation folder
  WriteRegStr HKCU "Software\Checksum Checker" "" $INSTDIR
  
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  
  ;Start menu shortcut
  createDirectory "$SMPROGRAMS\${APPNAME}"
  createShortCut "$SMPROGRAMS\${APPNAME}\${APPNAME}.lnk" "$INSTDIR\Checksum Checker.exe" ""
  createShortCut "$SMPROGRAMS\${APPNAME}\Uninstall ${APPNAME}.lnk" "$INSTDIR\Uninstall.exe" ""
  
  ;Add/Remove Programs
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\Checksum Checker" \
                   "DisplayName" "Checksum Checker"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\Checksum Checker" \
                   "UninstallString" "$\"$INSTDIR\uninstall.exe$\""
  ${GetSize} "$INSTDIR" "/S=0K" $0 $1 $2
  IntFmt $0 "0x%08X" $0
  WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\Checksum Checker" "EstimatedSize" "$0"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\Checksum Checker" \
                   "DisplayVersion" "${Version}"
  WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\Checksum Checker" \
                   "NoModify" "1"

SectionEnd

Section "Context menu" SecContextMenu
 
  SetOutPath "$INSTDIR"
 
  ;add to user's explorer context menu in registry
  WriteRegStr HKCU "Software\Classes\*\shell\${ContextMenuString}\command" "" "$INSTDIR\Checksum Checker.exe $\"%1$\""
  WriteRegStr HKCU "Software\Classes\*\shell\${ContextMenuString} -sha1\command" "" "$INSTDIR\Checksum Checker.exe $\"%1$\" -sha1"
 
SectionEnd

Function .onInit
  ; set section 'BaseInstall' as selected and read-only
  IntOp $0 ${SF_SELECTED} | ${SF_RO}
  SectionSetFlags ${BaseInstall} $0
FunctionEnd

;--------------------------------
;Descriptions

  ;Language strings
  LangString DESC_BaseInstall ${LANG_ENGLISH} "Install the program."
  LangString DESC_SecContextMenu ${LANG_ENGLISH} "Add to the Explorer right-click menu to quickly checksum a file or group of files."

  ;Assign language strings to sections
  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${BaseInstall} $(DESC_BaseInstall)
    !insertmacro MUI_DESCRIPTION_TEXT ${SecContextMenu} $(DESC_SecContextMenu)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  ;ADD YOUR OWN FILES HERE...
  
  ;Delete from start menu
  RMDir /r "$SMPROGRAMS\${APPNAME}"
  
  Delete "$INSTDIR\Checksum Checker.exe"

  Delete "$INSTDIR\Uninstall.exe"

  RMDir "$INSTDIR"

  ;subtract from explorer context menu in registry
  DeleteRegKey HKCU "Software\Classes\*\shell\${ContextMenuString}"
  DeleteRegKey HKCU "Software\Classes\*\shell\${ContextMenuString} -sha1"
  
  ;Add/Remove Programs
  DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\Checksum Checker"

  DeleteRegKey /ifempty HKCU "Software\Checksum Checker"

SectionEnd