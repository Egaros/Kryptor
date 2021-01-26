[![CodeFactor](https://www.codefactor.io/repository/github/samuel-lucas6/kryptor/badge)](https://www.codefactor.io/repository/github/samuel-lucas6/kryptor) [![License: GPL v3](https://img.shields.io/badge/License-GPL%20v3-blue.svg)](http://www.gnu.org/licenses/gpl-3.0)

# Kryptor

Kryptor is free and open source file encryption software for Windows, Linux, and macOS.

![Screenshot](https://kryptor.co.uk/screenshots/file-encryption.gif)

## Main Features

- File encryption using XChaCha20 with 256-bit keys.
- Key derivation using Argon2id.
- A unique encryption key per file.
- Supports passwords and/or keyfiles.
- Optional file/folder name obfuscation.
- Password sharing using libsodium Sealed Boxes (Curve25519, XSalsa20-Poly1305).
- Can be run offline. No account needed.

For more information, head over to the [Kryptor website](https://kryptor.co.uk).

## CLI Usage
```
Usage: kryptor [options] <Arguments>

Options:
  -e|--encrypt           encrypt files/folders
  -d|--decrypt           decrypt files/folders
  -p|--password          specify a password for file encryption/decryption
  -k|--keyfile           specify a keyfile for file encryption/decryption
  --generate-keyfile     generate a random keyfile at the specified path
  --generate-password    generate a random password of a specified length
  --generate-passphrase  generate a random passphrase of a specified length
  --generate-keys        generate a recipient key pair for password sharing
  --encrypt-password     encrypt a password using the recipient's public key
  --decrypt-password     decrypt a ciphertext password using your private key
  --settings             view/edit your settings
  --benchmark            perform the Argon2 benchmark
  --documentation        view the documentation
  --source               view the source code
  --about                view the program version and license
  --update               check for updates   
  -h|--help              show help information

Examples:
  -e -p [password] [file]
  -e -k [keyfile] [file]
  -e -p [password] -k [keyfile] [file]
  --generate-password [length]
  --encrypt-password [publickey] [password]
  ```
When referencing file paths/file names that contain spaces, you must surround them with ' ' on Linux/macOS and " " on Windows.
```
$ ./kryptor -e -p Stumbling-Sixfold7-Disband-Riverboat 'GitHub Logo.png'
$ ./kryptor -e -p Stumbling-Sixfold7-Disband-Riverboat '/home/samuel/Downloads/GitHub Logo.png'

$ kryptor -e -p Stumbling-Sixfold7-Disband-Riverboat "GitHub Logo.png"
$ kryptor -e -p Stumbling-Sixfold7-Disband-Riverboat "C:\Users\Samuel\Downloads\GitHub Logo.png"
```

## Building Kryptor

Before you do anything with the source code, make sure you understand the [GPLv3](https://www.gnu.org/licenses/gpl-3.0.en.html) license used by Kryptor. Click [here](https://tldrlegal.com/license/gnu-general-public-license-v3-(gpl-3)) for a summary of GPLv3.

#### CLI

1. Go to the latest [release](https://github.com/samuel-lucas6/Kryptor/releases) and download ```Source code (zip)```.
2. Extract the ZIP, navigate to the ```KryptorCLI``` folder, and open the ```KryptorCLI.sln``` file in [Visual Studio 2019 Community](https://visualstudio.microsoft.com/vs/community/).
3. Right click on ```KryptorCLI``` in the Solution Explorer and choose ```Publish...```.
4. Select ```win-x64```, ```linux-x64```, or ```osx-x64``` depending on the operating system you want to publish for, and then click ```Publish```. You will find the published program at the path listed next to ```Target location```.
5. I recommend using the default profile settings, but you can publish the application as Framework-dependent, change the target location, etc by changing the profile settings via ```Edit``` or the pencil icons.

#### GUI

1. Go to the latest [release](https://github.com/samuel-lucas6/Kryptor/releases) and download ```Source code (zip)```.
2. Extract the ZIP, navigate to the ```KryptorGUI``` folder, and open the ```KryptorGUI.sln``` file in [Visual Studio 2019 Community](https://visualstudio.microsoft.com/vs/community/).
3. Select ```Release``` and ```x64``` for the build options, and then click ```Build``` and ```Build Kryptor```.
4. The [libsodium-core](https://github.com/tabrath/libsodium-core/issues/44) library used by Kryptor does not support building to ```AnyCPU``` in Visual Studio - you must either build to ```x86``` or ```x64```. Build to ```x64``` when possible.

#### GUI Requirements

- Windows: The [libsodium](https://libsodium.org) library requires the [Visual C++ Redistributable for Visual Studio 2015-2019](https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads) to work. Therefore, you must keep the ```vcruntime140.dll``` file in the same folder as ```Kryptor.exe``` on Windows.
- Linux: You must build Kryptor as ```x64```. To retrieve the libsodium library for Linux, download the latest KryptorGUI portable [release](https://kryptor.co.uk/downloads.html), extract ```KryptorGUI-Portable.zip```, and copy the ```libsodium.so``` file to the same folder as the ```Kryptor.exe``` file you've built.
- macOS: You must build Kryptor as ```x64```. To retrieve the libsodium library for macOS, download the latest KryptorGUI portable [release](https://kryptor.co.uk/downloads.html), extract ```KryptorGUI-Portable.zip```, and copy the ```libsodium.dylib``` file to the same folder as the ```Kryptor.exe``` file you've built.

#### Notes

Kryptor is developed using [Visual Studio 2019 Community](https://visualstudio.microsoft.com/vs/community/). I recommend using this IDE, but it isn't open source. Furthermore, you have to sign into a Microsoft account after 30 days, although this can be bypassed.

If you just want to view the code, then you can use a text editor like [Atom](https://atom.io/) or a source code editor like [VSCodium](https://vscodium.com), which is the open source version of VSCode. However, you must use [Visual Studio 2019 Community](https://visualstudio.microsoft.com/vs/community/) to view the Windows Forms Designer for the GUI version and to publish the CLI version as a self-contained executable.

## Donate

If you want to support the project and help me afford the website domain, please consider donating through Paypal, GitHub, Bitcoin, or Monero. Every little helps. Thank you for the support.

Find out how to donate [here](https://kryptor.co.uk/donate.html).
