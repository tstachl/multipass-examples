using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Web;
using System.Diagnostics;
using Newtonsoft.Json;

namespace ConsoleApplication1
{
    class Program
    {
        public const string site_key = "your_site_name";
        private const string api_key = "your_api_key";

        static byte[] Encrypt(string json, byte[] Key, byte[] IV)
        {
            byte[] encrypted;

            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(json);
                        }

                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return encrypted;
        }

        static byte[] EncryptionKey()
        {
            byte[] key;
            byte[] salt = Encoding.UTF8.GetBytes(api_key + site_key);

            using (SHA1 sha1 = new SHA1CryptoServiceProvider())
            {
                key = sha1.ComputeHash(salt);
                Array.Resize(ref key, 16);
            }

            return key;
        }

        static byte[] Signature(string multipass)
        {
            byte[] signature;

            using (HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(api_key)))
            {
                using (MemoryStream msHmac = new MemoryStream(Encoding.UTF8.GetBytes(multipass)))
                {
                    signature = hmac.ComputeHash(msHmac);
                }
            }

            return signature;
        }

        static void Main(string[] args)
        {
            try
            {
                Debug.WriteLine("== Generating ==");

                Debug.WriteLine("    Build json data");
                var json = JsonConvert.SerializeObject(new Dictionary<string, string>(){
                    {"uid", "19238333"},
                    {"expires", DateTime.UtcNow.AddMinutes(10).ToString("o")},
                    {"customer_email", "john@example.com"},
                    {"customer_name", "John"}
                });
                Debug.WriteLine("    Data: {0}", json);

                using (AesManaged myAes = new AesManaged())
                {
                    byte[] encrypted = Encrypt(json, EncryptionKey(), myAes.IV);

                    Debug.WriteLine("    Prepend the IV to the encrypted data");
                    byte[] combined = new byte[myAes.IV.Length + encrypted.Length];
                    Array.Copy(myAes.IV, 0, combined, 0, myAes.IV.Length);
                    Array.Copy(encrypted, 0, combined, myAes.IV.Length, encrypted.Length);

                    Debug.WriteLine("    Base64 encode the encrypted data");
                    var multipass = Convert.ToBase64String(combined);

                    Debug.WriteLine("    Build an HMAC-SHA1 signature using the encoded string and your api key");
                    byte[] encrypted_signature = Signature(multipass);
                    var signature = Convert.ToBase64String(encrypted_signature);

                    Debug.WriteLine("    Finally, URL encode the multipass and signature");
                    multipass = Uri.EscapeDataString(multipass);
                    signature = Uri.EscapeDataString(signature);

                    Debug.WriteLine("== Finished ==");
                    Debug.WriteLine("https://{0}.desk.com/customer/authentication/multipass/callback?multipass={1}&signature={2}", site_key, multipass, signature);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception {0} \n{1}", e.Message, e.StackTrace);
            }
        }
    }
}
