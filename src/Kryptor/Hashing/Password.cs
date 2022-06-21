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

using System.Text;
using System.Security.Cryptography;
using Sodium;

namespace Kryptor;

public static class Password
{
    public static char[] GetNewPassword(char[] password)
    {
        if (password.Length == 0) { return PasswordPrompt.EnterNewPassword(); }
        if (password.Length == 1 && Arrays.Compare(password, new[] { ' ' })) { return PasswordPrompt.UseRandomPassphrase(); }
        return password;
    }
    
    public static byte[] Prehash(char[] password, string keyfilePath = null)
    {
        if (password.Length == 0 && string.IsNullOrEmpty(keyfilePath)) { return null; }
        var passwordBytes = password.Length != 0 ? Encoding.UTF8.GetBytes(password) : null;
        Arrays.ZeroMemory(password);
        var keyfileBytes = !string.IsNullOrEmpty(keyfilePath) ? Keyfiles.ReadKeyfile(keyfilePath) : null;
        if (passwordBytes == null) { return keyfileBytes; }
        passwordBytes = GenericHash.Hash(passwordBytes, key: keyfileBytes, Constants.HashLength);
        CryptographicOperations.ZeroMemory(keyfileBytes);
        return passwordBytes;
    }
}