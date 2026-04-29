using System;
using System.Collections.Generic;
using System.Text;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Runtime.ExceptionServices;

namespace EleonHotel.Data
{
    public static class Argon2PasswordHasher
    {
        // Хэширование пароля с помощью Argon2id

        private const int DegreeOfParallelism = 4; // Сколько "полос" ядер процессора используется для генерации хэша
        private const int MemorySize = 1024 * 64; // Количество памяти(Кб) ОЗУ используемой для генерации хэша
        private const int Iterations = 4; // Количество итераций для генерации хэша
        private const int SaltSize = 32; // Размер соли в байтах
        private const int HashSize = 32; // Размер хэша в байтах

        /// <summary>
        /// Хэширует пароль и возвращает пару (хэш, соль)
        /// </summary>

        public static (byte[] Hash, byte[] Salt) HashPassword(string password)
        {
            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = DegreeOfParallelism,
                MemorySize = MemorySize,
                Iterations = Iterations

            };

            return (argon2.GetBytes(HashSize), salt);
        }

        /// <summary>
        /// Проверяет пароль против сохранённых хэша и соли
        /// </summary>
        /// 

        public static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null || storedHash == null || storedSalt == null)
            {
                return false;
            }

            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = storedSalt,
                MemorySize = MemorySize,
                Iterations = Iterations,
                DegreeOfParallelism = DegreeOfParallelism
            };

            byte[] computedHash = argon2.GetBytes(HashSize);
            return FixedTimeEquals(storedHash, computedHash);
        }

        // Безопасное сравнение байт-массивов (защита от timing-атак)

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int result = 0;
            for(int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }
           
    }
}
