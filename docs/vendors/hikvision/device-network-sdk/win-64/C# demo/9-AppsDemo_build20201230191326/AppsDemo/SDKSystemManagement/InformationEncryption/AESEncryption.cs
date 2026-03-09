using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SDKSystemManagement.InformationEncryption
{
    class AESEncryption
    {
        /// <summary>
        /// Hexadecimal to binary
        /// </summary>
        /// <param name="chstr"></param>
        /// <returns></returns>
        public static byte hexToBinary(byte chstr)
        {
            char crtn = '\0';
            if (('0' <= chstr) && ('9' >= chstr))
            {
                crtn = (char)(chstr & 0x0F);
            }
            else if (('A' <= chstr) && ('F' >= chstr))
            {
                crtn = (char)(chstr - 'A' + 10);
            }
            else if (('a' <= chstr) && ('f' >= chstr))
            {
                crtn = (char)(chstr - 'a' + 10);
            }
            return (byte)crtn;
        }

        /// <summary>
        /// Converts an array of characters into an array of bytes
        /// </summary>
        /// <param name="pSrc"></param>
        /// <param name="nSrcLen"></param>
        /// <returns></returns>
        public static byte[] convertCharArrayToByteArray(byte[] pSrc, int nSrcLen)
        {
            byte[] byChallengeDst2 = new byte[nSrcLen / 2];

            for (int i = 0; i < nSrcLen; i = i + 2)
            {
                byChallengeDst2[i / 2] = (byte)(hexToBinary(pSrc[i]) << 4);
                byChallengeDst2[i / 2] += (byte)hexToBinary(pSrc[i + 1]);
            }

            return byChallengeDst2;
        }

        /// <summary>
        /// Converts an array of bytes to an array of characters
        /// </summary>
        /// <param name="pSrc"></param>
        /// <param name="nSrcLen"></param>
        /// <returns></returns>
        public static byte[] converByteArrayToCharArray(byte[] pSrc, int nSrcLen)
        {
            StringBuilder strB = new StringBuilder();
            for (int i = 0; i < nSrcLen; i++)
            {
                strB.Append(pSrc[i].ToString("x2"));//Here x must be lowercase, which means to convert to lowercase hexadecimal
            }
            return Encoding.UTF8.GetBytes(strB.ToString());
        }

        /// <summary>
        /// Aes CBC Encrypt
        /// </summary>
        /// <param name="strSrcContent"></param>
        /// <param name="szAESKey"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static byte[] AesEncrypt(string strSrcContent, byte[] szAESKey, byte[] iv)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();

            rijndaelCipher.Mode = CipherMode.CBC;

            rijndaelCipher.Padding = PaddingMode.Zeros;//The bottom fill 0

            rijndaelCipher.KeySize = 128;

            rijndaelCipher.BlockSize = 128;

            byte[] keyBytes = new byte[16];

            int len = szAESKey.Length;

            if (len > keyBytes.Length)
            { 
                len = keyBytes.Length; 
            }

            System.Array.Copy(szAESKey, keyBytes, len);

            rijndaelCipher.Key = keyBytes;

            rijndaelCipher.IV = iv;

            ICryptoTransform transform = rijndaelCipher.CreateEncryptor();

            byte[] szSrcContent = Encoding.UTF8.GetBytes(strSrcContent);

            byte[] szDstContent = transform.TransformFinalBlock(szSrcContent, 0, szSrcContent.Length);

            return szDstContent;
        }

        /// <summary>
        /// Aes CBC Decrypt
        /// </summary>
        /// <param name="szSrcContent"></param>
        /// <param name="szAESKey"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string AesDecrypt(byte[] szSrcContent, byte[] szAESKey, byte[] iv)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();

            rijndaelCipher.Mode = CipherMode.CBC;

            rijndaelCipher.Padding = PaddingMode.Zeros;//The bottom fill 0

            rijndaelCipher.KeySize = 128;

            rijndaelCipher.BlockSize = 128;

            byte[] keyBytes = new byte[16];

            int len = szAESKey.Length;

            if (len > keyBytes.Length)
            {
                len = keyBytes.Length;
            }

            System.Array.Copy(szAESKey, keyBytes, len);

            rijndaelCipher.Key = keyBytes;

            rijndaelCipher.IV = iv;

            ICryptoTransform transform = rijndaelCipher.CreateDecryptor();

            byte[] szDstContent = transform.TransformFinalBlock(szSrcContent, 0, szSrcContent.Length);

            //When decrypting to the last 16 bytes of ciphertext, check the data of 16 bytes after decrypting.
            //If the value of the last byte is 16, discard the string of data of 16 bytes. 
            //If the value of the last byte is less than 16, it indicates that the original text was filled during encryption, and discard the last filled part.
            for (int i = 0; i < szDstContent.Length; i++)
            {
                if (szDstContent[i] <= 16)
                {
                    szDstContent[i] = 0;
                }
            }

            //Need to remove the trailing \0
            return Encoding.UTF8.GetString(szDstContent).Replace("\0", "");
        }

    }
}
