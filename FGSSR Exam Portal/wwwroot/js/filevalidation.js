function validateFile(inputId, onlyExcel) {
    var errorMsg = "";
    var result = "";
    var containInvalidExcel = false;
    const fi = document.getElementById(inputId);
    // Check if any file is selected.
    if (fi.files.length > 0) {
        var inbyte = 0;
        var inmb = 0;
        var containSepcialCharacter = false;
        for (var i = 0; i <= fi.files.length - 1; i++) {
            var filename = fi.files.item(i).name;
            //console.log(filename + ' filename');
            //console.log(onlyExcel + ' onlyExcel');
            if (onlyExcel == true) {
                if (filename.includes("xlsx") || filename.includes("xlsm") || filename.includes("xls") || filename.includes("csv")) {
                    containInvalidExcel = false;
                } else {
                    containInvalidExcel = true;
                }
            }
            var fileNameExcludeExtension = filename.split('.').slice(0, -1).join('.');
            //var regexp = /^[a-zA-Z0-9-_()% ]*$/;
            var regexp = /[`!@#$%^&*+\=\[\]{};':"\\|,.<>\/?~]/;
            if (regexp.test(fileNameExcludeExtension) == true) {
                containSepcialCharacter = true;
            }
            inbyte += fi.files.item(i).size;
            inmb += Math.round((inbyte / 1024));
        }
        if (containSepcialCharacter == true) {
            errorMsg += "<li>File name cannot contain special characters. Only letter, digit, hyphen, underscore, brackets, and percent are allowed.</li>";
        }
        if (inbyte > 52428800) {
            errorMsg += "<li>Total file size cannot be larger than 50mb.</li>";
        }
        /*console.log(containInvalidExcel + ' containInvalidExcel');*/
        if (containInvalidExcel == true) {
            errorMsg += "<li>Please upload a valid excel file.</li>";
        }

        if (errorMsg != "") {
            result = "<ul>" + errorMsg + "</ul>";
        }
        return result;
    }
}