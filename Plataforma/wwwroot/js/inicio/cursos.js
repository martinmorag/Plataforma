//const saveChangesButton = document.getElementById('saveChangesButton');
//const courseRows = document.querySelectorAll('.course-card-item');

//// Store pending changes: { courseId: 'register' | 'deregister' | 'none' }
//const pendingChanges = {};

//// Initialize pendingChanges based on current state (all 'none' initially)
//courseRows.forEach(row => {
//    const courseId = row.dataset.courseId;
//    pendingChanges[courseId] = 'none';
//});

//// Function to update the save button state
//function updateSaveButton() {
//    const hasChanges = Object.values(pendingChanges).some(status => status !== 'none');
//    if (hasChanges) {
//        saveChangesButton.classList.add('visible');
//        saveChangesButton.removeAttribute('disabled');
//    } else {
//        saveChangesButton.classList.remove('visible');
//        saveChangesButton.setAttribute('disabled', 'true');
//    }
//}

//// Add click listener to each course row
//courseRows.forEach(row => {

//    row.addEventListener('click', () => {
//        console.log(pendingChanges);
//        const courseId = row.dataset.courseId;
//        const originalState = row.dataset.originalState;
//        let currentState = pendingChanges[courseId];

//        // Logic to toggle selection state
//        if (originalState === 'available') {
//            if (currentState === 'none') { // Not selected, select for register
//                row.classList.add('selected-for-register');
//                pendingChanges[courseId] = 'register';
//            } else { // Already selected for register, revert
//                row.classList.remove('selected-for-register');
//                pendingChanges[courseId] = 'none';
//            }
//        } else if (originalState === 'registered') {
//            if (currentState === 'none') { // Not selected, select for deregister
//                row.classList.add('selected-for-deregister');
//                pendingChanges[courseId] = 'deregister';
//            } else { // Already selected for deregister, revert
//                row.classList.remove('selected-for-deregister');
//                pendingChanges[courseId] = 'none';
//            }
//        }
//        updateSaveButton(); // Update button visibility/disabled state
//    });
//});

//// Event listener for the Save Changes button
//saveChangesButton.addEventListener('click', async () => {
//    const changesToSend = {
//        coursesToRegister: [],
//        coursesToDeregister: []
//    };

//    for (const courseId in pendingChanges) {
//        if (pendingChanges[courseId] === 'register') {
//            changesToSend.coursesToRegister.push(courseId);
//        } else if (pendingChanges[courseId] === 'deregister') {
//            changesToSend.coursesToDeregister.push(courseId);
//        }
//    }

//    try {
//        const response = await fetch('/cursos/ManageCourses', { // Adjust URL to your actual API endpoint
//            method: 'POST',
//            headers: {
//                'Content-Type': 'application/json',
//                // Add anti-forgery token here if needed
//                // '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
//            },
//            body: JSON.stringify(changesToSend)
//        });

//        if (response.ok) {
//            location.reload(); // Reload to reflect the new state
//        } else {
//            const errorData = await response.json(); // Assuming JSON error response
//            console.error('Server error:', errorData);
//        }
//    } catch (error) {
//        console.error('Error:', error);
//    }
//});

//// Initial update of the save button state (it will be hidden/disabled initially)
//updateSaveButton();



const saveChangesButton = document.getElementById('saveChangesButton');
const courseCards = document.querySelectorAll('.course-card-item');

// Store pending changes: using Sets for efficient add/delete operations
let coursesToEnroll = new Set();
let coursesToDeEnroll = new Set();

// --- Helper Functions ---

// Function to update the save button state and visibility
function updateSaveButton() {
    if (coursesToEnroll.size > 0 || coursesToDeEnroll.size > 0) {
        saveChangesButton.disabled = false;
        saveChangesButton.classList.add('visible');
    } else {
        saveChangesButton.disabled = true;
        saveChangesButton.classList.remove('visible');
    }
}

// Function to get the anti-forgery token from the meta tag
// This requires a <meta name="x-request-verification-token" content="@Html.AntiForgeryToken()" /> in your _Layout.cshtml or view
function getAntiForgeryToken() {
    const tokenElement = document.querySelector('meta[name="x-request-verification-token"]');
    if (tokenElement) {
        return tokenElement.content;
    }
    // Fallback for debugging, but you MUST have the meta tag for production security
    console.warn("Anti-forgery token meta tag not found. AJAX requests might fail.");
    return null;
}

// --- Event Listeners ---

// Add click listener to each course card
courseCards.forEach(card => {
    card.addEventListener('click', () => {
        const courseId = card.dataset.courseId; // This is the actual CursoId
        const originalState = card.dataset.originalState; // 'available' or 'registered'

        if (originalState === 'available') {
            if (card.classList.contains('selected-for-enrollment')) {
                // Deselect: If it was selected for enrollment, remove that state
                card.classList.remove('selected-for-enrollment');
                coursesToEnroll.delete(courseId);
            } else {
                // Select: Add to enrollment list
                card.classList.add('selected-for-enrollment');
                coursesToEnroll.add(courseId);
                // Ensure it's not marked for de-enrollment (just in case, though logically impossible from 'available')
                coursesToDeEnroll.delete(courseId);
            }
        } else if (originalState === 'registered') {
            if (card.classList.contains('selected-for-deenrollment')) {
                // Deselect: If it was selected for de-enrollment, remove that state
                card.classList.remove('selected-for-deenrollment');
                coursesToDeEnroll.delete(courseId);
            } else {
                // Select: Add to de-enrollment list
                card.classList.add('selected-for-deenrollment');
                coursesToDeEnroll.add(courseId);
                // Ensure it's not marked for enrollment
                coursesToEnroll.delete(courseId);
            }
        }
        updateSaveButton(); // Update button visibility/disabled state
    });
});

// Event listener for the Save Changes button
saveChangesButton.addEventListener('click', async () => {
    const changesToSend = {
        // Ensure property names match your C# CourseChangesModel exactly (case-sensitive)
        CoursesToRegister: Array.from(coursesToEnroll),
        CoursesToDeregister: Array.from(coursesToDeEnroll)
        // No need to send EstudianteId from here, the backend will get it from the authenticated user.
    };

    const antiForgeryToken = getAntiForgeryToken();

    try {
        const response = await fetch('/cursos/ManageCourses', { // Correct URL for your controller action
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                // Only include anti-forgery token if it's available
                ...(antiForgeryToken && { 'RequestVerificationToken': antiForgeryToken })
            },
            body: JSON.stringify(changesToSend)
        });

        if (response.ok) {
            // Parse the success response, if any message is sent from the backend
            const result = await response.json();
            console.log('Success:', result.message);
            if (result.errors && result.errors.length > 0) {
                //alert('Algunos cambios se guardaron, pero hubo errores: \n' + result.errors.join('\n'));
            } else {
                //alert('Cambios guardados con éxito.');
            }
            location.reload(); // Reload the page to reflect the new state
        } else {
            // Attempt to parse JSON error response from backend
            let errorData;
            try {
                errorData = await response.json();
            } catch (e) {
                errorData = { message: 'Error desconocido en el servidor.', details: await response.text() };
            }
            console.error('Server error:', response.status, errorData);
            //alert(`Error al guardar cambios: ${errorData.message || 'Desconocido'}\nDetalles: ${errorData.errorDetails || ''}`);
        }
    } catch (error) {
        console.error('Network or client-side error:', error);
        //alert('Error de conexión o inesperado. Por favor, inténtelo de nuevo.'); // Inform the user
    }
});

// Initial update of the save button state (it will be hidden/disabled initially)
updateSaveButton();