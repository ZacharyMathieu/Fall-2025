@REM PUT THIS IN bin/Release/net9.0/win-x64/publish
@echo off
type nul | ollama run llama3.1
start http://localhost:5000
.\ReceiptCompiler.exe