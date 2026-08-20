window.csTomlPlayground = {
    clickElement: function (id) {
        const element = document.getElementById(id);
        if (element) {
            element.click();
        }
    },
    copyText: function (text) {
        return navigator.clipboard.writeText(text ?? "").then(function () { return true; }, function () { return false; });
    }
};
