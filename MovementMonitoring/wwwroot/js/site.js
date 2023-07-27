// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    jQueryModalGet = (url, title) => {
        try {
            var uri = window.location.origin + url
            $.ajax({
                type: 'GET',
                url: uri,
                contentType: false,
                processData: false,
                success: function (res) {
                    $('#form-modal .modal-body').html(res.html);
                    $('#form-modal .modal-title').html(title);
                    $('#form-modal').modal('show');
                },
                error: function (err) {
                    console.log(uri)
                    console.log(err)
                }
            })
            return false;
        } catch (ex) {
            console.log(ex)
        }
    }
    jQueryModalPost = form => {
        try {
            $.ajax({
                type: 'POST',
                url: form.action,
                data: new FormData(form),
                contentType: false,
                processData: false,
                success: function (res) {
                    if (res.isValid) {
                        $('#viewAll').html(res.html)
                        $('#form-modal').modal('hide');
                    }
                },
                error: function (err) {
                    console.log(err)
                }
            })
            return false;
        } catch (ex) {
            console.log(ex)
        }
    }
    jQueryModalDelete = form => {
        if (confirm('Вы точно хотите удалить эту запись?')) {
            try {
                $.ajax({
                    type: 'POST',
                    url: form.action,
                    data: new FormData(form),
                    contentType: false,
                    processData: false,
                    success: function (res) {
                        $('#viewAll').html(res.html);
                    },
                    error: function (err) {
                        console.log(err)
                    }
                })
            } catch (ex) {
                console.log(ex)
            }
        }
        return false;
    }

    createTable = () => {
        let table = $("#table").DataTable({
            stateSave: true,
            language: {
                url: "//cdn.datatables.net/plug-ins/1.13.4/i18n/ru.json",
            },
            pagingType: "full_numbers",
            lengthMenu: [5, 10, 25, 50, 100],
            initComplete: function () {
                var count = this.api().columns().count();
                this.api().columns()
                    .every(function () {
                        var column = this;
                        var select = $('<select class="form-select form-select-sm"><option value="">Все</option></select>')
                            .appendTo($(column.footer()).empty())
                            .on('change', function () {
                                var val = $.fn.dataTable.util.escapeRegex($(this).val());
                                column.search(val ? '^' + val + '$' : '', true, false).draw();
                            });
                        if (column.index() === (count - 1)) {
                            $(column.footer()).empty()
                        }
                        column.data().unique().sort().each(function (d, j) {
                            select.append('<option value="' + d + '">' + d + '</option>');
                        });
                    });
            },
        });
        $('#table tfoot tr').appendTo('#table thead');
    }
});