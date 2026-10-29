// Punto único de mensajes al usuario. Para usar SweetAlert2 más adelante
// alcanza con reemplazar el cuerpo de estas dos funciones.

/** Muestra un aviso breve. tipo: 'exito' | 'error' | 'info'. */
window.notificar = function (mensaje, tipo) {
    if (!mensaje) {
        return;
    }

    const color = tipo === 'error' ? 'danger' : tipo === 'info' ? 'primary' : 'success';
    const $toast = $(
        '<div class="toast align-items-center text-bg-' + color + ' border-0" role="alert" aria-live="assertive" aria-atomic="true">' +
        '<div class="d-flex"><div class="toast-body"></div>' +
        '<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Cerrar"></button></div></div>');

    $toast.find('.toast-body').text(mensaje);
    $('#contenedorToasts').append($toast);

    const toast = new bootstrap.Toast($toast[0], { delay: 4000 });
    $toast[0].addEventListener('hidden.bs.toast', function () { $toast.remove(); });
    toast.show();
};

/** Pide confirmación. Devuelve una promesa que resuelve true si el usuario acepta. */
window.confirmar = function (mensaje) {
    return new Promise(function (resolver) {
        const elemento = document.getElementById('modalConfirmar');
        const modal = bootstrap.Modal.getOrCreateInstance(elemento);
        let aceptado = false;

        $('#modalConfirmarMensaje').text(mensaje);

        $('#modalConfirmarAceptar').off('click').one('click', function () {
            aceptado = true;
            modal.hide();
        });

        $(elemento).off('hidden.bs.modal.confirmar').one('hidden.bs.modal.confirmar', function () {
            resolver(aceptado);
        });

        modal.show();
    });
};
