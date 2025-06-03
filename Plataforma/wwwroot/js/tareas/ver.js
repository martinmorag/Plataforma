const courseListItems = document.querySelectorAll('#cursosList .list-item');
const tareasHeader = document.getElementById('tareasHeader');
const tareasContentContainer = document.getElementById('tareasContentContainer');
const noCourseSelectedMessage = document.getElementById('noCourseSelectedMessage');

let currentActiveCursoId = null; // Variable to keep track of the currently active course ID

// Cache to store fetched tasks
// Key: cursoId (string)
// Value: Array of ProfesorTareaViewModel objects
const tasksCache = {};

// Function to load tasks for a given course ID
async function loadTasksForCourse(cursoId, cursoNombre) {
    // Show loading state or hide previous content
    tareasContentContainer.style.display = 'none';
    noCourseSelectedMessage.style.display = 'none';
    tareasHeader.textContent = `Cargando tareas para: ${cursoNombre}...`; // Indicate loading

    // --- Check cache first ---
    if (tasksCache[cursoId]) {
        console.log(`Cargando tareas para ${cursoNombre} desde caché.`);
        renderTasks(tasksCache[cursoId], cursoNombre);
        currentActiveCursoId = cursoId;
        return; // Exit, data already loaded from cache
    }
    // --- End cache check ---

    try {
        console.log(`Solicitando tareas para ${cursoNombre} desde el servidor.`);
        const response = await fetch(`/profesor/tareas/GetTareasByCurso?cursoId=${cursoId}`);

        if (!response.ok) {
            if (response.status === 401) {
                alert('Su sesión ha expirado. Por favor, inicie sesión de nuevo.');
                window.location.href = '/Identity/Account/Login';
            } else if (response.status === 403) {
                alert('No tiene permisos para ver las tareas de este curso.');
            }
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const tareas = await response.json();
        tasksCache[cursoId] = tareas; // Store fetched data in cache

        renderTasks(tareas, cursoNombre); // Render the fetched data
        currentActiveCursoId = cursoId; // Set the newly active course ID
    } catch (error) {
        console.error('Error loading tasks:', error);
        tareasHeader.textContent = `Error al cargar tareas para: ${cursoNombre}`;
        tareasContentContainer.innerHTML = '<div class="info-message error-message">Hubo un error al cargar las tareas. Intente de nuevo.</div>';
        tareasContentContainer.style.display = 'block';
        currentActiveCursoId = null; // Reset if loading fails
    }
}

// New helper function to render tasks (moved out of loadTasksForCourse)
function renderTasks(tareas, cursoNombre) {
    tareasHeader.textContent = `Tareas para: ${cursoNombre}`;
    tareasContentContainer.innerHTML = ''; // Clear previous content

    if (tareas.length === 0) {
        tareasContentContainer.innerHTML = '<div class="info-message">No hay tareas para este curso.</div>';
    } else {
        let tableHtml = `
                <table class="data-table">
                    <thead>
                        <tr>
                            <th>Nombre de la Tarea</th>
                            <th>Clase</th>
                            <th>Fecha Límite</th>
                            <th>Entregas</th>
                            <th>Pendientes de Revisión</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>`;
        tareas.forEach(tarea => {
            tableHtml += `
                    <tr>
                        <td data-label="Nombre de la Tarea">${tarea.nombre}</td>
                        <td data-label="Clase">${tarea.claseNombre}</td>
                        <td data-label="Fecha Límite">${new Date(tarea.fechaLimite).toLocaleDateString()}</td>
                        <td data-label="Entregas">${tarea.totalEntregas}</td>
                        <td data-label="Pendientes de Revisión">${tarea.entregasPendientes}</td>
                        <td data-label="">
                            <a href="/profesor/tareas/entregas?tareaId=${tarea.tareaId}" class="action-button view-button">
                                <i class="fa-solid fa-list"></i> Ver Entregas
                            </a>
                        </td>
                    </tr>`;
        });
        tableHtml += `</tbody></table>`;
        tareasContentContainer.innerHTML = tableHtml;
    }
    tareasContentContainer.style.display = 'block'; // Show the content
}


// Function to hide tasks
function hideTasks() {
    tareasContentContainer.style.display = 'none';
    noCourseSelectedMessage.style.display = 'block'; // Show "select a course" message
    tareasHeader.textContent = 'Tareas'; // Reset header
    currentActiveCursoId = null; // Clear active course
    // Remove active class from all course items
    courseListItems.forEach(li => li.classList.remove('active-item'));
}

// Add click listeners to course list items
courseListItems.forEach(item => {
    item.addEventListener('click', function () {
        const clickedCursoId = this.dataset.cursoId;
        const clickedCursoNombre = this.dataset.cursoNombre;

        if (!clickedCursoId || !clickedCursoNombre) {
            console.error("Course ID or Name not found for clicked item.");
            return; // Exit if data attributes are missing
        }

        // Check if the clicked course is already the active one
        if (clickedCursoId === currentActiveCursoId) {
            // It's the active course, so hide it
            hideTasks();
        } else {
            // It's a new course, load its tasks
            // First, remove 'active-item' from all
            courseListItems.forEach(li => li.classList.remove('active-item'));
            // Add 'active-item' to the clicked one
            this.classList.add('active-item');
            loadTasksForCourse(clickedCursoId, clickedCursoNombre);
        }
    });
});

// Initial state check: if a course is pre-selected (via URL/ViewBag), load its tasks
const activeCourseInitial = document.querySelector('#cursosList .list-item.active-item');
if (activeCourseInitial) {
    const cursoId = activeCourseInitial.dataset.cursoId;
    const cursoNombre = activeCourseInitial.dataset.cursoNombre;
    loadTasksForCourse(cursoId, cursoNombre);
} else {
    // If no course is active initially, ensure the "select a course" message is visible
    hideTasks(); // Use the new hideTasks function
}