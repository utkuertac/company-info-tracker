namespace CompanyInfoTracker
{
    /// <summary>
    /// Proje klasöründeki .env dosyasını (varsa) ortam değişkeni olarak yükler.
    /// Zaten tanımlı olan ortam değişkenlerinin üzerine yazmaz.
    /// "SeedUsers__AdminPassword" gibi çift alt çizgili adlar yapılandırmada "SeedUsers:AdminPassword" olarak okunur.
    /// </summary>
    public static class DotEnv
    {
        public static void Load(string path)
        {
            if (!File.Exists(path))
                return;

            foreach (var rawLine in File.ReadAllLines(path))
            {
                var line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith('#'))
                    continue;

                var separator = line.IndexOf('=');
                if (separator <= 0)
                    continue;

                var key = line[..separator].Trim();
                var value = line[(separator + 1)..].Trim().Trim('"', '\'');

                if (Environment.GetEnvironmentVariable(key) == null)
                    Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}
