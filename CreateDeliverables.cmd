REM Run from dev command line

@ECHO OFF

VERIFY ON

D:
cd \ProjectsPersonal\JWLMerge
rd JWLMerge\bin /q /s
rd JWLMergeCLI\bin /q /s
rd Installer\Output /q /s
rd Installer\Staging /q /s

ECHO.
ECHO Publishing JWLMerge
dotnet publish JWLMerge\JWLMerge.csproj -p:PublishProfile=FolderProfile -c:Release
IF %ERRORLEVEL% NEQ 0 goto ERROR

ECHO.
ECHO Publishing JWLMergeCLI
dotnet publish JWLMergeCLI\JWLMergeCLI.csproj -p:PublishProfile=FolderProfile -c:Release
IF %ERRORLEVEL% NEQ 0 goto ERROR

md Installer\Staging

ECHO.
ECHO Copying JWLMergeCLI items into staging area
xcopy JWLMergeCLI\bin\Release\net6.0\publish\*.* Installer\Staging /q /s /y /d

ECHO Copying JWLMerge items into staging area
xcopy JWLMerge\bin\Release\net6.0-windows\publish\*.* Installer\Staging /q /s /y /d

ECHO.
ECHO Creating installer
"D:\Program Files (x86)\Inno Setup 6\iscc" Installer\jwlmergesetup.iss
IF %ERRORLEVEL% NEQ 0 goto ERROR

ECHO.
ECHO Creating portable zip
powershell Compress-Archive -Path Installer\Staging\* -DestinationPath Installer\Output\JWLMergePortable.zip 
IF %ERRORLEVEL% NEQ 0 goto ERROR

goto SUCCESS

:ERROR
ECHO.
ECHO ******************
ECHO An ERROR occurred!
ECHO ******************

:SUCCESS

PAUSE