# JWLMerge <img src="https://ci.appveyor.com/api/projects/status/2nl90i7apndaxcac?svg=true">

## NOW ARCHIVED

Note that this project is no longer compatible with the latest version of the JW Library. In July 2023 the database schema was changed from v8 to v13 and I will not be updating JWLMerge to support the new version. In Jan 2023, the JW Library licence agreement was strengthened, but it also gave opportunity for 3rd party developers to apply for official permission to do the sort of work that is present in JWLMerge. I made an application in January but have not received permission, and in these circumstances feel it best to archive the project at this juncture. Thank you for your understanding, and for your valuable input and feedback on the project over the last 6 years.

### Introduction

JWLMerge is a utility to merge 2 or more .jwlibrary backup files. These backup files are created using the JW Library® backup command, and contain your personal notes, highlighting, etc.

At time of writing, JW Library has _backup_ and _restore_ commands, but no _merge_ command. This means you can add personal annotations in JW Library on several devices and transfer these between devices, but cannot combine them into a single set. JWLMerge is a Windows application that allows you to merge 2 or more .jwlibrary files into a single backup file that can then be restored onto any device.

![Main Window](jwlmerge.png)

### Usage

Download the JWLMergeSetup.exe file from the latest release and run it. There is also a "portable" version (so you can just copy files to a folder).

https://github.com/AntonyCorbett/JWLMerge/releases/latest

**Please see the wiki for further information:**

https://github.com/AntonyCorbett/JWLMerge/wiki

#### Command-line

JWLMergeCLI.exe is a command-line version of the application. Run in at a command prompt to display a description of usage.

#### Wine

If you are interested in running JWLMerge on Wine, please see these notes: https://github.com/SuperJC710e/JWLMerge/wiki/Running-Under-Wine

### Important Notes

Please use at your own risk! It is possible to crash JW Library by attempting to restore a corrupt backup file, and sometimes the only solution is to reinstall JW Library and reset its data. (This hasn't happened yet with a JWLMerge-generated file.)

The format of your data in the backup files may change with future JW Library releases and I will try to keep the code up-to-date. If you receive an error stating that the backup file version is not supported then please come back here to see if an update is available.

It is possible that JW Library will eventually feature a _merge_ command or similar functionality, in which case this project will be archived.

Please review the JW Library terms and conditions of use. Some view the backup data files as their _own_ data and not subject to the conditions; others feel differently. Please make your own decision on this matter.

There is no guarantee of software support. However, if you find a bug or other problem, please feel free to create an issue in this project.

Log files are stored in your Documents\JWLMerge\Logs folder.

"JW Library" is a registered trademark of Watch Tower Bible and Tract Society of Pennsylvania.
