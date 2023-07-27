"use strict";

function CheckUpdate(tableName) {
    var connection = new signalR.HubConnectionBuilder().withUrl("/UpdateTable").build();

    connection.on("ReceiveUpdateTableNotify", function (table) {
        if (table === tableName) {
            $(document).ready(function () {
                $('#viewAll').load('?handler=ViewAllPartial');
            });
        }
    });

    connection.start().then(function () { }).catch(function (err)
    {
        return console.error(err.toString());
    });
}