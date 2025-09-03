using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using RPGMakerDecrypter.Common;
using RPGMakerDecrypter.MVMZ;
using RPGMakerDecrypter.MVMZ.MV;
using RPGMakerDecrypter.MVMZ.MZ;
using RPGMakerDecrypter.RGSSAD;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RPGMakerDecrypter.GUI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _inputPath = string.Empty;

        [ObservableProperty]
        private string _outputPath = string.Empty;

        [ObservableProperty]
        private bool _recreateProject = false;

        [ObservableProperty]
        private bool _isProcessing = false;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        [ObservableProperty]
        private int _progressValue = 0;

        [ObservableProperty]
        private bool _isIndeterminate = false;

        [ObservableProperty]
        private ObservableCollection<string> _logMessages = new ObservableCollection<string>();

        [ObservableProperty]
        private RPGMakerVersion _detectedVersion = RPGMakerVersion.Unknown;

        [ObservableProperty]
        private string _versionText = "No file selected";

        public ICommand BrowseInputCommand { get; }
        public ICommand BrowseOutputCommand { get; }
        public ICommand BrowseFolderInputCommand { get; }
        public AsyncRelayCommand DecryptCommand { get; }
        public ICommand ClearLogCommand { get; }

        public MainViewModel()
        {
            BrowseInputCommand = new RelayCommand(BrowseInput);
            BrowseOutputCommand = new RelayCommand(BrowseOutput);
            BrowseFolderInputCommand = new RelayCommand(BrowseFolderInput);
            DecryptCommand = new AsyncRelayCommand(DecryptAsync, CanDecrypt);
            ClearLogCommand = new RelayCommand(ClearLog);
        }

        partial void OnInputPathChanged(string value)
        {
            DecryptCommand.NotifyCanExecuteChanged();
        }

        partial void OnOutputPathChanged(string value)
        {
            DecryptCommand.NotifyCanExecuteChanged();
        }

        partial void OnDetectedVersionChanged(RPGMakerVersion value)
        {
            DecryptCommand.NotifyCanExecuteChanged();
        }

        partial void OnIsProcessingChanged(bool value)
        {
            DecryptCommand.NotifyCanExecuteChanged();
        }

        private void BrowseInput()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select RPG Maker Archive",
                Filter = "RPG Maker Archives|*.rgssad;*.rgss2a;*.rgss3a|All Files|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                InputPath = dialog.FileName;
                DetectVersion();
                
                // Auto-set output path if empty
                if (string.IsNullOrEmpty(OutputPath))
                {
                    OutputPath = Path.Combine(Path.GetDirectoryName(InputPath) ?? "", "Extracted");
                }
            }
        }

        private void BrowseFolderInput()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select RPG Maker MV/MZ Game Folder - Choose any file in the game folder",
                Filter = "All files (*.*)|*.*",
                CheckFileExists = false,
                CheckPathExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                // Get the directory from the selected file
                InputPath = Path.GetDirectoryName(dialog.FileName) ?? "";
                DetectVersion();
                
                // Auto-set output path if empty
                if (string.IsNullOrEmpty(OutputPath))
                {
                    OutputPath = Path.Combine(Path.GetDirectoryName(InputPath) ?? "", "Extracted");
                }
            }
        }

        private void BrowseOutput()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Output Folder - Choose any location and type folder name",
                Filter = "All files (*.*)|*.*",
                CheckFileExists = false,
                CheckPathExists = true,
                Multiselect = false,
                FileName = "SelectFolder"
            };

            if (dialog.ShowDialog() == true)
            {
                // Get the directory from the selected location
                OutputPath = Path.GetDirectoryName(dialog.FileName) ?? "";
            }
        }

        private void DetectVersion()
        {
            if (string.IsNullOrEmpty(InputPath))
            {
                DetectedVersion = RPGMakerVersion.Unknown;
                VersionText = "No file selected";
                return;
            }

            try
            {
                // First try as archive file
                if (File.Exists(InputPath))
                {
                    DetectedVersion = RPGMakerDecrypter.RGSSAD.RGSSAD.GetRPGMakerVersion(InputPath);
                }
                
                // If unknown, try as MV/MZ directory
                if (DetectedVersion == RPGMakerVersion.Unknown && Directory.Exists(InputPath))
                {
                    var mvMzVersionFinder = new RPGMakerVersionFinder();
                    DetectedVersion = mvMzVersionFinder.FindVersion(InputPath);
                }

                VersionText = DetectedVersion switch
                {
                    RPGMakerVersion.Xp => "RPG Maker XP",
                    RPGMakerVersion.Vx => "RPG Maker VX",
                    RPGMakerVersion.VxAce => "RPG Maker VX Ace",
                    RPGMakerVersion.MV => "RPG Maker MV",
                    RPGMakerVersion.MZ => "RPG Maker MZ",
                    _ => "Unknown version"
                };
            }
            catch (Exception ex)
            {
                DetectedVersion = RPGMakerVersion.Unknown;
                VersionText = "Detection failed: " + ex.Message;
            }
        }

        private bool CanDecrypt()
        {
            return !IsProcessing && 
                   !string.IsNullOrEmpty(InputPath) && 
                   !string.IsNullOrEmpty(OutputPath) &&
                   DetectedVersion != RPGMakerVersion.Unknown;
        }

        private async Task DecryptAsync()
        {
            IsProcessing = true;
            LogMessages.Clear();
            ProgressValue = 0;
            IsIndeterminate = true;
            
            try
            {
                await Task.Run(() =>
                {
                    switch (DetectedVersion)
                    {
                        case RPGMakerVersion.Xp:
                        case RPGMakerVersion.Vx:
                        case RPGMakerVersion.VxAce:
                            DecryptRGSSAD();
                            break;
                        case RPGMakerVersion.MV:
                        case RPGMakerVersion.MZ:
                            DecryptMVMZ();
                            break;
                    }
                });

                StatusMessage = "Decryption completed successfully!";
                AddLog("Process completed successfully!");
                MessageBox.Show("Decryption completed successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = "Error: " + ex.Message;
                AddLog($"Error: {ex.Message}");
                AddLog($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"An error occurred:\n{ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
                IsIndeterminate = false;
                ProgressValue = 100;
            }
        }

        private void DecryptRGSSAD()
        {
            AddLog($"Starting RGSSAD extraction from: {InputPath}");
            AddLog($"Output directory: {OutputPath}");
            AddLog($"Version: {VersionText}");

            // Create output directory if it doesn't exist
            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
            }

            // Create the appropriate RGSSAD instance based on version
            switch (DetectedVersion)
            {
                case RPGMakerVersion.Xp:
                case RPGMakerVersion.Vx:
                    {
                        using var archive = new RGSSADv1(InputPath);
                        archive.ExtractAllFiles(OutputPath, true);
                        AddLog($"Extracted {archive.ArchivedFiles.Count} files from archive");
                        break;
                    }
                case RPGMakerVersion.VxAce:
                    {
                        using var archive = new RGSSADv3(InputPath);
                        archive.ExtractAllFiles(OutputPath, true);
                        AddLog($"Extracted {archive.ArchivedFiles.Count} files from archive");
                        break;
                    }
            }

            if (RecreateProject)
            {
                AddLog("Creating project structure...");
                ProjectGenerator.GenerateProject(DetectedVersion, OutputPath, true);
                AddLog("Project structure created");
            }
        }

        private void DecryptMVMZ()
        {
            AddLog($"Starting MV/MZ extraction from: {InputPath}");
            AddLog($"Output directory: {OutputPath}");
            AddLog($"Version: {VersionText}");

            // Create output directory if it doesn't exist
            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
            }

            var encryptionKeyFinder = new EncryptionKeyFinder();
            var encryptionKey = encryptionKeyFinder.FindKey(InputPath);
            
            AddLog($"Found encryption key: {BitConverter.ToString(encryptionKey)}");

            // Copy all files first, then decrypt in place
            AddLog("Copying files to output directory...");
            CopyDirectory(InputPath, OutputPath);
            
            StatusMessage = "Decrypting files...";
            
            if (DetectedVersion == RPGMakerVersion.MV)
            {
                var decrypter = new MvDirectoryFilesDecrypter();
                decrypter.DecryptFiles(encryptionKey, OutputPath, false, true);
            }
            else if (DetectedVersion == RPGMakerVersion.MZ)
            {
                var decrypter = new MzDirectoryFilesDecrypter();
                decrypter.DecryptFiles(encryptionKey, OutputPath, false, true);
            }

            AddLog("Files decrypted successfully");

            if (RecreateProject)
            {
                AddLog("Recreating project structure...");
                var projectOutputPath = Path.Combine(OutputPath, "Project");
                
                if (DetectedVersion == RPGMakerVersion.MV)
                {
                    var reconstructor = new MVProjectReconstructor();
                    reconstructor.Reconstruct(OutputPath, projectOutputPath);
                }
                else if (DetectedVersion == RPGMakerVersion.MZ)
                {
                    var reconstructor = new MZProjectReconstructor();
                    reconstructor.Reconstruct(OutputPath, projectOutputPath);
                }
                    
                AddLog($"Project structure recreated at: {projectOutputPath}");
            }
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFilePath = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, destFilePath, true);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                var destDirectoryPath = Path.Combine(destinationDir, Path.GetFileName(directory));
                CopyDirectory(directory, destDirectoryPath);
            }
        }

        private void AddLog(string message)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                LogMessages.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            });
        }

        private void ClearLog()
        {
            LogMessages.Clear();
        }
    }
}