using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tool.SecurityTools
{
    using System.Security.Cryptography;
    using System.Text;

    public static class CryptoHelper
    {
        public static string ComputeSHA256Hash(string input)
        {
            // ایجاد یک نمونه از الگوریتم SHA256
            using (SHA256 sha256 = SHA256.Create())
            {
                if (sha256 == null)
                {
                    throw new InvalidOperationException("Failed to create SHA256 algorithm instance");
                }

                // تبدیل رشته ورودی به بایت‌آرایه
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);

                // محاسبه هش
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                // تبدیل هش به رشته HEX
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2")); // فرمت هگزادسیمال با دو رقم
                }

                return builder.ToString();
            }
        }

        public static byte[] ComputeSHA256Hash(byte[] input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                if (sha256 == null)
                {
                    throw new InvalidOperationException("Failed to create SHA256 algorithm instance");
                }

                return sha256.ComputeHash(input);
            }
        }
    }
}
