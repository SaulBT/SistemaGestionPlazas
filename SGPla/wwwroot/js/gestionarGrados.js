const tabla = document.getElementById("contenedorTabla");
const idDocente = document.getElementById("idDocente");

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
    errorGradoAgregar.textContent = "";
    errorGradoAgregar.style.display = "none";

    errorAreaAgregar.textContent = "";
    errorAreaAgregar.style.display = "none";
}

async function agregarGrado(edicion) {
    limpiarErrorresAgregar();
    var datosGrado = {};

    if (verificarCamposAgregar()) {
        if (edicion) {
            datosGrado = {
                "IdDocente": idDocente.value,
                "Grado": gradoAgregar.value,
                "Titulo": areaAgregar.value,
                "Ultimo": ultimoAgregar.checked
            }
        } else {
            datosGrado = {
                "Grado": gradoAgregar.value,
                "Titulo": areaAgregar.value,
                "Ultimo": ultimoAgregar.checked
            }
        }

        const response = await fetch(UrlAgregarGrado, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(datosGrado)
        });

        if (response.ok) {
            const html = await response.text();
            tabla.innerHTML = html;

            cerrarModal("modalAgregarGrado");
        } else {
            console.log("Error");
        }
    }
}

function verificarCamposAgregar() {
    var correcto = true;

    if (!gradoAgregar.value) {
        correcto = false;
        errorGradoAgregar.textContent = "El Grado es obligatorio.";
        errorGradoAgregar.style.display = "block";
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

var idGradoEditar = 0;
var idTemporalEditar = 0;
const gradoEditar = document.getElementById("sfGradoEditar");
const areaEditar = document.getElementById("ifAreaEditar");
const ultimoEditar = document.getElementById("cfUltimoEditar");

const errorGradoEditar = document.getElementById("sfGradoEditar-Error");
const errorAreaEditar = document.getElementById("ifAreaEditar-Error");

function abrirModalEditarGrado(idTemporal, idGrado, grado, titulo, ultimo) {

    limpiarErrorresEditar();

    idGradoEditar = idGrado;
    idTemporalEditar = idTemporal;
    gradoEditar.value = grado;
    areaEditar.value = titulo;
    ultimoEditar.checked = ultimo;

    abrirModal("modalEditarGrado");
}

function limpiarErrorresEditar() {
    errorGradoEditar.textContent = "";
    errorGradoEditar.style.display = "none";

    errorAreaEditar.textContent = "";
    errorAreaEditar.style.display = "none";
}

async function editarGrado(edicion) {
    limpiarErrorresEditar();
    var datosGrado = {};

    if (verificarCamposEditar()) {
        if (edicion) {
            datosGrado = {
                "IdGrado": idGradoEditar,
                "IdDocente": idDocente.value,
                "IdTemporal": idTemporalEditar,
                "Grado": gradoEditar.value,
                "Titulo": areaEditar.value,
                "Ultimo": ultimoEditar.checked
            }
        } else {
            datosGrado = {
                "IdTemporal": idTemporalEditar,
                "Grado": gradoEditar.value,
                "Titulo": areaEditar.value,
                "Ultimo": ultimoEditar.checked
            }
        }

        const response = await fetch(UrlEditarGrado, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(datosGrado)
        });

        if (response.ok) {
            const html = await response.text();
            tabla.innerHTML = html;

            cerrarModal("modalEditarGrado");
        } else {
            console.log("Error");
        }
    }
}

function verificarCamposEditar() {
    var correcto = true;

    if (!gradoEditar.value) {
        correcto = false;
        errorGradoEditar.textContent = "El Grado es obligatorio.";
        errorGradoEditar.style.display = "block";
    }
    if (!areaEditar.value) {
        correcto = false;
        errorAreaAgregar.textContent = "El Area es obligatoria.";
        errorAreaAgregar.style.display = "block";
    }

    return correcto;
}

// ==========
// Eliminar
// ==========
var idGradoEliminar = 0;
var temporalEliminar = false;

function abrirModalEliminarGrado(id, temporal) {
    idGradoEliminar = id;
    temporalEliminar = temporal;
    abrirModal("modalEliminarGrado");
}

async function eliminarGrado(edicion) {
    var url = "";
    if (edicion) 
        url = `${UrlEliminarGrado}?idGrado=${encodeURIComponent(idGradoEliminar)}&temporal=${encodeURIComponent(temporalEliminar)}`;
    else
        url = `${UrlEliminarGrado}?idTemporal=${encodeURIComponent(idGradoEliminar)}`;
    const response = await fetch(url, {
        method: "GET"
    });

    if (response.ok) {
        const html = await response.text();
        tabla.innerHTML = html;

        cerrarModal("modalEliminarGrado");
    } else {
        console.log("Error");
    }
}