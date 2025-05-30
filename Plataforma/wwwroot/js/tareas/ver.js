//const cursosList = document.getElementById('cursosList');
//const cursosMessage = document.getElementById('cursosMessage');
//const tareasHeader = document.getElementById('tareasHeader');
//const tareasContent = document.getElementById('tareasContent');
//const tareasMessage = document.getElementById('tareasMessage');

//// --- Function to load courses ---
//async function loadCursos() {
//    cursosList.innerHTML = '<div class="list-group-item disabled text-muted"><i class="fa-solid fa-spinner fa-spin-pulse"></i> Cargando cursos...</div>';
//    cursosMessage.innerHTML = '';
//    tareasContent.innerHTML = '<div class="alert alert-info">Selecciona un curso para ver sus tareas.</div>';
//    tareasHeader.textContent = 'Tareas';

//    try {
//        const response = await fetch('/api/Profesores/MisCursos'); // New API endpoint
//        if (!response.ok) {
//            const errorData = await response.json();
//            throw new Error(errorData.message || 'Error al cargar los cursos.');
//        }
//        const cursos = await response.json();

//        cursosList.innerHTML = ''; // Clear loading message

//        if (cursos.length === 0) {
//            cursosMessage.innerHTML = '<div class="alert alert-warning">No hay cursos asignados a este profesor.</div>';
//        } else {
//            cursos.forEach(curso => {
//                const link = document.createElement('a');
//                link.href = '#';
//                link.classList.add('list-group-item', 'list-group-item-action');
//                link.textContent = `${curso.nombreCurso} (${curso.totalTareas} Tareas)`;
//                link.dataset.cursoId = curso.cursoId;
//                link.dataset.cursoNombre = curso.nombreCurso;

//                link.addEventListener('click', function (event) {
//                    event.preventDefault();
//                    // Remove 'active' class from all other links
//                    cursosList.querySelectorAll('.list-group-item').forEach(item => item.classList.remove('active'));
//                    // Add 'active' class to the clicked link
//                    this.classList.add('active');
//                    loadTareasForCurso(this.dataset.cursoId, this.dataset.cursoNombre);
//                });
//                cursosList.appendChild(link);
//            });
//        }
//    } catch (error) {
//        console.error('Error loading courses:', error);
//        cursosMessage.innerHTML = `<div class="alert alert-danger">Error: ${error.message}</div>`;
//    }
//}

//// --- Function to load tasks for a specific course ---
//async function loadTareasForCurso(cursoId, cursoNombre) {
//    tareasHeader.textContent = `Tareas para: ${cursoNombre}`;
//    tareasContent.innerHTML = '<div class="text-center p-3"><i class="fa-solid fa-spinner fa-spin-pulse"></i> Cargando tareas...</div>';
//    tareasMessage.innerHTML = '';

//    try {
//        const response = await fetch(`/api/Profesores/Cursos/${cursoId}/Tareas`); // New API endpoint
//        if (!response.ok) {
//            const errorData = await response.json();
//            throw new Error(errorData.message || 'Error al cargar las tareas.');
//        }
//        const tareas = await response.json();

//        if (tareas.length === 0) {
//            tareasContent.innerHTML = '<div class="alert alert-info">No hay tareas para este curso.</div>';
//        } else {
//            let tareasTableHtml = `
//                            <table class="table table-striped mt-3">
//                                <thead>
//                                    <tr>
//                                        <th>Nombre de la Tarea</th>
//                                        <th>Clase</th> @* Added Clase column back *
//                                        <th>Fecha Límite</th>
//                                        <th>Entregas</th>
//                                        <th>Pendientes de Revisión</th>
//                                        <th>Acciones</th>
//                                    </tr>
//                                </thead>
//                                <tbody>
//                        `;
//            tareas.forEach(tarea => {
//                tareasTableHtml += `
//                                <tr>
//                                    <td>${tarea.nombre}</td>
//                                    <td>${tarea.claseNombre}</td> @* Display class name *
//                                    <td>${new Date(tarea.fechaLimite).toLocaleDateString()}</td>
//                                    <td>${tarea.totalEntregas}</td>
//                                    <td>${tarea.entregasPendientes}</td>
//                                    <td>
//                                        <a href="/Profesores/VerEntregas?tareaId=${tarea.tareaId}" class="btn btn-info btn-sm">
//                                            <i class="fa-solid fa-list"></i> Ver Entregas
//                                        </a>
//                                    </td>
//                                </tr>
//                            `;
//            });
//            tareasTableHtml += `</tbody></table>`;
//            tareasContent.innerHTML = tareasTableHtml;
//        }
//    } catch (error) {
//        console.error('Error loading tasks:', error);
//        tareasMessage.innerHTML = `<div class="alert alert-danger">Error: ${error.message}</div>`;
//    }
//}

//// Load courses when the page loads
//loadCursos();



//document.addEventListener('DOMContentLoaded', function () {
//    const cursosList = document.getElementById('cursosList');

//    cursosList.querySelectorAll('.list-item').forEach(link => {
//        link.addEventListener('click', function (event) {
//            event.preventDefault();
//            const cursoId = this.dataset.cursoId;
//            // This will trigger a full page reload to the controller
//            // with the selected cursoId as a query parameter.
//            window.location.href = `/Profesor/MisCursosYTareas?selectedCursoId=${cursoId}`;
//        });
//    });
//});