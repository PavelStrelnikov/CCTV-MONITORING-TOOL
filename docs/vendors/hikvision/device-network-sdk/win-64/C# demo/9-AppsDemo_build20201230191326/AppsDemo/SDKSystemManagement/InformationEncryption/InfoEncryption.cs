using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SDKSystemManagement.InformationEncryption
{
    class InfoEncryption
    {
        /// <summary>
        /// Generate iv
        /// </summary>
        /// <param name="strInitVector"></param>
        /// <returns></returns>
        public static void GetInitVector(out string strInitVector)
        {
            byte[] szInitVector = new byte[16];
            Random ra = new Random();
            ra.NextBytes(szInitVector);
            byte[] byHexAes = AESEncryption.converByteArrayToCharArray(szInitVector, szInitVector.Length);
            strInitVector = Encoding.UTF8.GetString(byHexAes).ToLower();//This must be lowercase!
        }

        /// <summary>
        /// Calculate sha256 algorithm
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public static string sha256(string strData)
        {
            byte[] szData = Encoding.UTF8.GetBytes(strData);
            byte[] szHash = SHA256Managed.Create().ComputeHash(szData);

            byte[] szSha256 = AESEncryption.converByteArrayToCharArray(szHash, 32);//The standard algorithm is hash. Length, but 32 according to the sample document

            return Encoding.UTF8.GetString(szSha256);
        }

        /// <summary>
        /// Calculate the irreversible sha256 algorithm
        /// </summary>
        /// <param name="strUser"></param>
        /// <param name="szSalt"></param>
        /// <param name="strPassword"></param>
        /// <returns></returns>
        public static string calcSha256(string strUser, byte[] szSalt, string strPassword)
        {
            string strSrcData = strUser;
            byte[] szRealSalt = new byte[64];//The salt value takes up 64 bits
            if (szSalt.Length > 64)
            {
                return null;
            }
            Array.Copy(szSalt, szRealSalt, szSalt.Length);

            strSrcData = strSrcData + Encoding.UTF8.GetString(szRealSalt) + strPassword;

            return sha256(strSrcData);
        }

        /// <summary>
        /// Generate encryption key
        /// </summary>
        /// <param name="strUserName"></param>
        /// <param name="strSalt"></param>
        /// <param name="strPassword"></param>
        /// <param name="szOut"></param>
        /// <param name="iKeyIterateNum"></param>
        /// <param name="bIrreversible"></param>
        /// <returns></returns>
        public static void getEncryptKey(string strUserName, string strSalt, string strPassword, out byte[] szOut, int iKeyIterateNum, bool bIrreversible)
        {
            byte[] szSalt = null;
            if (strSalt != null)
            {
                szSalt = Encoding.UTF8.GetBytes(strSalt);
            }

            string strSrcData = string.Empty;

            if (bIrreversible && szSalt.Length > 0)
            {
                string strIrrPsw = calcSha256(strUserName, szSalt, strPassword);
                if (strIrrPsw.Length > 64)
                {
                    strSrcData = strIrrPsw.Substring(0,64);
                }
                else
                {
                    strSrcData = strIrrPsw;
                }
            }
            else
            {
                if (strPassword.Length > 64)
                {
                    strSrcData = strPassword.Substring(0,64);
                }
                else
                {
                    strSrcData = strPassword;
                }
            }

            strSrcData +="AaBbCcDd1234!@#$";

            if (iKeyIterateNum <= 0)//iterations
            {
                iKeyIterateNum = 100;
            }

            //For the SHA256 iteration, iKeyIterateNum represents the number of iterations returned by the capability set
            for (int i = 0; i < iKeyIterateNum; i++)
            {
                strSrcData = sha256(strSrcData);
            }

            byte[] szSHA256 = Encoding.UTF8.GetBytes(strSrcData);

            //The last time the result of SHA256 is converted to a Byte array
            byte[] szByteArray = AESEncryption.convertCharArrayToByteArray(szSHA256, szSHA256.Length);

            if (szByteArray.Length > 32)
            {
                szOut = new byte[32];
                Array.Copy(szByteArray, szOut, 32);
            }
            else
            {
                szOut = new byte[szByteArray.Length];
                Array.Copy(szByteArray, szOut, szByteArray.Length);
            }
        }

        /// <summary>
        /// Content encryption
        /// </summary>
        /// <param name="strInitVextor"></param>
        /// <param name="szAESKey"></param>
        /// <param name="strSrcContent"></param>
        /// <param name="strOut"></param>
        /// <param name="iSecurityVersion"></param>
        /// <returns></returns>
        public static void getEncryptContent(string strInitVextor, byte[] szAESKey, string strSrcContent, out string strOut, int iSecurityVersion)
        {
            if (iSecurityVersion != 1)//1 is an AES128 algorithm, currently only 1 is supported
            {
                strOut = strSrcContent;
                return;
            }

            //convert to utf8
            byte[] szInitVextor = Encoding.UTF8.GetBytes(strInitVextor);

            byte[] szInitVextorByteArray = AESEncryption.convertCharArrayToByteArray(szInitVextor, szInitVextor.Length);

            byte[] szSrcBytes = Encoding.UTF8.GetBytes(strSrcContent);

            string strSrcBase64 = Convert.ToBase64String(szSrcBytes);
            
            //2、aes
            byte[] szAesData = AESEncryption.AesEncrypt(strSrcBase64, szAESKey, szInitVextorByteArray);

            byte[] szOut = AESEncryption.converByteArrayToCharArray(szAesData, szAesData.Length);

            strOut = Encoding.UTF8.GetString(szOut);
        }

        /// <summary>
        /// Content decryption
        /// </summary>
        /// <param name="strInitVextor"></param>
        /// <param name="szAESKey"></param>
        /// <param name="strSrcContent"></param>
        /// <param name="strOut"></param>
        /// <param name="iSecurityVersion"></param>
        /// <returns></returns>
        public static void getDecryptContent(string strInitVextor, byte[] szAESKey, string strSrcContent, out string strOut, int iSecurityVersion)
        {
            if (iSecurityVersion != 1)//1 is an AES128 algorithm, currently only 1 is supported
            {
                strOut = strSrcContent;
                return;
            }

            //convert to utf8
            byte[] szInitVextor = Encoding.UTF8.GetBytes(strInitVextor);

            byte[] szInitVextorByteArray = AESEncryption.convertCharArrayToByteArray(szInitVextor, szInitVextor.Length);

            byte[] szSrcBytes = Encoding.UTF8.GetBytes(strSrcContent);

            byte[] szSrcByteArray = AESEncryption.convertCharArrayToByteArray(szSrcBytes, szSrcBytes.Length);

            string strAesData = AESEncryption.AesDecrypt(szSrcByteArray, szAESKey, szInitVextorByteArray);

            byte[] szOut = Convert.FromBase64String(strAesData);

            strOut = Encoding.UTF8.GetString(szOut);
        }
    }
}
