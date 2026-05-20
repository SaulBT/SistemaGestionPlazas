document.addEventListener("DOMContentLoaded", function () {

    inicializarControles();

});

function inicializarControles() {
    const region = document.getElementById("Region");
    const area = document.getElementById("Area");
    const entidad = document.getElementById("Entidad");
    const programa = document.getElementById("Programa");

    configurarCambioRegion(region, area);
    configurarCambioArea(region, area, entidad);
    configurarCambioEntidad(region, area, entidad, programa);

    actualizarVisibilidad(region, area, entidad, programa);
}

async function configurarCambioRegion(region, area) {
    if (!region) return;

    region.addEventListener("change", async function () {
        limpiarSelect(area);

        ocultarContenedor("contenedorEntidad");
        ocultarContenedor("contenedorPrograma");

        if (!region.value) {
            ocultarContenedor("contenedorArea");
            return;
        }

        await cargarOpcionesAsync(`${UrlObtenerAreas}?idAreaAcademica=${area.value}`, area, "Seleccione un área");
        mostrarContenedor("contenedorArea");
    });
}

async function configurarCambioArea(region,area,entidad) {
    if (!area) return;

    area.addEventListener("change", async function () {
        limpiarSelect(entidad);

        ocultarContenedor("contenedorPrograma");

        if (!area.value) {
            ocultarContenedor("contenedorEntidad");
            return;
        }

        await cargarOpcionesAsync(`${UrlObtenerEntidades}?idAreaAcademica=${area.value}&region=${region.value}&idEntidadAcademica=${entidad.value}`,
            entidad, "Seleccione una entidad");
        mostrarContenedor("contenedorEntidad");
    });
}

async function configurarCambioEntidad(region, area, entidad, programa) {
    if (!entidad) return;

    entidad.addEventListener("change", async function () {
        limpiarSelect(programa);

        if (!entidad.value) {
            ocultarContenedor("contenedorPrograma");
            return;
        }

        await cargarOpcionesAsync(`${UrlObtenerProgramas}?idAreaAcademica=${area.value}&region=${region.value}&idEntidadAcademica=${entidad.value}&idProgramaEducativo=${programa.value}`,
            programa, "Seleccione un programa");

        mostrarContenedor("contenedorPrograma");
    });
}

async function cargarOpcionesAsync(url, selectDestino, placeholder) {
    const response = await fetch(url);
    const data = await response.json();

    limpiarSelect(selectDestino);

    agregarPlaceholder(selectDestino, placeholder);

    data.forEach(item => {
        const option = document.createElement("option");
        option.value = item.value;
        option.text = item.text;

        if (item.selected) option.selected = true;

        selectDestino.appendChild(option);
    });
}

function limpiarSelect(select) {
    if (!select) return;

    select.innerHTML = "";
}

function agregarPlaceholder(select, texto) {
    const option = document.createElement("option");
    option.value = "";
    option.text = texto;

    select.appendChild(option);
}

function mostrarContenedor(id) {
    const contenedor = document.getElementById(id);

    if (!contenedor) return;

    contenedor.style.display = "inline";
}

function ocultarContenedor(id) {
    const contenedor = document.getElementById(id);

    if (!contenedor) return;

    contenedor.style.display = "none";
}

function actualizarVisibilidad(region, area, entidad, programa) {
    console.log('Region: ' + region.value + '\nArea: ' + area.value + "\nEntidad: " + entidad.value + "\nPrograma: " + programa.value)
    if (region?.value) mostrarContenedor("contenedorArea");
    if (area?.value) mostrarContenedor("contenedorEntidad");
    if (entidad?.value) mostrarContenedor("contenedorPrograma");
}