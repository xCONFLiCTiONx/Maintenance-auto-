﻿using Shell32;
using System;
using System.IO;

namespace Maintenance
{
    internal class BrokenShortcuts
    {
        static string programs = Environment.GetFolderPath(Environment.SpecialFolder.Programs), commonPrograms = Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms);

        internal static void Remove()
        {
            try
            {
                string[] directories = { Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), Environment.GetFolderPath(Environment.SpecialFolder.Programs), Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms) };

                foreach (string directory in directories)
                {
                    try
                    {
                        EasyLogger.Info("Searching " + directory + " for broken shortcuts...");

                        var files = Directory.EnumerateFiles(directory, "*.lnk", SearchOption.AllDirectories);

                        foreach (var file in files)
                        {
                            RemoveBrokenShortcuts(file);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        continue;
                    }
                    catch (PathTooLongException)
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);
            }

            try
            {
                string[] directories = { Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), Environment.GetFolderPath(Environment.SpecialFolder.Programs), Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms) };
                foreach (string directory in directories)
                {
                    try
                    {
                        DeleteEmptySubdirectories(directory);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);
            }
        }

        private static void RemoveBrokenShortcuts(string shortcut)
        {
            try
            {
                Shell shell = new Shell();

                string shortcut_path = shortcut.Substring(0, shortcut.LastIndexOf("\\"));
                string shortcut_name = shortcut.Substring(shortcut.LastIndexOf("\\") + 1);

                if (shortcut_name.EndsWith(".lnk"))
                {
                    Folder shortcut_folder = shell.NameSpace(shortcut_path);

                    FolderItem folder_item = shortcut_folder.Items().Item(shortcut_name);

                    try
                    {
                        ShellLinkObject lnk = (ShellLinkObject)folder_item.GetLink;
                        string path = lnk.Path;
                        if (!string.IsNullOrEmpty(path))
                        {
                            try
                            {
                                if (!FileExistance.FileExists(path))
                                {
                                    EasyLogger.Info("Deleting broken shortcut: " + shortcut + ". The target doesn't exits: " + path);
                                    try
                                    {
                                        File.Delete(shortcut);
                                    }
                                    catch { /* ignore */ }
                                    try
                                    {
                                        Directory.Delete(shortcut);
                                    }
                                    catch { /* ignore */ }
                                }
                            }
                            catch { /* ignore */ }
                        }
                    }
                    catch { /* ignore */ }
                }
            }
            catch (Exception ex)
            {
                EasyLogger.Error(ex);
            }
        }

        private static void DeleteEmptySubdirectories(string parentDirectory)
        {
            foreach (string directory in Directory.GetDirectories(parentDirectory))
            {
                if (directory != Environment.GetFolderPath(Environment.SpecialFolder.Startup) && directory != Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup) && directory != programs + "\\Administrative Tools" && directory != commonPrograms + "\\Administrative Tools" && directory != programs + "\\Windows Administrative Tools" && directory != commonPrograms + "\\Windows Administrative Tools" && directory != programs + "\\Windows System" && directory != commonPrograms + "\\Windows System" && directory != programs + "\\Windows Accessories" && directory != commonPrograms + "\\Windows Accessories" && directory != programs + "\\Windows Ease of Access" && directory != commonPrograms + "\\Windows Ease of Access" && directory != programs + "\\Games" && directory != commonPrograms + "\\Games")
                {
                    try
                    {
                        DeleteEmptySubdirectories(directory);
                    }
                    catch { continue; }

                    try
                    {
                        int totalDirectories = 0;
                        foreach (string dir in Directory.GetDirectories(directory))
                        {
                            totalDirectories++;
                        }
                        if (totalDirectories == 0)
                        {
                            int totalFiles = 0;
                            foreach (string file in Directory.GetFiles(directory))
                            {
                                if (Path.GetFileName(file.ToLower()) != "desktop.ini" && Path.GetFileName(file.ToLower()) != "thumbs.db")
                                {
                                    totalFiles++;
                                }
                            }
                            if (totalFiles == 0)
                            {
                                foreach (string file in Directory.GetFiles(directory))
                                {
                                    if (Path.GetFileName(file.ToLower()) == "desktop.ini" || Path.GetFileName(file.ToLower()) == "thumbs.db")
                                    {
                                        File.Delete(file);
                                    }
                                }
                                EasyLogger.Info("Removing empty directory: " + directory);
                                try
                                {
                                    Directory.Delete(directory);
                                }
                                catch (Exception ex)
                                {
                                    EasyLogger.Error(ex);
                                }
                            }
                        }
                    }
                    catch { continue; }
                }
            }
        }
    }
}
