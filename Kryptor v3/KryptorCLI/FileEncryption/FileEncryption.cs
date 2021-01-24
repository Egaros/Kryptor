﻿using System;
using System.Text;

/*
    Kryptor: Modern and secure file encryption.
    Copyright(C) 2020 Samuel Lucas

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

namespace KryptorCLI
{
    public static class FileEncryption
    {
        public static void EncryptEachFileWithPassword(string[] filePaths, byte[] passwordBytes)
        {
            Globals.TotalCount = filePaths.Length;
            foreach (string inputFilePath in filePaths)
            {
                bool validFilePath = FilePathValidation.FileEncryption(inputFilePath);
                if (!validFilePath)
                {
                    --Globals.TotalCount;
                    continue;
                }
                UsingPassword(inputFilePath, passwordBytes);
            }
            Utilities.ZeroArray(passwordBytes);
            DisplayMessage.SuccessfullyEncrypted();
        }

        private static void UsingPassword(string inputFilePath, byte[] passwordBytes)
        {
            try
            {
                bool fileIsDirectory = FileHandling.IsDirectory(inputFilePath);
                if (fileIsDirectory)
                {
                    DirectoryEncryption.UsingPassword(inputFilePath, passwordBytes);
                    return;
                }
                // Derive a unique KEK per file
                byte[] salt = Generate.RandomSalt();
                byte[] keyEncryptionKey = Argon2.DeriveKey(passwordBytes, salt);
                // Fill the ephemeral public key header with random bytes (since not in use)
                byte[] randomEphemeralPublicKeyHeader = Generate.RandomEphemeralPublicKeyHeader();
                string outputFilePath = GetOutputFilePath(inputFilePath);
                EncryptFile.Initialize(inputFilePath, outputFilePath, randomEphemeralPublicKeyHeader, salt, keyEncryptionKey);
                EncryptionSuccessful(inputFilePath, outputFilePath);
            }
            catch (Exception ex) when (ExceptionFilters.FileAccess(ex))
            {
                Logging.LogException(ex.ToString(), Logging.Severity.Error);
                DisplayMessage.FilePathException(inputFilePath, ex.GetType().Name, "Unable to encrypt the file.");
            }
        }

        public static void EncryptEachFileWithPublicKey(string[] filePaths, byte[] senderPrivateKey, byte[] recipientPublicKey)
        {
            Globals.TotalCount = filePaths.Length;
            senderPrivateKey = PrivateKey.Decrypt(senderPrivateKey);
            if (senderPrivateKey == null) { return; }
            byte[] sharedSecret = KeyExchange.GetLongTermSharedSecret(senderPrivateKey, recipientPublicKey);
            Utilities.ZeroArray(senderPrivateKey);
            foreach (string inputFilePath in filePaths)
            {
                bool validFilePath = FilePathValidation.FileEncryption(inputFilePath);
                if (!validFilePath)
                {
                    --Globals.TotalCount;
                    continue;
                }
                UsingPublicKey(inputFilePath, sharedSecret, recipientPublicKey);
            }
            Utilities.ZeroArray(sharedSecret);
            DisplayMessage.SuccessfullyEncrypted();
        }

        private static void UsingPublicKey(string inputFilePath, byte[] sharedSecret, byte[] recipientPublicKey)
        {
            try
            {
                bool fileIsDirectory = FileHandling.IsDirectory(inputFilePath);
                if (fileIsDirectory)
                {
                    DirectoryEncryption.UsingPublicKey(inputFilePath, sharedSecret, recipientPublicKey);
                    return;
                }
                // Derive a unique KEK per file
                (byte[] ephemeralSharedSecret, byte[] ephemeralPublicKey) = KeyExchange.GetEphemeralSharedSecret(recipientPublicKey);
                byte[] salt = Generate.RandomSalt();
                byte[] keyEncryptionKey = Generate.KeyEncryptionKey(sharedSecret, ephemeralSharedSecret, salt);
                string outputFilePath = GetOutputFilePath(inputFilePath);
                EncryptFile.Initialize(inputFilePath, outputFilePath, ephemeralPublicKey, salt, keyEncryptionKey);
                EncryptionSuccessful(inputFilePath, outputFilePath);
            }
            catch (Exception ex) when (ExceptionFilters.FileAccess(ex))
            {
                Logging.LogException(ex.ToString(), Logging.Severity.Error);
                DisplayMessage.FilePathException(inputFilePath, ex.GetType().Name, "Unable to encrypt the file.");
            }
        }

        public static string GetOutputFilePath(string inputFilePath)
        {
            try
            {
                if (Globals.ObfuscateFileNames)
                {
                    ObfuscateFileName.AppendFileName(inputFilePath);
                    inputFilePath = ObfuscateFileName.ReplaceFilePath(inputFilePath);
                }
            }
            catch (Exception ex) when (ExceptionFilters.FileAccess(ex) || ex is EncoderFallbackException)
            {
                Logging.LogException(ex.ToString(), Logging.Severity.Error);
                DisplayMessage.FilePathException(inputFilePath, ex.GetType().Name, "Unable to store file name.");
            }
            return inputFilePath + Constants.EncryptedExtension;
        }

        public static void EncryptionSuccessful(string inputFilePath, string outputFilePath)
        {
            DisplayMessage.FileEncryptionResult(inputFilePath, outputFilePath);
            Globals.SuccessfulCount += 1;
        }
    }
}
