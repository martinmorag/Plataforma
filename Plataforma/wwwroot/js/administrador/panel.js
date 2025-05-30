//var confirmDeleteModalEstudiante = document.getElementById('confirmDeleteModalEstudiante');
//    confirmDeleteModalEstudiante.addEventListener('show.bs.modal', function (event) {
//        var button = event.relatedTarget; // El botón que activó el modal
//        var studentId = button.getAttribute('data-student-id');
//        var studentName = button.getAttribute('data-student-name');
//        var modalBodyInput = confirmDeleteModalEstudiante.querySelector('#studentToDeleteName');
//        var deleteFormInput = confirmDeleteModalEstudiante.querySelector('#studentToDeleteId');

//        modalBodyInput.textContent = studentName;
//        deleteFormInput.value = studentId;
//    });

//var confirmDeleteModalProfesor = document.getElementById('confirmDeleteModalProfesor');
//confirmDeleteModalProfesor.addEventListener('show.bs.modal', function (event) {
//    var button = event.relatedTarget; // El botón que activó el modal
//    var profesorId = button.getAttribute('data-profesor-id');
//    var profesorName = button.getAttribute('data-profesor-name');
//    var modalBodyInput = confirmDeleteModalProfesor.querySelector('#profesorToDeleteName');
//    var deleteFormInput = confirmDeleteModalProfesor.querySelector('#profesorToDeleteId');

//    modalBodyInput.textContent = profesorName;
//    deleteFormInput.value = profesorId;
//});




///* Dropdown of tables */

//let dropdownButtonStudent = document.querySelector(".title-list-s i");
//let listContainer = document.querySelectorAll(".table-container");
//let droppedStudent = false;

//let dropdownButtonProfesor = document.querySelector(".title-list-p i");
//let droppedProfesor = false;


//dropdownButtonStudent.addEventListener("click", function () {
//    if (!droppedStudent) {
//        dropdownButtonStudent.style.transform = "rotate(180deg)";
//        listContainer[0].style.display = "block";
//        droppedStudent = true;
//    } else {
//        dropdownButtonStudent.style.transform = "rotate(0deg)";
//        listContainer[0].style.display = "none";
//        droppedStudent = false
//    }
//});

//dropdownButtonProfesor.addEventListener("click", function () {
//    if (!droppedProfesor) {
//        dropdownButtonProfesor.style.transform = "rotate(180deg)";
//        listContainer[1].style.display = "block";
//        droppedProfesor = true;
//    } else {
//        dropdownButtonProfesor.style.transform = "rotate(0deg)";
//        listContainer[1].style.display = "none";
//        droppedProfesor = false
//    }
//});


document.addEventListener('DOMContentLoaded', function () {
    // Event listener for opening delete modals (Students)
    const studentDeleteModal = document.getElementById('confirmDeleteModalEstudiante');
    if (studentDeleteModal) {
        studentDeleteModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget; // Button that triggered the modal
            const userId = button.getAttribute('data-user-id');
            const userName = button.getAttribute('data-user-name');

            const modalTitle = studentDeleteModal.querySelector('.modal-title');
            const modalBodyName = studentDeleteModal.querySelector('#studentToDeleteName');
            const formInputId = studentDeleteModal.querySelector('#studentToDeleteId');

            modalTitle.textContent = `Confirmar Eliminación de Estudiante`; // Update modal title if needed
            modalBodyName.textContent = userName;
            formInputId.value = userId;
        });
    }

    // Event listener for opening delete modals (Professors)
    const professorDeleteModal = document.getElementById('confirmDeleteModalProfesor');
    if (professorDeleteModal) {
        professorDeleteModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget; // Button that triggered the modal
            const userId = button.getAttribute('data-user-id');
            const userName = button.getAttribute('data-user-name');

            const modalTitle = professorDeleteModal.querySelector('.modal-title');
            const modalBodyName = professorDeleteModal.querySelector('#profesorToDeleteName');
            const formInputId = professorDeleteModal.querySelector('#profesorToDeleteId');

            modalTitle.textContent = `Confirmar Eliminación de Profesor`; // Update modal title if needed
            modalBodyName.textContent = userName;
            formInputId.value = userId;
        });
    }

    const toggleButtons = document.querySelectorAll('.toggle-table-btn');

    toggleButtons.forEach(button => {
        const targetId = button.dataset.target; // Get the ID of the table container from data-target
        const tableContainer = document.getElementById(targetId);
        const icon = button.querySelector('i');

        // Initial setup for the table visibility based on its current state
        // If the table container has 'active' class on page load (meaning visible)
        // ensure its max-height is correctly set for the transition.
        if (tableContainer && tableContainer.classList.contains('active')) {
            // Set max-height to its scrollHeight to allow smooth collapse later
            // This is crucial for the very first collapse animation to work.
            tableContainer.style.maxHeight = tableContainer.scrollHeight + 'px';
            // Ensure the icon is 'up' if it's active
            icon.classList.remove('fa-chevron-down');
            icon.classList.add('fa-chevron-up');
            button.setAttribute('aria-expanded', 'true');
        } else if (tableContainer) {
            // If it's not 'active' (e.g., hidden by default), make sure it's collapsed
            tableContainer.classList.add('collapsed');
            tableContainer.style.maxHeight = '0'; // Ensure it's explicitly 0
            icon.classList.remove('fa-chevron-up');
            icon.classList.add('fa-chevron-down');
            button.setAttribute('aria-expanded', 'false');
        }


        // Add click listener to the button
        button.addEventListener('click', function () {
            if (!tableContainer) return; // Exit if element not found

            const isCollapsed = tableContainer.classList.contains('collapsed');

            if (isCollapsed) {
                // If it's currently collapsed, expand it
                tableContainer.classList.remove('collapsed');
                // Set max-height to scrollHeight to animate to its full height
                // This will make it expand smoothly.
                tableContainer.style.maxHeight = tableContainer.scrollHeight + 'px';

                icon.classList.remove('fa-chevron-down');
                icon.classList.add('fa-chevron-up');
                this.setAttribute('aria-expanded', 'true');
            } else {
                // If it's currently expanded, collapse it
                // First, set max-height to its current scrollHeight (important for a smooth transition from current size)
                tableContainer.style.maxHeight = tableContainer.scrollHeight + 'px';

                // Force a reflow (re-render) to ensure the browser registers the scrollHeight before setting to 0
                // This is a common trick for `max-height` transitions when collapsing.
                void tableContainer.offsetWidth; // Triggers reflow

                tableContainer.classList.add('collapsed');
                // Set max-height to 0 after reflow to trigger the collapse animation
                tableContainer.style.maxHeight = '0';

                icon.classList.remove('fa-chevron-up');
                icon.classList.add('fa-chevron-down');
                this.setAttribute('aria-expanded', 'false');
            }
        });
    });
});