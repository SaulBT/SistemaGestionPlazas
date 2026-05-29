window.configBanderaArchivo = window.configBanderaArchivo || {
    idBandera: 'banderaArchivoDefault'
};


var archivo = null;
const input = document.getElementById("inputFile");
const fileContainer = document.getElementById("fileContainer");
const fileName = document.getElementById("fileName");
const btnCargar = document.getElementById("btnCargar");
const btnConfirmar = document.getElementById("btnConfirmar");

var cambio = false;

function cargarArchivo() {
    input.click();
}

function desplegarArchivo() {

    if (input.files.length > 0) {
        archivo = input.files[0];
        mostrarArchivo();
        cambio = true;
    }
}

function actualizarBandera(archivoCargado) {
    const banderaElement = document.getElementById(window.configBanderaArchivo.idBandera);
    const icono = banderaElement.querySelector('i');
    if (archivoCargado) {
        icono.className = 'bi bi-file-earmark-check-fill';
        icono.style.color = "var(--VerdeSecundario)";
    } else {
        icono.className = 'bi bi-file-earmark-x-fill';
        icono.style.color = "var(--negativo-rojo-letra)";
    }
}

function quitarArchivo() {
    if (archivo != null) {
        ocultarArchivo();
        actualizarBandera(false);
        cambio = true;
    }
}

function confirmar(nombreModal) {
    cambio = false;
    actualizarBandera(true);
    cerrarModal(nombreModal)
}

function cancelar(nombreModal) {
    if (cambio) {
        if (archivo != null) {
            input.value = "";
            archivo = null;
            ocultarArchivo();
        } else {
            archivo = input.files[0];
            mostrarArchivo();
        }
    }

    cambio = false;

    cerrarModal(nombreModal)
}

function mostrarArchivo() {
    fileContainer.style.display = "flex";
    fileName.textContent = archivo.name;
    btnCargar.style.display = "none";
    btnConfirmar.style.display = "inline"
}

function ocultarArchivo() {
    fileContainer.style.display = "none";
    fileName.textContent = "";
    btnCargar.style.display = "inline";
    btnConfirmar.style.display = "none"
}