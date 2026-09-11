let idAvisoFirmar = 0;
let idAvisoPublicar = 0;

function errorOperacion(modalId, mensaje = '') {
    const modal = document.getElementById(modalId);
    let error = modal.querySelector('.aviso-operacion-error');
    if (!error) {
        error = document.createElement('p');
        error.className = 'aviso-operacion-error';
        error.setAttribute('role', 'alert');
        modal.querySelector('.modal-body').append(error);
    }
    error.textContent = mensaje;
}

function abrirModalFirmar(idAviso) {
    idAvisoFirmar = idAviso;
    const input = document.getElementById('inputFile-modalFirmarAviso');
    input.value = '';
    archivos.modalFirmarAviso = null;
    ocultarArchivo('modalFirmarAviso');
    document.querySelector('#modalFirmarAviso h3').textContent = 'Adjuntar aviso firmado';
    errorOperacion('modalFirmarAviso');
    abrirModal('modalFirmarAviso');
}

async function guardarOperacion(endpoint, datos, modalId, tab) {
    const modal = document.getElementById(modalId);
    if (modal.dataset.guardando) return;
    modal.dataset.guardando = 'true';
    modal.setAttribute('aria-busy', 'true');
    const botones = [...modal.querySelectorAll('button')];
    botones.forEach(b => b.disabled = true);
    errorOperacion(modalId);
    const progreso = document.createElement('p');
    progreso.setAttribute('role', 'status');
    progreso.textContent = 'Guardando…';
    modal.querySelector('.modal-body').append(progreso);
    datos.append('__RequestVerificationToken', document.querySelector('#avisoToken input').value);
    try {
        const response = await fetch(endpoint, { method: 'POST', body: datos, headers: { 'Accept': 'application/json' } });
        if (response.redirected || !response.ok) {
            let mensaje = response.status === 403 ? 'Su cuenta no tiene permisos para esta acción.'
                : response.status === 413 ? 'El archivo excede el tamaño permitido.'
                : 'No se pudo guardar. Verifique su sesión e inténtelo nuevamente.';
            if (response.headers.get('content-type')?.includes('application/json')) {
                const detalle = await response.json();
                mensaje = detalle.message || mensaje;
            }
            throw new Error(mensaje);
        }
        seleccionarTab('tabs-avisos-cea', tab);
        document.getElementById('filtrosForm').submit();
    } catch (error) {
        errorOperacion(modalId, error.message || 'No se pudo conectar con el servidor. Inténtelo nuevamente.');
    } finally {
        delete modal.dataset.guardando;
        modal.removeAttribute('aria-busy');
        botones.forEach(b => b.disabled = false);
        progreso.remove();
    }
}

async function firmarAviso() {
    const archivo = document.getElementById('inputFile-modalFirmarAviso').files[0];
    if (!archivo || !archivo.name.toLowerCase().endsWith('.pdf') || archivo.size === 0 || archivo.size > 20 * 1024 * 1024) {
        errorOperacion('modalFirmarAviso', 'Seleccione un PDF firmado, no vacío y de hasta 20 MB.');
        return;
    }
    const datos = new FormData();
    datos.append('idAviso', idAvisoFirmar);
    datos.append('archivo', archivo);
    await guardarOperacion(UrlFirmarAviso, datos, 'modalFirmarAviso', 5);
}

function abrirModalPublicar(idAviso) {
    idAvisoPublicar = idAviso;
    document.getElementById('urlPublicacion').value = '';
    errorOperacion('modalPublicar');
    abrirModal('modalPublicar');
}

async function publicar() {
    const valor = document.getElementById('urlPublicacion').value.trim();
    try {
        const direccion = new URL(valor);
        if (!['https:', 'http:'].includes(direccion.protocol)) throw new Error();
    } catch {
        errorOperacion('modalPublicar', 'Ingrese una URL válida que comience con https:// o http://.');
        return;
    }
    const datos = new FormData();
    datos.append('idAviso', idAvisoPublicar);
    datos.append('url', valor);
    await guardarOperacion(UrlPublicarAviso, datos, 'modalPublicar', 6);
}

function mostrarComentariosAviso(boton) {
    document.getElementById('comentariosAvisoTitulo').textContent = `Comentarios · ${boton.dataset.folio}`;
    document.getElementById('comentariosAvisoTexto').textContent = boton.dataset.comentarios;
    document.getElementById('comentariosAviso').showModal();
}

async function archivarAsync(idAviso, archivado) {
    const datos = new FormData();
    datos.append('idAviso', idAviso);
    datos.append('__RequestVerificationToken', document.querySelector('#avisoToken input').value);
    const response = await fetch(archivado ? UrlDesarchivarAviso : UrlArchivarAviso, { method: 'POST', body: datos });
    if (response.ok && !response.redirected) document.getElementById('filtrosForm').submit();
}

function mostrarFuncionPendienteAviso(nombre) {
    document.getElementById('funcionPendienteTitulo').textContent = nombre;
    document.getElementById('funcionPendienteAviso').showModal();
}
