const tabla = document.getElementById("contenedorTabla");

// ==========
// Agregar
// ==========

const gradoAgregar = document.getElementById("sfGradoAgregar");
const areaAgregar = document.getElementById("ifAreaAgregar");
const ultimoAgregar = document.getElementById("cfUltimoAgregar");

const errorGradoAgregar = document.getElementById("sfGradoAgregar-Error");
const errorAreaAgregar = document.getElementById("ifAreaAgregar-Error");

function abrirModalAgregarGrado() {
    limpiarErrorresAgregar();
    abrirModal("modalAgregarGrado")

    gradoAgregar.value = "Licenciatura";
    areaAgregar.value = "";
    ultimoAgregar.checked = false;
}

function limpiarErrorresAgregar() {
    //errorGradoAgregar.textContent = "";
    //errorGradoAgregar.style.display = "none";

    errorAreaAgregar.textContent = "";
    errorAreaAgregar.style.display = "none";
}

async function agregarGrado() {
    console.log("Si está entrando a la agregación.");
    limpiarErrorresAgregar();
    if (verificarCamposAgregar()) {
        const response = await fetch(`${UrlAgregarGrado}?grado=${encodeURIComponent(gradoAgregar.value)}&titulo=${encodeURIComponent(areaAgregar.value)}&ultimo=${encodeURIComponent(ultimoAgregar.checked)}`, {
            method: "GET"
        });

        if (response.ok) {
            const html = await response.text();
            tabla.innerHTML = html;

            cerrarModal("modalAgregarGrado");
        }
    }
}

function verificarCamposAgregar() {
    var correcto = true;

    if (!gradoAgregar.value) {
        correcto = false;
        //errorGradoAgregar.textContent = "El Grado es obligatorio.";
        //errorGradoAgregar.style.display = "block";
    }
    if (!areaAgregar.value) {
        correcto = false;
        errorAreaAgregar.textContent = "El Area es obligatoria.";
        errorAreaAgregar.style.display = "block";
    }

    return correcto;
}

// ==========
// Editar
// ==========

const idTemporalEditar = document.getElementById("idTemporalEditar")
const gradoEditar = document.getElementById("sfGradoEditar");
const areaEditar = document.getElementById("ifAreaEditar");
const ultimoEditar = document.getElementById("cfUltimoEditar");

const errorGradoEditar = document.getElementById("sfGradoEditar-Error");
const errorAreaEditar = document.getElementById("ifAreaEditar-Error");

function abrirModalEditarGrado(idTemporal, grado, titulo, ultimo) {
    limpiarErrorresEditar();
    abrirModal("modalEditarGrado")

    idTemporalEditar.value = idTemporal;
    gradoEditar.value = grado;
    areaEditar.value = titulo;
    ultimoEditar.checked = ultimo;
}

function limpiarErrorresEditar() {
    //errorGradoEditar.textContent = "";
    //errorGradoEditar.style.display = "none";

    errorAreaEditar.textContent = "";
    errorAreaEditar.style.display = "none";
}

async function editarGrado() {
    console.log("Si está entrando a la edición.");
    limpiarErrorresEditar();
    if (verificarCamposEditar()) {
        const response = await fetch(`${UrlEditarGrado}?idTemporal=${encodeURIComponent(idTemporalEditar.value)}&grado=${encodeURIComponent(gradoEditar.value)}&titulo=${encodeURIComponent(areaEditar.value)}&ultimo=${encodeURIComponent(ultimoEditar.checked)}`, {
            method: "GET"
        });

        if (response.ok) {
            const html = await response.text();
            tabla.innerHTML = html;

            cerrarModal("modalEditarGrado");
        }
    }
}

function verificarCamposEditar() {
    var correcto = true;

    if (!gradoEditar.value) {
        correcto = false;
        //errorGradoAgregar.textContent = "El Grado es obligatorio.";
        //errorGradoAgregar.style.display = "block";
    }
    if (!areaEditar.value) {
        correcto = false;
        errorAreaAgregar.textContent = "El Area es obligatoria.";
        errorAreaAgregar.style.display = "block";
    }

    return correcto;
}