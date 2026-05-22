const codigoAgregar = document.getElementById("CodigoAgregar");
const nombreAgregar = document.getElementById("NombreAgregar");
const perfilDocenteAgregar = document.getElementById("PerfilDocenteAgregar");

const codigoEditar = document.getElementById("CodigoEditar");
const nombreEditar = document.getElementById("NombreEditar");
const perfilDocenteEditar = document.getElementById("PerfilDocenteEditar");

const tbody = document.querySelector("#tabla tbody");

var codigoOriginal = "";
var codigoEliminar = "";
var idExperienciaEducativa = 0;

const errorCodigoAgregar = document.getElementById("CodigoAgregar-Error");
const errorNombreAgregar = document.getElementById("NombreAgregar-Error");
const errorPerfilDocenteAgregar = document.getElementById("PerfilDocenteAgregar-Error");

const errorCodigoEditar = document.getElementById("CodigoEditar-Error");
const errorNombreEditar = document.getElementById("NombreEditar-Error");
const errorPerfilDocenteEditar = document.getElementById("PerfilDocenteEditar-Error");

function abrirModalAgregarExperiencia() {
    limpiarErroresAgregar();
    abrirModal("modalAgregarExperiencia");

    codigoAgregar.value = "";
    nombreAgregar.value = "";
    perfilDocenteAgregar.value = "";
}

function abrirModalEditarExperiencia(codigo, nombre, perfilDocente) {
    codigoOriginal = codigo;
    
    codigoEditar.value = codigo;
    nombreEditar.value = nombre;
    perfilDocenteEditar.value = perfilDocente;

    abrirModal("modalEditarExperiencia");
}

function abrirModalEliminarExperiencia(codigo) {
    codigoEliminar = codigo;

    abrirModal("modalEliminarExperiencia");
}

async function agregarExperiencia() {
    limpiarErroresAgregar();
    if (verificarCamposAgregar(codigoAgregar.value)) {
        const response = await fetch(`${UrlAgregarExperiencia}?codigo=${encodeURIComponent(codigoAgregar.value)}&nombre=${encodeURIComponent(nombreAgregar.value)}&perfilDocente=${encodeURIComponent(perfilDocenteAgregar.value)}`);
        const data = await response.json();

        if (!data.error) {
            const experiencia = data.experiencia;

            const fila = generarFila(experiencia.codigo, experiencia.nombre, experiencia.perfilDocente, 0);
            tbody.appendChild(fila);
            refrescarTablaCliente("tablaExperiencias");

            cerrarModal("modalAgregarExperiencia")
        } else {
            if (edicion) {
                await cancelarAccion();
            } else {
                await regresarPaso1();
            }
        }
    }
}

async function editarExperiencia(edicion) {
    var ruta = "";
    if (verificarCamposEditar(codigoEditar.value)) {
        const response = await fetch(`${UrlEditarExperiencia}?codigo=${encodeURIComponent(codigoEditar.value)}&nombre=${encodeURIComponent(nombreEditar.value)}&perfilDocente=${encodeURIComponent(perfilDocenteEditar.value)}&codigoOriginal=${encodeURIComponent(codigoOriginal)}`);
        const data = await response.json();

        if (!data.error) {
            const experiencias = data.experiencias;

            tbody.innerHTML = "";
            experiencias.forEach(ee => {
                const fila = generarFila(ee.codigo, ee.nombre, ee.perfilDocente);
                tbody.appendChild(fila);
            });
            refrescarTablaCliente("tablaExperiencias");

            cerrarModal("modalEditarExperiencia")
        } else {
            if (edicion) {
                await cancelarAccion();
            } else {
                await regresarPaso1();
            }
        }
    }
}

async function eliminarExperiencia(edicion) {
    const response = await fetch(`${UrlEliminarExperiencia}?codigo=${encodeURIComponent(codigoEliminar)}`);
    const data = await response.json();

    if (!data.error) {
        const experiencias = data.experiencias;

        tbody.innerHTML = "";
        experiencias.forEach(ee => {
            const fila = generarFila(ee.codigo, ee.nombre, ee.perfilDocente);
            tbody.appendChild(fila);
        });
        refrescarTablaCliente("tablaExperiencias");

        cerrarModal("modalEliminarExperiencia")
    } else {
        if (edicion) {
            await cancelarAccion();
        } else {
            await regresarPaso1();
        }
    }

    
}

function generarFila(codigo, nombre, perfilDocente) {
    const fila = document.createElement("tr");
    fila.id = codigo;
    fila.innerHTML = `
            <td>${codigo}</td>
            <td>${nombre}</td>
            <td>
                <div class="table-actions">
                    <button
                        type="button"
                        class="boton-primario boton-icono"
                        onclick="abrirModalPerfilDocente('${perfilDocente}')">
                        <i class="bi bi-info-circle-fill"></i>
                    </button>
                </div>
            </td>
            <td>
                <div class="table-actions">
                    <button
                        type="button"
                        class="boton-primario boton-icono"
                        onclick="abrirModalEditarExperiencia('${codigo}', '${nombre}', '${perfilDocente}')">

                        <i class="bi bi-pencil-fill"></i>
                    </button>
                    <button
                        type="button"
                        class="boton-primario boton-icono"
                        onclick="abrirModalEliminarExperiencia('${codigo}')">
                        <i class="bi bi-trash-fill"></i>
                    </button>
                </div>
            </td>
        `;
    return fila;
}

function verificarCamposAgregar(codigo) {
    var bandera = true;

    if (!codigoAgregar.value) {
        bandera = false;
        errorCodigoAgregar.textContent = "El código es obligatorio.";
        errorCodigoAgregar.style.display = "block";
    }
    if (!nombreAgregar.value) {
        bandera = false;
        errorNombreAgregar.textContent = "El nombre es obligatorio.";
        errorNombreAgregar.style.display = "block";
    }
    if (!perfilDocenteAgregar.value) {
        bandera = false;
        errorPerfilDocenteAgregar.textContent = "El perfil docente es obligatorio.";
        errorPerfilDocenteAgregar.style.display = "block";
    }
    if (document.getElementById(codigo) != null) {
        bandera = false;
        errorCodigoAgregar.textContent = "Ya hay una Experiencia con ese código en el Plan.";
        errorCodigoAgregar.style.display = "block";
    }
    if (codigoAgregar.value && !verificarFormatoCodigo(codigo)) {
        bandera = false;
        errorCodigoAgregar.textContent = "El formato del código es incorrecto. Debe ser 4 letras mayúsculas seguidas de un espacio y 5 dígitos (Ejemplo: ABCD 12345).";
        errorCodigoAgregar.style.display = "block";
    }

    return bandera;
}

function limpiarErroresAgregar() {
    errorCodigoAgregar.textContent = "";
    errorCodigoAgregar.style.display = "none";

    errorNombreAgregar.textContent = "";
    errorNombreAgregar.style.display = "none";

    errorPerfilDocenteAgregar.textContent = "";
    errorPerfilDocenteAgregar.style.display = "none";
}

function verificarCamposEditar(codigo) {
    var bandera = true;

    if (!codigoEditar.value) {
        bandera = false;
        errorCodigoEditar.textContent = "El código es obligatorio.";
    }
    if (!nombreAgregar.value) {
        bandera = false;
        errorNombreEditar.textContent = "El nombre es obligatorio.";
    }
    if (!perfilDocenteAgregar.value) {
        bandera = false;
        errorPerfilDocenteEditar.textContent = "El perfil docente es obligatorio.";
    }
    if (document.getElementById(codigo) != null) {
        bandera = false;
        errorCodigoEditar.textContent = "Ya hay una Experiencia con ese código en el Plan.";
    }
    if (!verificarFormatoCodigo(codigo)) {
        bandera = false;
        errorCodigoEditar.textContent = "El formato del código es incorrecto. Debe ser 4 letras mayúsculas seguidas de un espacio y 5 dígitos (Ejemplo: ABCD 12345).";
        errorCodigoEditar.style.display = "block";
    }

    return bandera;
}

function limpiarErroresEditar() {

    errorCodigoEditar.textContent = "";
    errorCodigoEditar.style.display = "none";

    errorNombreEditar.textContent = "";
    errorNombreEditar.style.display = "none";

    errorPerfilDocenteEditar.textContent = "";
    errorPerfilDocenteEditar.style.display = "none";
}

function verificarFormatoCodigo(codigo) {
    const regex = /^[A-Z]{4} \d{5}$/;

    if (regex.test(codigo)) {
        return true;
    }
    else {
        return false;
    }
}