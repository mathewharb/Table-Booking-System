function ShowPopup(data) {
    $("#popupContainer").html(data);
    $("#popupModal").modal("show");
}
function ClosePopup() {
    $("#popupContainer").html("");
    $("#popupModal").modal("hide");
}
function ShowNotification(data) {

    $("#messageTitle").text(data.Title);
    $("#messageContent").text(data.Content);

    if (data.Type === "success") {
        ClosePopup();
        $("#messageModal").addClass("modal-primary");
    } else {
        $("#messageModal").addClass("modal-danger");
    }

    $("#messageModal").modal("show");

    if (data.Type === "success") {
        setTimeout(function() {
                history.go(0);
            },
            1000);
    } else {
        setTimeout(function () {
                $("#messageModal").modal("hide");
        },
            1500);
    }
}