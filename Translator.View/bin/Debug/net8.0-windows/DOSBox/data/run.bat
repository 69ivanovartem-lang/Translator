@echo off
echo ========================================
echo Compiling compile.asm...
echo ========================================
MASM.EXE compile.asm, compile.obj, compile.lst, compile.crf
if errorlevel 1 goto error

echo.
echo ========================================
echo Linking...
echo ========================================
LINK.EXE compile.obj, compile.exe, compile.map, /NODEFAULTLIB
if errorlevel 1 goto error

echo.
echo ========================================
echo Running program...
echo ========================================
echo.
compile.exe
echo.
echo ========================================
echo Press any key to exit...
pause > nul
goto end

:error
echo.
echo ========================================
echo ERROR during compilation!
echo ========================================
pause

:end
