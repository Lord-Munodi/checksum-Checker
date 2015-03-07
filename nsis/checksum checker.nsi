;--------------------------------
;Include Modern UI

  !include "MUI2.nsh"

;--------------------------------
;General

  ;Name and file
  Name "Checksum Checker 1.0"
  OutFile "Checksum Checker installer.exe"

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
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  !insertmacro MUI_PAGE_FINISH
  
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
  WriteRegStr HKCU "Software\Modern UI Test" "" $INSTDIR
  
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"

SectionEnd

Section "Context menu" SecContextMenu
 
  SetOutPath "$INSTDIR"
 
  ;ADD YOUR OWN FILES HERE...
 
SectionEnd

Function .onInit
  # set section 'test' as selected and read-only
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
  
  Delete "$INSTDIR\Checksum Checker.exe"

  Delete "$INSTDIR\Uninstall.exe"

  RMDir "$INSTDIR"

  DeleteRegKey /ifempty HKCU "Software\Modern UI Test"

SectionEnd