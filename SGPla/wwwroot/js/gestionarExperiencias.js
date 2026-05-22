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

function abrirModalAgregarExperiencia() {
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

    if (!codigoAgregar.value)
        bandera = false;
    if (!nombreAgregar.value)
        bandera = false;
    if (!perfilDocenteAgregar.value)
        bandera = false;
    if (document.getElementById(codigo) != null)
        bandera = false;

    return bandera;
}

function verificarCamposEditar(codigo) {
    var bandera = true;

    if (!codigoEditar.value)
        bandera = false;
    if (!nombreEditar.value)
        bandera = false;
    if (!perfilDocenteEditar.value)
        bandera = false;
    if (document.getElementById(codigo) != null)
        bandera = false;

    return bandera;
}