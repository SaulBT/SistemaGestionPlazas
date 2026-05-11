
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



