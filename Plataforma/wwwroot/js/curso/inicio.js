
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









//window.initializeVideoPlayerForTarea = function (tareaIdFromViewModel, initialTimeFromViewModel, videoCompletedFromViewModel) {
//    const videoPlayerId = 'my-video';
//    const videoElement = document.getElementById(videoPlayerId);

//    // Ensure the video element exists before attempting to initialize
//    if (!videoElement) {
//        console.warn('Video element not found for Video.js initialization.');
//        return;
//    }

//    // If a player already exists for this ID, dispose of it first
//    if (videojs.getPlayer(videoPlayerId)) {
//        videojs.getPlayer(videoPlayerId).dispose();
//    }

//    const player = videojs(videoPlayerId);

//    let lastSavedTime = initialTimeFromViewModel;
//    let lastKnownDuration = 0;
//    let isSeeking = false;

//    player.on('loadedmetadata', function () {
//        lastKnownDuration = player.duration();
//        if (!videoCompletedFromViewModel && initialTimeFromViewModel > 0) {
//            player.currentTime(initialTimeFromViewModel);
//            // Optional: player.play(); // Auto-play from where they left off
//        }
//    });

//    player.on('seeking', function () { isSeeking = true; });
//    player.on('seeked', function () {
//        isSeeking = false;
//        const currentTime = player.currentTime();
//        if (currentTime > lastSavedTime + 2 && lastSavedTime > 0 && !videoCompletedFromViewModel) {
//            player.currentTime(lastSavedTime);
//            player.pause();
//            //alert('No puedes adelantar el video. Debes verlo secuencialmente.');
//        }
//        lastSavedTime = player.currentTime();
//    });

//    player.on('timeupdate', function () {
//        if (!isSeeking && !videoCompletedFromViewModel) {
//            const currentTime = player.currentTime();
//            if (currentTime > lastSavedTime + 1) {
//                lastSavedTime = currentTime;
//                sendVideoProgress(tareaIdFromViewModel, currentTime, lastKnownDuration);
//            }
//        }
//    });

//    player.on('ended', function () {
//        if (!videoCompletedFromViewModel) {
//            sendVideoCompleted(tareaIdFromViewModel, lastKnownDuration);
//            videoCompletedFromViewModel = true;
//            if (typeof loadTareaDetails === 'function') {
//                loadTareaDetails(tareaIdFromViewModel); // Re-load to update UI
//            }
//        }
//    });

//    // Helper functions (could be defined once globally or passed)
//    async function sendVideoProgress(tareaId, currentTime, videoDuration) {
//        try {
//            const response = await fetch('/api/Tarea/SaveVideoProgress', {
//                method: 'POST',
//                headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value },
//                body: JSON.stringify({ tareaId: tareaId, currentTime: currentTime, videoDuration: videoDuration })
//            });
//            if (!response.ok) { console.error('Failed to save video progress:', response.statusText); }
//        } catch (error) { console.error('Network error while saving video progress:', error); }
//    }

//    async function sendVideoCompleted(tareaId, videoDuration) {
//        try {
//            const response = await fetch('/api/Tarea/MarkVideoTaskCompleted', {
//                method: 'POST',
//                headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value },
//                body: JSON.stringify({ tareaId: tareaId, videoDuration: videoDuration })
//            });
//            if (!response.ok) { console.error('Failed to mark task completed:', response.statusText); }
//        } catch (error) { console.error('Network error while marking task completed:', error); }
//    }
//};


window.initializeVideoPlayerForTarea = function (tareaIdFromViewModel, initialTimeFromViewModel, videoCompletedFromViewModel) {
    // --- DEBUGGING: Confirm function execution ---
    console.log("[INITIALIZATION] initializeVideoPlayerForTarea function started.");

    const videoPlayerId = 'my-video';
    const videoElement = document.getElementById(videoPlayerId);

    if (!videoElement) {
        console.warn('Video element not found for Video.js initialization. Aborting.');
        return;
    }

    // Dispose of any existing player instance to prevent memory leaks or duplicate events
    if (videojs.getPlayer(videoPlayerId)) {
        console.log(`[INITIALIZATION] Disposing existing Video.js player for ${videoPlayerId}.`);
        videojs.getPlayer(videoPlayerId).dispose();
    }

    const player = videojs(videoPlayerId);

    // This variable tracks the furthest point watched sequentially by the user.
    // It's crucial for allowing rewinds but preventing forward skips.
    let furthestWatchedTime = initialTimeFromViewModel; // This is the "max progress" from server
    let lastKnownDuration = 0;
    let isSeeking = false; // Flag to indicate if player is currently in a seeking operation
    const allowedForwardBuffer = 2; // Allow 2 seconds for minor discrepancies, network lag, etc.
    const completionThresholdPercentage = 0.95; // Video must be watched to at least 95% to be considered complete

    // Flag to prevent multiple alert pop-ups for a single illegal skip attempt
    let hasAttemptedIllegalForwardSkip = false;

    // --- Player Event Listeners ---

    player.on('loadedmetadata', function () {
        lastKnownDuration = player.duration();
        console.log(`[loadedmetadata] Video Duration: ${lastKnownDuration.toFixed(2)}s, Initial Time (from ViewModel): ${initialTimeFromViewModel.toFixed(2)}s`);

        if (!videoCompletedFromViewModel && initialTimeFromViewModel > 0) {
            // Set the video's current time to the last saved progress.
            // This allows resuming where the user left off.
            player.currentTime(initialTimeFromViewModel);
            console.log(`[loadedmetadata] Set current time to initial progress: ${initialTimeFromViewModel.toFixed(2)}s.`);
            // You might choose to auto-play here, or wait for user interaction.
            // player.play();
        }
    });

    player.on('seeking', function () {
        isSeeking = true;
        console.log(`[seeking] User started seeking. CurrentPlayerTime: ${player.currentTime().toFixed(2)}s, FurthestWatchedTime: ${furthestWatchedTime.toFixed(2)}s`);
    });

    player.on('seeked', function () {
        isSeeking = false;
        const currentTime = player.currentTime(); // The time after the seek operation completes
        console.log(`[seeked] User finished seeking. New CurrentPlayerTime: ${currentTime.toFixed(2)}s, FurthestWatchedTime: ${furthestWatchedTime.toFixed(2)}s`);

        // Only enforce sequential watching if the video is not yet completed
        if (!videoCompletedFromViewModel) {
            // Check if the user sought *forward* significantly beyond the furthest watched point + buffer
            if (currentTime > furthestWatchedTime + allowedForwardBuffer) {
                console.warn(`[seeked] DETECTED ILLEGAL FORWARD SEEK! From ${furthestWatchedTime.toFixed(2)}s to ${currentTime.toFixed(2)}s. Resetting time.`);
                player.currentTime(furthestWatchedTime); // Rewind to the furthest watched point
                player.pause(); // Pause to indicate the action was disallowed

                if (!hasAttemptedIllegalForwardSkip) {
                    alert('No puedes adelantar el video. Debes verlo secuencialmente.');
                    hasAttemptedIllegalForwardSkip = true; // Set flag to prevent multiple alerts
                }
            } else {
                // This branch is hit for legitimate backward seeks or minor forward jumps within the buffer.
                // IMPORTANT: furthestWatchedTime is *not* updated here. It will only update via 'timeupdate'
                // when the user plays past the current furthestWatchedTime. This allows rewinding.
                console.log(`[seeked] Allowed seek (backward or within buffer). CurrentPlayerTime: ${currentTime.toFixed(2)}s, FurthestWatchedTime: ${furthestWatchedTime.toFixed(2)}s.`);
                hasAttemptedIllegalForwardSkip = false; // Reset the flag after a valid seek/action
            }
        }
    });

    player.on('timeupdate', function () {
        // Only process time updates if not currently seeking and the video is not yet completed
        if (!isSeeking && !videoCompletedFromViewModel) {
            const currentTime = player.currentTime();

            // Uncomment the following line for very verbose timeupdate logging if needed for deep debugging
            // console.log(`[timeupdate] CurrentPlayerTime: ${currentTime.toFixed(2)}s, FurthestWatchedTime: ${furthestWatchedTime.toFixed(2)}s`);

            // This is the core logic for *allowing* rewind while restricting forward progress.
            // furthestWatchedTime only updates if the current time is truly *new* sequential progress.
            if (currentTime > furthestWatchedTime) { // User is playing *forward* past their furthest point
                // Check for sudden large jumps in currentTime that aren't explicit 'seeking' events.
                // This catches fast-forwarding via play controls or internal player glitches.
                if (currentTime > furthestWatchedTime + allowedForwardBuffer) {
                    console.warn(`[timeupdate] DETECTED UNEXPECTED FORWARD JUMP! From ${furthestWatchedTime.toFixed(2)}s to ${currentTime.toFixed(2)}s. Resetting.`);
                    player.currentTime(furthestWatchedTime); // Rewind to the furthest watched point
                    player.pause(); // Pause playback

                    if (!hasAttemptedIllegalForwardSkip) {
                        alert('El video se reinició debido a un avance no permitido.');
                        hasAttemptedIllegalForwardSkip = true; // Set flag to prevent multiple alerts
                    }
                } else {
                    // This is legitimate sequential progress (moving forward smoothly)
                    furthestWatchedTime = currentTime; // Update the furthest point watched
                    hasAttemptedIllegalForwardSkip = false; // Reset flag after valid progression

                    // Send progress to the server frequently, e.g., every 1-2 seconds of actual playback.
                    // This reduces API calls compared to sending on every timeupdate.
                    if (currentTime - (player.lastSentProgressTime || 0) >= 1) { // Send every 1 second of unique time
                        sendVideoProgress(tareaIdFromViewModel, currentTime, lastKnownDuration);
                        player.lastSentProgressTime = currentTime; // Store last time progress was sent
                    }
                }
            }
            // IMPORTANT: If currentTime <= furthestWatchedTime (meaning user rewound or time hasn't advanced),
            // furthestWatchedTime is *not* updated. This is precisely what allows re-watching without losing
            // the highest point reached.
        }
    });

    //player.on('ended', function () {
    //    console.log(`[ended] Video playback finished. CurrentPlayerTime: ${player.currentTime().toFixed(2)}s, FurthestWatchedTime: ${furthestWatchedTime.toFixed(2)}s, Video Duration: ${lastKnownDuration.toFixed(2)}s`);

    //    // Client-side check to prevent marking as completed if the user simply skipped to the end.
    //    // This acts as a first line of defense, but server-side validation is CRITICAL.
    //    if (!videoCompletedFromViewModel && lastKnownDuration > 0) {
    //        // Check if the furthest watched time is close enough to the end of the video
    //        if (furthestWatchedTime >= lastKnownDuration * completionThresholdPercentage) {
    //            console.log(`[ended] Video watched sufficiently. Marking as completed.`);
    //            sendVideoCompleted(tareaIdFromViewModel, lastKnownDuration);
    //            videoCompletedFromViewModel = true; // Update client-side status to reflect completion
    //            player.removeClass('vjs-no-seek'); // Remove custom CSS class as restrictions are lifted

    //            // Optionally, re-load Tarea details to reflect completion status in UI
    //            if (typeof loadTareaDetails === 'function') {
    //                loadTareaDetails(tareaIdFromViewModel);
    //            }
    //        } else {
    //            console.warn(`[ended] Video ended but NOT sufficiently watched (watched up to ${furthestWatchedTime.toFixed(2)}s / ${lastKnownDuration.toFixed(2)}s required ${(lastKnownDuration * completionThresholdPercentage).toFixed(2)}s). NOT marking as completed.`);
    //            alert('Para completar esta tarea de video, debes ver el video en su totalidad y de forma secuencial.');
    //            // Optionally, guide the user back to the point where they need to resume
    //            player.currentTime(furthestWatchedTime); // Go back to the furthest watched point
    //            player.play(); // And resume playing to encourage completion
    //        }
    //    }
    //});

    // ... (inside your initializeVideoPlayerForTarea function) ...

    player.on('ended', async function () { // <--- The 'async' MUST be right here
        console.log(`[ended] Video playback finished. CurrentPlayerTime: ${player.currentTime().toFixed(2)}s, FurthestWatchedTime: ${furthestWatchedTime.toFixed(2)}s, Video Duration: ${lastKnownDuration.toFixed(2)}s`);

        if (!videoCompletedFromViewModel && lastKnownDuration > 0) {
            if (furthestWatchedTime >= lastKnownDuration * completionThresholdPercentage) {
                console.log(`[ended] Video watched sufficiently. Attempting to mark as completed on server.`);

                try {
                    await sendVideoCompleted(tareaIdFromViewModel, lastKnownDuration); // <--- 'await' is valid here
                    console.log("[ended] Server confirmed completion. Now reloading task details to reflect status...");
                    videoCompletedFromViewModel = true;

                    player.removeClass('vjs-no-seek');
                    if (typeof loadTareaDetails === 'function') {
                        loadTareaDetails(tareaIdFromViewModel);
                    }
                } catch (error) {
                    console.error("[ended] Failed to mark video as completed on server, UI will not refresh.", error);
                }

            } else {
                console.warn(`[ended] Video ended but NOT sufficiently watched...`);
                alert('Para completar esta tarea de video, debes ver el video en su totalidad y de forma secuencial.');
                player.currentTime(furthestWatchedTime);
                player.play();
            }
        }
    });

    // ... (rest of your initializeVideoPlayerForTarea function) ...


    if (!videoCompletedFromViewModel) {
        player.addClass('vjs-no-seek');
    }

    // This listener handles removing the custom class when the video becomes completed.
    // It's triggered if the video completes during the current session.
    player.on('useractive', function () {
        if (videoCompletedFromViewModel && player.hasClass('vjs-no-seek')) {
            player.removeClass('vjs-no-seek');
            console.log(`[useractive] Video is now completed, removing 'vjs-no-seek' class.`);
        }
    });


    // --- Helper Functions for API Calls ---

    // Function to send video progress to the server
    async function sendVideoProgress(tareaId, currentTime, videoDuration) {
        try {
            const response = await fetch('/api/Tarea/SaveVideoProgress', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({ tareaId: tareaId, currentTime: currentTime, videoDuration: videoDuration })
            });
            if (!response.ok) {
                const errorText = await response.text();
                console.error('Failed to save video progress:', response.status, response.statusText, errorText);
            } else {
                // console.log(`Progress saved: TareaId: ${tareaId}, Time: ${currentTime.toFixed(2)}s`); // Keep this commented for less spam
            }
        } catch (error) {
            console.error('Network error while saving video progress:', error);
        }
    }

    // Function to mark the video task as completed on the server
    async function sendVideoCompleted(tareaId, videoDuration) {
        try {
            const response = await fetch('/api/Tarea/MarkVideoTaskCompleted', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({ tareaId: tareaId, videoDuration: videoDuration })
            });
            if (!response.ok) {
                const errorText = await response.text();
                console.error('Failed to mark task completed:', response.status, response.statusText, errorText);
                alert('Hubo un error al intentar marcar la tarea como completada. Por favor, inténtalo de nuevo.');
            } else {
                console.log(`Task ${tareaId} marked as completed on server.`);
            }
        } catch (error) {
            console.error('Network error while marking task completed:', error);
            alert('Error de red al intentar marcar la tarea como completada. Verifica tu conexión.');
        }
    }
};


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