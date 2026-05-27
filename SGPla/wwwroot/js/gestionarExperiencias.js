const codigoAgregar = document.getElementById("CodigoAgregar");
const nombreAgregar = document.getElementById("NombreAgregar");
const perfilDocenteAgregar = document.getElementById("PerfilDocenteAgregar");
const horasAgregar = document.getElementById("HorasAgregar");
const creditosAgregar = document.getElementById("CreditosAgregar");

const codigoEditar = document.getElementById("CodigoEditar");
const nombreEditar = document.getElementById("NombreEditar");
const perfilDocenteEditar = document.getElementById("PerfilDocenteEditar");
const horasEditar = document.getElementById("HorasEditar");
const creditosEditar = document.getElementById("CreditosEditar");

const tbody = document.querySelector("#tabla tbody");

var codigoOriginal = "";
var codigoEliminar = "";
var idExperienciaEducativa = 0;

const errorCodigoAgregar = document.getElementById("CodigoAgregar-Error");
const errorNombreAgregar = document.getElementById("NombreAgregar-Error");
const errorPerfilDocenteAgregar = document.getElementById("PerfilDocenteAgregar-Error");
const errorHorasAgregar = document.getElementById("HorasAgregar-Error");
const errorCreditosAgregar = document.getElementById("CreditosAgregar-Error");

const errorCodigoEditar = document.getElementById("CodigoEditar-Error");
const errorNombreEditar = document.getElementById("NombreEditar-Error");
const errorPerfilDocenteEditar = document.getElementById("PerfilDocenteEditar-Error");
const errorHorasEditar = document.getElementById("HorasEditar-Error");
const errorCreditosEditar = document.getElementById("CreditosEditar-Error");

function abrirModalAgregarExperiencia() {
    limpiarErroresAgregar();
    abrirModal("modalAgregarExperiencia");

    codigoAgregar.value = "";
    nombreAgregar.value = "";
    perfilDocenteAgregar.value = "";
    horasAgregar.value = "";
    creditosAgregar.value = "";
}

function abrirModalEditarExperiencia(codigo, nombre, horas, creditos, perfilDocente) {
    limpiarErroresEditar();
    codigoOriginal = codigo;
    
    codigoEditar.value = codigo;
    nombreEditar.value = nombre;
    horasEditar.value = horas;
    creditosEditar.value = creditos;
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
        const response = await fetch(`${UrlAgregarExperiencia}?codigo=${encodeURIComponent(codigoAgregar.value)}&nombre=${encodeURIComponent(nombreAgregar.value)}&perfilDocente=${encodeURIComponent(perfilDocenteAgregar.value)}&horas=${encodeURIComponent(horasAgregar.value)}&creditos=${encodeURIComponent(creditosAgregar.value)}`);
        const data = await response.json();

        if (!data.error) {
            const experiencia = data.experiencia;

            const fila = generarFila(experiencia.codigo, experiencia.nombre, experiencia.perfilDocente, experiencia.horas, experiencia.creditos);
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
    limpiarErroresEditar();
    var ruta = "";
    if (verificarCamposEditar(codigoEditar.value)) {
        const response = await fetch(`${UrlEditarExperiencia}?codigo=${encodeURIComponent(codigoEditar.value)}&nombre=${encodeURIComponent(nombreEditar.value)}&perfilDocente=${encodeURIComponent(perfilDocenteEditar.value)}&horas=${encodeURIComponent(horasEditar.value)}&creditos${encodeURIComponent(creditosEditar.value)}&codigoOriginal=${encodeURIComponent(codigoOriginal)}`);
        const data = await response.json();

        if (!data.error) {
            const experiencias = data.experiencias;

            tbody.innerHTML = "";
            experiencias.forEach(ee => {
                const fila = generarFila(ee.codigo, ee.nombre, ee.perfilDocente, ee.horas, ee.creditos);
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
            const fila = generarFila(ee.codigo, ee.nombre, ee.perfilDocente, ee.horas, ee.creditos);
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

function generarFila(codigo, nombre, perfilDocente, horas, creditos) {
    const fila = document.createElement("tr");
    fila.id = codigo;
    fila.innerHTML = `
            <td>${codigo}</td>
            <td>${nombre}</td>
            <td>${horas}</td>
            <td>${creditos}</td>
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
                        onclick="abrirModalEditarExperiencia('${codigo}', '${nombre}', '${horas}', '${creditos}', '${perfilDocente}')">

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
    if (!horasAgregar.value) {
        bandera = false;
        errorHorasAgregar.textContent = "Las horas son obligatorias.";
        errorHorasAgregar.style.display = "block";
    } else {
        const horas = verificarNumero(horasAgregar.value);
        if (horas < 0) {
            bandera = false;
            errorHorasAgregar.textContent = "Tiene que ser un número entero positivo.";
            errorHorasAgregar.style.display = "block";
        }
    }
    if (!creditosAgregar.value) {
        bandera = false;
        errorCreditosAgregar.textContent = "Los créditos son obligatorios.";
        errorCreditosAgregar.style.display = "block";
    } else {
        const creditos = verificarNumero(creditosAgregar.value);
        if (creditos < 0) {
            bandera = false;
            errorCreditosAgregar.textContent = "Tiene que ser un número entero positivo.";
            errorCreditosAgregar.style.display = "block";
        }
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

    errorCreditosAgregar.textContent = "";
    errorCreditosAgregar.style.display = "none";

    errorHorasAgregar.textContent = "";
    errorHorasAgregar.style.display = "none";

    errorPerfilDocenteAgregar.textContent = "";
    errorPerfilDocenteAgregar.style.display = "none";
}

function verificarCamposEditar(codigo) {
    var bandera = true;

    if (!codigoEditar.value) {
        bandera = false;
        errorCodigoEditar.textContent = "El código es obligatorio.";
        errorCodigoEditar.style.display = "block";
    }
    if (!nombreEditar.value) {
        bandera = false;
        errorNombreEditar.textContent = "El nombre es obligatorio.";
        errorNombreEditar.style.display = "block";
    }
    if (!horasEditar.value) {
        bandera = false;
        errorHorasEditar.textContent = "Las horas son obligatorias.";
        errorHorasEditar.style.display = "block";
    } else {
        const horas = verificarNumero(horasEditar.value);
        if (horas < 0) {
            bandera = false;
            errorHorasEditar.textContent = "Tiene que ser un número entero positivo.";
            errorHorasEditar.style.display = "block";
        }
    }
    if (!creditosEditar.value) {
        bandera = false;
        errorCreditosEditar.textContent = "Los créditos son obligatorios.";
        errorCreditosEditar.style.display = "block";
    } else {
        const creditos = verificarNumero(creditosEditar.value);
        if (creditos < 0) {
            bandera = false;
            errorCreditosEditar.textContent = "Tiene que ser un número entero positivo.";
            errorCreditosEditar.style.display = "block";
        }
    }
    if (!perfilDocenteEditar.value) {
        bandera = false;
        errorPerfilDocenteEditar.textContent = "El perfil docente es obligatorio.";
        errorPerfilDocenteEditar
    }
    const elemento = document.getElementById(codigo)
    if (elemento != null && elemento.id != codigoOriginal) {
        bandera = false;
        errorCodigoEditar.textContent = "Ya hay una Experiencia con ese código en el Plan.";
        errorCodigoEditar.style.display = "block";
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

    errorCreditosEditar.textContent = "";
    errorCreditosEditar.style.display = "none";

    errorHorasEditar.textContent = "";
    errorHorasEditar.style.display = "none";

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

function verificarNumero(texto) {
    if (!isNaN(texto) && texto.trim() !== "") {
        const numero = parseInt(texto, 10);
        return numero;
    }
    else {
        return -1;
    }
}