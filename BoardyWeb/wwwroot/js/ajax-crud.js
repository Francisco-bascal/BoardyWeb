// Infraestructura AJAX para los ABM: modales con formularios, acciones POST con
// confirmación y refresco de las regiones de la página sin recargarla.
//
// Convenciones de marcado:
//   data-modal-url="/X/Crear"     abre la parcial devuelta por la URL en #modalGlobal.
//   data-accion-url="/X/Eliminar" POST por AJAX (token antiforgery incluido);
//     data-confirmar="..."        pide confirmación antes.
//     data-cierra-modal           cierra el modal si la acción tiene éxito.
//   data-region-url="/X/Y"        región que se vuelve a pedir tras cada acción exitosa;
//                                 la parcial devuelta incluye su propio contenedor.
//   <form data-ajax="true" data-ajax-success="ajaxFormExito" data-ajax-failure="ajaxFormFallo">
(function ($) {
    'use strict';

    const elementoModal = function () { return document.getElementById('modalGlobal'); };
    const contenidoModal = function () { return $('#modalGlobal .modal-content'); };

    function token() {
        return $('#antiforgeryToken input[name="__RequestVerificationToken"]').val();
    }

    function mensajeDeError(xhr) {
        if (xhr.responseJSON && xhr.responseJSON.mensaje) {
            return xhr.responseJSON.mensaje;
        }

        switch (xhr.status) {
            case 0: return 'No se pudo conectar con el servidor.';
            case 401: return 'Tu sesión expiró. Volvé a iniciar sesión.';
            case 403: return 'No tenés permiso para realizar esta acción.';
            case 404: return 'El elemento solicitado ya no existe.';
            default: return 'Ocurrió un error inesperado.';
        }
    }

    function errorAjax(xhr) {
        notificar(mensajeDeError(xhr), 'error');
    }

    function parsearValidacion($contenedor) {
        $contenedor.find('form').each(function () {
            $(this).removeData('validator').removeData('unobtrusiveValidation');
        });
        $.validator.unobtrusive.parse($contenedor);
    }

    function abrirModal(html) {
        contenidoModal().html(html);
        parsearValidacion(contenidoModal());
        bootstrap.Modal.getOrCreateInstance(elementoModal()).show();
    }

    function cerrarModal() {
        const modal = bootstrap.Modal.getInstance(elementoModal());
        if (modal) {
            modal.hide();
        }
        // Se vacía para que las regiones del modal no se refresquen cuando ya no existe su contenido.
        contenidoModal().empty();
    }

    function refrescarRegiones() {
        const peticiones = $('[data-region-url]').map(function () {
            const $region = $(this);
            return $.get($region.data('region-url'))
                .done(function (html) {
                    $region.replaceWith(html);
                    $(document).trigger('regionRefrescada');
                })
                .fail(errorAjax);
        }).get();

        return $.when.apply($, peticiones);
    }

    function exito(respuesta, cerrar) {
        if (cerrar) {
            cerrarModal();
        }
        notificar(respuesta && respuesta.mensaje, 'exito');
        refrescarRegiones();
    }

    // Utilidades para otros scripts (por ejemplo kanban-dnd.js).
    window.ajaxCrud = {
        refrescarRegiones: refrescarRegiones,
        errorAjax: errorAjax,
        token: token
    };

    // Abrir formulario o detalle en el modal.
    $(document).on('click', '[data-modal-url]', function (e) {
        e.preventDefault();
        $.get($(this).data('modal-url')).done(abrirModal).fail(errorAjax);
    });

    // Acción POST por AJAX (eliminar, mover, asignar, etc.).
    $(document).on('click', '[data-accion-url]', async function (e) {
        e.preventDefault();
        const $boton = $(this);
        const mensaje = $boton.data('confirmar');

        if (mensaje && !(await window.confirmar(mensaje))) {
            return;
        }

        $.ajax({
            url: $boton.data('accion-url'),
            type: 'POST',
            headers: { 'RequestVerificationToken': token() }
        })
            .done(function (respuesta) {
                const destino = $boton.data('redirigir');
                if (destino) {
                    window.location.href = destino;
                    return;
                }
                exito(respuesta, $boton.is('[data-cierra-modal]'));
            })
            .fail(errorAjax);
    });

    // Callbacks de jquery-unobtrusive-ajax para formularios (this = formulario).
    window.ajaxFormExito = function (data, status, xhr) {
        const tipo = xhr.getResponseHeader('Content-Type') || '';
        const $form = $(this);

        if (tipo.indexOf('application/json') >= 0) {
            const cerrar = $form.is('[data-cierra-modal]');
            if (!cerrar) {
                $form[0].reset();
            }
            exito(data, cerrar);
            return;
        }

        // El servidor devolvió el formulario con errores de validación.
        const $contenedor = $form.closest('.modal-content');
        if ($contenedor.length) {
            $contenedor.html(data);
            parsearValidacion($contenedor);
        }
    };

    window.ajaxFormFallo = function (xhr) {
        errorAjax(xhr);
    };
})(jQuery);
