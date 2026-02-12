namespace BaseConLogin.Services.TrabajosImpresion
{
    public interface IImpresionService
    {
        string LimpiarNif(string nif);
        string ObtenerRutaUsuario(string nif);
        Task<bool> ArchivoExisteAsync(string nif, string nombreArchivo);
        Task GuardarArchivoAsync(IFormFile archivo, string rutaDestino);
        void EliminarArchivoFisico(string ruta);
    }
}
