const saveChangesButton = document.getElementById('saveChangesButton');
const courseRows = document.querySelectorAll('.course-row');

// Store pending changes: { courseId: 'register' | 'deregister' | 'none' }
const pendingChanges = {};

// Initialize pendingChanges based on current state (all 'none' initially)
courseRows.forEach(row => {
    const courseId = row.dataset.courseId;
    pendingChanges[courseId] = 'none';
});

// Function to update the save button state
function updateSaveButton() {
    const hasChanges = Object.values(pendingChanges).some(status => status !== 'none');
    if (hasChanges) {
        saveChangesButton.classList.add('visible');
        saveChangesButton.removeAttribute('disabled');
    } else {
        saveChangesButton.classList.remove('visible');
        saveChangesButton.setAttribute('disabled', 'true');
    }
}

// Add click listener to each course row
courseRows.forEach(row => {
    
    row.addEventListener('click', () => {
        console.log(pendingChanges);
        const courseId = row.dataset.courseId;
        const originalState = row.dataset.originalState;
        let currentState = pendingChanges[courseId];

        // Logic to toggle selection state
        if (originalState === 'available') {
            if (currentState === 'none') { // Not selected, select for register
                row.classList.add('selected-for-register');
                pendingChanges[courseId] = 'register';
            } else { // Already selected for register, revert
                row.classList.remove('selected-for-register');
                pendingChanges[courseId] = 'none';
            }
        } else if (originalState === 'registered') {
            if (currentState === 'none') { // Not selected, select for deregister
                row.classList.add('selected-for-deregister');
                pendingChanges[courseId] = 'deregister';
            } else { // Already selected for deregister, revert
                row.classList.remove('selected-for-deregister');
                pendingChanges[courseId] = 'none';
            }
        }
        updateSaveButton(); // Update button visibility/disabled state
    });
});

// Event listener for the Save Changes button
saveChangesButton.addEventListener('click', async () => {
    const changesToSend = {
        coursesToRegister: [],
        coursesToDeregister: []
    };

    for (const courseId in pendingChanges) {
        if (pendingChanges[courseId] === 'register') {
            changesToSend.coursesToRegister.push(courseId);
        } else if (pendingChanges[courseId] === 'deregister') {
            changesToSend.coursesToDeregister.push(courseId);
        }
    }

    try {
        const response = await fetch('/cursos/ManageCourses', { // Adjust URL to your actual API endpoint
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                // Add anti-forgery token here if needed
                // '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            body: JSON.stringify(changesToSend)
        });

        if (response.ok) {
            location.reload(); // Reload to reflect the new state
        } else {
            const errorData = await response.json(); // Assuming JSON error response
            console.error('Server error:', errorData);
        }
    } catch (error) {
        console.error('Error:', error);
    }
});

// Initial update of the save button state (it will be hidden/disabled initially)
updateSaveButton();