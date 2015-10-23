$("#weatherSearchForm").submit(function (e) {
    e.preventDefault();
    
    // Get info
    var submitUrl = "/" + $(this).data("controller") + "/" + $(this).data("action");
    var formData = $(this).serialize();
    var address = $("#Address", this).val();
    
    getWeatherInfo(submitUrl, formData, address);
});

function getWeatherInfo(submitUrl, formData, address) {
    // display a loading icon

    $.ajax({
        url: submitUrl,
        data: formData,
        method: "POST"
    })
    .done(function (data) {
        //stop loading icon

        $("#weatherSearch").html(data);

        storageEngine.init(function () {
            storageEngine.initObjectStore('weatherQueries', function () {
                // Add query to storage for access by user in the future
                var objToSave = {};
                objToSave["id"] = address;
                objToSave["dateTime"] = $.now();
                storageEngine.save('weatherQueries', objToSave, function () { }, errorLogger);

                storageEngine.findAll('weatherQueries', function (weatherQueries) {
                    if (weatherQueries.length > 5) {
                        var oldestObj = {};

                        // Find oldest query
                        $.each(weatherQueries, function (index, weatherObj) {
                            if (!oldestObj["dateTime"] || weatherObj["dateTime"] < oldestObj["dateTime"]) {
                                oldestObj = weatherObj;
                            }
                        });

                        // Remove oldest query
                        storageEngine.delete('weatherQueries', oldestObj["id"], function () { }, errorLogger);
                    }
                }, errorLogger);
            }, errorLogger)
        }, errorLogger);
    })
    .fail(function (e, x, r) {
        alert("Failed to retrieve weather info.");
    });
}

$("#previousSearch").on("click", ".previousSearchLink", function (e) {
    e.preventDefault();

    var searchValue = $(e.target).text();
    $("#Address").val(searchValue);
    $("#weatherSearchForm").submit();
});

$(document).ready(function () {
    storageEngine.init(function () {
        storageEngine.initObjectStore('weatherQueries', function () {
            storageEngine.findAll('weatherQueries', function (weatherQueries) {
                var previousSearchList = $('#previousSearchList');

                // Add all queries to page
                if (weatherQueries.length > 0) {

                    // Add header
                    $("#previousSearchHeader").html("Previous Searches");

                    // Sort by date
                    weatherQueries.sort(function (a, b) {
                        return new Date(b["dateTime"]) - new Date(a["dateTime"]);
                    });

                    // Add previous weather queries to list
                    $.each(weatherQueries, function (index, weatherObj) {
                        previousSearchList.append("<p'><a href='#' class='previousSearchLink'>" + weatherObj["id"] + "</a></p>");
                    });
                }
            }, errorLogger);
        }, errorLogger)
    }, errorLogger);
});

function errorLogger(errorCode, errorMessage) {
    alert(errorCode + ':' + errorMessage);
}

// Used to display a "loading" modal
$body = $("body");
$(document).on({
    ajaxStart: function () { $body.addClass("loading"); },
    ajaxStop: function () { $body.removeClass("loading"); }
});