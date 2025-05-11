using BusinessLogicLayer.ServiceContracts;
using System.Security.Cryptography;
using System.Text;

namespace BusinessLogicLayer.Services
{
    public class Sha256ImageHashProvider : IImageHashProvider
    {
        public string ComputeHash(byte[] imageBytes)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(imageBytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
