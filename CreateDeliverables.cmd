REM Run from dev command line
D:
cd \ProjectsPersonal\JWLMerge
rd JWLMerge\bin /q /s
rd JWLMergeCLI\bin /q /s
rd Installer\Output /q /s

REM build / publishtar --help

dotnet publish JWLMerge\JWLMerge.csproj -p:PublishProfile=FolderProfile -c:Release
dotnet publish JWLMergeCLI\JWLMergeCLI.csproj -p:PublishProfile=FolderProfile -c:Release

REM copy items into delivery
xcopy JWLMergeCLI\bin\Release\net5.0\publish\*.* bin\Release\net5.0-windows\publish /q /s /y /d

REM Create installer
"C:\Program Files (x86)\Inno Setup 6\iscc" Installer\jwlmergesetup.iss

REM create portable zip
powershell Compress-Archive -Path JWLMerge\bin\Release\net5.0-windows\publish\* -DestinationPath Installer\Output\JWLMergePortable.zip 