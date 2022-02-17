window.ShowToastr = (type, title, message) => {
    if (type === "success") {
        toastr.success(message, title, { timeOut: 10000 });
    }
    else if (type === "error") {
        toastr.error(message, title, { timeOut: 10000 });
    }
}

window.ShowSwal = (type, message) => {
    if (type === "success") {
        Swal.fire(
            'Success Notification',
            message,
            'success'
        )
    }
    if (type === "error") {
        Swal.fire(
            'Error Notification',
            message,
            'error'
        )
    }
}