// Crear tarea

const courseSelect = document.getElementById("courseSelect");
const moduloSelect = document.getElementById("moduloSelect");
const claseSelect = document.getElementById("claseSelect");

const moduloGroup = document.getElementById("moduloSelectGroup");
const claseGroup = document.getElementById("claseSelectGroup");
const submitButton = document.querySelector('.upload-tarea-button');

// Store original options
const allModuloOptions = Array.from(moduloSelect.querySelectorAll("option[data-curso-id]"));
const allClaseOptions = Array.from(claseSelect.querySelectorAll("option[data-modulo-id]"));

courseSelect.addEventListener("change", function () {
    const selectedCursoId = this.value;

    moduloSelect.innerHTML = '<option value="">Seleccione un módulo</option>';
    claseSelect.innerHTML = '<option value="">Seleccione una clase</option>';
    claseGroup.style.display = "none";
    submitButton.disabled = true;

    if (!selectedCursoId) {
        moduloGroup.style.display = "none";
        return;
    }

    const filteredModulos = allModuloOptions.filter(
        option => option.dataset.cursoId === selectedCursoId
    );

    filteredModulos.forEach(option => {
        moduloSelect.appendChild(option);
    });

    moduloGroup.style.display = "block";
});

moduloSelect.addEventListener("change", function () {
    const selectedModuloId = this.value;

    claseSelect.innerHTML = '<option value="">Seleccione una clase</option>';
    submitButton.disabled = true;

    if (!selectedModuloId) {
        claseGroup.style.display = "none";
        return;
    }

    const filteredClases = allClaseOptions.filter(
        option => option.dataset.moduloId === selectedModuloId
    );

    filteredClases.forEach(option => {
        claseSelect.appendChild(option);
    });

    claseGroup.style.display = "block";
});

claseSelect.addEventListener("change", validateSubmit);
document.querySelectorAll('input[name="contentType"]').forEach(radio =>
    radio.addEventListener("change", validateSubmit)
);

function validateSubmit() {
    const selectedClaseId = claseSelect.value;
    const selectedContentType = document.querySelector('input[name="contentType"]:checked')?.value;

    submitButton.disabled = !(selectedClaseId && selectedContentType);
}

document.getElementById("uploadForm").addEventListener("submit", function (event) {
    event.preventDefault();

    const selectedClaseId = claseSelect.value;
    const selectedContentType = document.querySelector('input[name="contentType"]:checked')?.value;

    if (selectedClaseId && selectedContentType) {
        window.location.href = `tareas/crear?claseId=${selectedClaseId}&contentType=${selectedContentType}`;
    } else {
        alert("Por favor, seleccione una clase y un tipo de contenido.");
    }
});





// Crear clase

const cursoIdForClaseSelect = document.getElementById('cursoIdForClase');
const moduloIdForClaseSelect = document.getElementById('moduloIdForClase');
const moduloSelectForClaseGroup = document.getElementById('moduloSelectForClaseGroup');

if (cursoIdForClaseSelect && moduloIdForClaseSelect) {

    const allModuloOptionsClase = Array.from(
        moduloIdForClaseSelect.querySelectorAll("option[data-curso-id]")
    );

    moduloSelectForClaseGroup.style.display = 'none';

    cursoIdForClaseSelect.addEventListener('change', function () {

        const selectedCursoId = this.value;

        moduloIdForClaseSelect.innerHTML = '<option value="">Seleccione un módulo</option>';

        if (!selectedCursoId) {
            moduloSelectForClaseGroup.style.display = 'none';
            return;
        }

        const filtered = allModuloOptionsClase.filter(
            option => option.dataset.cursoId === selectedCursoId
        );

        filtered.forEach(option => {
            moduloIdForClaseSelect.appendChild(option);
        });

        moduloSelectForClaseGroup.style.display = 'block';
    });
}






// deseleccionar seleccionar tarea item

document.querySelectorAll('.radio-card input[type="radio"]').forEach(radio => {

    radio.addEventListener('click', function (e) {

        if (this.dataset.waschecked === "true") {
            this.checked = false;
            this.dataset.waschecked = "false";
            this.dispatchEvent(new Event('change'));
        } else {
            document.querySelectorAll('.radio-card input[type="radio"]')
                .forEach(r => r.dataset.waschecked = "false");

            this.dataset.waschecked = "true";
        }
    });

});