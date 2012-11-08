// Assistly C# Multipass Example
// Contributed by Bryan Siders: http://sypher-news.blogspot.com/2011/03/assistly-multipass-sso.html
// Note: This example was contributed from a third party and has not been tested at Assistly

using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace Sypher.Multipass
{
    public partial class _Login : System.Web.UI.Page
    {
        protected class UserData
        {
            public string uid;
            public string expires;
            public string customer_email;
            public string customer_name;
            public string customer_custom_siteid;    // custom fields
            public string customer_custom_sitename;    // add your own as needed
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            UserData user_data = new UserData();

            // Set these values however you need
            user_data.uid = "12345678-0000-0000-0000-123456789012";
            user_data.expires = DateTime.UtcNow.AddMinutes(2).ToString("yyyy-MM-ddTHH:mm:sszzz"); // ISO 8601 like 2011-12-29T10:25:28-08:00
            user_data.customer_email = "testing@testing.com";
            user_data.customer_name = "Test User";
            user_data.customer_custom_siteid = "01234567-0000-0000-0000-890123456789";
            user_data.customer_custom_sitename = "Test Organization";

            string assistly_url = string.Format("http://{0}.assistly.com/customer/authentication/multipass/callback?multipass={1}",
                ConfigurationManager.AppSettings["assistly-site-key"], // See encryptUserData for appSettings example
                encryptUserData(user_data));

            Response.Redirect(assistly_url);
        }

        protected static string encryptUserData(UserData user_data)
        {
            // Encode the data into a JSON object
            JavaScriptSerializer s = new JavaScriptSerializer();
            string json_data = s.Serialize(user_data);

            // Example of web.config configuration/appSettings section:
            // <add key="assistly-site-key" value="sitename" /> <!-- from sitename.assistly.com -->
            // <add key="assistly-api-key" value="0123456789abcdef0123456789abcdef" />
            // <add key="assistly-iv" value="OpenSSL for Ruby" /> <!-- Static value from Assistly -->
            string site_key = ConfigurationManager.AppSettings["assistly-site-key"];
            string api_key = ConfigurationManager.AppSettings["assistly-api-key"];
            string iv = ConfigurationManager.AppSettings["assistly-iv"];

            // Using byte arrays now instead of strings
            byte[] encrypted = null;
            byte[] bIV = Encoding.ASCII.GetBytes(iv);
            byte[] data = Encoding.ASCII.GetBytes(json_data);

            // XOR the first block (16 bytes) 
            // once before the full XOR
            // so it gets double XORed
            for (var i = 0; i < 16; i++)
                data[i] = (byte)(data[i] ^ bIV[i]);

            // Use the AesManaged object to do the encryption
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.IV = bIV;
                aesAlg.KeySize = 16 * 8; // = 128-bit, originally defaulted to 256

                // Create the 16-byte salted hash
                SHA1 sha1 = SHA1.Create();
                byte[] saltedHash = sha1.ComputeHash(Encoding.ASCII.GetBytes(api_key + site_key), 0, (api_key + site_key).Length);
                Array.Resize(ref saltedHash, 16);
                aesAlg.Key = saltedHash;

                // Encrypt using the AES Managed object
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(data, 0, data.Length);
                        csEncrypt.FlushFinalBlock();
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }

            // Return the Base64-encoded encrypted data
            return Convert.ToBase64String(encrypted, Base64FormattingOptions.None)
                .TrimEnd("=".ToCharArray())    // Remove trailing equal (=) characters
                .Replace("+", "-")            // Change any plus (+) characters to dashes (-)
                .Replace("/", "_");            // Change any slashes (/) characters to underscores (_)
        }
    }
}