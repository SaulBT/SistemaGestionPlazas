const periodo = document.getElementById("IdPeriodo");
const articulo = document.getElementById("IdArticulo");
const ofertas = document.getElementById("contenedorOfertas");
const modalidad = document.getElementById("Modalidad");
const lugar = document.getElementById("Lugar");

periodo.addEventListener("change", actualizarOfertas);
articulo.addEventListener("change", actualizarOfertas);

modalidad.addEventListener("change", () => cambiarVisibilidad("contenedorLugar"));

async function actualizarOfertas() {

    const idPeriodo = periodo.value;
    const idArticulo = articulo.value;

    const response = await fetch(
        `${UrlActualizarOfertas}?idPeriodo=${idPeriodo}&idArticulo=${idArticulo}`
    );

    const html = await response.text();

    ofertas.innerHTML = html;
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
    console.error("Error:" + modalidad.value);
}