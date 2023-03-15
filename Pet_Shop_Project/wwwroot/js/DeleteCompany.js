$(document).on("click", ".delete-link", function () {
    var id = $(this).data("id");
    if (confirm("Are you sure you want to delete this item?")) {
        $.ajax({
            type: "DELETE",
            url: "/Company/Delete/" + id,
            success: function (data) {
                if (data.success) {
                    alert(data.message);
                    window.location.href = `@Url.Action("Index", "Company")`;
                } else {
                    alert(data.message);
                }
            },
            error: function () {
                alert("An error occurred while processing your request.");
            }
        });
    }
    return false;
});