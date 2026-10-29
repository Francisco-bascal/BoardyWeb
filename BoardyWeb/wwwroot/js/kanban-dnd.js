// Arrastrar y soltar del tablero kanban (SortableJS).
//   - Tarjetas: entre listas y dentro de una lista  -> Tareas/Mover
//   - Columnas: reordenar las listas del tablero    -> ListasTarea/Reordenar
//
// El arrastre nunca se bloquea: cada movimiento queda donde se soltó y se envía al servidor
// en una cola (en orden). El tablero solo se vuelve a pedir cuando algo falló o cuando la cola
// quedó vacía y no hay un arrastre en curso, para corregir cualquier desfase de orden.
(function ($) {
    'use strict';

    const ESPERA_RESINCRONIZACION_MS = 1000;

    let instancias = [];
    let cola = Promise.resolve();
    let pendientes = 0;
    let huboError = false;
    let temporizador = null;

    function destruir() {
        instancias.forEach(function (instancia) { instancia.destroy(); });
        instancias = [];
    }

    function actualizarContadores() {
        $('.kanban-columna').each(function () {
            $(this).find('.kanban-columna-titulo .chip').text($(this).find('.kanban-tarjeta').length);
        });
    }

    function hayArrastreEnCurso() {
        return typeof Sortable !== 'undefined' && Sortable.active !== null && Sortable.active !== undefined;
    }

    function programarResincronizacion() {
        clearTimeout(temporizador);
        temporizador = setTimeout(intentarResincronizar, huboError ? 0 : ESPERA_RESINCRONIZACION_MS);
    }

    function intentarResincronizar() {
        if (pendientes > 0 || hayArrastreEnCurso()) {
            temporizador = setTimeout(intentarResincronizar, 400);
            return;
        }

        huboError = false;
        window.ajaxCrud.refrescarRegiones();
    }

    function encolar(url, parametros) {
        pendientes++;

        cola = cola.then(function () {
            return new Promise(function (resolver) {
                $.ajax({
                    url: url + '?' + $.param(parametros),
                    type: 'POST',
                    headers: { 'RequestVerificationToken': window.ajaxCrud.token() }
                })
                    .done(function () { resolver(true); })
                    .fail(function (xhr) {
                        window.ajaxCrud.errorAjax(xhr);
                        resolver(false);
                    });
            });
        }).then(function (correcto) {
            pendientes--;
            if (!correcto) {
                huboError = true;
            }
            programarResincronizacion();
        });
    }

    // Sortable informa dos índices: newIndex/oldIndex cuentan TODOS los hijos del contenedor (incluido el
    // aviso "Sin tareas"), mientras que newDraggableIndex/oldDraggableIndex cuentan solo las tarjetas.
    // El servidor ordena únicamente tarjetas, por lo que siempre se usan los índices "draggable".
    function posicionNueva(evento) {
        return evento.newDraggableIndex !== undefined ? evento.newDraggableIndex : evento.newIndex;
    }

    function posicionAnterior(evento) {
        return evento.oldDraggableIndex !== undefined ? evento.oldDraggableIndex : evento.oldIndex;
    }

    function alSoltarTarea(evento) {
        const listaDestino = $(evento.to).data('lista-id');
        const listaOrigen = $(evento.from).data('lista-id');

        if (listaDestino === listaOrigen && posicionNueva(evento) === posicionAnterior(evento)) {
            return;
        }

        actualizarContadores();
        encolar($(evento.item).data('url-mover'), {
            ListaTareaId: listaDestino,
            NuevoOrden: posicionNueva(evento) + 1
        });
    }

    function alSoltarLista(evento) {
        if (posicionNueva(evento) === posicionAnterior(evento)) {
            return;
        }

        encolar($(evento.item).data('url-reordenar'), { NuevoOrden: posicionNueva(evento) + 1 });
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
                    filter: 'a, button, .dropdown, .dropdown-menu, .tarjeta-fija',
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
