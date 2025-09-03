# RPG Maker Decrypter GUI

A modern, user-friendly graphical interface for the RPGMakerDecrypter tool. This GUI makes it easy to extract and decrypt RPG Maker game files without using the command line.

## Features

- **Intuitive Interface**: Modern Material Design UI with dark theme
- **Multi-Version Support**: Supports RPG Maker XP, VX, VX Ace, MV, and MZ
- **Automatic Version Detection**: Automatically detects the RPG Maker version
- **Real-time Progress**: Visual progress bar and detailed log output
- **Project Recreation**: Optional recreation of original project structure
- **Drag & Drop Support**: Easy file and folder selection

## Supported Formats

- **RPG Maker XP**: .rgssad files
- **RPG Maker VX**: .rgss2a files  
- **RPG Maker VX Ace**: .rgss3a files
- **RPG Maker MV/MZ**: Game folders with encrypted assets

## Requirements

- Windows 10/11
- .NET 8.0 Runtime (included with Windows 11, downloadable for Windows 10)

## Building

1. Open `RPGMakerDecrypter.sln` in Visual Studio 2022
2. Right-click on `RPGMakerDecrypter.GUI` project
3. Select "Set as Startup Project"
4. Press F5 to build and run

## Usage

1. **Select Input**:
   - For XP/VX/VX Ace: Click "Browse File" and select the archive file
   - For MV/MZ: Click "Browse Folder" and select the game directory

2. **Select Output**:
   - Click "Browse" to choose where to extract files
   - Default is an "Extracted" folder in the same location as input

3. **Options**:
   - Check "Recreate original project structure" to attempt project reconstruction

4. **Decrypt**:
   - Click the "Decrypt" button to start extraction
   - Monitor progress in the log window

## Screenshots

The GUI features:
- Clean Material Design interface
- Dark theme for comfortable viewing
- Real-time progress tracking
- Detailed operation logging

## Troubleshooting

### "Unable to find encryption key"
- For MV/MZ games, ensure you selected the correct game folder
- The folder should contain a System.json file

### "Unknown version"
- Ensure the file extension matches the RPG Maker version
- For MV/MZ, select the game folder, not individual files

### Extraction fails
- Check you have write permissions to the output folder
- Ensure sufficient disk space is available

## License

This GUI uses the same license as the original RPGMakerDecrypter project.

## Credits

- Original RPGMakerDecrypter by uuksu
- GUI implementation using WPF and Material Design themes
- Community Toolkit MVVM for clean architecture