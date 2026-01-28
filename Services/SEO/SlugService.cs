using System.Text;
using System.Text.RegularExpressions;

namespace BaseConLogin.Services.Seo
{
    public static class SlugService
    {
        public static string Generar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "";

            texto = texto.ToLowerInvariant();

            texto = QuitarAcentos(texto);

            texto = Regex.Replace(texto, @"[^a-z0-9\s-]", "");

            texto = Regex.Replace(texto, @"\s+", " ").Trim();

            texto = texto.Replace(" ", "-");

            return texto;
        }

        private static string QuitarAcentos(string texto)
        {
            var normalized = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                    != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
