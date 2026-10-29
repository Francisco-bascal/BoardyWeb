// ---- Tema diurno / nocturno ----
// El atributo inicial lo fija un script en el <head> del layout (sin parpadeo);
// aquí se maneja el botón y el seguimiento de la preferencia del sistema.
(function () {
    const CLAVE = 'boardy-tema';
    const raiz = document.documentElement;

    function eleccionGuardada() {
        try { return localStorage.getItem(CLAVE); } catch (e) { return null; }
    }

    window.alternarTema = function () {
        const nuevo = raiz.getAttribute('data-bs-theme') === 'dark' ? 'light' : 'dark';
        raiz.setAttribute('data-bs-theme', nuevo);
        try { localStorage.setItem(CLAVE, nuevo); } catch (e) { }
    };

    document.addEventListener('click', function (e) {
        if (e.target.closest('#botonTema')) {
            window.alternarTema();
        }
    });

    // Mientras no haya elección manual, se sigue el tema del sistema.
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function (e) {
        if (!eleccionGuardada()) {
            raiz.setAttribute('data-bs-theme', e.matches ? 'dark' : 'light');
        }
    });
})();

// Los menús dentro del tablero (con scroll interno) se posicionan en modo "fixed"
// para que no queden recortados por el contenedor.
window.iniciarMenusDelTablero = function () {
    document.querySelectorAll('.kanban [data-bs-toggle="dropdown"]').forEach(function (el) {
        bootstrap.Dropdown.getOrCreateInstance(el, {
            popperConfig: function (config) { return Object.assign({}, config, { strategy: 'fixed' }); }
        });
    });
};

document.addEventListener('DOMContentLoaded', window.iniciarMenusDelTablero);
$(document).on('regionRefrescada', window.iniciarMenusDelTablero);

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
