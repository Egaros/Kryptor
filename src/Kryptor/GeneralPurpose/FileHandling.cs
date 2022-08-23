﻿/*
    Kryptor: A simple, modern, and secure encryption and signing tool.
    Copyright (C) 2020-2022 Samuel Lucas

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program. If not, see https://www.gnu.org/licenses/.
*/

using System;
using System.IO;
using System.Linq;
using System.IO.Compression;
using Geralt;

namespace Kryptor;

public static class FileHandling
{
    public static bool IsDirectoryEmpty(string directoryPath) => !Directory.EnumerateFiles(directoryPath, searchPattern: "*", SearchOption.AllDirectories).Any();
    
    public static string TrimTrailingSeparatorChars(string filePath) => filePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar);
    
    public static string ReplaceFileName(string originalFilePath, string newFileName)
    {
        string directoryPath = Path.GetDirectoryName(Path.GetFullPath(originalFilePath));
        string newPath = Path.GetFullPath(Path.Combine(directoryPath, newFileName));
        if (!newPath.StartsWith(Path.GetFullPath(directoryPath))) {
            throw new ArgumentException("Invalid new path.");
        }
        return newPath;
    }
    
    public static bool? IsKryptorFile(string filePath)
    {
        try
        {
            Span<byte> magicBytes = ReadFileHeader(filePath, offset: 0, Constants.EncryptionMagicBytes.Length);
            return ConstantTime.Equals(magicBytes, Constants.EncryptionMagicBytes);
        }
        catch (Exception ex) when (ExceptionFilters.FileAccess(ex))
        {
            return null;
        }
    }
    
    public static bool? IsValidEncryptionVersion(string filePath)
    {
        try
        {
            Span<byte> version = ReadFileHeader(filePath, Constants.EncryptionMagicBytes.Length, Constants.EncryptionVersion.Length);
            return ConstantTime.Equals(version, Constants.EncryptionVersion);
        }
        catch (Exception ex) when (ExceptionFilters.FileAccess(ex))
        {
            return null;
        }
    }
    
    public static bool? IsSignatureFile(string filePath)
    {
        try
        {
            Span<byte> magicBytes = ReadFileHeader(filePath, offset: 0, Constants.SignatureMagicBytes.Length);
            return ConstantTime.Equals(magicBytes, Constants.SignatureMagicBytes);
        }
        catch (Exception ex) when (ExceptionFilters.FileAccess(ex))
        {
            return null;
        }
    }
    
    public static bool? IsValidSignatureVersion(string filePath)
    {
        try
        {
            Span<byte> version = ReadFileHeader(filePath, Constants.SignatureMagicBytes.Length, Constants.SignatureVersion.Length);
            return ConstantTime.Equals(version, Constants.SignatureVersion);
        }
        catch (Exception ex) when (ExceptionFilters.FileAccess(ex))
        {
            return null;
        }
    }
    
    private static Span<byte> ReadFileHeader(string filePath, long offset, int length)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        fileStream.Seek(offset, SeekOrigin.Begin);
        Span<byte> header = new byte[length];
        fileStream.Read(header);
        return header;
    }

    public static void OverwriteFile(string fileToDelete, string fileToCopy)
    {
        try
        {
            File.SetAttributes(fileToDelete, FileAttributes.Normal);
            File.Copy(fileToCopy, fileToDelete, overwrite: true);
            File.Delete(fileToDelete);
        }
        catch (Exception ex) when (ExceptionFilters.FileAccess(ex))
        {
            DisplayMessage.FilePathException(fileToDelete, ex.GetType().Name, "Unable to overwrite the file.");
        }
    }
    
    public static void DeleteFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) {
                return;
            }
            File.SetAttributes(filePath, FileAttributes.Normal);
            File.Delete(filePath);
        }
        catch (Exception ex) when (ExceptionFilters.FileAccess(ex))
        {
            DisplayMessage.FilePathException(filePath, ex.GetType().Name, "Unable to delete the file.");
        }
    }

    private static void DeleteDirectory(string directoryPath)
    {
        try
        {
            if (!Directory.Exists(directoryPath)) {
                return;
            }
            foreach (string filePath in Directory.GetFiles(directoryPath, searchPattern: "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
            }
            Directory.Delete(directoryPath, recursive: true);
        }
        catch (Exception ex) when (ExceptionFilters.FileAccess(ex))
        {
            DisplayMessage.FilePathException(directoryPath, ex.GetType().Name, "Unable to delete the directory.");
        }
    }

    public static string GetUniqueFilePath(string filePath)
    {
        filePath = RemoveFileNameNumber(filePath);
        if (!File.Exists(filePath)) {
            return filePath;
        }
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        int fileNumber = 2;
        string fileExtension = Path.GetExtension(filePath);
        string directoryPath = Path.GetDirectoryName(filePath);
        do
        {
            string newFileName = $"{fileNameWithoutExtension} ({fileNumber}){fileExtension}";
            filePath = Path.Combine(directoryPath, newFileName);
            fileNumber++;
        }
        while (File.Exists(filePath));
        return filePath;
    }

    private static string RemoveFileNameNumber(string filePath)
    {
        if (!filePath.EndsWith(')') || !char.IsDigit(filePath[^2])) {
            return filePath;
        }
        int index = filePath.LastIndexOf(" (", StringComparison.Ordinal);
        return filePath[..index];
    }
    
    public static string RenameFile(string filePath, string newFileName)
    {
        try
        {
            if (string.Equals(newFileName, RemoveFileNameNumber(Path.GetFileName(filePath)))) {
                return filePath;
            }
            string newFilePath = ReplaceFileName(filePath, newFileName);
            newFilePath = GetUniqueFilePath(newFilePath);
            Console.WriteLine($"Renaming \"{Path.GetFileName(filePath)}\" => \"{Path.GetFileName(newFilePath)}\"...");
            File.Move(filePath, newFilePath);
            return newFilePath;
        }
        catch (Exception ex) when (ExceptionFilters.FileAccess(ex))
        {
            DisplayMessage.FilePathException(filePath, ex.GetType().Name, "Unable to restore the original file name.");
            return filePath;
        }
    }

    public static string GetUniqueDirectoryPath(string directoryPath)
    {
        if (!Directory.Exists(directoryPath)) {
            return directoryPath;
        }
        string parentDirectory = Directory.GetParent(directoryPath)?.FullName;
        string directoryName = Path.GetFileName(directoryPath);
        int directoryNumber = 2;
        do
        {
            directoryPath = Path.Combine(parentDirectory ?? string.Empty, $"{directoryName} ({directoryNumber})");
            directoryNumber++;
        }
        while (Directory.Exists(directoryPath));
        return directoryPath;
    }

    public static void SetReadOnly(string filePath)
    {
        try
        {
            File.SetAttributes(filePath, FileAttributes.ReadOnly);
        }
        catch (Exception ex) when (ExceptionFilters.FileAccess(ex))
        {
            DisplayMessage.FilePathException(filePath, ex.GetType().Name, "Unable to mark the file as read-only.");
        }
    }
    
    public static void CreateZipFile(string directoryPath, string zipFilePath)
    {
        DisplayMessage.CreatingZipFile(directoryPath, zipFilePath);
        ZipFile.CreateFromDirectory(directoryPath, zipFilePath, CompressionLevel.NoCompression, includeBaseDirectory: false);
        if (Globals.Overwrite) {
            DeleteDirectory(directoryPath);
        }
    }

    public static void ExtractZipFile(string zipFilePath)
    {
        try
        {
            string directoryPath = GetUniqueDirectoryPath(zipFilePath[..^Path.GetExtension(zipFilePath).Length]);
            DisplayMessage.ExtractingZipFile(zipFilePath, directoryPath);
            ZipFile.ExtractToDirectory(zipFilePath, directoryPath);
            DeleteFile(zipFilePath);
        }
        catch (Exception ex) when (ExceptionFilters.FileAccess(ex))
        {
            DisplayMessage.FilePathException(zipFilePath, ex.GetType().Name, "Unable to extract the file.");
        }
    }
}