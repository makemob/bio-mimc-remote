@echo off
echo Capturing... Close window to stop.
"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" logcat -s Unity > DebugOutput.txt
