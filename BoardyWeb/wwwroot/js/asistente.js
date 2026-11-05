// Chat del asistente. El historial vive solo en la página. Las respuestas se insertan como texto, nunca como HTML.
(function ($) {
    'use strict';

    const historial = [];
    const $panel = $('#asistentePanel');
    const $mensajes = $('#asistenteMensajes');
    const $entrada = $('#asistenteEntrada');
    const $abrir = $('#asistenteAbrir');

    function agregar(texto, clase) {
        const $mensaje = $('<div class="asistente-mensaje"></div>').addClass(clase).text(texto);
        $mensajes.append($mensaje).scrollTop($mensajes[0].scrollHeight);
        return $mensaje;
    }

    function alternar(abierto) {
        $panel.prop('hidden', !abierto);
        $abrir.attr('aria-expanded', abierto);
        (abierto ? $entrada : $abrir).trigger('focus');
    }

    $abrir.on('click', function () { alternar($panel.prop('hidden')); });
    $('#asistenteCerrar').on('click', function () { alternar(false); });

    $('#asistenteFormulario').on('submit', function (e) {
        e.preventDefault();
        const mensaje = $entrada.val().trim();
        if (!mensaje) {
            return;
        }

        $entrada.val('').prop('disabled', true);
        agregar(mensaje, 'asistente-pregunta');
        const $escribiendo = agregar('Pensando...', 'asistente-respuesta');

        $.ajax({
            url: '/Asistente/Preguntar',
            type: 'POST',
            contentType: 'application/json',
            headers: { 'RequestVerificationToken': window.ajaxCrud.token() },
            data: JSON.stringify({ mensaje: mensaje, historial: historial })
        })
            .done(function (respuesta) {
                $escribiendo.text(respuesta.texto);
                historial.push({ esUsuario: true, texto: mensaje }, { esUsuario: false, texto: respuesta.texto });

                if (respuesta.ofrecerReportePdf) {
                    $escribiendo.append($('<a class="btn btn-sm btn-outline-primary d-block mt-2" href="/Panel/ReportePdf"><i class="bi bi-file-earmark-pdf me-1"></i>Descargar PDF</a>'));
                }
            })
            .fail(function (xhr) {
                const texto = xhr.status === 429
                    ? 'Hiciste muchas preguntas seguidas. Esperá un minuto.'
                    : (xhr.responseJSON && xhr.responseJSON.mensaje) || 'El asistente no está disponible en este momento.';
                $escribiendo.text(texto);
            })
            .always(function () {
                $mensajes.scrollTop($mensajes[0].scrollHeight);
                $entrada.prop('disabled', false).trigger('focus');
            });
    });
})(jQuery);
