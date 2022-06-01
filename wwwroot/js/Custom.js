function deleteFunction(uniqueId, deleteButtonClicked) {
    deleteButton = "deleteButton_" + uniqueId;
    deleteConfirmation = "deleteConfirmation_" + uniqueId;
    if (deleteButtonClicked) {
        $("#" + deleteButton).hide();
        $("#" + deleteConfirmation).show()
    } else {
        $("#" + deleteButton).show();
        $("#" + deleteConfirmation).hide();
    }

}