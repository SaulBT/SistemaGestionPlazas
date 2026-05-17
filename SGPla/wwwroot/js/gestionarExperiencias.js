const codigoAgregar = document.getElementById("CodigoAgregar");
const nombreAgregar = document.getElementById("NombreAgregar");
const perfilDocenteAgregar = document.getElementById("PerfilDocenteAgregar");

const codigoEditar = document.getElementById("CodigoEditar");
const nombreEditar = document.getElementById("NombreEditar");
const perfilDocenteEditar = document.getElementById("PerfilDocenteEditar");

const tbody = document.querySelector("#tabla tbody");
var codigoOriginal = "";
var codigoEliminar = "";

function abrirModalAgregarExperiencia() {
    abrirModal("modalAgregarExperiencia");
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

async function agregarExperienciaCreacion() {
    const response = await fetch(`${UrlAgregarExperiencia}?codigo=${encodeURIComponent(codigoAgregar.value)}&nombre=${encodeURIComponent(nombreAgregar.value)}&perfilDocente=${encodeURIComponent(perfilDocenteAgregar.value)}`);
    const experiencia = await response.json();

    const fila = generarFila(experiencia.codigo, experiencia.nombre, experiencia.perfilDocente);
    tbody.appendChild(fila);

    cerrarModal("modalAgregarExperiencia")
}

async function editarExperienciaCreacion() {
    const response = await fetch(`${UrlEditarExperiencia}?codigo=${encodeURIComponent(codigoEditar.value)}&nombre=${encodeURIComponent(nombreEditar.value)}&perfilDocente=${encodeURIComponent(perfilDocenteEditar.value)}&codigoOriginal=${encodeURIComponent(codigoOriginal)}`);
    const experiencias = await response.json();

    tbody.innerHTML = "";
    experiencias.forEach(ee => {
        const fila = generarFila(ee.codigo, ee.nombre, ee.perfilDocente);
        tbody.appendChild(fila);

        cerrarModal("modalEditarExperiencia")
    });
}

async function eliminarExperienciaCreacion() {
    console.log("Codigo: " + codigoEliminar);
    const response = await fetch(`${UrlEliminarExperiencia}?codigo=${encodeURIComponent(codigoEliminar)}`);
    const experiencias = await response.json();

    tbody.innerHTML = "";
    experiencias.forEach(ee => {
        const fila = generarFila(ee.codigo, ee.nombre, ee.perfilDocente);
        tbody.appendChild(fila);

        cerrarModal("modalEliminarExperiencia")
    });
}

function generarFila(codigo, nombre, perfilDocente) {
    const fila = document.createElement("tr");
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