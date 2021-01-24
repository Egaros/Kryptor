﻿using System;
using System.Security.Cryptography;
using Sodium;

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
    public static class HeaderEncryption
    {
        public static byte[] ComputeAdditionalData(long fileLength)
        {
            byte[] fileFormatVersion = FileHeaders.GetFileFormatVersion();
            long chunkCount = Utilities.RoundUp(fileLength, Constants.FileChunkSize);
            long ciphertextLength = chunkCount * Constants.TotalChunkLength;
            byte[] ciphertextSize = BitConverter.GetBytes(ciphertextLength);
            return Utilities.ConcatArrays(fileFormatVersion, ciphertextSize);
        }

        public static byte[] Encrypt(byte[] header, byte[] nonce, byte[] keyEncryptionKey, byte[] additionalData)
        {
            return SecretAeadXChaCha20Poly1305.Encrypt(header, nonce, keyEncryptionKey, additionalData);
        }

        public static byte[] GetAdditionalData(string inputFilePath)
        {
            long fileLength = FileHandling.GetFileLength(inputFilePath);
            byte[] fileFormatVersion = FileHeaders.ReadFileFormatVersion(inputFilePath);
            int headersLength = FileHeaders.GetHeadersLength();
            byte[] ciphertextLength = BitConverter.GetBytes(fileLength - headersLength);
            return Utilities.ConcatArrays(fileFormatVersion, ciphertextLength);
        }

        public static byte[] Decrypt(byte[] encryptedHeader, byte[] nonce, byte[] keyEncryptionKey, byte[] additionalData)
        {
            try
            {
                return SecretAeadXChaCha20Poly1305.Decrypt(encryptedHeader, nonce, keyEncryptionKey, additionalData);
            }
            catch (CryptographicException)
            {
                return null;
            }
        }
    }
}
