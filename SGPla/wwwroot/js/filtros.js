window.configFiltros = window.configFiltros || {
    storageKey: 'filtrosVisibles_default'
};


window.configFiltros.mostrarTexto = "Mostrar Filtros";
window.configFiltros.ocultarTexto = "Ocultar Filtros";

function actualizarBotonVer(boton, mostrar) {
    const icono = boton.querySelector('i');
    const texto = boton.querySelector('.text-wrapper');

    if (mostrar) {
        if (icono) icono.className = 'bi bi-eye-slash-fill';
        if (texto) texto.textContent = window.configFiltros.ocultarTexto;
    } else {
        if (icono) icono.className = 'bi bi-eye-fill';
        if (texto) texto.textContent = window.configFiltros.mostrarTexto;
    }
}

function toggleFilters() {
    const filtrosContainer = document.getElementById('filtrosContainer');
    const boton = document.getElementById('botonFiltros');

    if (!filtrosContainer || !boton) return;

    const estaVisible = filtrosContainer.style.display !== 'none';

    if (estaVisible) {
        filtrosContainer.style.opacity = '0';
        setTimeout(() => {
            filtrosContainer.style.display = 'none';
            actualizarBotonVer(boton, false);
        }, 200);
        localStorage.setItem(window.configFiltros.storageKey, 'false');
    } else {
        filtrosContainer.style.display = 'inline-flex';
        setTimeout(() => {
            filtrosContainer.style.opacity = '1';
        }, 10);
        actualizarBotonVer(boton, true);
        localStorage.setItem(window.configFiltros.storageKey, 'true');
    }
}

document.addEventListener('DOMContentLoaded', function () {
    const filtrosContainer = document.getElementById('filtrosContainer');
    const boton = document.getElementById('botonFiltros');

    if (!filtrosContainer || !boton) return;

    const filtrosGuardados = localStorage.getItem(window.configFiltros.storageKey);

    if (filtrosGuardados === 'true') {
        filtrosContainer.style.display = 'inline-flex';
        filtrosContainer.style.opacity = '1';
        actualizarBotonVer(boton, true);
    } else {
        filtrosContainer.style.display = 'none';
        filtrosContainer.style.opacity = '0';
        actualizarBotonVer(boton, false);
    }
});