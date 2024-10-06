    $('#datatable').DataTable({
        processing: true,
        serverSide: true,
        filter: true,
        ajax: {
            url: '/Home/GetData',
            type: 'POST',
            dataType: 'json'
        },
        columns: [
            { data: "id", name: "Id", autowidth: true },
            { data: "productName", name: "ProductName", autowidth: true },
            { data: "brand", name: "Brand", autowidth: true },
            { data: "category", name: "Category", autowidth: true },
            { data: "price", name: "Price", autowidth: true },
            { data: "dateOfPurchase", name: "DateOfPurchase", autowidth: true }
        ]
    });
