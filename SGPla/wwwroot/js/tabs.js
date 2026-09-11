function seleccionarTab(containerId, index) {
    const container = document.getElementById(containerId);
    if (!container) return;
    const buttons = container.querySelectorAll(':scope > .tabs-header > .tab-button');
    const contents = container.querySelectorAll(':scope > .tabs-body > .tab-content');
    if (!Number.isInteger(index) || index < 0 || index >= buttons.length) index = 0;
    buttons.forEach((button, i) => {
        button.classList.toggle('active', i === index);
        button.setAttribute('aria-selected', String(i === index));
        button.tabIndex = i === index ? 0 : -1;
    });
    contents.forEach((panel, i) => panel.classList.toggle('active', i === index));
    try { localStorage.setItem(`tabs-${containerId}`, index); } catch { /* Almacenamiento opcional. */ }
}

document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.tabs-container').forEach(container => {
        if (container.dataset.tabsReady) return;
        container.dataset.tabsReady = 'true';
        const buttons = [...container.querySelectorAll(':scope > .tabs-header > .tab-button')];
        let index = Math.max(0, buttons.findIndex(button => button.classList.contains('active')));
        try {
            const saved = localStorage.getItem(`tabs-${container.id}`);
            if (saved !== null) index = parseInt(saved, 10);
        } catch {}
        seleccionarTab(container.id, index);
        const header = container.querySelector(':scope > .tabs-header');
        header.addEventListener('keydown', event => {
            const buttons = [...header.querySelectorAll('.tab-button')];
            const current = buttons.indexOf(event.target);
            if (current < 0) return;
            let next;
            if (event.key === 'ArrowRight') next = (current + 1) % buttons.length;
            else if (event.key === 'ArrowLeft') next = (current - 1 + buttons.length) % buttons.length;
            else if (event.key === 'Home') next = 0;
            else if (event.key === 'End') next = buttons.length - 1;
            else return;
            event.preventDefault();
            seleccionarTab(container.id, next);
            buttons[next].focus();
        });
    });
});
