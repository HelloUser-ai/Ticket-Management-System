function assignTicket(ticketId, assignedTo) {
    if (!assignedTo) return;
    if (!confirm("Assign this ticket to " + assignedTo + "?")) return;

    fetch('/Ticket/AssignTicket', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id: ticketId, assignedTo: assignedTo })
    })
    .then(res => res.json())
    .then(data => data.success ? location.reload() : alert("Assignment failed"));
}

function updateStatus(ticketId, status) {
    fetch('/Ticket/UpdateStatus', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id: ticketId, status: status })
    })
    .then(res => res.json())
    .then(data => data.success ? location.reload() : alert("Status update failed"));
}

function openTicket(id) {
    window.location.href = '/Ticket/Details/' + id;
}

function handleExport(format) {
    const clientName = document.querySelector('select[name="clientName"]').value;
    const search = document.querySelector('input[name="search"]').value;
    const status = document.querySelector('select[name="status"]').value;
    const fromDate = document.querySelector('input[name="fromDate"]').value;
    const toDate = document.querySelector('input[name="toDate"]').value;

    const urlParams = new URLSearchParams(window.location.search);
    const fallbackViewType = (window.ticketIndexConfig && window.ticketIndexConfig.viewType) || 'all';
    const viewType = urlParams.get('viewType') || fallbackViewType;

    const baseUrl = format === 'Excel' ? '/Ticket/ExportExcel' : '/Ticket/ExportPdf';
    const params = new URLSearchParams({
        search: search,
        status: status,
        clientName: clientName,
        assignedTo: "",
        fromDate: fromDate,
        toDate: toDate,
        viewType: viewType
    });

    window.location.href = baseUrl + '?' + params.toString();
}



