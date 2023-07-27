"use strict";

function CheckViolationNotification() {
    var connection = new signalR.HubConnectionBuilder().withUrl("/ViolationNotification").build();
    console.log('connection')
    connection.on("ViolationNotify", function (violationId, dateTime, roomName) {
        let uniqueId = Date.now().toString(36) + Math.random().toString(36).substring(2);
        const toastGroup = document.getElementById('toastGroup')
        let html = toastGroup.innerHTML
        toastGroup.innerHTML = html + `
        <div id="`+uniqueId+`" class="toast" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="toast-body">
                В помещении `+roomName+` в `+dateTime+` зафиксировано нарушение!
                <div class="mt-2 pt-2 border-top">
                    <button type="button" class="btn btn-sm btn_green btn_sm_round" onclick="jQueryModalGet('/CRUD/ViolationPage?handler=Details&id=`+violationId+`','Подробнее о нарушении');"><b>Подробнее</b></button>
                    <button type="button" class="btn btn-sm btn_dark btn_sm_round" data-bs-dismiss="toast"><b>Закрыть</b></button>
                </div>
            </div>
        </div>`
        const toastLiveExample = document.getElementById(uniqueId)
        const toastBootstrap = bootstrap.Toast.getOrCreateInstance(toastLiveExample)
        toastBootstrap.show()
    });


    connection.start().then(function () { }).catch(function (err)
    {
        return console.error(err.toString());
    });
}