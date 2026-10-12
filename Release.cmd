@echo off
rem Release package: dotnet pack with the version from Directory.Build.props (automatic).
rem The bump harness runs after evaluation, so the package version can be one
rem BuildNumber ahead of the DLL inside - accepted, see WORK_IN_PROGRESS.md.
dotnet pack Src\MediaDevices\ -c Release %*
