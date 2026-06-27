document.addEventListener("DOMContentLoaded", function () {

    const tipo = document.getElementById("TipoEntregaEsperado");
    const fechaGroup = document.getElementById("fechaVencimientoGroup");
    const fechaInput = document.getElementById("FechaVencimiento");

    function actualizarFormulario() {

        const requiereEntrega =
            tipo.value === "Documento" ||
            tipo.value === "video";

        fechaGroup.style.display = requiereEntrega ? "block" : "none";

        if (!requiereEntrega) {
            fechaInput.value = "";
        }
    }

    tipo.addEventListener("change", actualizarFormulario);

    actualizarFormulario();
});