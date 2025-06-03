const courseCards = document.querySelectorAll('.course-card-item');
const saveChangesButton = document.getElementById('saveChangesButton');
const assignmentForm = document.getElementById('assignmentForm'); // Changed from enrollmentForm to assignmentForm

// Sets to store the IDs of courses the user wants to assign/de-assign
let coursesToAssign = new Set();
let coursesToDeassign = new Set();

function updateSaveButtonState() {
    if (coursesToAssign.size > 0 || coursesToDeassign.size > 0) {
        saveChangesButton.disabled = false;
        saveChangesButton.classList.add('visible');
    } else {
        saveChangesButton.disabled = true;
        saveChangesButton.classList.remove('visible');
    }
}

courseCards.forEach(card => {
    card.addEventListener('click', function () {
        const courseId = this.dataset.courseId;
        const originalState = this.dataset.originalState; // 'available' or 'assigned'

        if (originalState === 'available') {
            if (this.classList.contains('selected-for-register')) {
                // Deselect
                this.classList.remove('selected-for-register');
                coursesToAssign.delete(courseId);
            } else {
                // Select
                this.classList.add('selected-for-register');
                coursesToAssign.add(courseId);
                // If previously marked for de-assignment (e.g., if it was assigned and then de-assigned and then selected again)
                coursesToDeassign.delete(courseId); // Ensure it's not in both lists
            }
        } else if (originalState === 'assigned') {
            if (this.classList.contains('selected-for-deregister')) {
                // Deselect
                this.classList.remove('selected-for-deregister');
                coursesToDeassign.delete(courseId);
            } else {
                // Select
                this.classList.add('selected-for-deregister');
                coursesToDeassign.add(courseId);
                // If previously marked for assignment
                coursesToAssign.delete(courseId); // Ensure it's not in both lists
            }
        }
        updateSaveButtonState();
    });
});

saveChangesButton.addEventListener('click', function () {
    // Clear previous hidden inputs to avoid duplicates on resubmit
    assignmentForm.querySelectorAll('input[name="CursosSeleccionadosParaInscripcion"], input[name="CursosSeleccionadosParaDesinscripcion"]').forEach(input => input.remove());

    // Get the anti-forgery token from the existing hidden input in the form (rendered by @Html.AntiForgeryToken())
    const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

    // Add hidden input for the anti-forgery token if not already part of the form submission
    // (The form itself should include this, but this is a safeguard for dynamic inputs)
    const antiForgeryInput = document.createElement('input');
    antiForgeryInput.type = 'hidden';
    antiForgeryInput.name = '__RequestVerificationToken';
    antiForgeryInput.value = antiForgeryToken;
    assignmentForm.appendChild(antiForgeryInput);


    // Add hidden inputs for courses to assign
    coursesToAssign.forEach(id => {
        const input = document.createElement('input');
        input.type = 'hidden';
        input.name = 'CursosSeleccionadosParaInscripcion'; // Matches model property name
        input.value = id;
        assignmentForm.appendChild(input);
    });

    // Add hidden inputs for courses to de-assign
    coursesToDeassign.forEach(id => {
        const input = document.createElement('input');
        input.type = 'hidden';
        input.name = 'CursosSeleccionadosParaDesinscripcion'; // Matches model property name
        input.value = id;
        assignmentForm.appendChild(input);
    });

    assignmentForm.submit(); // Submit the form
});