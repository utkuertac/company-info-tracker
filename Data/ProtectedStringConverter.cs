using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CompanyInfoTracker.Data
{
    /// <summary>
    /// Değeri veritabanına yazarken Data Protection ile şifreler, okurken çözer.
    /// </summary>
    public class ProtectedStringConverter : ValueConverter<string?, string?>
    {
        public ProtectedStringConverter(IDataProtector protector)
            : base(
                value => value == null ? null : protector.Protect(value),
                value => value == null ? null : Unprotect(protector, value))
        {
        }

        private static string Unprotect(IDataProtector protector, string value)
        {
            try
            {
                return protector.Unprotect(value);
            }
            catch (CryptographicException)
            {
                // Şifreleme eklenmeden önce düz metin olarak kaydedilmiş eski kayıtlar.
                // Kayıt bir sonraki güncellemede şifreli olarak yazılır.
                return value;
            }
        }
    }
}
