async function loadTareas(cursoId)
{
    const response = await fetch(
        `/profesor/tareas/GetByCurso?cursoId=${cursoId}`
    );

    const tareas = await response.json();

    let html = "";

    tareas.forEach(t => {

        html += `
        <div class="tarea-card"
             onclick="loadDetalle('${t.tareaId}')">

            <strong>${t.nombre}</strong>

            <small>${t.tipoEntregaEsperado}</small>

        </div>
        `;
    });

    document.getElementById("tareasList").innerHTML = html;
}

async function loadDetalle(tareaId)
{
    const response = await fetch(
        `/profesor/tareas/GetDetalle?tareaId=${tareaId}`
    );

    const tarea = await response.json();

    let content = `
        <h3>${tarea.nombre}</h3>
        <p>${tarea.descripcion ?? ''}</p>
    `;

    if (tarea.reunionUrl)
    {
        content += `
        <a href="${tarea.reunionUrl}"
           target="_blank"
           class="btn btn-primary">
            Abrir reunión
        </a>`;
    }

    if (tarea.tipo === "Documento")
    {
        content += `
        <a href="${tarea.archivoUrl}"
           target="_blank"
           class="btn btn-info">
           Ver documento
        </a>`;
    }

    if (tarea.tipo === "lectura")
    {
        content += `
        <a href="${tarea.archivoUrl}"
           target="_blank"
           class="btn btn-success">
           Abrir lectura
        </a>`;
    }

    if (tarea.tipo === "video")
    {
        content += `
        <video controls width="100%">
            <source src="${tarea.archivoUrl}">
        </video>`;
    }

    document.getElementById("tareaPreview").innerHTML = content;
}