
function toggleModule(header) {
    header.classList.toggle('expanded');
    const content = header.nextElementSibling; // The .class-nav-list div
    content.classList.toggle('expanded');
    const arrow = header.querySelector('.arrow i');

    if (content.classList.contains('expanded')) {
        content.style.display = 'block';
        content.style.maxHeight = content.scrollHeight + 'px';
        arrow.classList.remove('fa-chevron-right');
        arrow.classList.add('fa-chevron-down');
    } else {
        content.style.maxHeight = '0';
        arrow.classList.remove('fa-chevron-down');
        arrow.classList.add('fa-chevron-right');

        // After the animation, set display to 'none'
        // You'll need to listen for the 'transitionend' event or use a timeout
        content.addEventListener('transitionend', function handler() {
            if (!content.classList.contains('expanded')) { // Only hide if it's truly collapsed
                content.style.display = 'none';
            }
            content.removeEventListener('transitionend', handler); // Remove the listener
        }, { once: true }); // Ensure the listener runs only once
    }
}

//// Function to load class details via AJAX
//async function loadClassDetails(clickedLink, claseId) {
//    // Remove 'active' class from previously selected link
//    document.querySelectorAll('.class-nav-item.active').forEach(link => {
//        link.classList.remove('active');
//    });
//    // Add 'active' class to the newly clicked link
//    //clickedLink.classList.add('active');

//    // Add 'active' class to the parent .class-nav-item of the newly clicked link
//    // We assume clickedLink is the <a> tag, so we get its parent element (the <li>)
//    clickedLink.closest('.class-nav-item').classList.add('active');

//    const container = document.getElementById('class-details-container');
//    container.innerHTML = '<div class="loading-spinner"><i class="fa-solid fa-spinner fa-spin-pulse"></i> Cargando clase...</div>'; // Show loading indicator

//    try {
//        const response = await fetch(`/api/Clase/GetClassDetails?claseId=${claseId}`);
//        if (!response.ok) {
//            throw new Error(`HTTP error! status: ${response.status}`);
//        }
//        const html = await response.text();
//        container.innerHTML = html; // Inject the partial view HTML
//        attachTaskLinkListeners();
//    } catch (error) {
//        console.error("Error loading class details:", error);
//        container.innerHTML = '<div class="error-message"><i class="fa-solid fa-triangle-exclamation"></i> Error al cargar el contenido de la clase. Inténtalo de nuevo.</div>';
//    }
//}
async function loadClassDetails(clickedLink, claseId) {
    console.log('Loading class details for classId:', claseId);

    const classDetailsContainer = document.getElementById('class-details-container');
    const tareaDetailsContainer = document.getElementById('tarea-details-container'); // Get reference here

    // 1. Hide task details container if it's currently visible
    if (tareaDetailsContainer && tareaDetailsContainer.style.display !== 'none') {
        tareaDetailsContainer.style.display = 'none';
        tareaDetailsContainer.innerHTML = ''; // Clear task content
    }

    // 2. Prepare class details container for loading
    classDetailsContainer.innerHTML = '<div class="loading-spinner"><i class="fa-solid fa-spinner fa-spin-pulse"></i> Cargando clase...</div>'; // Show loading indicator
    classDetailsContainer.style.display = 'block'; // Ensure class details container is visible


    // 3. Update active state in sidebar navigation
    document.querySelectorAll('.class-nav-item').forEach(item => {
        item.classList.remove('active');
    });

    if (clickedLink) {
        // If a specific link was clicked (from sidebar), activate its parent li
        clickedLink.closest('.class-nav-item').classList.add('active');
    } else {
        // If not from a clicked link (e.g., returning from task), find and activate by classId
        const sidebarLink = document.querySelector(`.class-nav-item a[data-class-id="${claseId}"]`);
        if (sidebarLink) {
            sidebarLink.closest('.class-nav-item').classList.add('active');
            // Also ensure the parent module is expanded and link is visible in sidebar
            const moduleContent = sidebarLink.closest('.module-content');
            if (moduleContent && !moduleContent.style.maxHeight) { // if module is closed
                const moduleHeader = moduleContent.previousElementSibling;
                if (moduleHeader && moduleHeader.classList.contains('module-nav-header')) {
                    const arrowIcon = moduleHeader.querySelector('.arrow i');
                    if (arrowIcon && arrowIcon.classList.contains('fa-chevron-right')) {
                        toggleModule(moduleHeader); // assuming toggleModule handles expansion
                    }
                }
            }
            sidebarLink.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    }


    // 4. Fetch and inject class details
    try {
        const response = await fetch(`/api/Clase/GetClassDetails?claseId=${claseId}`);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const html = await response.text();
        classDetailsContainer.innerHTML = html; // Inject the partial view HTML
        attachTaskLinkListeners(); // Re-attach listeners for newly loaded tasks

        // Scroll the main content area to the top for a fresh view
        const mainContentArea = document.querySelector('.course-main-content');
        if (mainContentArea) {
            mainContentArea.scrollTo({ top: 0, behavior: 'smooth' });
        }

    } catch (error) {
        console.error("Error loading class details:", error);
        classDetailsContainer.innerHTML = '<div class="error-message"><i class="fa-solid fa-triangle-exclamation"></i> Error al cargar el contenido de la clase. Inténtalo de nuevo.</div>';
    }
}










const classDetailsContainer = document.getElementById('class-details-container');
const tareaDetailsContainer = document.getElementById('tarea-details-container');

async function loadTareaDetails(tareaId) {
    try {
        const response = await fetch(`/api/Tarea/GetTareaDetails?tareaId=${tareaId}`);
        if (response.ok) {
            const html = await response.text();
            tareaDetailsContainer.innerHTML = html;
            tareaDetailsContainer.style.display = 'block'; // Show task details
            classDetailsContainer.style.display = 'none'; // Hide class details (optional, depends on layout)

            initializeTareaDetailsScripts(tareaId);

            //// Wait for the DOM to update (optional but helps with timing)
            //await new Promise(resolve => setTimeout(resolve, 20));

            //// Now get the dynamically injected video element
            //const videoElement = document.getElementById('my-video');
            //if (videoElement) {
            //    const tareaIdFromDataset = videoElement.dataset.tareaId;
            //    const initialTime = parseFloat(videoElement.dataset.initialTime || '0');
            //    const videoCompleted = videoElement.dataset.completed === 'true';

            //    // Store globally or pass to next function
            //    console.log('Loaded tareaId from dataset:', tareaIdFromDataset);

            //    initializeVideoPlayerFor(tareaIdFromDataset, initialTime, videoCompleted);
            //}
        } else {
            console.error('Failed to load tarea details:', response.statusText);
            tareaDetailsContainer.innerHTML = `<p class="alert alert-danger">Error al cargar los detalles de la tarea.</p>`;
            tareaDetailsContainer.style.display = 'block';
            classDetailsContainer.style.display = 'none';
            
            
        }
    } catch (error) {
        console.error('Network error loading tarea details:', error);
        tareaDetailsContainer.innerHTML = `<p class="alert alert-danger">Error de red al cargar los detalles de la tarea.</p>`;
        tareaDetailsContainer.style.display = 'block';
        classDetailsContainer.style.display = 'none';
    }
}

// Function to attach event listeners to newly loaded task links
function attachTaskLinkListeners() {
    // Select all 'a' tags that have the 'AccederLink' and target a Tarea
    const taskLinks = document.querySelectorAll('.task-actions a'); // Select the <a> tag inside task-actions
    taskLinks.forEach(link => {
        // Remove existing listener to prevent duplicates if partial is reloaded
        link.removeEventListener('click', handleTaskLinkClick);
        link.addEventListener('click', handleTaskLinkClick);
    });
}

// Event handler for task links
function handleTaskLinkClick(event) {
    event.preventDefault(); // Prevent default link navigation
    // Or if you embed the ID as a data attribute (recommended for clarity):
    const tareaId = event.currentTarget.dataset.tareaId;

    // Call the new function to load tarea details
    loadTareaDetails(tareaId);
}

// Initial load of some default class or first class if desired
// Example: loadClassDetails(null, 'some-default-class-id');

// Attach event listeners to initial class links in the sidebar
document.querySelectorAll('.class-item a').forEach(link => {
    link.addEventListener('click', function (e) {
        e.preventDefault();
        const claseId = this.getAttribute('data-clase-id');
        loadClassDetails(this, claseId);
    });
});

attachTaskLinkListeners();




// --- NEW FUNCTION FOR RETURNING FROM TASK TO CLASS ---
async function loadClassDetailsFromTask(clickedLink, claseId) {
    console.log('Returning to class details from task for classId:', claseId);

    // Re-use the existing loadClassDetails logic to load the class content
    // We pass null for clickedLink because this call isn't from a sidebar link directly
    // The active class logic will handle highlighting the correct sidebar item if found
    await loadClassDetails(null, claseId);

    // After loading, ensure the correct sidebar class item is highlighted
    // and its parent module is expanded if necessary.
    const classLinkInSidebar = document.querySelector(`.class-nav-item a[data-class-id="${claseId}"]`);
    if (classLinkInSidebar) {
        // Ensure only this one is active in the sidebar
        document.querySelectorAll('.class-nav-item.active').forEach(link => {
            link.classList.remove('active');
        });
        classLinkInSidebar.closest('.class-nav-item').classList.add('active');

        // Expand parent module if collapsed (assuming toggleModule handles this)
        const moduleContent = classLinkInSidebar.closest('.module-content');
        if (moduleContent && !moduleContent.style.maxHeight) { // if module is closed
            const moduleHeader = moduleContent.previousElementSibling;
            if (moduleHeader && moduleHeader.classList.contains('module-nav-header')) {
                // Ensure toggleModule is called if it's currently collapsed
                const arrowIcon = moduleHeader.querySelector('.arrow i');
                if (arrowIcon && arrowIcon.classList.contains('fa-chevron-right')) {
                    toggleModule(moduleHeader);
                }
            }
        }
        // Scroll sidebar to make the active link visible if needed
        classLinkInSidebar.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }

    // Scroll the main content area to the top for a fresh view
    const mainContentArea = document.querySelector('.course-main-content');
    if (mainContentArea) {
        mainContentArea.scrollTo({ top: 0, behavior: 'smooth' });
    }
}









window.initializeVideoPlayerForTarea = function (tareaIdFromViewModel, initialTimeFromViewModel, videoCompletedFromViewModel) {
    const videoPlayerId = 'my-video';
    const videoElement = document.getElementById(videoPlayerId);

    // Ensure the video element exists before attempting to initialize
    if (!videoElement) {
        console.warn('Video element not found for Video.js initialization.');
        return;
    }

    // If a player already exists for this ID, dispose of it first
    if (videojs.getPlayer(videoPlayerId)) {
        videojs.getPlayer(videoPlayerId).dispose();
    }

    const player = videojs(videoPlayerId);

    let lastSavedTime = initialTimeFromViewModel;
    let lastKnownDuration = 0;
    let isSeeking = false;

    player.on('loadedmetadata', function () {
        lastKnownDuration = player.duration();
        if (!videoCompletedFromViewModel && initialTimeFromViewModel > 0) {
            player.currentTime(initialTimeFromViewModel);
            // Optional: player.play(); // Auto-play from where they left off
        }
    });

    player.on('seeking', function () { isSeeking = true; });
    player.on('seeked', function () {
        isSeeking = false;
        const currentTime = player.currentTime();
        if (currentTime > lastSavedTime + 2 && lastSavedTime > 0 && !videoCompletedFromViewModel) {
            player.currentTime(lastSavedTime);
            player.pause();
            //alert('No puedes adelantar el video. Debes verlo secuencialmente.');
        }
        lastSavedTime = player.currentTime();
    });

    player.on('timeupdate', function () {
        if (!isSeeking && !videoCompletedFromViewModel) {
            const currentTime = player.currentTime();
            if (currentTime > lastSavedTime + 1) {
                lastSavedTime = currentTime;
                sendVideoProgress(tareaIdFromViewModel, currentTime, lastKnownDuration);
            }
        }
    });

    player.on('ended', function () {
        if (!videoCompletedFromViewModel) {
            sendVideoCompleted(tareaIdFromViewModel, lastKnownDuration);
            videoCompletedFromViewModel = true;
            if (typeof loadTareaDetails === 'function') {
                loadTareaDetails(tareaIdFromViewModel); // Re-load to update UI
            }
        }
    });

    // Helper functions (could be defined once globally or passed)
    async function sendVideoProgress(tareaId, currentTime, videoDuration) {
        try {
            const response = await fetch('/api/Tarea/SaveVideoProgress', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value },
                body: JSON.stringify({ tareaId: tareaId, currentTime: currentTime, videoDuration: videoDuration })
            });
            if (!response.ok) { console.error('Failed to save video progress:', response.statusText); }
        } catch (error) { console.error('Network error while saving video progress:', error); }
    }

    async function sendVideoCompleted(tareaId, videoDuration) {
        try {
            const response = await fetch('/api/Tarea/MarkVideoTaskCompleted', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value },
                body: JSON.stringify({ tareaId: tareaId, videoDuration: videoDuration })
            });
            if (!response.ok) { console.error('Failed to mark task completed:', response.statusText); }
        } catch (error) { console.error('Network error while marking task completed:', error); }
    }
};




// Video progress

//const videoPlayerId = 'my-video';
//const videoElement = document.getElementById(videoPlayerId);;

//// --- Video.js Player Initialization and Progress Tracking ---
//function initializeVideoPlayer(tareaId, initialTime, videoCompleted) {
//    console.log("intiailized")
//    // Destroy any existing player instance to prevent multiple players on the same ID
//    if (videojs.getPlayer(videoPlayerId)) {
//        videojs.getPlayer(videoPlayerId).dispose();
//    }

//    const player = videojs(videoPlayerId);

//    let lastSavedTime = initialTime;
//    let lastKnownDuration = 0;
//    let isSeeking = false; // Flag to manage seeking behavior
//    let hasAttemptedSkip = false; // Flag to indicate if a forward skip was attempted

//    // Event listener for when the video metadata is loaded
//    player.on('loadedmetadata', function () {
//        lastKnownDuration = player.duration();
//        // Only set current time if not already completed and initial time is available
//        if (!videoCompleted && initialTime > 0) {
//            player.currentTime(initialTime);
//            player.play(); // Auto-play from where they left off
//        }
//    });

//    // Event listeners for preventing skipping forward
//    player.on('seeking', function () {
//        isSeeking = true;
//    });

//    player.on('seeked', function () {
//        isSeeking = false;
//        const currentTime = player.currentTime();
//        // If current time is significantly ahead of last saved time, and it's a forward skip
//        // Consider a small buffer (e.g., 2 seconds) to account for minor discrepancies
//        if (currentTime > lastSavedTime + 2 && lastSavedTime > 0 && !videoCompleted) {
//            player.currentTime(lastSavedTime); // Rewind to last saved position
//            player.pause();
//            alert('No puedes adelantar el video. Debes verlo secuencialmente.');
//            hasAttemptedSkip = true; // Mark that a skip was attempted
//        }
//        // Reset last saved time if a skip occurred and was corrected
//        lastSavedTime = player.currentTime();
//    });

//    // Event listener for time updates (fired frequently)
//    player.on('timeupdate', function () {
//        // Only save progress if not seeking and not completed
//        if (!isSeeking && !videoCompleted) {
//            const currentTime = player.currentTime();
//            // Only save if current time has truly advanced beyond last saved time
//            if (currentTime > lastSavedTime + 1) { // Save every 1 second of progress
//                lastSavedTime = currentTime;
//                sendVideoProgress(tareaId, currentTime, lastKnownDuration);
//            }
//        }
//    });

//    // Event listener for video completion
//    player.on('ended', function () {
//        if (!videoCompleted) { // Ensure it's marked complete only once
//            sendVideoCompleted(tareaId, lastKnownDuration);
//            videoCompleted = true; // Update client-side status
//            alert('¡Video completado! Tarea marcada como completada.');
//            // Optionally, update the UI to show the task as completed
//            document.getElementById('current-tarea-details').classList.add('task-completed');
//            // You might want to refresh the class details to update the task status in the sidebar
//            // (requires emitting an event or direct manipulation of the sidebar)
//            // Example: A global event for sidebar update
//            // window.dispatchEvent(new CustomEvent('taskCompleted', { detail: { tareaId: tareaId } }));
//        }
//    });
//}

//// --- API Calls ---

//// Function to send video progress to the backend
//async function sendVideoProgress(tareaId, currentTime, videoDuration) {
//    try {
//        const response = await fetch('/api/Tarea/SaveVideoProgress', {
//            method: 'POST',
//            headers: {
//                'Content-Type': 'application/json',
//                'RequestVerificationToken': getAntiForgeryToken()
//            },
//            body: JSON.stringify({
//                tareaId: tareaId,
//                currentTime: currentTime,
//                videoDuration: videoDuration
//            })
//        });
//        if (!response.ok) {
//            console.error('Failed to save video progress:', response.statusText);
//            // Handle error (e.g., show a small non-intrusive message)
//        }
//    } catch (error) {
//        console.error('Network error while saving video progress:', error);
//    }
//}

//// Function to mark video task as completed
//async function sendVideoCompleted(tareaId, videoDuration) {
//    try {
//        const response = await fetch('/api/Tarea/MarkVideoTaskCompleted', {
//            method: 'POST',
//            headers: {
//                'Content-Type': 'application/json',
//                'RequestVerificationToken': getAntiForgeryToken()
//            },
//            body: JSON.stringify({
//                tareaId: tareaId,
//                videoDuration: videoDuration
//            })
//        });
//        if (!response.ok) {
//            console.error('Failed to mark task completed:', response.statusText);
//            // Handle error
//        }
//    } catch (error) {
//        console.error('Network error while marking task completed:', error);
//    }
//}

// Initialize the player when the partial view is inserted into the DOM
// For AJAX loaded content, you might need to call this explicitly after innerHTML assignment
// For this specific setup, `initializeVideoPlayer()` will be called when the script is parsed,
// and since it disposes existing players, it should work for repeated loads.


function getAntiForgeryToken() {
    const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenElement ? tokenElement.value : null;
}













// FILE SUBMISSION

window.showSubmissionForm =  function(tareaId) {
    document.getElementById(`submission-form-container-${tareaId}`).style.display = 'block';
}

window.hideSubmissionForm = function(tareaId) {
    document.getElementById(`submission-form-container-${tareaId}`).style.display = 'none';
    document.getElementById(`submission-message-${tareaId}`).innerHTML = ''; // Clear message
    document.getElementById(`submissionForm-${tareaId}`).reset(); // Clear form fields
}


// This is the main initialization function called AFTER the HTML is injected
window.initializeTareaDetailsScripts = function (tareaid) {
    const tareaId = tareaid;
    const submissionForm = document.getElementById(`submissionForm-${tareaId}`);

    console.log('initializeTareaDetailsScripts called for TareaId:', tareaId); // Debugging
    console.log('Attempting to find submission form with ID:', `submissionForm-${tareaId}`);
    console.log('Found submission form element:', submissionForm);

    if (submissionForm) {
        // Remove any old event listener to prevent duplicates if partial is reloaded multiple times
        submissionForm.removeEventListener('submit', handleSubmissionFormSubmit);
        // Attach the new event listener
        submissionForm.addEventListener('submit', handleSubmissionFormSubmit);
    } else {
        console.warn('Submission form element not found for TareaId:', tareaId);
    }

    // Initialize Video.js player if a video exists
    const videoElement = document.getElementById('my-video');
    if (videoElement) {
        const initialTime = parseFloat(videoElement.dataset.initialTime || '0');
        const videoCompleted = videoElement.dataset.completed === 'true';
        window.initializeVideoPlayerForTarea(tareaId, initialTime, videoCompleted);
    }
};

// Named function for the event listener so it can be removed/added
async function handleSubmissionFormSubmit(event) {
    event.preventDefault();

    const formData = new FormData(this); // 'this' refers to the form element
    const tareaId = this.querySelector('input[name="TareaId"]').value; // Get tareaId from form data
    const messageDiv = document.getElementById(`submission-message-${tareaId}`);

    messageDiv.innerHTML = '<div class="loading-spinner"><i class="fa-solid fa-spinner fa-spin-pulse"></i> Enviando...</div>';
    messageDiv.className = 'mt-2';

    try {
        const response = await fetch('/api/Tarea/SubmitAssignment', {
            method: 'POST',
            body: formData
        });

        const result = await response.json();

        if (response.ok) {
            messageDiv.innerHTML = `<div class="alert alert-success">${result.message || 'Entrega enviada con éxito.'}</div>`;
            hideSubmissionForm(tareaId);
            if (typeof loadTareaDetails === 'function') {
                loadTareaDetails(tareaId); // Re-fetch details to update status
            }
        } else {
            const errorMessage = result.message || 'No se pudo enviar la entrega.';
            messageDiv.innerHTML = `<div class="alert alert-danger">Error: ${errorMessage}</div>`;
        }
    } catch (error) {
        console.error('Error submitting assignment:', error);
        messageDiv.innerHTML = '<div class="alert alert-danger">Error de red al enviar la entrega.</div>';
    }
}