function seleccionarTab(containerId, index) {

    const container = document.getElementById(containerId);

    const buttons =
        container.querySelectorAll(".tab-button");

    const contents =
        container.querySelectorAll(".tab-content");

    buttons.forEach(btn =>
        btn.classList.remove("active"));

    contents.forEach(content =>
        content.classList.remove("active"));

    buttons[index].classList.add("active");
    contents[index].classList.add("active");

    localStorage.setItem(
        `tabs-${containerId}`,
        index
    );
}

document.addEventListener("DOMContentLoaded", () => {

    document
        .querySelectorAll(".tabs-container")
        .forEach(container => {

            const containerId = container.id;

            const savedIndex = localStorage.getItem(
                `tabs-${containerId}`
            );

            if (savedIndex === null)
                return;

            seleccionarTab(
                containerId,
                parseInt(savedIndex)
            );
        });
});