using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace BoardyWeb.Comun
{
    /// <summary>
    /// Asigna mensajes en español a los atributos de validación que no definen su propio
    /// ErrorMessage, para que no aparezcan los textos por defecto en inglés.
    /// </summary>
    public class MensajesValidacionEsProvider : IValidationMetadataProvider
    {
        public void CreateValidationMetadata(ValidationMetadataProviderContext context)
        {
            foreach (var atributo in context.ValidationMetadata.ValidatorMetadata.OfType<ValidationAttribute>())
            {
                if (!string.IsNullOrEmpty(atributo.ErrorMessage) || atributo.ErrorMessageResourceName is not null)
                {
                    continue;
                }

                atributo.ErrorMessage = atributo switch
                {
                    RequiredAttribute => "El campo {0} es obligatorio.",
                    StringLengthAttribute { MinimumLength: > 0 } => "El campo {0} debe tener entre {2} y {1} caracteres.",
                    StringLengthAttribute => "El campo {0} no puede superar los {1} caracteres.",
                    MinLengthAttribute => "El campo {0} debe tener al menos {1} caracteres.",
                    MaxLengthAttribute => "El campo {0} no puede superar los {1} caracteres.",
                    RangeAttribute => "El campo {0} debe estar entre {1} y {2}.",
                    EmailAddressAttribute => "El campo {0} no es un correo electrónico válido.",
                    PhoneAttribute => "El campo {0} no es un teléfono válido.",
                    UrlAttribute => "El campo {0} no es una URL válida.",
                    CompareAttribute => "El campo {0} y {1} no coinciden.",
                    RegularExpressionAttribute => "El campo {0} no tiene un formato válido.",
                    EnumDataTypeAttribute => "El valor seleccionado para {0} no es válido.",
                    _ => atributo.ErrorMessage
                };
            }
        }
    }

    public static class MensajesBindingEs
    {
        /// <summary>Configura los mensajes de enlace de modelos (valores vacíos, tipos inválidos) en español.</summary>
        public static void Aplicar(DefaultModelBindingMessageProvider proveedor)
        {
            proveedor.SetValueMustNotBeNullAccessor(_ => "Este campo es obligatorio.");
            proveedor.SetMissingBindRequiredValueAccessor(campo => $"Falta el valor de '{campo}'.");
            proveedor.SetMissingKeyOrValueAccessor(() => "Se requiere un valor.");
            proveedor.SetMissingRequestBodyRequiredValueAccessor(() => "Se requiere un cuerpo de solicitud.");
            proveedor.SetValueIsInvalidAccessor(valor => $"El valor '{valor}' no es válido.");
            proveedor.SetValueMustBeANumberAccessor(campo => $"El campo {campo} debe ser un número.");
            proveedor.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"El valor '{valor}' no es válido para {campo}.");
            proveedor.SetUnknownValueIsInvalidAccessor(campo => $"El valor ingresado no es válido para {campo}.");
            proveedor.SetNonPropertyAttemptedValueIsInvalidAccessor(valor => $"El valor '{valor}' no es válido.");
            proveedor.SetNonPropertyUnknownValueIsInvalidAccessor(() => "El valor ingresado no es válido.");
            proveedor.SetNonPropertyValueMustBeANumberAccessor(() => "El campo debe ser un número.");
        }
    }
}
