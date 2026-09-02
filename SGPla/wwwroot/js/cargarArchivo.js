
const archivos = {};
const cambios = {};

function obtenerElementos(idModal) {
    return {
        input: document.getElementById(`inputFile-${idModal}`),
        fileContainer: document.getElementById(`fileContainer-${idModal}`),
        fileName: document.getElementById(`fileName-${idModal}`),
        btnCargar: document.getElementById(`btnCargar-${idModal}`),
        btnConfirmar: document.getElementById(`btnConfirmar-${idModal}`)
    };
}

function cargarArchivo(idModal) {
    const { input } = obtenerElementos(idModal);

    if (input) {
        input.click();
    }
}

function desplegarArchivo(idModal) {
    const { input } = obtenerElementos(idModal);

    if (input && input.files.length > 0) {
        archivos[idModal] = input.files[0];
        mostrarArchivo(idModal);
        cambios[idModal] = true;
    }
}

function actualizarBandera(idBandera, archivoCargado) {

    // compatibilidad con sistema viejo
    if (!idBandera) {
        const config = window.configBanderaArchivo;
        if (!config || !config.idBandera) return;
        idBandera = config.idBandera;
    }

    const banderaElement = document.getElementById(idBandera);
    if (!banderaElement) return;

    const icono = banderaElement.querySelector("i");
    if (!icono) return;

    icono.className = archivoCargado
        ? "bi bi-file-earmark-check-fill"
        : "bi bi-file-earmark-x-fill";

    icono.style.color = archivoCargado
        ? "var(--VerdeSecundario)"
        : "var(--negativo-rojo-letra)";
}

function quitarArchivo(idModal, idBandera) {
    if (archivos[idModal]) {
        archivos[idModal] = null;
        ocultarArchivo(idModal);
        cambios[idModal] = true;

        actualizarBandera(idBandera, false);
    }
}

function confirmar(idModal, idBandera) {

    cambios[idModal] = false;

    actualizarBandera(idBandera, true);

    cerrarModal(idModal);
}

function cancelar(idModal) {
    const { input } = obtenerElementos(idModal);

    if (cambios[idModal]) {
        if (archivos[idModal]) {
            if (input) {
                input.value = "";
            }

            archivos[idModal] = null;
            ocultarArchivo(idModal);
        }
    }

    cambios[idModal] = false;
    cerrarModal(idModal);
}

function mostrarArchivo(idModal) {
    const {
        fileContainer,
        fileName,
        btnCargar,
        btnConfirmar
    } = obtenerElementos(idModal);

    if (!archivos[idModal]) {
        return;
    }

    fileContainer.style.display = "flex";
    fileName.textContent = archivos[idModal].name;
    btnCargar.style.display = "none";
    btnConfirmar.style.display = "inline";
}

function ocultarArchivo(idModal) {
    const {
        fileContainer,
        fileName,
        btnCargar,
        btnConfirmar
    } = obtenerElementos(idModal);

    fileContainer.style.display = "none";
    fileName.textContent = "";
    btnCargar.style.display = "inline";
    btnConfirmar.style.display = "none";
}