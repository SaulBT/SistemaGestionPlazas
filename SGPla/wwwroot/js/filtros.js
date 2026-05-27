// botón mostrar filtros

function actualizarBotonVer(boton, mostrar, mensaje) {
    const icono = boton.querySelector('i');
    const texto = boton.querySelector('.text-wrapper');

    if (mostrar) {
        if (icono) icono.className = 'bi bi-eye-slash-fill';
        if (texto) texto.textContent = mensaje;
    } else {
        if (icono) icono.className = 'bi bi-eye-fill';
        if (texto) texto.textContent = mensaje;
    }
}

function toggleFilters() {
    const filtrosContainer = document.getElementById('filtrosContainer');
    const boton = document.getElementById('botonFiltros');

    const estaVisible = filtrosContainer.style.display !== 'none';

    if (estaVisible) {
        filtrosContainer.style.opacity = '0';
        setTimeout(() => {
            filtrosContainer.style.display = 'none';
            actualizarBotonVer(boton, false, "Ocultar Filtros");
        }, 200);
    } else {
        filtrosContainer.style.display = 'inline-flex';
        setTimeout(() => {
            filtrosContainer.style.opacity = '1';
        }, 10);
        actualizarBotonVer(boton, true, "Mostrar Filtros");
    }
}

document.addEventListener('DOMContentLoaded', function () {
    const filtrosContainer = document.getElementById('filtrosContainer');
    const boton = document.getElementById('botonFiltros');


    const filtrosGuardados = localStorage.getItem('filtrosVisibles');

    if (filtrosGuardados === 'true') {
        filtrosContainer.style.display = 'inline-flex';
        filtrosContainer.style.opacity = '1';
        actualizarBotonVer(boton, true, "Mostrar Filtros");
    } else {
        filtrosContainer.style.display = 'none';
        filtrosContainer.style.opacity = '0';
        actualizarBotonVer(boton, false, "Ocultar Filtros");
    }
});