(function ($) {
    $.validator.addMethod("requiredif", function (value, element, param) {
        var isChecked = $(param).is(":checked");
        var elementIsEmpty = $("#" + element.name).val().length === 0;

        if (isChecked && elementIsEmpty) {
            return false;
        }

        return true;
    }, "");

    $.validator.unobtrusive.adapters.add("requiredif", ["param"], function (options) {
        options.rules["requiredif"] = "#" + options.params.param;
        options.messages["requiredif"] = options.message;
    });
})(jQuery);