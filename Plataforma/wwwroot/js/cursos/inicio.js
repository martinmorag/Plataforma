// Crear tarea

const courseSelect = document.getElementById("courseSelect");
const moduloSelect = document.getElementById("moduloSelect");
const claseSelect = document.getElementById("claseSelect");
const moduloGroup = document.getElementById("moduloSelectGroup");
const claseGroup = document.getElementById("claseSelectGroup");
const submitButton = document.querySelector('.upload-tarea-button');

// Populate Curso options
cursos.forEach(curso => {
    const option = document.createElement("option");
    option.value = curso.CursoId;
    option.textContent = curso.Nombre;
    courseSelect.appendChild(option);
});

courseSelect.addEventListener("change", function () {
    const selectedCursoId = this.value;

    // Reset next dropdowns
    moduloSelect.innerHTML = '<option value="">Seleccione un módulo</option>';
    claseSelect.innerHTML = '<option value="">Seleccione una clase</option>';
    claseGroup.style.display = "none";
    submitButton.disabled = true; // Disable submit button

    if (!selectedCursoId) {
        moduloGroup.style.display = "none";
        return;
    }

    // Filter and populate Modulos
    const filteredModulos = modulos.filter(m => m.CursoId === selectedCursoId);
    filteredModulos.forEach(modulo => {
        const option = document.createElement("option");
        option.value = modulo.ModuloId;
        option.textContent = modulo.Titulo;
        moduloSelect.appendChild(option);
    });

    moduloGroup.style.display = "block";
});

moduloSelect.addEventListener("change", function () {
    const selectedModuloId = this.value;

    claseSelect.innerHTML = '<option value="">Seleccione una clase</option>';
    submitButton.disabled = true; // Disable submit button
    if (!selectedModuloId) {
        claseGroup.style.display = "none";
        return;
    }

    const filteredClases = clases.filter(clase => clase.ModuloId === selectedModuloId);
    filteredClases.forEach(clase => {
        const option = document.createElement("option");
        option.value = clase.ClaseId;
        option.textContent = clase.Nombre;
        claseSelect.appendChild(option);
    });

    claseGroup.style.display = "block";
});

claseSelect.addEventListener("change", function () {
    const selectedClaseId = this.value;
    const selectedContentType = document.querySelector('input[name="contentType"]:checked')?.value;

    if (selectedClaseId && selectedContentType) {
        submitButton.disabled = false; // Enable submit button
    } else {
        submitButton.disabled = true; // Disable if either is not selected
    }
});

const radioDivs = document.querySelectorAll('.radio-card');
const radioInputs = document.querySelectorAll('.upload-tarea-radio');

radioDivs.forEach((div, index) => {
    div.addEventListener('click', () => {
        // Unselect all radio inputs and divs
        radioInputs.forEach(input => input.checked = false);
        radioDivs.forEach(d => d.classList.remove('selected'));

        // Select the clicked radio input and div
        radioInputs[index].checked = true;
        div.classList.add('selected');

        // Check if a clase is selected to update submit button state
        const selectedClaseId = claseSelect.value;
        const selectedContentType = document.querySelector('input[name="contentType"]:checked')?.value;
        if (selectedClaseId && selectedContentType) {
            submitButton.disabled = false;
        } else {
            submitButton.disabled = true;
        }
    });
});

document.getElementById("uploadForm").addEventListener("submit", function (event) {
    event.preventDefault(); // Prevent the default form submission

    const selectedClaseId = claseSelect.value;
    const selectedContentType = document.querySelector('input[name="contentType"]:checked')?.value;

    if (selectedClaseId && selectedContentType) {
        // Redirect to the CrearTarea action with query parameters
        const redirectUrl = `tareas/crear?claseId=${selectedClaseId}&contentType=${selectedContentType}`;
        window.location.href = redirectUrl;
    } else {
        alert("Por favor, seleccione una clase y un tipo de contenido.");
    }
});





// Crear clase

const cursoIdForClaseSelect = document.getElementById('cursoIdForClase');
const moduloIdForClaseSelect = document.getElementById('moduloIdForClase');
const moduloSelectForClaseGroup = document.getElementById('moduloSelectForClaseGroup');

if (cursoIdForClaseSelect && moduloIdForClaseSelect && moduloSelectForClaseGroup) {
    // Initially hide the modulo dropdown
    moduloSelectForClaseGroup.style.display = 'none';

    cursoIdForClaseSelect.addEventListener('change', function () {
        const selectedCursoId = this.value;

        // Reset the modulo dropdown
        moduloIdForClaseSelect.innerHTML = '<option value="">Seleccione un módulo</option>';

        if (selectedCursoId) {
            // Filter modulos based on the selected CursoId
            const filteredModulos = modulos.filter(modulo => modulo.CursoId === selectedCursoId);

            // Populate the modulo dropdown with the filtered options
            filteredModulos.forEach(modulo => {
                const option = document.createElement('option');
                option.value = modulo.ModuloId;
                option.textContent = modulo.Titulo;
                option.dataset.cursoId = modulo.CursoId; // Keep the curso ID for filtering (already in HTML)
                moduloIdForClaseSelect.appendChild(option);
            });

            // Show the modulo dropdown
            moduloSelectForClaseGroup.style.display = 'block';
        } else {
            // Hide the modulo dropdown if no curso is selected
            moduloSelectForClaseGroup.style.display = 'none';
        }
    });
}