
/* * * * * * Componentes * * * * * */

// Modal

function abrirModal(id) {
    document.getElementById(id).classList.add("show");
}

function cerrarModal(id) {
    document.getElementById(id).classList.remove("show");
}

// Modal Eliminar
let accionConfirmacion = null;

function abrirModalConfirmacion(mensaje, onConfirm) {

    document.querySelector("#modalEliminar .modal-body p")
        .innerText = mensaje;

    accionConfirmacion = onConfirm;

    abrirModal("modalEliminar");
}

function confirmarModal() {

    if (accionConfirmacion) {
        accionConfirmacion();
    }

    cerrarModal("modalEliminar");
}

function eliminarUsuario(id, rol) {

    document.getElementById("deleteUserId").value = id;
    document.getElementById("deleteUserRol").value = rol;

    document.getElementById("formEliminarUsuario").submit();
}