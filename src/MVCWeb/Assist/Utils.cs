using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace MVCWeb
{
    public static class Utils
    {
        /// <summary>
        /// Rijndael秘钥16或32个字符
        /// </summary>
        public static string RijndaelKey { get; set; }

        /// <summary>
        /// Rijndael向量16个字符
        /// </summary>
        public static string RijndaelIV { get; set; }

        #region 加解密

        /// <summary>
        /// 秘钥和向量检查
        /// </summary>
        private static void CheckEncryptKeyAndIV()
        {
            if (string.IsNullOrEmpty(RijndaelKey) || string.IsNullOrEmpty(RijndaelIV))
            {
                throw new Exception("未设置属性RijndaelKey和RijndaelIV的值，加解密前需要设置秘钥和向量");
            }
            if ((RijndaelKey.Length != 16 && RijndaelKey.Length != 32) || RijndaelIV.Length != 16)
            {
                throw new Exception("RijndaelKey的长度为16或32个字符，RijndaelIV的长度为16个字符");
            }
        }

        /// <summary>
        /// Rijndael加密
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string RijndaelEncrypt(string value)
        {
            CheckEncryptKeyAndIV();
            byte[] bytesResult;
            using (Rijndael rijAlg = Rijndael.Create())
            {
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(Encoding.ASCII.GetBytes(RijndaelKey), Encoding.ASCII.GetBytes(RijndaelIV));
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(value);
                        }
                        bytesResult = msEncrypt.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(bytesResult);
        }

        /// <summary>
        /// Rijndael解密
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string RijndaelDecrypt(string value)
        {
            CheckEncryptKeyAndIV();
            byte[] bytesValue = Convert.FromBase64String(value);
            string result;
            using (Rijndael rijAlg = Rijndael.Create())
            {
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(Encoding.ASCII.GetBytes(RijndaelKey), Encoding.ASCII.GetBytes(RijndaelIV));
                using (MemoryStream msDecrypt = new MemoryStream(bytesValue))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            result = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return result;
        }

        #endregion
    }
}