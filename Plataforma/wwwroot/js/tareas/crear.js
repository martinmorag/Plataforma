document.addEventListener("DOMContentLoaded", () => {

    const tipoSelect = document.getElementById("TipoEntregaEsperado");
    const fechaGroup = document.getElementById("fechaVencimientoGroup");
    const fechaInput = document.getElementById("FechaVencimiento");

    function actualizarFormulario() {

        const requiereEntrega =
            tipoSelect.value === "Documento" ||
            tipoSelect.value === "video";

        if (requiereEntrega) {
            fechaGroup.style.display = "block";
        } else {
            fechaGroup.style.display = "none";

            // Clear the value so it isn't accidentally submitted
            fechaInput.value = "";
        }
    }

    tipoSelect.addEventListener("change", actualizarFormulario);

    // Set the correct state when the page loads
    actualizarFormulario();
});