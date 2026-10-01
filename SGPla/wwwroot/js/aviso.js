const periodo = document.getElementById("IdPeriodo");
const articulo = document.getElementById("IdArticulo");
const ofertas = document.getElementById("contenedorOfertas");
const modalidad = document.getElementById("Modalidad");
const lugar = document.getElementById("Lugar");

const horarios = document.getElementById("contenedorHorarios");

let listaHorarios = [];
let indiceHorarioEnEdicion = null;
let horarioPendienteEliminar = null;
let ofertaPendienteExcluir = null;
let colaOfertasPendientesExcluir = [];
const inputOfertasExcluidas = document.getElementById("OfertasExcluidasJson");
const motivosOfertasExcluidas = new Map(
    leerOfertasExcluidas()
        .map(exclusion => [String(exclusion.IdOferta ?? exclusion.idOferta), exclusion.Motivo ?? exclusion.motivo])
);

periodo.addEventListener("change", () => actualizarOfertas(true));
articulo.addEventListener("change", () => actualizarOfertas(true));
modalidad.addEventListener("change", () => cambiarVisibilidad("contenedorLugar"));
document.getElementById("FechaPublicacion")?.addEventListener("change", actualizarMinimoFechaRecepcion);
document.getElementById("FechaCT")?.addEventListener("change", () => {
    actualizarMinimoFechaRecepcion();
    validarFechaConsejoTecnico();
});


document.addEventListener("DOMContentLoaded", cargarFormulario);

document.addEventListener("change", (event) => {
    if (event.target.id === "seleccionarTodasLasOfertas") {
        const casillas = [...document.querySelectorAll('input[name="OfertasId"]')];
        if (event.target.checked) {
            casillas.forEach(checkbox => {
                checkbox.checked = true;
                motivosOfertasExcluidas.delete(checkbox.value);
            });
            guardarMotivosExclusion();
        } else {
            event.target.checked = true;
            colaOfertasPendientesExcluir = casillas.filter(checkbox => checkbox.checked);
            solicitarSiguienteExclusion();
        }
        sincronizarSeleccionGlobalOfertas();
        return;
    }

    if (event.target.matches('input[name="OfertasId"]')) {
        if (!event.target.checked) {
            event.target.checked = true;
            solicitarExclusionOferta(event.target);
        } else {
            motivosOfertasExcluidas.delete(event.target.value);
            guardarMotivosExclusion();
        }
        sincronizarSeleccionGlobalOfertas();
    }
});

async function cargarFormulario() {
    document.querySelector("#modalMotivoExclusion .modal-close")
        ?.addEventListener("click", cancelarExclusionOferta);
    await actualizarOfertas();
    cambiarVisibilidad("contenedorLugar");
    await cargarHorarios();
    actualizarMinimoFechaRecepcion();
}

function leerOfertasExcluidas() {
    try {
        const exclusiones = JSON.parse(inputOfertasExcluidas?.value || "[]");
        return Array.isArray(exclusiones) ? exclusiones : [];
    } catch {
        return [];
    }
}

async function actualizarOfertas(reiniciarSeleccion = false) {

    const idPeriodo = periodo.value;
    const idArticulo = articulo.value;

    const casillasAnteriores = [...document.querySelectorAll('input[name="OfertasId"]')];
    const esCargaInicialNueva = IdAviso === null && casillasAnteriores.length === 0 && motivosOfertasExcluidas.size === 0;
    if (reiniciarSeleccion) {
        motivosOfertasExcluidas.clear();
        guardarMotivosExclusion();
    }
    const seleccionadas = (reiniciarSeleccion ? [] : casillasAnteriores.filter(input => input.checked))
        .map(input => `ofertasSeleccionadas=${encodeURIComponent(input.value)}`).join("&");
    const response = await fetch(
        `${UrlActualizarOfertas}?idPeriodo=${idPeriodo}&idArticulo=${idArticulo}&${seleccionadas}&version=${VersionOfertas}`,
        { cache: "no-store" }
    );

    const html = await response.text();

    ofertas.innerHTML = html;
    if (reiniciarSeleccion || esCargaInicialNueva) {
        document.querySelectorAll('input[name="OfertasId"]').forEach(checkbox => checkbox.checked = true);
    }
    inicializarTooltipsDeOfertas();
    sincronizarSeleccionGlobalOfertas();
}

function solicitarExclusionOferta(checkbox) {
    ofertaPendienteExcluir = checkbox;
    const nombre = checkbox.dataset.ee || "la experiencia educativa";
    const nrc = checkbox.dataset.nrc ? ` (NRC ${checkbox.dataset.nrc})` : "";
    document.getElementById("modalMotivoExclusion-mensaje").textContent =
        `Indique el motivo por el cual se quitará ${nombre}${nrc} del aviso.`;
    const motivo = document.getElementById("modalMotivoExclusion-motivo");
    motivo.value = motivosOfertasExcluidas.get(checkbox.value) || "";
    limpiarErrorMotivoExclusion();
    abrirModal("modalMotivoExclusion");
    motivo.focus();
}

function confirmarExclusionOferta() {
    if (!ofertaPendienteExcluir) return;
    const motivo = document.getElementById("modalMotivoExclusion-motivo");
    const valor = motivo.value.trim();
    if (!valor) {
        motivo.classList.add("input-error");
        let error = document.getElementById("MotivoExclusion-Error");
        if (!error) {
            error = document.createElement("span");
            error.id = "MotivoExclusion-Error";
            error.className = "input-error-text";
            motivo.insertAdjacentElement("afterend", error);
        }
        error.textContent = "El motivo es obligatorio.";
        return;
    }

    ofertaPendienteExcluir.checked = false;
    motivosOfertasExcluidas.set(ofertaPendienteExcluir.value, valor);
    ofertaPendienteExcluir = null;
    guardarMotivosExclusion();
    cerrarModal("modalMotivoExclusion");
    sincronizarSeleccionGlobalOfertas();
    solicitarSiguienteExclusion();
}

function cancelarExclusionOferta() {
    ofertaPendienteExcluir = null;
    colaOfertasPendientesExcluir = [];
    limpiarErrorMotivoExclusion();
    sincronizarSeleccionGlobalOfertas();
}

function solicitarSiguienteExclusion() {
    if (ofertaPendienteExcluir) return;
    const siguiente = colaOfertasPendientesExcluir.shift();
    if (siguiente) solicitarExclusionOferta(siguiente);
}

function limpiarErrorMotivoExclusion() {
    document.getElementById("modalMotivoExclusion-motivo")?.classList.remove("input-error");
    document.getElementById("MotivoExclusion-Error")?.remove();
}

function guardarMotivosExclusion() {
    if (!inputOfertasExcluidas) return;
    inputOfertasExcluidas.value = JSON.stringify([...motivosOfertasExcluidas].map(([IdOferta, Motivo]) => ({
        IdOferta: Number(IdOferta),
        Motivo
    })));
}

function inicializarTooltipsDeOfertas() {
    if (!window.bootstrap || !window.bootstrap.Tooltip) {
        return;
    }

    // Las ofertas se insertan después de DOMContentLoaded, por lo que no las
    // alcanza el inicializador general de site.js. Limitamos la inicialización
    // a los dos botones de ojo de este flujo; otros iconos dinámicos
    // permanecen sin tooltip.
    ofertas
        .querySelectorAll(
            '[aria-label="Ver horario"], ' +
            '[aria-label="Ver perfil"]'
        )
        .forEach((boton) => {
            window.bootstrap.Tooltip.getOrCreateInstance(boton, {
                container: "body"
            });
        });
}

function sincronizarSeleccionGlobalOfertas() {
    const selectorGlobal = document.getElementById("seleccionarTodasLasOfertas");
    const casillasOfertas = [...document.querySelectorAll('input[name="OfertasId"]')];

    if (!selectorGlobal) {
        return;
    }

    selectorGlobal.checked = casillasOfertas.length > 0 && casillasOfertas.every(checkbox => checkbox.checked);
    selectorGlobal.indeterminate = casillasOfertas.some(checkbox => checkbox.checked) && !selectorGlobal.checked;
}

async function cargarHorarios() {
    const parametroAviso = IdAviso ? `?idAviso=${IdAviso}` : "";
        const response = await fetch(`${UrlObtenerHorarios}${parametroAviso}`);

    if (!response.ok) {
        console.error("No se pudieron obtener los horarios.");
        return;
    }

    listaHorarios = (await response.json()).map(normalizarHorario);
    actualizarMinimoFechaRecepcion();

    const responseTabla = await fetch(`${UrlObtenerTablaHorarios}${parametroAviso}`);

    if (!responseTabla.ok) {
        console.error("No se pudo cargar la tabla de horarios.");
        return;
    }

    horarios.innerHTML = await responseTabla.text();
    inicializarTooltipsDeHorarios();
}

function inicializarTooltipsDeHorarios() {
    if (!window.bootstrap || !window.bootstrap.Tooltip) {
        return;
    }

    horarios
        .querySelectorAll(
            '[aria-label="Editar horario"], ' +
            '[aria-label="Eliminar horario"]'
        )
        .forEach((boton) => {
            window.bootstrap.Tooltip.getOrCreateInstance(boton, {
                container: "body"
            });
        });
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
    const modalHorario = document.getElementById("modalAgregarHorario");
    if (!modalHorario) {
        console.error("No se encontró el modal para agregar horarios.");
        return;
    }

    // Mostrar primero el diálogo: la preparación de sus controles no debe
    // impedir que el botón responda si cambia la estructura interna del modal.
    abrirModal("modalAgregarHorario");
    indiceHorarioEnEdicion = null;
    limpiarErroresHorario();
    limpiarCamposHorario();
    actualizarMinimoFechaRecepcion();
    establecerModoModalHorario("Agregar horario", "Cancelar", "Guardar");
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
    actualizarMinimoFechaRecepcion();
    establecerModoModalHorario("Editar horario", "Cancelar cambios", "Guardar");
    abrirModal("modalAgregarHorario");
}

function establecerModoModalHorario(titulo, textoCancelar, textoConfirmar) {
    const encabezado = document.querySelector("#modalAgregarHorario .modal-header h3");
    if (encabezado) encabezado.textContent = titulo;
    const botones = document.querySelectorAll("#modalAgregarHorario .modal-footer .text-wrapper");
    if (botones[0]) botones[0].textContent = textoCancelar;
    if (botones[1]) botones[1].textContent = textoConfirmar;
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
    if (fecha) fecha.value = "";
    if (horaInicio) horaInicio.value = "";
    if (horaTermino) horaTermino.value = "";
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

    if (fecha.value && !esFechaRecepcionValida(fecha.value)) {
        mostrarErrorHorario("Fecha", mensajeFechaRecepcionInvalida());
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


    if (horaInicio.value && horaTermino.value && horaInicio.value >= horaTermino.value) {
        mostrarErrorHorario("HoraTermino", "La hora de término debe ser posterior a la hora de inicio");
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
    actualizarMinimoFechaRecepcion();
    cerrarModal("modalAgregarHorario");

}

async function aplicarHorarioSugerido() {
    limpiarErroresHorario();
    const fecha = document.getElementById("Fecha");
    if (!fecha.value) {
        mostrarErrorHorario("Fecha", "Seleccione primero una fecha");
        return;
    }
    if (!esFechaRecepcionValida(fecha.value)) {
        mostrarErrorHorario("Fecha", mensajeFechaRecepcionInvalida());
        return;
    }

    const sugeridos = [
        { Fecha: fecha.value, HoraInicio: "10:00", HoraTermino: "14:00" },
        { Fecha: fecha.value, HoraInicio: "17:00", HoraTermino: "19:00" }
    ];
    sugeridos.forEach(sugerido => {
        if (buscarIndiceHorario(sugerido.Fecha, sugerido.HoraInicio, sugerido.HoraTermino) === -1) {
            listaHorarios.push(sugerido);
        }
    });
    await actualizarHorarios();
    actualizarMinimoFechaRecepcion();
    cerrarModal("modalAgregarHorario");
}

function actualizarMinimoFechaRecepcion() {
    const fechaRecepcion = document.getElementById("Fecha");
    const fechaConsejo = document.getElementById("FechaCT");
    const hoy = new Date();
    hoy.setHours(0, 0, 0, 0);
    const publicacion = document.getElementById("FechaPublicacion")?.value || "";
    let minimo = hoy;
    if (publicacion) {
        const posteriorPublicacion = new Date(`${publicacion}T00:00:00`);
        posteriorPublicacion.setDate(posteriorPublicacion.getDate() + 1);
        if (posteriorPublicacion > minimo) minimo = posteriorPublicacion;
    }
    if (fechaRecepcion) {
        fechaRecepcion.min = formatearFechaIsoLocal(minimo);
        fechaRecepcion.max = fechaConsejo?.value || "";
    }

    if (fechaConsejo) {
        const ultimaRecepcion = obtenerUltimaFechaRecepcion();
        fechaConsejo.min = ultimaRecepcion && ultimaRecepcion > minimo
            ? formatearFechaIsoLocal(ultimaRecepcion)
            : formatearFechaIsoLocal(minimo);
        validarFechaConsejoTecnico();
    }
}

function esFechaRecepcionValida(valor) {
    const fecha = new Date(`${valor}T00:00:00`);
    const hoy = new Date();
    hoy.setHours(0, 0, 0, 0);
    const publicacionValor = document.getElementById("FechaPublicacion")?.value || "";
    const consejoValor = document.getElementById("FechaCT")?.value || "";
    if (fecha < hoy) return false;
    if (publicacionValor && fecha <= new Date(`${publicacionValor}T00:00:00`)) return false;
    return !consejoValor || fecha <= new Date(`${consejoValor}T00:00:00`);
}

function obtenerUltimaFechaRecepcion() {
    if (listaHorarios.length === 0) return null;
    return listaHorarios.reduce((ultima, horario) => {
        const fecha = new Date(`${horario.Fecha}T00:00:00`);
        return !ultima || fecha > ultima ? fecha : ultima;
    }, null);
}

function validarFechaConsejoTecnico() {
    const fechaConsejo = document.getElementById("FechaCT");
    if (!fechaConsejo) return true;
    const ultimaRecepcion = obtenerUltimaFechaRecepcion();
    const esValida = !fechaConsejo.value || !ultimaRecepcion ||
        new Date(`${fechaConsejo.value}T00:00:00`) >= ultimaRecepcion;
    fechaConsejo.setCustomValidity(esValida
        ? ""
        : "La fecha de consejo técnico debe ser igual o posterior a la última fecha de recepción.");
    return esValida;
}

function mensajeFechaRecepcionInvalida() {
    return "La fecha debe ser posterior a la publicación, no anterior al día actual y no posterior al consejo técnico";
}

function formatearFechaIsoLocal(fecha) {
    const anio = fecha.getFullYear();
    const mes = String(fecha.getMonth() + 1).padStart(2, "0");
    const dia = String(fecha.getDate()).padStart(2, "0");
    return `${anio}-${mes}-${dia}`;
}

async function actualizarHorarios() {

    const url = IdAviso ? `${UrlAgregarHorario}?idAviso=${IdAviso}` : UrlAgregarHorario;
    const datos = new FormData();
    datos.append("horariosJson", JSON.stringify(listaHorarios));
    datos.append("__RequestVerificationToken", document.querySelector('input[name="__RequestVerificationToken"]').value);
    const response = await fetch(url, {
        method: "POST",
        body: datos
    });

    horarios.innerHTML = await response.text();
    inicializarTooltipsDeHorarios();
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

function solicitarEliminarHorario(fecha, horaInicio, horaTermino) {
    horarioPendienteEliminar = { fecha, horaInicio, horaTermino };
    abrirModal("modalEliminarHorario");
}

function cancelarEliminacionHorario() {
    horarioPendienteEliminar = null;
}

async function confirmarEliminarHorario() {
    if (!horarioPendienteEliminar) {
        return;
    }

    const { fecha, horaInicio, horaTermino } = horarioPendienteEliminar;
    horarioPendienteEliminar = null;

    await eliminarHorario(fecha, horaInicio, horaTermino);
    cerrarModal("modalEliminarHorario");
}

async function eliminarHorario(fecha, horaInicio, horaTermino) {
    const index = buscarIndiceHorario(fecha, horaInicio, horaTermino);

    if (index === -1) {
        console.error("No se encontró el horario.");
        return;
    }

    listaHorarios.splice(index, 1);

    await actualizarHorarios();
    actualizarMinimoFechaRecepcion();
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
