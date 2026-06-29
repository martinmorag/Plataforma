document.addEventListener("DOMContentLoaded", () => {

    const tipoSelect = document.getElementById("TipoEntregaEsperado");

    const fechaGroup = document.getElementById("fechaVencimientoGroup");
    const fechaInput = document.getElementById("FechaVencimiento");

    const entregaUrlGroup = document.getElementById("entregaUrlGroup");
    const entregaUrlInput = document.getElementById("UrlEntrega");

    function actualizarFormulario() {

        const requiereEntrega =
            tipoSelect.value === "Documento" ||
            tipoSelect.value === "link" ||
            tipoSelect.value === "video";

        const requiereUrl = tipoSelect.value === "link";

        // Fecha de vencimiento
        if (requiereEntrega) {
            fechaGroup.style.display = "block";
        } else {
            fechaGroup.style.display = "none";
            fechaInput.value = "";
        }

        // URL de la tarea
        if (requiereUrl) {
            entregaUrlGroup.style.display = "block";
        } else {
            entregaUrlGroup.style.display = "none";
            entregaUrlInput.value = "";
        }
    }

    tipoSelect.addEventListener("change", actualizarFormulario);

    actualizarFormulario();
});