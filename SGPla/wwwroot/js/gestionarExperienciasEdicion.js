const codigoAgregar = document.getElementById("CodigoAgregar");
const nombreAgregar = document.getElementById("NombreAgregar");
const perfilDocenteAgregar = document.getElementById("PerfilDocenteAgregar");

const codigoEditar = document.getElementById("CodigoEditar");
const nombreEditar = document.getElementById("NombreEditar");
const perfilDocenteEditar = document.getElementById("PerfilDocenteEditar");

const archivoInput = document.getElementById("inputFile");
const nuevoArchivo = document.getElementById("nuevoArchivo");

const tbody = document.querySelector("#tabla tbody");
var idExperienciaEducativa = 0;

function abrirModalAgregarExperiencia() {
    abrirModal("modalAgregarExperiencia");
}

function abrirModalEditarExperiencia(codigo, nombre, perfilDocente, idExperiencia) {
    idExperienciaEducativa = idExperiencia;
    codigoEditar.value = codigo;
    nombreEditar.value = nombre;
    perfilDocenteEditar.value = perfilDocente;

    abrirModal("modalEditarExperiencia");
}

function abrirModalEliminarExperiencia(idExperiencia) {
    idExperienciaEducativa = idExperiencia;

    abrirModal("modalEliminarExperiencia");
}

function abrirModalArchivo() {
    abrirModal("modalArchivo");
}

async function agregarExperienciaEdicion() {
    const response = await fetch(`${UrlAgregarExperiencia}?codigo=${encodeURIComponent(codigoAgregar.value)}&nombre=${encodeURIComponent(nombreAgregar.value)}&perfilDocente=${encodeURIComponent(perfilDocenteAgregar.value)}`);
    const experiencia = await response.json();

    const fila = generarFila(experiencia.codigo, experiencia.nombre, experiencia.perfilDocente, experiencia.idExperienciaEducativa);
    tbody.appendChild(fila);

    cerrarModal("modalAgregarExperiencia")
}

async function editarExperienciaCreacion() {
    const response = await fetch(`${UrlEditarExperiencia}?codigo=${encodeURIComponent(codigoEditar.value)}&nombre=${encodeURIComponent(nombreEditar.value)}&perfilDocente=${encodeURIComponent(perfilDocenteEditar.value)}&idExperienciaEducativa=${encodeURIComponent(idExperienciaEducativa)}`);
    const experiencias = await response.json();

    tbody.innerHTML = "";
    experiencias.forEach(ee => {
        const fila = generarFila(ee.codigo, ee.nombre, ee.perfilDocente, ee.idExperienciaEducativa);
        tbody.appendChild(fila);

        cerrarModal("modalEditarExperiencia")
    });
}

async function eliminarExperienciaCreacion() {
    const response = await fetch(`${UrlEliminarExperiencia}?idExperienciaEducativa=${encodeURIComponent(idExperienciaEducativa)}`);
    const experiencias = await response.json();

    tbody.innerHTML = "";
    experiencias.forEach(ee => {
        const fila = generarFila(ee.codigo, ee.nombre, ee.perfilDocente, ee.idExperienciaEducativa);
        tbody.appendChild(fila);

        cerrarModal("modalEliminarExperiencia")
    });
}

async function recargarExperiencias() {
    const formData = new FormData();
    formData.append("archivo", archivoInput.files[0]);

    const response = await fetch(UrlCargarArchivo, {method: "POST", body: formData});
    const experiencias = await response.json();

    tbody.innerHTML = "";
    experiencias.forEach(ee => {
        const fila = generarFila(ee.codigo, ee.nombre, ee.perfilDocente, ee.idExperienciaEducativa);
        tbody.appendChild(fila);

        cerrarModal("modalArchivo")
    });

    nuevoArchivo.value = "true";
}

function generarFila(codigo, nombre, perfilDocente, idExperiencia) {
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
                        onclick="abrirModalEditarExperiencia('${codigo}', '${nombre}', '${perfilDocente}', '${idExperiencia}')">

                        <i class="bi bi-pencil-fill"></i>
                    </button>
                    <button
                        type="button"
                        class="boton-primario boton-icono"
                        onclick="abrirModalEliminarExperiencia('${idExperiencia}')">
                        <i class="bi bi-trash-fill"></i>
                    </button>
                </div>
            </td>
        `;

    return fila;
}