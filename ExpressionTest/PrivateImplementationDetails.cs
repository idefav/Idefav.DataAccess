using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionTest
{
   public class PrivateImplementationDetails
    {
        public static string ComputeStringHash(string password)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(password);
            SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
            byte[] hash = sha.ComputeHash(buffer);
            StringBuilder passwordbullder = new StringBuilder(32);
            foreach (byte hashByte in hash)
            {
                passwordbullder.Append(hashByte.ToString("x2"));

            }
            return passwordbullder.ToString();
        }
    }
}
