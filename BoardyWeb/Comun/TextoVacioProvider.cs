using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace BoardyWeb.Comun
{
    /// <summary>
    /// Conserva los textos vacíos como cadena vacía en lugar de convertirlos a null.
    /// Los DTOs usan string no nulo con valor por defecto vacío para los campos opcionales
    /// (por ejemplo Descripcion), y los servicios asumen que nunca llegan null.
    /// La obligatoriedad se exige solo con [Required] explícito.
    /// </summary>
    public class TextoVacioProvider : IDisplayMetadataProvider
    {
        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            // Los campos con DataType (teléfono, correo, URL, contraseña) conservan la conversión a null
            // para que un valor vacío no dispare su validación de formato.
            if (context.Key.ModelType == typeof(string) && !context.Attributes.OfType<DataTypeAttribute>().Any())
            {
                context.DisplayMetadata.ConvertEmptyStringToNull = false;
            }
        }
    }
}
