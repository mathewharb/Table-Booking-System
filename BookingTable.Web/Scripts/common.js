function ShowPopup(data) {
    $("#popupContainer").html(data);
    $("#popupModal").modal("show");
}
function ClosePopup() {
    $("#popupContainer").html("");
    $("#popupModal").modal("hide");
}

function ShowMessage(data) {
    if (data.ClosePopup === true) {
        ClosePopup();
    }
    if (data.Type !== "Hidden") {

        if (data.Type === "Success") {
            $("#messageModal").addClass("modal-primary");
            $("#messageModal").removeClass("modal-danger");
            $("#messageTitle").text(data.Title);
            $("#messageContent").text(data.Content);
            $("#messageModal").modal("show");
            setTimeout(function () {
                $("#messageModal").modal("hide");
            }, 5000);
        } else if (data.Type === "Error") {
            $("#messageModal").addClass("modal-danger");
            $("#messageReloadModal").removeClass("modal-primary");
            $("#messageTitle").text(data.Title);
            $("#messageContent").text(data.Content);
            $("#messageModal").modal("show");
            setTimeout(function () {
                $("#messageModal").modal("hide");
            }, 5000);
        } else if (data.Type === "SuccessReload") {
            $("#messageReloadModal").addClass("modal-primary");
            $("#messageModal").removeClass("modal-danger");
            $("#messageReloadTitle").text(data.Title);
            $("#messageReloadContent").text(data.Content);
            $("#messageReloadModal").modal("show");
            setTimeout(function () {
                $("#messageReloadModal").modal("hide");
            }, 5000);
        } else if (data.Type === "ErrorReload") {
            $("#messageReloadModal").addClass("modal-danger");
            $("#messageReloadModal").removeClass("modal-primary");
            $("#messageReloadTitle").text(data.Title);
            $("#messageReloadContent").text(data.Content);
            $("#messageReloadModal").modal("show");
            setTimeout(function () {
                $("#messageReloadModal").modal("hide");
            }, 5000);
        } else {
            window.location.href = window.location.href
        }
        

    }
}
function readURL(input, review) {

    if (input.files && input.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {
            review.attr('src', e.target.result);
        }

        reader.readAsDataURL(input.files[0]);
    }
}
function addDefaultOption(selectList, objectText) {
    selectList
            .prepend($("<option></option>")
            .attr("value", "")
            .text(objectText));
}
function addSelectListItems(selectList, results, nodata) {
    selectList.html("");
    if (results.length === 0) {
        selectList.html('<option value="">' + nodata + '</option>');
        return;
    }
    if (results.length === 1) {
        selectList
                .append($("<option></option>")
                .attr("value", results[0].Value)
                .attr("selected", true)
                .text(results[0].Text));
        selectList.change();
        return;
    }
    $.each(results, function (i, value) {
        selectList.append($("<option></option>")
            .attr("value", value.Value).text(value.Text));
    });
}