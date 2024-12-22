//show back to top button at bottom right corner after the page scrolled
const topnav = document.querySelector("#top-navigation");
void 0 !== topnav && null != topnav && document.addEventListener("scroll", () => {
    window.scrollY > 99 ? topnav.classList.add("scrolled") : topnav.classList.remove("scrolled"), topnav.classList.contains("scrolled-shadow") && (window.scrollY > 100 ? topnav.classList.add("shadow") : topnav.classList.remove("shadow"))
});
const scrollTop = document.querySelector(".scroll-top");
if (void 0 !== scrollTop && null != scrollTop) {
    let d = function () {
        window.scrollY > 99 ? scrollTop.classList.add("active") : scrollTop.classList.remove("active")
    },
        g = function () {
            window.scrollTo({
                top: 0,
                behavior: "smooth"
            })
        };
    window.addEventListener("load", d), document.addEventListener("scroll", d), scrollTop.addEventListener("click", g)
}

//hide the dummy header and footer of a table after finish loading table data
function hideDummySpinnerHeaderFooter(tablewrapperid) {
    let spinner = document.querySelector("#" + tablewrapperid + " .spinner");
    let dummyFooter = document.querySelector("#" + tablewrapperid + " .dummyfooter");
    spinner.classList.remove("d-flex");
    spinner.classList.add("d-none");
    dummyFooter.classList.add("d-none");
}

//set background image of div with data-img tag
var bgimg = document.querySelector("[data-img]");
if (void 0 !== bgimg && null != bgimg)
    for (var element, dataimgs = document.querySelectorAll("[data-img]"), i = 0; element = dataimgs[i]; i++) {
        var h = element.getAttribute("data-img"),
            a = element.getAttribute("data-img-position"),
            b = element.getAttribute("data-img-attachment");
        element.style.background = "url(" + h + ")", void 0 !== a && null != a ? element.style.backgroundPosition = a : element.style.backgroundPosition = "center center", void 0 !== b && null != b ? element.style.backgroundAttachment = b : element.style.backgroundAttachment = "scroll", element.style.backgroundSize = "cover", element.style.backgroundRepeat = "no-repeat"
    }

// self executing function
(function () {
    initDropdownlist();
})();


//initialize drop down list
function initDropdownlist() {
    var selectwrapper = document.querySelector('.select-wrapper');
    if (void 0 !== selectwrapper && null != selectwrapper) {
        for (const ddl of document.querySelectorAll(".select-wrapper")) {
            ddl.addEventListener('click', function () {
                this.querySelector('.select').classList.toggle('open');
            });
        }
        window.addEventListener('click', function (e) {
            for (const select of document.querySelectorAll(".select")) {
                if (!select.contains(e.target)) {
                    select.classList.remove('open');
                }
            }
        });
    }
    var ddls = document.querySelector(".select-wrapper");
    if (void 0 !== ddls && null != ddls) {
        for (const ddl of document.querySelectorAll(".select-wrapper")) {
            let selected = ddl.querySelector('.custom-option.selected');
            if (void 0 !== selected && null != selected) {
                let hiddenInput = ddl.querySelector('.select input.custom-option');
                if (void 0 !== hiddenInput && null != hiddenInput) {
                    hiddenInput.setAttribute("value", selected.getAttribute("data-value"));
                }
            }
        }
    }
    var ddlOptions = document.querySelector(".select input.custom-option");
    if (void 0 !== ddlOptions && null != ddlOptions) {
        for (const option of document.querySelectorAll(".custom-option")) {
            option.addEventListener('click', function () {
                this.parentNode.querySelector('.custom-option.selected').classList.remove('selected');
                this.classList.add('selected');
                this.closest('.select').querySelector('.select__trigger span').textContent = this.textContent;
                var input = this.closest('.select').querySelector('input');
                if (void 0 !== input && null != input) {
                    this.closest('.select').querySelector('input').value = this.getAttribute("data-value");
                }
            })
        }
    }
}

//this will reset drop down list to "Please select"
function resetDropDownList(elementId) {
    var ddlOptions = document.querySelector("#" + elementId + " .select-wrapper .custom-option");
    if (void 0 !== ddlOptions && null != ddlOptions) {
        for (const option of document.querySelectorAll("#" + elementId + " .select-wrapper .custom-option")) {
            //console.log(option.getAttribute("data-value"));
            if (option.getAttribute("data-value") == "null" || option.getAttribute("data-value") == null) {
                option.classList.add('selected');
            } else {
                if (option.classList.contains('selected')) {
                    option.classList.remove('selected');
                }
            }
            let pleaseSelectNode = option.parentNode.querySelector('.custom-option');
            option.closest('.select').querySelector('.select__trigger span').textContent = pleaseSelectNode.textContent;
            var input = option.closest('.select').querySelector('input');
            if (void 0 !== input && null != input) {
                option.closest('.select').querySelector('input').value = pleaseSelectNode.getAttribute("data-value");
            }
        }
    }
}

function setNewOptionsForDdl(elementId, text, value) {
    $(`#${elementId} .custom-options`).append(`<span class='custom-option' data-text='${text}' data-value='${value}' value='${value}'>${text}</span>`);
    for (const option of document.querySelectorAll(`#${elementId} .custom-options .custom-option`)) {
        option.addEventListener('click', function () {
            this.parentNode.querySelector('.custom-option.selected').classList.remove('selected');
            this.classList.add('selected');
            this.closest('.select').querySelector('.select__trigger span').textContent = this.textContent;
            var input = this.closest('.select').querySelector('input');
            if (void 0 !== input && null != input) {
                this.closest('.select').querySelector('input').value = this.getAttribute("data-value");
            }
        })
    }
}

//this function will adjust the table that export to pdf to fit the width of pdf
function adjustPdfColWidth(tableIdPrefix) {
    var colCount = new Array();
    $('#' + tableIdPrefix + '-table').find('tbody tr:first-child td').each(function () {
        let col = $(this).html();
        if (col.includes("actioncol") == false) {
            if ($(this).attr('colspan')) {
                for (var i = 1; i <= $(this).attr('colspan'); $i++) {
                    colCount.push('*');
                }
            }
            else { colCount.push('*'); }
        }
    });
    return colCount;
}

//this function will exclude the action column when export to pdf/excel
function getTotalColumns(tableIdPrefix) {
    var colsToBeExported = new Array();
    var count = 0;
    $('#' + tableIdPrefix + '-table').find('tbody tr:first-child td').each(function () {
        colsToBeExported.push(count);
        count++;
    });
    return colsToBeExported;
}

function convertToLocalDatetimeIsoString(isoUtcString) {
    const dateTimeUtc = new Date(isoUtcString);
    const dateTimeLocal = new Date(dateTimeUtc.getTime() - dateTimeUtc.getTimezoneOffset() * 60 * 1000);
    const formattedDateTime = dateTimeLocal.getFullYear() + '-' +
        (dateTimeLocal.getMonth() + 1).toString().padStart(2, '0') + '-' +
        dateTimeLocal.getDate().toString().padStart(2, '0') + 'T' +
        dateTimeLocal.getHours().toString().padStart(2, '0') + ':' +
        dateTimeLocal.getMinutes().toString().padStart(2, '0');
    return formattedDateTime; //example: 2023-02-11T14:44 (this format used in <input type="datetime-local"> element)
}

function getFormattedDateTime(value) {
    const dateTimeUtc = new Date(value);
    const dateTimeLocal = new Date(dateTimeUtc.getTime() - dateTimeUtc.getTimezoneOffset() * 60 * 1000);
    var fullDateTime = dateTimeLocal.toLocaleTimeString([], {
        year: 'numeric', //numeric = 2022, 2-digit = 22
        month: '2-digit', //2-digit = 12, short = Dec, long = December
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    });
    //example: 02/11/2023, 09:44 PM
    //this format used in any element for display the date time text
    //Note: do not use this in input element, instead, call convertToLocalDatetimeIsoString function when it's an input
    return fullDateTime;
}

function getFormattedDate(value) {
    const dateTimeUtc = new Date(value);
    const dateTimeLocal = new Date(dateTimeUtc.getTime() - dateTimeUtc.getTimezoneOffset() * 60 * 1000);
    var fullDate = dateTimeLocal.toLocaleDateString([], {
        year: 'numeric', //numeric = 2022, 2-digit = 22
        month: '2-digit', //2-digit = 12, short = Dec, long = December
        day: '2-digit'
    });
    //example: 02/11/2023
    //this format used in any element for display the date text
    //Note: do not use this in input element, instead, call convertToLocalDatetimeIsoString function when it's an input
    return fullDate;
}
formatDateTimeText();
function formatDateTimeText() {
    var dtText = document.querySelector(".datetimetext");
    if (dtText) {
        for (const option of document.querySelectorAll(".datetimetext")) {
            var dt = $(option).text();
            if (dt != "" && dt != null) {
                var result = getFormattedDateTime(dt);
                $(option).text(result);
            }
        }
    }
}

var dtInput = document.querySelector("input.datetimetext-input");
if (dtInput) {
    for (const datetimeInput of document.querySelectorAll("input.datetimetext-input")) {
        let datetime = new Date(datetimeInput.value);
        let clientMachineDateTime = new Date(datetime.getTime() - datetime.getTimezoneOffset() * 60 * 1000);
        datetimeInput.value = clientMachineDateTime.toISOString().slice(0, -1);
    }
}
formatDateText();
function formatDateText() {
    var dText = document.querySelector(".datetext");
    if (dText) {
        for (const option of document.querySelectorAll(".datetext")) {
            var dt = $(option).text();
            if (dt != "" && dt != null) {
                var result = getFormattedDate(dt);
                $(option).text(result);
            }
        }
    }
}

//for multi-select drop down list
var multichoice = document.querySelector("select.multichoice");
if (void 0 !== multichoice && null != multichoice)
    for (var element, multichoices = document.querySelectorAll("select.multichoice"), i = 0; element = multichoices[i]; i++) {
        var multiselect = new Choices(element, {
            removeItemButton: true
        });
    }

//initialize bootstrap tooltip
var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl)
});

//Hide success message at top right corner automatically after 2500ms (2.5 second)
setTimeout(() => {
    $('#successtoast-container .toast.show').removeClass('show');
    $('#successtoast-container').hide();
}, 2500);

//Hide fail message at top right corner automatically after 4500ms (4.5 second)
setTimeout(() => {
    $('#failedtoast-container .toast.show').removeClass('show');
    $('#failedtoast-container').hide();
}, 4500);

setTimeout(() => {
    $('#notifytoast-container .toast.show').removeClass('show');
    $('#notifytoast-container').hide();
}, 2500);

//Convert 100,000 to 100K etc (Not used in this project, but might used in other projects)
function getNumberAbbreviation(a) {
    var e = a;
    if (a >= 1e3) {
        for (var f = ["", "k", "m", "b", "t"], c = Math.floor(("" + a).length / 3), b = "", d = 2; d >= 1 && !(((b = parseFloat((0 != c ? a / Math.pow(1e3, c) : a).toPrecision(d))) + "").replace(/[^a-zA-Z 0-9]+/g, "").length <= 2); d--);
        b % 1 != 0 && (b = b.toFixed(1)), e = b + f[c]
    }
    return e
}

//copy to clipboard (Not used in this project, but might used in other projects)
function copyToClipboard(b, c) {
    var a = document.createElement("input"),
        d = document.querySelector("#" + b).innerText;
    a.value = d, document.body.appendChild(a), a.select(), document.execCommand("copy"), document.body.removeChild(a), new bootstrap.Modal(document.getElementById(c), {}).show()
}

function exportToExcel(tableIdPrefix, tableNotFoundMessage) {
    var table = document.getElementById(tableIdPrefix + "-table");
    if (table != null) {
        /* Create worksheet from HTML DOM TABLE */
        var wb = XLSX.utils.table_to_book(table);
        /* Export to file (start a download) */
        XLSX.writeFile(wb, `${fileName}.xlsx`);
    } else {
        $("#notifytoast .toast-body").text(tableNotFoundMessage);
        $('#notifytoast-container').show();
        $("#notifytoast").addClass("show");
    }
}

function exportToPdf(tableIdPrefix, tableNotFoundMessage) {
    // Get the HTML table element
    var table = document.getElementById(tableIdPrefix + "-table");
    if (table != null) {
        // Define the columns for the table
        var columns = [];
        var headers = table.querySelectorAll("th:not(.notexport)");
        headers.forEach(function (header) {
            //ignore the action column when export to pdf
            if (header.innerText != "" || header.innerText != " ") {
                columns.push({ text: header.innerText, style: "tableHeader" });
            }
        });

        // Define the data for the table
        var data = [];
        var rows = table.querySelectorAll("tbody tr");
        rows.forEach(function (row) {
            var rowData = [];
            var cells = row.querySelectorAll("td:not(.notexport)");
            cells.forEach(function (cell) {
                if (cell.innerHTML.includes("actioncol") == false) {
                    rowData.push(cell.innerText);
                }
            });
            data.push(rowData);
        });

        var colWidth = [];
        columns.forEach(function (col) {
            colWidth.push("auto");
        });

        // Define the pdfmake table definition
        var tableDefinition = {
            headerRows: 1,
            widths: colWidth, // Set the column widths
            body: [columns].concat(data), // Add the column headers to the beginning of the data array
            style: "tableStyle", // Apply a custom style to the table
        };

        // Define the pdfmake document definition
        var docDefinition = {
            content: [
                {
                    table: tableDefinition, // Add the table definition to the pdfmake document definition
                },
            ],
            styles: {
                tableHeader: {
                    bold: true,
                    fontSize: 12,
                    color: "black",
                    alignment: "center",
                },
                tableStyle: {
                    margin: [0, 5, 0, 15],
                    fontSize: 9,
                },
            },
        };

        // Create the pdf document and download it
        pdfMake.createPdf(docDefinition).download(`${fileName}.pdf`);
    } else {
        $("#notifytoast .toast-body").text(tableNotFoundMessage);
        $('#notifytoast-container').show();
        $("#notifytoast").addClass("show");
    }

}

function openHintToast(displayText) {
    var toastBody = document.querySelector('#selectedtoast .toast-body');
    toastBody.innerText = displayText;
    var toastEl = document.querySelector('#selectedtoast');
    var toast = new bootstrap.Toast(toastEl);
    toast.show();
}