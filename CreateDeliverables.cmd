REM Run from dev command line
D:
cd \ProjectsPersonal\JWLMerge
rd JWLMerge\bin /q /s
rd JWLMergeCLI\bin /q /s
rd Installer\Output /q /s
rd Installer\Staging /q /s

REM build / publish
dotnet publish JWLMerge\JWLMerge.csproj -p:PublishProfile=FolderProfile -c:Release
dotnet publish JWLMergeCLI\JWLMergeCLI.csproj -p:PublishProfile=FolderProfile -c:Release

md Installer\Staging

REM copy items into staging area
xcopy JWLMergeCLI\bin\Release\net6.0\publish\*.* Installer\Staging /q /s /y /d
xcopy JWLMerge\bin\Release\net6.0-windows\publish\*.* Installer\Staging /q /s /y /d

REM Create installer
"C:\Program Files (x86)\Inno Setup 6\iscc" Installer\jwlmergesetup.iss

REM create portable zip
powershell Compress-Archive -Path Installer\Staging\* -DestinationPath Installer\Output\JWLMergePortable.zip 