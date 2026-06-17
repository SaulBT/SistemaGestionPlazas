function toggleExpansionPanel(panelId) {

    const panel = document.getElementById(panelId);
    const icon = document.getElementById(`icon-${panelId}`);

    const isOpen = panel.classList.toggle("show");

    actualizarIcono(icon, isOpen);

    localStorage.setItem(
        `expansion-panel-${panelId}`,
        isOpen
    );
}

function actualizarIcono(icon, isOpen) {

    if (isOpen) {
        icon.textContent = "►";
    }
    else {
        icon.textContent = "▼";
    }
}

document.addEventListener("DOMContentLoaded", () => {

    const panels = document.querySelectorAll(".expansion-content");

    panels.forEach(panel => {

        const panelId = panel.id;

        const savedState = localStorage.getItem(
            `expansion-panel-${panelId}`
        );

        if (savedState === null)
            return;

        const icon = document.getElementById(`icon-${panelId}`);

        const isOpen = savedState === "true";

        panel.classList.toggle("show", isOpen);

        actualizarIcono(icon, isOpen);
    });
});