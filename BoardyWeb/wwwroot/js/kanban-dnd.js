// Arrastrar y soltar del tablero kanban (SortableJS).
//   - Tarjetas: entre listas y dentro de una lista  -> Tareas/Mover
//   - Columnas: reordenar las listas del tablero    -> ListasTarea/Reordenar
// La tarjeta queda donde se suelta; tras la respuesta se vuelven a pedir las regiones, lo que
// resincroniza contadores y orden y, si el servidor rechazó el cambio, restaura el estado real.
(function ($) {
    'use strict';

    let instancias = [];

    function destruir() {
        instancias.forEach(function (instancia) { instancia.destroy(); });
        instancias = [];
    }

    function bloquear(bloqueado) {
        instancias.forEach(function (instancia) { instancia.option('disabled', bloqueado); });
    }

    function enviar(url, parametros) {
        bloquear(true);

        return $.ajax({
            url: url + '?' + $.param(parametros),
            type: 'POST',
            headers: { 'RequestVerificationToken': window.ajaxCrud.token() }
        })
            .fail(window.ajaxCrud.errorAjax)
            .always(function () {
                window.ajaxCrud.refrescarRegiones().always(function () { bloquear(false); });
            });
    }

    function alSoltarTarea(evento) {
        const listaDestino = $(evento.to).data('lista-id');
        const listaOrigen = $(evento.from).data('lista-id');

        if (listaDestino === listaOrigen && evento.newIndex === evento.oldIndex) {
            return;
        }

        enviar($(evento.item).data('url-mover'), {
            ListaTareaId: listaDestino,
            NuevoOrden: evento.newIndex + 1
        });
    }

    function alSoltarLista(evento) {
        if (evento.newIndex === evento.oldIndex) {
            return;
        }

        enviar($(evento.item).data('url-reordenar'), { NuevoOrden: evento.newIndex + 1 });
    }

    function inicializar() {
        destruir();

        const tablero = document.querySelector('.kanban');
        if (!tablero || typeof Sortable === 'undefined') {
            return;
        }

        if (tablero.dataset.puedeColaborar === 'true') {
            tablero.querySelectorAll('.kanban-tareas').forEach(function (contenedor) {
                instancias.push(Sortable.create(contenedor, {
                    group: 'tareas',
                    animation: 150,
                    draggable: '.kanban-tarjeta',
                    filter: 'a, button, .dropdown, .dropdown-menu',
                    preventOnFilter: false,
                    delay: 150,
                    delayOnTouchOnly: true,
                    onEnd: alSoltarTarea
                }));
            });
        }

        if (tablero.dataset.puedeAdministrar === 'true') {
            instancias.push(Sortable.create(tablero, {
                animation: 150,
                draggable: '.kanban-columna',
                handle: '.kanban-columna-asa',
                onEnd: alSoltarLista
            }));
        }
    }

    window.kanbanDnd = { inicializar: inicializar, alSoltarTarea: alSoltarTarea, alSoltarLista: alSoltarLista };

    $(inicializar);
    $(document).on('regionRefrescada', inicializar);
})(jQuery);
