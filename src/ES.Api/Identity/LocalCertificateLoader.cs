using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using ES.Api;

namespace ES.Api.Identity
{
    // Use this for macOS
    // Cannot load user added certs from macOS System Keychain
    // .NET Core 1.x currently uses openssl and you should be able to load a cert from:
    // /usr/local/etc/openssl/certs
    // I have not been able to get it to work with .NET Core 1.0
    // https://github.com/dotnet/corefx/issues/11182#issuecomment-242763503

    public class LocalCertificateLoader : ICertificateLoader
    {
        private readonly IHostingEnvironment _env;
        private readonly TokenSettings _tokenSettings;

        public LocalCertificateLoader(IHostingEnvironment env, TokenSettings tokenSettings)
        {
            _env = env;
            _tokenSettings = tokenSettings;
        }

        public X509Certificate2 Load()
        {
            var path = Path.Combine(_env.ContentRootPath, _tokenSettings.CertFileName);
            return new X509Certificate2(path, "", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
        }
    }
}
