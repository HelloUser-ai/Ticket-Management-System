function loadClientDetails(id) {
    $.ajax({
        url: '/Client/GetClientDetails/' + id,
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            $('#editId').val(data.id);
            $('#editName').val(data.name);
            $('#editContact').val(data.contactPerson);
            $('#editEmail').val(data.email);

            var myModal = new bootstrap.Modal(document.getElementById('editClientModal'));
            myModal.show();
        },
        error: function () {
            alert("Client data load nahi ho paya!");
        }
    });
}
