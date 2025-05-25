
function toggleModule(header) {
    header.classList.toggle('expanded');
    const content = header.nextElementSibling; // The .class-nav-list div
    content.classList.toggle('expanded');
    const arrow = header.querySelector('.arrow i');

    if (content.classList.contains('expanded')) {
        content.style.maxHeight = content.scrollHeight + 'px';
        arrow.classList.remove('fa-chevron-right');
        arrow.classList.add('fa-chevron-down');
    } else {
        content.style.maxHeight = '0';
        arrow.classList.remove('fa-chevron-down');
        arrow.classList.add('fa-chevron-right');
    }
}

// Function to load class details via AJAX
async function loadClassDetails(clickedLink, claseId) {
    // Remove 'active' class from previously selected link
    document.querySelectorAll('.class-nav-item a.active').forEach(link => {
        link.classList.remove('active');
    });
    // Add 'active' class to the newly clicked link
    clickedLink.classList.add('active');

    const container = document.getElementById('class-details-container');
    container.innerHTML = '<div class="loading-spinner"><i class="fa-solid fa-spinner fa-spin-pulse"></i> Cargando clase...</div>'; // Show loading indicator

    try {
        const response = await fetch(`/api/Clase/GetClassDetails?claseId=${claseId}`);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const html = await response.text();
        container.innerHTML = html; // Inject the partial view HTML
        attachTaskLinkListeners();
    } catch (error) {
        console.error("Error loading class details:", error);
        container.innerHTML = '<div class="error-message"><i class="fa-solid fa-triangle-exclamation"></i> Error al cargar el contenido de la clase. Inténtalo de nuevo.</div>';
    }
}





//let tareaId;
//let initialTime;
//let videoCompleted;



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

            // Wait for the DOM to update (optional but helps with timing)
            await new Promise(resolve => setTimeout(resolve, 20));

            // Now get the dynamically injected video element
            const videoElement = document.getElementById('my-video');
            if (videoElement) {
                const tareaIdFromDataset = videoElement.dataset.tareaId;
                const initialTime = parseFloat(videoElement.dataset.initialTime || '0');
                const videoCompleted = videoElement.dataset.completed === 'true';

                // Store globally or pass to next function
                console.log('Loaded tareaId from dataset:', tareaIdFromDataset);

                    initializeVideoPlayer(tareaIdFromDataset, initialTime, videoCompleted);
            }
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





// Video progress

const videoPlayerId = 'my-video';
const videoElement = document.getElementById(videoPlayerId);;

// --- Video.js Player Initialization and Progress Tracking ---
function initializeVideoPlayer(tareaId, initialTime, videoCompleted) {
    console.log("intiailized")
    // Destroy any existing player instance to prevent multiple players on the same ID
    if (videojs.getPlayer(videoPlayerId)) {
        videojs.getPlayer(videoPlayerId).dispose();
    }

    const player = videojs(videoPlayerId);

    let lastSavedTime = initialTime;
    let lastKnownDuration = 0;
    let isSeeking = false; // Flag to manage seeking behavior
    let hasAttemptedSkip = false; // Flag to indicate if a forward skip was attempted

    // Event listener for when the video metadata is loaded
    player.on('loadedmetadata', function () {
        lastKnownDuration = player.duration();
        // Only set current time if not already completed and initial time is available
        if (!videoCompleted && initialTime > 0) {
            player.currentTime(initialTime);
            player.play(); // Auto-play from where they left off
        }
    });

    // Event listeners for preventing skipping forward
    player.on('seeking', function () {
        isSeeking = true;
    });

    player.on('seeked', function () {
        isSeeking = false;
        const currentTime = player.currentTime();
        // If current time is significantly ahead of last saved time, and it's a forward skip
        // Consider a small buffer (e.g., 2 seconds) to account for minor discrepancies
        if (currentTime > lastSavedTime + 2 && lastSavedTime > 0 && !videoCompleted) {
            player.currentTime(lastSavedTime); // Rewind to last saved position
            player.pause();
            alert('No puedes adelantar el video. Debes verlo secuencialmente.');
            hasAttemptedSkip = true; // Mark that a skip was attempted
        }
        // Reset last saved time if a skip occurred and was corrected
        lastSavedTime = player.currentTime();
    });

    // Event listener for time updates (fired frequently)
    player.on('timeupdate', function () {
        // Only save progress if not seeking and not completed
        if (!isSeeking && !videoCompleted) {
            const currentTime = player.currentTime();
            // Only save if current time has truly advanced beyond last saved time
            if (currentTime > lastSavedTime + 1) { // Save every 1 second of progress
                lastSavedTime = currentTime;
                sendVideoProgress(tareaId, currentTime, lastKnownDuration);
            }
        }
    });

    // Event listener for video completion
    player.on('ended', function () {
        if (!videoCompleted) { // Ensure it's marked complete only once
            sendVideoCompleted(tareaId, lastKnownDuration);
            videoCompleted = true; // Update client-side status
            alert('¡Video completado! Tarea marcada como completada.');
            // Optionally, update the UI to show the task as completed
            document.getElementById('current-tarea-details').classList.add('task-completed');
            // You might want to refresh the class details to update the task status in the sidebar
            // (requires emitting an event or direct manipulation of the sidebar)
            // Example: A global event for sidebar update
            // window.dispatchEvent(new CustomEvent('taskCompleted', { detail: { tareaId: tareaId } }));
        }
    });
}

// --- API Calls ---

// Function to send video progress to the backend
async function sendVideoProgress(tareaId, currentTime, videoDuration) {
    try {
        const response = await fetch('/api/Tarea/SaveVideoProgress', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify({
                tareaId: tareaId,
                currentTime: currentTime,
                videoDuration: videoDuration
            })
        });
        if (!response.ok) {
            console.error('Failed to save video progress:', response.statusText);
            // Handle error (e.g., show a small non-intrusive message)
        }
    } catch (error) {
        console.error('Network error while saving video progress:', error);
    }
}

// Function to mark video task as completed
async function sendVideoCompleted(tareaId, videoDuration) {
    try {
        const response = await fetch('/api/Tarea/MarkVideoTaskCompleted', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify({
                tareaId: tareaId,
                videoDuration: videoDuration
            })
        });
        if (!response.ok) {
            console.error('Failed to mark task completed:', response.statusText);
            // Handle error
        }
    } catch (error) {
        console.error('Network error while marking task completed:', error);
    }
}

// Initialize the player when the partial view is inserted into the DOM
// For AJAX loaded content, you might need to call this explicitly after innerHTML assignment
// For this specific setup, `initializeVideoPlayer()` will be called when the script is parsed,
// and since it disposes existing players, it should work for repeated loads.


function getAntiForgeryToken() {
    const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenElement ? tokenElement.value : null;
}