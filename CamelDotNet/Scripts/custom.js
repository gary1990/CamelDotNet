function removeNestedForm(element, container, deleteElement) {
    $container = $(element).parents(container);
    $container.find(deleteElement).val('True');
    $container.hide();

}

function realRemoveNestedForm(element, container, deleteElement) {
    $container = $(element).parents(container);
    $container.remove();

}

function addNestedForm(container, counter, ticks, content) {
    var nextIndex = $(counter).length;
    var pattern = new RegExp(ticks, "gi");
    content = content.replace(pattern, nextIndex);
    $(container).append(content);
}

function approve(relatedId, value) {
    $("#" + relatedId).val(value)
}

function getFileExt(filename) {
    // Use a regular expression to trim everything before final dot
    var extension = filename.replace(/^.*\./, '');

    // Iff there is no dot anywhere in filename, we would have extension == filename,
    // so we account for this possibility now
    if (extension == filename) {
        extension = '';
    } else {
        // if there is an extension, we convert to lower case
        // (N.B. this conversion will not effect the value of the extension
        // on the file upload.)
        extension = extension.toLowerCase();
    }
    return extension;
}

function setFilter() {
    var fields = $(".filter").serializeArray();
    var tmpVal = "";
    jQuery.each(fields, function (i, field) {
        if (field.value != null && field.value != "") {
            tmpVal += (field.name + ":" + field.value + ";");
        }
    });
    $("#filter").val(tmpVal);
}