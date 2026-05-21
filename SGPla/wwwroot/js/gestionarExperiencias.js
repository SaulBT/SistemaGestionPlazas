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

function abrirModalEditarExperiencia(edicion, codigo, nombre, perfilDocente, idExperiencia) {
    if (edicion) {
        idExperienciaEducativa = idExperiencia;
    } else {
        codigoOriginal = codigo;
    }
    
    codigoEditar.value = codigo;
    nombreEditar.value = nombre;
    perfilDocenteEditar.value = perfilDocente;

    abrirModal("modalEditarExperiencia");
}

function abrirModalEliminarExperiencia(edicion, identificador) {
    if (edicion) {
        idExperienciaEducativa = identificador;
    } else {
        codigoEliminar = identificador;
    }

    abrirModal("modalEliminarExperiencia");
}

async function agregarExperiencia(edicion) {
    if (verificarCamposAgregar()) {
        const response = await fetch(`${UrlAgregarExperiencia}?codigo=${encodeURIComponent(codigoAgregar.value)}&nombre=${encodeURIComponent(nombreAgregar.value)}&perfilDocente=${encodeURIComponent(perfilDocenteAgregar.value)}`);
        const experiencia = await response.json();

        const fila = generarFila(edicion, experiencia.codigo, experiencia.nombre, experiencia.perfilDocente, 0);
        tbody.appendChild(fila);
        refrescarTablaCliente("tablaExperiencias");

        cerrarModal("modalAgregarExperiencia")
    }
}

async function editarExperiencia(edicion) {
    var ruta = "";
    if (verificarCamposEditar()) {
        if (edicion) {
            ruta = `${UrlEditarExperiencia}?codigo=${encodeURIComponent(codigoEditar.value)}&nombre=${encodeURIComponent(nombreEditar.value)}&perfilDocente=${encodeURIComponent(perfilDocenteEditar.value)}&idExperienciaEducativa=${encodeURIComponent(idExperienciaEducativa)}`
        } else {
            ruta = `${UrlEditarExperiencia}?codigo=${encodeURIComponent(codigoEditar.value)}&nombre=${encodeURIComponent(nombreEditar.value)}&perfilDocente=${encodeURIComponent(perfilDocenteEditar.value)}&codigoOriginal=${encodeURIComponent(codigoOriginal)}`
        }
        const response = await fetch(ruta);
        const experiencias = await response.json();

        tbody.innerHTML = "";
        experiencias.forEach(ee => {
            const fila = generarFila(edicion, ee.codigo, ee.nombre, ee.perfilDocente, ee.idExperienciaEducativa);
            tbody.appendChild(fila);
        });
        refrescarTablaCliente("tablaExperiencias");

        cerrarModal("modalEditarExperiencia")
    }
}

async function eliminarExperiencia(edicion) {
    var ruta = "";
    if (edicion) {
        ruta = `${UrlEliminarExperiencia}?idExperienciaEducativa=${encodeURIComponent(idExperienciaEducativa)}`
    } else {
        ruta = `${UrlEliminarExperiencia}?codigo=${encodeURIComponent(codigoEliminar)}`
    }
    const response = await fetch(ruta);
    const experiencias = await response.json();

    tbody.innerHTML = "";
    experiencias.forEach(ee => {
        const fila = generarFila(edicion, ee.codigo, ee.nombre, ee.perfilDocente, ee.idExperienciaEducativa);
        tbody.appendChild(fila);
    });
    refrescarTablaCliente("tablaExperiencias");

    cerrarModal("modalEliminarExperiencia")
}

function generarFila(edicion, codigo, nombre, perfilDocente, idExperiencia) {
    const fila = document.createElement("tr");
    var contenido = "";

    if (edicion) {
        contenido = `
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
                        onclick="abrirModalEditarExperiencia(true, '${codigo}', '${nombre}', '${perfilDocente}', ${idExperiencia})">

                        <i class="bi bi-pencil-fill"></i>
                    </button>
                    <button
                        type="button"
                        class="boton-primario boton-icono"
                        onclick="abrirModalEliminarExperiencia(true, ${idExperiencia})">
                        <i class="bi bi-trash-fill"></i>
                    </button>
                </div>
            </td>
        `;
    } else {
        contenido = `
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
                        onclick="abrirModalEditarExperiencia(false, '${codigo}', '${nombre}', '${perfilDocente}', 0)">

                        <i class="bi bi-pencil-fill"></i>
                    </button>
                    <button
                        type="button"
                        class="boton-primario boton-icono"
                        onclick="abrirModalEliminarExperiencia(false, '${codigo}')">
                        <i class="bi bi-trash-fill"></i>
                    </button>
                </div>
            </td>
        `;
    }

    fila.innerHTML = contenido
    return fila;
}

function verificarCamposAgregar() {
    const bandera = true;

    if (!codigoAgregar.value)
        bandera = false;
    if (!nombreAgregar.value)
        bandera = false;
    if (!perfilDocenteAgregar.value)
        bandera = false;

    return bandera;
}

function verificarCamposEditar() {
    const bandera = true;

    if (!codigoEditar.value)
        bandera = false;
    if (!nombreEditar.value)
        bandera = false;
    if (!perfilDocenteEditar.value)
        bandera = false;

    return bandera;
}