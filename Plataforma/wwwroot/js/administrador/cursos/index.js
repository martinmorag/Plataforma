function confirmDelete(id, nombre) {

    const confirmed = confirm(
        `¿Estás seguro que deseas eliminar el curso "${nombre}"?\n\nEsta acción no se puede deshacer.`
    );

    if (confirmed) {
        const form = document.getElementById("deleteForm");
        form.action = `/administrador/cursos/eliminar/${id}`;
        form.submit();
    }
}




async function toggleCurso(button) {

    const id = button.getAttribute("data-id");

    const response = await fetch(`/administrador/cursos/toggle/${id}`, {
        method: "POST",
    headers: {
        "RequestVerificationToken":
    document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    });

    if (!response.ok) {
        alert("Error al actualizar el curso.");
    return;
    }

    const result = await response.json();

    if (result.success) {

        const badge = button.closest(".course-card")
    .querySelector(".status-badge");

    if (result.habilitado) {

        badge.textContent = "Habilitado";
    badge.classList.remove("disabled");
    badge.classList.add("enabled");

    button.textContent = "Deshabilitar";
    button.classList.remove("enable");
    button.classList.add("disable");

        } else {

        badge.textContent = "No habilitado";
    badge.classList.remove("enabled");
    badge.classList.add("disabled");

    button.textContent = "Habilitar";
    button.classList.remove("disable");
    button.classList.add("enable");
        }
    }
}