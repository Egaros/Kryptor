﻿using System.Security.Cryptography;
using Sodium;
using ChaCha20BLAKE2;

/*
    Kryptor: A simple, modern, and secure encryption tool.
    Copyright (C) 2020-2021 Samuel Lucas

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
    public static class PrivateKey
    {
        public static byte[] Encrypt(byte[] passwordBytes, byte[] keyAlgorithm, byte[] privateKey)
        {
            byte[] salt = SodiumCore.GetRandomBytes(Constants.SaltLength);
            byte[] key = KeyDerivation.Argon2id(passwordBytes, salt);
            CryptographicOperations.ZeroMemory(passwordBytes);
            byte[] nonce = SodiumCore.GetRandomBytes(Constants.XChaChaNonceLength);
            byte[] additionalData = Arrays.Concat(keyAlgorithm, Constants.PrivateKeyVersion);
            byte[] encryptedPrivateKey = XChaCha20BLAKE2b.Encrypt(privateKey, nonce, key, additionalData, TagLength.Medium);
            CryptographicOperations.ZeroMemory(privateKey);
            CryptographicOperations.ZeroMemory(key);
            return Arrays.Concat(additionalData, salt, nonce, encryptedPrivateKey);
        }

        public static byte[] Decrypt(byte[] privateKey)
        {
            try
            {
                char[] password = PasswordPrompt.EnterYourPassword();
                byte[] passwordBytes = Password.Prehash(password);
                return Decrypt(passwordBytes, privateKey);
            }
            catch (CryptographicException)
            {
                DisplayMessage.Error("Incorrect password, or the private key has been tampered with.");
                return null;
            }
        }

        private static byte[] Decrypt(byte[] passwordBytes, byte[] privateKey)
        {
            byte[] keyAlgorithm = Arrays.Copy(privateKey, sourceIndex: 0, Constants.Curve25519KeyHeader.Length);
            byte[] keyVersion = Arrays.Copy(privateKey, keyAlgorithm.Length, Constants.PrivateKeyVersion.Length);
            byte[] salt = Arrays.Copy(privateKey, keyAlgorithm.Length + keyVersion.Length, Constants.SaltLength);
            byte[] nonce = Arrays.Copy(privateKey, keyAlgorithm.Length + keyVersion.Length + salt.Length, Constants.XChaChaNonceLength);
            byte[] encryptedPrivateKey = Arrays.Copy(privateKey, keyAlgorithm.Length + keyVersion.Length + salt.Length + nonce.Length, privateKey.Length - (keyAlgorithm.Length + keyVersion.Length + salt.Length + nonce.Length));
            byte[] additionalData = Arrays.Concat(keyAlgorithm, keyVersion);
            byte[] key = KeyDerivation.Argon2id(passwordBytes, salt);
            CryptographicOperations.ZeroMemory(passwordBytes);
            byte[] decryptedPrivateKey = XChaCha20BLAKE2b.Decrypt(encryptedPrivateKey, nonce, key, additionalData, TagLength.Medium);
            CryptographicOperations.ZeroMemory(key);
            return decryptedPrivateKey;
        }
    }
}
