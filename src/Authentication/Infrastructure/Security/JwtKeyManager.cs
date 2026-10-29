using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Authentication.Infrastructure.Security
{
    public class JwtKeyManager
    {
        public ECDsaSecurityKey PrivateKey { get; }
        public JsonWebKey PublicJwk { get; }
        public string KeyId => "bookstore-auth-key-2026";
        public JwtKeyManager(IWebHostEnvironment env)
        {
            var keyPath = Path.Combine(env.ContentRootPath, "ecdsa_private.pem");
            var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

            if (File.Exists(keyPath))
            {
                ecdsa.ImportFromPem(File.ReadAllText(keyPath));
            }
            else
            {
                var pem = ecdsa.ExportECPrivateKeyPem();
                File.WriteAllText(keyPath, pem);
            }

            PrivateKey = new ECDsaSecurityKey(ecdsa) { KeyId = KeyId };

            var publicParams = ecdsa.ExportParameters(false);
            var pubblicEcdsa = ECDsa.Create(publicParams);
            var publicSecurityKey = new ECDsaSecurityKey(pubblicEcdsa) { KeyId = KeyId };

            PublicJwk = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(publicSecurityKey);
            PublicJwk.Use = "sig";
            PublicJwk.Alg = SecurityAlgorithms.EcdsaSha256;
            PublicJwk.Kid = KeyId;
        }
    }
}
