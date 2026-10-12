@echo off
rem Debug package: same as Release.cmd but -c Debug.
rem The bump harness runs after evaluation, so the package version can be one
rem BuildNumber ahead of the DLL inside - accepted, see WORK_IN_PROGRESS.md.
dotnet pack Src\MediaDevices\ -c Debug %*
