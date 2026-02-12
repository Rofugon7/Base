namespace BaseConLogin.Services.TrabajosImpresion
{
    public class ImpresionService : IImpresionService
    {
        private readonly string _storagePath;

        public ImpresionService(IWebHostEnvironment env)
        {
            // Almacén fuera de wwwroot
            _storagePath = Path.Combine(env.ContentRootPath, "AlmacenSeguro", "Impresiones");
        }

        public string LimpiarNif(string nif) =>
            new string(nif?.Where(char.IsLetterOrDigit).ToArray() ?? Array.Empty<char>()).ToUpper();

        public string ObtenerRutaUsuario(string nif)
        {
            var path = Path.Combine(_storagePath, LimpiarNif(nif));
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        public async Task<bool> ArchivoExisteAsync(string nif, string nombreArchivo) =>
            System.IO.File.Exists(Path.Combine(ObtenerRutaUsuario(nif), nombreArchivo));

        public async Task GuardarArchivoAsync(IFormFile archivo, string rutaDestino)
        {
            using var stream = new FileStream(rutaDestino, FileMode.Create);
            await archivo.CopyToAsync(stream);
        }

        public void EliminarArchivoFisico(string ruta)
        {
            if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
        }
    }
}
