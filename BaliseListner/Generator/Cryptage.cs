using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
namespace BaliseListner.Generator
{
    class Cryptage
    {
        private static byte[] ConcoxPassWord = { 0x56, 0x32, 0xA1, 0xEF, 0x73, 0x39, 0xB1, 0x5C, 0xD6, 0xAC, 0x66, 0x88, 0xEA, 0x19, 0x75, 0x35 };

        static byte[] Decrypt(byte[] data, byte[] Key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;

                byte[] IV = new byte[aesAlg.BlockSize / 8];

                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.ECB;
                aesAlg.Padding = PaddingMode.Zeros;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                return PerformCryptography(decryptor, data);
            }
        }


        static byte[] Crypt(byte[] data, byte[] Key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;

                byte[] IV = new byte[aesAlg.BlockSize / 8];

                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.ECB;
                aesAlg.Padding = PaddingMode.Zeros;

                byte[] data2 = new byte[16];
                // data.CopyTo(data2,0);
                ICryptoTransform decryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                return PerformCryptography(decryptor, data);
            }
        }

        static byte[] PerformCryptography(ICryptoTransform cryptoTransform, byte[] data)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
        }

        public static byte[] DecrytTrameConcox(byte[] data, byte[] key)
        {
            byte[] dataDecryt = Decrypt(data.ToList().GetRange(4, data.Length - 8).ToArray(), key);

            string s = BitConverter.ToString(dataDecryt.ToList().GetRange(0, 2).ToArray()).Replace("-", "");
            int i = int.Parse(s, System.Globalization.NumberStyles.AllowHexSpecifier);

            int startbit = 1;
            Byte header = 0x80;
            int j = 5;  // j = 2 bit of header + 1 bit of trame length + 2 bit of footer
            if (dataDecryt[2] == 0x94) // ca s ou le type de trame est analogique
            {
                startbit = 0;
                header = 0x79;
                j = 6; // j = 2 bit of header + 2 bit of trame length + 2 bit of footer
            }

            byte[] dataLogin = new byte[i + j];
            dataLogin[0] = header;
            dataLogin[1] = header;
            dataDecryt.ToList().GetRange(startbit, i).ToArray().CopyTo(dataLogin, 2);
            dataLogin[i + j - 4] = data[data.Length - 4];
            dataLogin[i + j - 3] = data[data.Length - 3];
            dataLogin[i + j - 2] = 0x0D;
            dataLogin[i + j - 1] = 0x0A;

            return dataLogin;
        }

        public static byte[] ConcoxCryptedLogineMessageToSend(byte[] data, byte[] key)
        {

            byte[] msgToCrypt = new byte[5];
            msgToCrypt[0] = 0x00;
            msgToCrypt[1] = 0x05;
            msgToCrypt[2] = data[3];
            msgToCrypt[3] = data[17];
            msgToCrypt[4] = data[18];

            byte[] msgToChecksum = new byte[4];

            msgToChecksum[0] = 0x05;
            msgToChecksum[1] = data[3];
            msgToChecksum[2] = data[17];
            msgToChecksum[3] = data[18];
            byte[] sum = BitConverter.GetBytes(Checksum.crc16(Checksum.CRC16_X25, msgToChecksum));
            byte[] msgCrypted = Crypt(msgToCrypt, key);
            int l = msgCrypted.Length + 2;
            byte[] BufferToSend = new byte[l + 6];
            BufferToSend[0] = 0x80;
            BufferToSend[1] = 0x80;
            BufferToSend[2] = 0x0;
            BufferToSend[3] = 0x12;
            msgCrypted.CopyTo(BufferToSend, 4);
            BufferToSend[l + 6 - 4] = sum[1];
            BufferToSend[l + 6 - 3] = sum[0];
            BufferToSend[l + 6 - 2] = 0x0d;
            BufferToSend[l + 6 - 1] = 0x0a;
            return BufferToSend;

        }

        public static byte[] CrypteConcoxReponseMsg(byte[] ConcoxReponse, byte[] key)
        {

            string str = BitConverter.ToString(ConcoxReponse.ToList().GetRange(2, 1).ToArray()).Replace("-", "");
            int LenghtTrame = int.Parse(str, System.Globalization.NumberStyles.AllowHexSpecifier);

            byte[] msgToCrypt = new byte[LenghtTrame];
            msgToCrypt[0] = 0x00;

            ConcoxReponse.ToList().GetRange(2, LenghtTrame - 1).ToArray().CopyTo(msgToCrypt, 1);

            byte[] msgCrypted = Crypt(msgToCrypt, key);
            int l = msgCrypted.Length + 2;
            byte[] BufferToSend = new byte[l + 6];
            BufferToSend[0] = 0x80;
            BufferToSend[1] = 0x80;
            BufferToSend[2] = 0x0;
            BufferToSend[3] = 0x12;
            msgCrypted.CopyTo(BufferToSend, 4);
            BufferToSend[l + 6 - 4] = ConcoxReponse[ConcoxReponse.Length - 4];
            BufferToSend[l + 6 - 3] = ConcoxReponse[ConcoxReponse.Length - 3];
            BufferToSend[l + 6 - 2] = 0x0d;
            BufferToSend[l + 6 - 1] = 0x0a;
            return BufferToSend;
        }
        //public static byte[] getKey()
        //{
        //    return ConcoxPassWord;
        //}


        public static byte[] getKey(string codeimei)
        {
            string keyfilePath = @"keys" + "\\" + codeimei + ".txt";

            string key = "";
            if (!File.Exists(keyfilePath))
            {
                Console.WriteLine("le fichier \"" + codeimei + ".txt\" n'existe pas ");
 throw new Exception("le fichier \"" + codeimei + ".txt\" n'existe pas ");
            }
               

            using (StreamReader f = new StreamReader(keyfilePath))
            {
                if ((key = f.ReadLine()) == null)
                {
                    Console.WriteLine("Clé de cryptage invalide"); throw new Exception("Clé de cryptage invalide");
                }
                   
                return StringToByteArray(key);

            }          
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }


        static string DecryptStringFromBytes_Aes(byte[] cipherTextCombined, byte[] Key)
        {

            string plaintext = null;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;

                byte[] IV = new byte[aesAlg.BlockSize / 8];

                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.ECB;
                aesAlg.Padding = PaddingMode.Zeros;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (var msDecrypt = new MemoryStream(cipherTextCombined))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;
        }

        //static string DecryptAES128(byte[] data, byte[] Key)
        //{
        //    {
        //        using (Aes aesAlg = Aes.Create())
        //        {
        //            aesAlg.Key = Key;
        //            byte[] IV = new byte[aesAlg.BlockSize / 8];
        //            aesAlg.IV = IV;
        //            aesAlg.Mode = CipherMode.ECB;
        //            aesAlg.Padding = PaddingMode.None;
        //            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
        //            return BitConverter.ToString(PerformCryptography(decryptor, data));
        //        }
        //    }
        //}

    }


}
