const periodo = document.getElementById("IdPeriodo");
const articulo = document.getElementById("IdArticulo");
const ofertas = document.getElementById("contenedorOfertas");
const modalidad = document.getElementById("Modalidad");
const lugar = document.getElementById("Lugar");

const horarios = document.getElementById("contenedorHorarios");

let listaHorarios = [];
let indiceHorarioEnEdicion = null;

periodo.addEventListener("change", actualizarOfertas);
articulo.addEventListener("change", actualizarOfertas);
modalidad.addEventListener("change", () => cambiarVisibilidad("contenedorLugar"));


document.addEventListener("DOMContentLoaded", cargarFormulario);

async function cargarFormulario() {
    await actualizarOfertas();
    cambiarVisibilidad("contenedorLugar");
    await cargarHorarios();
    
}

async function actualizarOfertas() {

    const idPeriodo = periodo.value;
    const idArticulo = articulo.value;

    const seleccionadas = [...document.querySelectorAll('input[name="OfertasId"]:checked')]
        .map(input => `ofertasSeleccionadas=${encodeURIComponent(input.value)}`).join("&");
    const response = await fetch(
        `${UrlActualizarOfertas}?idPeriodo=${idPeriodo}&idArticulo=${idArticulo}&${seleccionadas}&version=${VersionOfertas}`,
        { cache: "no-store" }
    );

    const html = await response.text();

    ofertas.innerHTML = html;
}

async function cargarHorarios() {
    const parametroAviso = IdAviso ? `?idAviso=${IdAviso}` : "";
        const response = await fetch(`${UrlObtenerHorarios}${parametroAviso}`);

    if (!response.ok) {
        console.error("No se pudieron obtener los horarios.");
        return;
    }

    listaHorarios = (await response.json()).map(normalizarHorario);

    const responseTabla = await fetch(`${UrlObtenerTablaHorarios}${parametroAviso}`);

    if (!responseTabla.ok) {
        console.error("No se pudo cargar la tabla de horarios.");
        return;
    }

    horarios.innerHTML = await responseTabla.text();
}

async function cambiarVisibilidad(idElemento) {
    const elemento = document.getElementById(idElemento);
    if (modalidad.value == 'Presencial') {
        elemento.style.display = "block";
    }
    else {
        elemento.style.display = "none";
        lugar.value = "";
    }
}

function abrirModalAgregarHorario() {
    indiceHorarioEnEdicion = null;
    limpiarErroresHorario();
    limpiarCamposHorario();
    establecerModoModalHorario("Agregar horario", "Cancelar", "Guardar");
    abrirModal("modalAgregarHorario");
}

function editarHorario(fecha, horaInicio, horaTermino) {
    const index = buscarIndiceHorario(fecha, horaInicio, horaTermino);
    if (index === -1) {
        console.error("No se encontró el horario a editar.");
        return;
    }

    const horario = listaHorarios[index];
    indiceHorarioEnEdicion = index;
    limpiarErroresHorario();
    document.getElementById("Fecha").value = horario.Fecha;
    document.getElementById("HoraInicio").value = horario.HoraInicio;
    document.getElementById("HoraTermino").value = horario.HoraTermino;
    establecerModoModalHorario("Editar horario", "Cancelar cambios", "Guardar");
    abrirModal("modalAgregarHorario");
}

function establecerModoModalHorario(titulo, textoCancelar, textoConfirmar) {
    document.querySelector("#modalAgregarHorario .modal-header h3").textContent = titulo;
    const botones = document.querySelectorAll("#modalAgregarHorario .modal-footer .text-wrapper");
    botones[0].textContent = textoCancelar;
    botones[1].textContent = textoConfirmar;
}

function verPerfilDocenteOferta(perfilDocente) {
    document.getElementById("modalVerPerfilDocente-mensaje").textContent = perfilDocente;
    abrirModal("modalVerPerfilDocente");
}

function abrirModalHorario(oferta) {
    const formatear = (h) => {
        const inicio = h?.Inicio ?? h?.inicio;
        const fin = h?.Fin ?? h?.fin;
        return (inicio && fin)
            ? `${inicio.substring(0, 5)} - ${fin.substring(0, 5)}`
            : "—";
    };

    const nombresDias = [
        "Lunes",
        "Martes",
        "Miércoles",
        "Jueves",
        "Viernes",
        "Sábado"
    ];

    const dias = nombresDias.map(nombre => ({
        nombre: nombre,
        valor: oferta.Horarios?.find(horario => horario.Dia === nombre) ?? null
    }));


    const html = `
    <table class="table table-bordered text-center">
        <thead>
            <tr>
                ${dias.map(d => `<th>${d.nombre}</th>`).join("")}
            </tr>
        </thead>
        <tbody>
            <tr>
                ${dias.map(d => `<td>${d.valor?.Hora ? d.valor.Hora : "N/A"}</td>`).join("")}
            </tr>
        </tbody>
    </table>
`;
    document.getElementById("modalVerHorarioOferta-body").innerHTML = html;
    abrirModal("modalVerHorarioOferta");
}

function limpiarCamposHorario() {
    const fecha = document.getElementById("Fecha");
    const horaInicio = document.getElementById("HoraInicio");
    const horaTermino = document.getElementById("HoraTermino");
    fecha.value = "";
    horaInicio.value = "";
    horaTermino.value = "";
}

function agregarHorario() {

    limpiarErroresHorario();

    let valido = true;

    const fecha = document.getElementById("Fecha");
    const horaInicio = document.getElementById("HoraInicio");
    const horaTermino = document.getElementById("HoraTermino");

    if (!fecha.value) {
        mostrarErrorHorario("Fecha", "La fecha es obligatoria");
        valido = false;
    }

    if (!horaInicio.value) {
        mostrarErrorHorario("HoraInicio", "La hora de inicio es obligatoria");
        valido = false;
    }

    if (!horaTermino.value) {
        mostrarErrorHorario("HoraTermino", "La hora de término es obligatoria");
        valido = false;
    }

    if (!valido) {
        return;
    }

    const datosHorario = {
        "Fecha": fecha.value,
        "HoraInicio": horaInicio.value,
        "HoraTermino": horaTermino.value
    };

    if (indiceHorarioEnEdicion === null) {
        listaHorarios.push(datosHorario);
    } else {
        listaHorarios[indiceHorarioEnEdicion] = datosHorario;
        indiceHorarioEnEdicion = null;
    }

    actualizarHorarios();
    cerrarModal("modalAgregarHorario");

}

async function actualizarHorarios() {

    const url = IdAviso ? `${UrlAgregarHorario}?idAviso=${IdAviso}` : UrlAgregarHorario;
    const response = await fetch(url, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(listaHorarios)
    });

    horarios.innerHTML = await response.text();
}
 
function mostrarErrorHorario(id, mensaje) {

    const input = document.getElementById(id);

    input.classList.add("input-error");

    let error = document.getElementById(`${id}-Error`);

    if (!error) {
        error = document.createElement("span");
        error.id = `${id}-Error`;
        error.className = "input-error-text";
        input.parentNode.appendChild(error);
    }

    error.textContent = mensaje;
}

function limpiarErroresHorario() {

    document
        .querySelectorAll("#formHorario .input-error")
        .forEach(x => x.classList.remove("input-error"));

    document
        .querySelectorAll("#formHorario .input-error-text")
        .forEach(x => x.remove());
}

async function eliminarHorario(fecha, horaInicio, horaTermino) {
    const index = buscarIndiceHorario(fecha, horaInicio, horaTermino);

    if (index === -1) {
        console.error("No se encontró el horario.");
        return;
    }

    listaHorarios.splice(index, 1);

    await actualizarHorarios();
}

function buscarIndiceHorario(fecha, horaInicio, horaTermino) {
    return listaHorarios.findIndex(h =>
        h.Fecha === fecha &&
        h.HoraInicio === horaInicio &&
        h.HoraTermino === horaTermino
    );
}

function normalizarHorario(horario) {
    return {
        Fecha: horario.Fecha ?? horario.fecha,
        HoraInicio: horario.HoraInicio ?? horario.horaInicio,
        HoraTermino: horario.HoraTermino ?? horario.horaTermino
    };
}
