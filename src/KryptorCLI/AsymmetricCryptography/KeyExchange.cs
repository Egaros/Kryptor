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

using Sodium;

namespace KryptorCLI;

public static class KeyExchange
{
    public static byte[] GetSharedSecret(byte[] privateKey, byte[] publicKey) => ScalarMult.Mult(privateKey, publicKey);

    public static byte[] GetPublicKeySharedSecret(byte[] publicKey, out byte[] ephemeralPublicKey)
    {
        using var ephemeralKeyPair = PublicKeyBox.GenerateKeyPair();
        ephemeralPublicKey = ephemeralKeyPair.PublicKey;
        return ScalarMult.Mult(ephemeralKeyPair.PrivateKey, publicKey);
    }

    public static byte[] GetPrivateKeySharedSecret(byte[] privateKey, out byte[] ephemeralPublicKey)
    {
        using var ephemeralKeyPair = PublicKeyBox.GenerateKeyPair();
        ephemeralPublicKey = ephemeralKeyPair.PublicKey;
        return ScalarMult.Mult(privateKey, ephemeralKeyPair.PublicKey);
    }
}