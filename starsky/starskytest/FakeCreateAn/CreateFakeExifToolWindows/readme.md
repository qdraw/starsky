
1. https://www.nasm.us/pub/nasm/releasebuilds/2.12.01rc2/win64/nasm-2.12.01rc2-installer-x64.exe


 C:\Users\$username\AppData\Local\bin\NASM>  .\nasm.exe -fwin64 Y:\git\starsky\starsky\starskytest\FakeCreateAn\CreateFakeExifToolWindows\code.asm
 
 https://sourceforge.net/projects/mingw-w64/files/
 
(e.g., C:\mingw). Install a current version and specify win32 as thread when requested. Additionally, choose the architecture x86_64.
 
cd "C:\Program Files\mingw-w64\x86_64-8.1.0-win32-seh-rt_v6-rev0" 
 
 .\gcc.exe C:\Users\$username\Desktop\code.obj
  
 
 C:\Program Files\mingw-w64\x86_64-8.1.0-win32-seh-rt_v6-rev0\mingw64\bin> .\a.exe  
  
 
  7z a -mm=Deflate -mfb=258 -mpass=15 foo.zip exiftool\(-k\).exe
  
  