@echo off
echo === Building LuBan.TrimTest with trimmer analysis ===
dotnet build LuBan.TrimTest\LuBan.TrimTest.csproj -c Release
if %ERRORLEVEL% NEQ 0 (
    echo FAIL: Trimmer analysis detected issues.
    exit /b 1
)
echo PASS: No trimmer warnings.