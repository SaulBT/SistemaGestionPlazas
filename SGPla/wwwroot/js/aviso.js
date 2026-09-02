const periodo = document.getElementById("IdPeriodo");
const articulo = document.getElementById("IdArticulo");
const ofertas = document.getElementById("contenedorOfertas");
const modalidad = document.getElementById("Modalidad");
const lugar = document.getElementById("Lugar");

const horarios = document.getElementById("contenedorHorarios");

let listaHorarios = [];

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

    const response = await fetch(
        `${UrlActualizarOfertas}?idPeriodo=${idPeriodo}&idArticulo=${idArticulo}`
    );

    const html = await response.text();

    ofertas.innerHTML = html;
}

async function cargarHorarios() {
        const response = await fetch(`${UrlObtenerHorarios}`);

    if (!response.ok) {
        console.error("No se pudieron obtener los horarios.");
        return;
    }

    listaHorarios = await response.json();

    const responseTabla = await fetch(`${UrlObtenerTablaHorarios}`);

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
    limpiarErroresHorario();
    limpiarCamposHorario();
    abrirModal("modalAgregarHorario");
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

    var datosHorario = {}
    datosHorario = {
        "Fecha": fecha.value,
        "HoraInicio": horaInicio.value,
        "HoraTermino": horaTermino.value
    }


    listaHorarios.push(datosHorario);

    actualizarHorarios();
    cerrarModal("modalAgregarHorario");

}

async function actualizarHorarios() {

    const response = await fetch(UrlAgregarHorario, {
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
    console.log(listaHorarios);
    console.log(fecha + " | " + horaInicio + " | " + horaTermino);
    const index = listaHorarios.findIndex(h =>
        h.Fecha === fecha &&
        h.HoraInicio === horaInicio &&
        h.HoraTermino === horaTermino
    );

    if (index === -1) {
        console.error("No se encontró el horario.");
        return;
    }

    listaHorarios.splice(index, 1);

    await actualizarHorarios();
}