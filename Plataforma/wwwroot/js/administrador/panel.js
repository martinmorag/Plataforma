var confirmDeleteModalEstudiante = document.getElementById('confirmDeleteModalEstudiante');
    confirmDeleteModalEstudiante.addEventListener('show.bs.modal', function (event) {
        var button = event.relatedTarget; // El botón que activó el modal
        var studentId = button.getAttribute('data-student-id');
        var studentName = button.getAttribute('data-student-name');
        var modalBodyInput = confirmDeleteModalEstudiante.querySelector('#studentToDeleteName');
        var deleteFormInput = confirmDeleteModalEstudiante.querySelector('#studentToDeleteId');

        modalBodyInput.textContent = studentName;
        deleteFormInput.value = studentId;
    });

var confirmDeleteModalProfesor = document.getElementById('confirmDeleteModalProfesor');
confirmDeleteModalProfesor.addEventListener('show.bs.modal', function (event) {
    var button = event.relatedTarget; // El botón que activó el modal
    var profesorId = button.getAttribute('data-profesor-id');
    var profesorName = button.getAttribute('data-profesor-name');
    var modalBodyInput = confirmDeleteModalProfesor.querySelector('#profesorToDeleteName');
    var deleteFormInput = confirmDeleteModalProfesor.querySelector('#profesorToDeleteId');

    modalBodyInput.textContent = profesorName;
    deleteFormInput.value = profesorId;
});




/* Dropdown of tables */

let dropdownButtonStudent = document.querySelector(".title-list-s i");
let listContainer = document.querySelectorAll(".table-container");
let droppedStudent = false;

let dropdownButtonProfesor = document.querySelector(".title-list-p i");
let droppedProfesor = false;


dropdownButtonStudent.addEventListener("click", function () {
    if (!droppedStudent) {
        dropdownButtonStudent.style.transform = "rotate(180deg)";
        listContainer[0].style.display = "block";
        droppedStudent = true;
    } else {
        dropdownButtonStudent.style.transform = "rotate(0deg)";
        listContainer[0].style.display = "none";
        droppedStudent = false
    }
});

dropdownButtonProfesor.addEventListener("click", function () {
    if (!droppedProfesor) {
        dropdownButtonProfesor.style.transform = "rotate(180deg)";
        listContainer[1].style.display = "block";
        droppedProfesor = true;
    } else {
        dropdownButtonProfesor.style.transform = "rotate(0deg)";
        listContainer[1].style.display = "none";
        droppedProfesor = false
    }
});