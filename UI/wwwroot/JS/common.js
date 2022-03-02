window.ShowToastr = (type, title, message) => {
    if (type === "success") {
        toastr.success(message, title, { timeOut: 10000 });
    }
    else if (type === "error") {
        toastr.error(message, title, { timeOut: 10000 });
    }
}

window.ShowSwal = (type, title, message) => {
    if (type === "success") {
        Swal.fire(
            title,
            message,
            'success'
        )
    }
    else if (type === "error") {
        Swal.fire(
            title,
            message,
            'error'
        )
    }
}