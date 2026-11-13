using BookStore.Common.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Authentication.Infrastructure.Security
{
    public class JwtKeyManager: IDisposable
    {
        private readonly ECDsa _privateEcdsa;
        public ECDsaSecurityKey PrivateKey { get; }
        public JsonWebKey PublicJwk { get; }
        public string KeyId => "bookstore-auth-key-2026";
        public JwtKeyManager(IWebHostEnvironment env, IConfiguration config)
        {
            var keyPath = SharedConfigurationExtensions.GetSharedFilePath(env.ContentRootPath, "ecdsa_private.pem", config["Jwt:PrivateKeyPath"]);
            _privateEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

            if (File.Exists(keyPath))
            {
                _privateEcdsa.ImportFromPem(File.ReadAllText(keyPath));
            }
            else
            {
                var pem = _privateEcdsa.ExportECPrivateKeyPem();
                File.WriteAllText(keyPath, pem);
            }

            PrivateKey = new ECDsaSecurityKey(_privateEcdsa) { KeyId = KeyId };

            var publicParams = _privateEcdsa.ExportParameters(false);
            using var publicEcdsa = ECDsa.Create(publicParams);
            var publicSecurityKey = new ECDsaSecurityKey(publicEcdsa) { KeyId = KeyId };

            PublicJwk = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(publicSecurityKey);
            PublicJwk.Use = "sig";
            PublicJwk.Alg = SecurityAlgorithms.EcdsaSha256;
            PublicJwk.Kid = KeyId;
        }
        public void Dispose() => _privateEcdsa.Dispose();
    }
}
