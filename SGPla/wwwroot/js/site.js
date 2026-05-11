
/* * * * * * Componentes * * * * * */

// Modal

function abrirModal(id) {
    document.getElementById(id).classList.add("show");
}

function cerrarModal(id) {
    document.getElementById(id).classList.remove("show");
}

// Modal Confirmacion
let accionConfirmacion = null;

function abrirModalConfirmacion(mensaje, onConfirm) {

    document.querySelector("#modalConfirmacion .modal-body p")
        .innerText = mensaje;

    accionConfirmacion = onConfirm;

    abrirModal("modalConfirmacion");
}

function confirmarModal() {

    if (accionConfirmacion) {
        accionConfirmacion();
    }

    cerrarModal("modalConfirmacion");
}

/* * * * * * * Paginación de Tablas * * * * * */

    // =========================
    // PAGINACIÓN GENÉRICA
    // =========================
    window.cambiarPagina = function(page, isPageSizeChange = false) {

        const form = document.getElementById('filtrosForm');
        const paginaHidden = document.getElementById('paginaHidden');
        const cantidadHidden = document.getElementById('cantidadHidden');

        if (!form || !paginaHidden || !cantidadHidden) {
            console.warn("No se encontraron elementos de paginación.");
            return;
        }

        if (isPageSizeChange) {
            cantidadHidden.value = page;
            paginaHidden.value = 1;
        } else {
            paginaHidden.value = page;
        }

        form.submit();
    };

    window.cambiarTamanioPagina = function(size) {
        cambiarPagina(size, true);
    };

