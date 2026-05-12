
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

        window.cambiandoPagina = true;

        const form = document.getElementById('filtrosForm');
        const paginaHidden = document.getElementById('paginaHidden');
        const cantidadHidden = document.getElementById('cantidadHidden');

        if (isPageSizeChange) {
            cantidadHidden.value = page;
            paginaHidden.value = 1;
        } else {
            paginaHidden.value = page;
        }

        form.submit();
    };

    // =========================
    // FILTROS Y PAGINACIÓN GENÉRICA
    // =========================
    document.addEventListener("DOMContentLoaded", function() {

        const form = document.getElementById("filtrosForm");

        if (!form) return;

        const paginaHidden = document.getElementById("paginaHidden");

        // =========================
        // REINICIAR PAGINACIÓN
        // =========================
        function reiniciarPaginacion() {

            if (paginaHidden) {
                paginaHidden.value = 1;
            }
        }

        // =========================
        // SUBMIT GENERAL
        // =========================
        form.addEventListener("submit", function() {

            if (!window.cambiandoPagina) {
                reiniciarPaginacion();
            }

            window.cambiandoPagina = false;
        });

        // =========================
        // SELECTS AUTO-SUBMIT
        // =========================
        const autoSubmitSelects = form.querySelectorAll("[data-autosubmit='true']");

        autoSubmitSelects.forEach(select => {

            select.addEventListener("change", function() {

                reiniciarPaginacion();

                form.submit();
            });

        });

    });

