
/* * * * * * Componentes * * * * * */

// Bootstrap no inicializa los tooltips automáticamente. Inicializamos los
// tooltips declarativos y el toggler, que ya usa data-bs-toggle="collapse".
document.addEventListener("DOMContentLoaded", function () {
    if (!window.bootstrap || !window.bootstrap.Tooltip) return;

    document
        .querySelectorAll('[data-bs-toggle="tooltip"]')
        .forEach(function (element) {
            window.bootstrap.Tooltip.getOrCreateInstance(element, {
                container: "body"
            });
        });

    document
        .querySelectorAll("[data-tooltip]")
        .forEach(function (element) {
            window.bootstrap.Tooltip.getOrCreateInstance(element, {
                container: "body",
                title: element.getAttribute("data-tooltip") || ""
            });
        });
});

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

function togglePassword(inputId, btn) {
    const input = document.getElementById(inputId);
    const icon = btn.querySelector('i');

    const esVisible = input.type === 'text';

    input.type = esVisible ? 'password' : 'text';
    icon.classList.toggle('bi-eye', !esVisible);
    icon.classList.toggle('bi-eye-slash', esVisible);
    btn.setAttribute('aria-label', esVisible ? 'Mostrar contraseña' : 'Ocultar contraseña');
}
