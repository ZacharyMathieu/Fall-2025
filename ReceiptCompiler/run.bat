@echo off
type nul | ollama run llama3.1
start http://localhost:5000
.\bin\Debug\net9.0\ReceiptCompiler.exe