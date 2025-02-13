﻿using System;
using System.Linq;

namespace Penguin.Code.Application.ExtensionMethods
{
    public static class StringExtensionMethods
    {
        private static readonly Random _random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}