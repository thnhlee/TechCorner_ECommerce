document.addEventListener("DOMContentLoaded", function () {

    let variants = window.productVariants || [];

    let selected = {};
    let selectedVariant = null;

    if (!variants.length) return;

    // ================= BUILD MAP =================
    const attrMap = {};

    variants.forEach(v => {
        const attrs = v.attributes || v.Attributes || [];

        attrs.forEach(a => {
            const name = a.Name || a.name;
            const value = a.Value || a.value;

            if (!attrMap[name]) attrMap[name] = new Set();
            attrMap[name].add(value);
        });
    });

    // ================= RENDER =================
    const container = document.getElementById("attribute-container");
    if (!container) return;

    Object.keys(attrMap).forEach(name => {
        let html = `<div><b>${name}</b><br>`;

        attrMap[name].forEach(val => {
            html += `
                <button class="attr-btn"
                    data-name="${name}"
                    data-value="${val}">
                    ${val}
                </button>`;
        });

        html += `</div>`;
        container.innerHTML += html;
    });

    // ================= CLICK =================
    document.querySelectorAll(".attr-btn").forEach(btn => {
        btn.addEventListener("click", function () {

            const name = btn.dataset.name;
            const value = btn.dataset.value;

            // toggle chọn
            if (selected[name] === value) {
                delete selected[name];
                this.classList.remove("active");
            } else {
                selected[name] = value;

                // remove active cùng group
                document.querySelectorAll(`[data-name="${name}"]`)
                    .forEach(b => b.classList.remove("active"));

                this.classList.add("active");
            }


            updateAvailableOptions();
            findVariant();
        })
    });

    // ================= FIND VARIANT =================
    function findVariant() {

        selectedVariant = variants.find(v => {

            const attrs = v.attributes || v.Attributes || [];

            return attrs.every(a => {
                const name = a.Name || a.name;
                const value = a.Value || a.value;

                return selected[name] === value;
            });
        });

        if (selectedVariant) {
            document.getElementById("dynamic-price").innerText =
                `$${selectedVariant.price || selectedVariant.Price}`;

            document.getElementById("dynamic-stock").innerText =
                `${selectedVariant.stock || selectedVariant.Stock} available`;

        }
    }

    // ================= UPDATE VALID =================
    function updateAvailableOptions() {

        document.querySelectorAll(".attr-btn").forEach(btn => {

            const name = btn.dataset.name;
            const value = btn.dataset.value;

            // check xem option này có tồn tại trong bất kỳ variant hợp lệ nào không
            const isAvailable = variants.some(v => {

                const attrs = v.attributes || v.Attributes || [];

                return attrs.every(a => {
                    const n = a.Name || a.name;
                    const val = a.Value || a.value;

                    // bỏ qua attribute đang check
                    if (n === name) return val === value;

                    // nếu user đã chọn attribute khác → phải match
                    if (selected[n] && selected[n] !== val) return false;

                    return true;
                }) && attrs.some(a => {
                    const n = a.Name || a.name;
                    const val = a.Value || a.value;
                    return n === name && val === value;
                });

            });

            // UI: làm mờ nhưng KHÔNG disable click
            if (isAvailable) {
                btn.classList.remove("disabled");
            } else {
                btn.classList.add("disabled");
            }

        });
    }

    // ================= AUTO SELECT =================
    //function autoSelect() {

    //    const v = variants[0];
    //    if (!v) return;

    //    selectedVariant = v;

    //    const attrs = v.attributes || v.Attributes || [];

    //    attrs.forEach(a => {
    //        const name = a.Name || a.name;
    //        const value = a.Value || a.value;

    //        selected[name] = value;

    //        document.querySelectorAll(`[data-name="${name}"]`)
    //            .forEach(btn => {
    //                if (btn.dataset.value === value)
    //                    btn.classList.add("active");
    //            });
    //    });

    //    updateAvailableOptions();
    //    findVariant();

    //}

    //autoSelect();
    // ================= AUTO SELECT Cách 2 =================
    //function autoSelect() {
    //    if (!variants.length) return;

    //    selected = {};
    //    selectedVariant = null;

    //    updateAvailableOptions();
    //}
    //autoSelect();

    // ================= ADD TO CART =================
    document.querySelector(".btn-black")?.addEventListener("click", async function () {

        if (!selectedVariant) {
            toastr.error("Vui lòng chọn thuộc tính");
            return;
        }

        const qty = document.getElementById("quantity").value;

        const id = selectedVariant.id || selectedVariant.Id;


        try {
            const res = await fetch(`/Cart/AddToCart?variantId=${id}&quantity=${qty}`);
            const data = await res.json();

            if (data.success) {

                const cartQty = document.getElementById("cart-qty");
                if (cartQty) cartQty.innerText = data.quantity;

                toastr.success("Đã thêm vào giỏ hàng!");
            }

        } catch (err) {
            console.error(err);
            toastr.error("Lỗi server!");
        }

    });

    // ================= BUY =================
    document.querySelector(".btn-primary")?.addEventListener("click", async function () {

        if (!selectedVariant) {
            toastr.error("Vui lòng chọn thuộc tính");
            document.getElementById("dynamic-price").innerText = "$0";
            document.getElementById("dynamic-stock").innerText = "Out of stock";
            return;
        }

        const qty = document.getElementById("quantity").value;

        const id = selectedVariant.id || selectedVariant.Id;

        await fetch(`/Cart/AddToCart?variantId=${id}&quantity=${qty}`);

        window.location.href = "/Cart";
    });

    toastr.options = {
        closeButton: true,
        progressBar: true,
        positionClass: "toast-top-right",
        timeOut: "800"
    };

});