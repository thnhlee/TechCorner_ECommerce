document.addEventListener("DOMContentLoaded", function () {

    let variants = window.productVariants || [];

    let selected = {};
    let selectedVariant = null;

    if (!variants.length) return;

    // ================= FILTER STOCK =================
    const availableVariants = variants.filter(v => {
        const stock = v.stock || v.Stock || 0;
        return stock > 0;
    });

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

    function escapeHtml(value) {
        return String(value ?? "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    async function postAddToCart(productId, quantity) {
        const res = await fetch("/Cart/AddToCart", {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded",
                "RequestVerificationToken": getToken()
            },
            body: new URLSearchParams({ productId, quantity })
        });

        if (!res.ok) {
            throw new Error("Cart request failed");
        }

        return res.json();
    }

    // ================= RENDER =================
    const container = document.getElementById("attribute-container");
    if (!container) return;

    Object.keys(attrMap).forEach(name => {
        let html = `
            <div class="variant-attribute-group">
                <span class="variant-attribute-title">${escapeHtml(name)}</span>
                <div class="variant-attribute-options">`;

        attrMap[name].forEach(val => {
            html += `
                <button class="attr-btn"
                    data-name="${escapeHtml(name)}"
                    data-value="${escapeHtml(val)}">
                    ${escapeHtml(val)}
                </button>`;
        });

        html += `
                </div>
            </div>`;
        container.innerHTML += html;
    });

    // ================= CHECK PRODUCT STOCK =================
    if (!availableVariants.length) {
        document.getElementById("dynamic-price").innerText = "$0";
        document.getElementById("dynamic-stock").innerText = "Out of stock";

        document.querySelectorAll(".attr-btn").forEach(btn => {
            btn.classList.add("disabled");
        });

        document.querySelector(".product-add-btn")?.setAttribute("disabled", true);
        document.querySelector(".product-buy-btn")?.setAttribute("disabled", true);

        return;
    }

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



    // ================= UPDATE AVAILABLE ATTRIBUTE =================
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

                    // attribute đang xét chính là attribute của button => phải match với value của button
                    if (n === name) return val === value;

                    // attribute khác => nếu đã chọn thì phải match với giá trị đã chọn, chưa chọn thì bỏ qua
                    if (selected[n] && selected[n] !== val) return false;

                    return true;
                }); //&& attrs.some(a => {
                //    const n = a.Name || a.name;
                //    const val = a.Value || a.value;
                //    return n === name && val === value;
                //});

                if (isAvailable) {
                    btn.classList.remove("disabled");
                } else {
                    btn.classList.add("disabled");
                }
            
            });

            // UI: làm mờ nhưng KHÔNG disable click
            if (isAvailable) {
                btn.classList.remove("disabled");
            } else {
                btn.classList.add("disabled");
            }

        });
    }


    // ================= FIND VARIANT =================
    function findVariant() {

        selectedVariant = variants.find(v => {

            const attrs = v.attributes || v.Attributes || [];

            return attrs.every(a => {
                const name = a.Name || a.name;
                const value = a.Value || a.value;

                return selected[name] === value;
            });
        }) || null;

        if (selectedVariant) {
            document.getElementById("dynamic-price").innerText =
                `$${selectedVariant.price || selectedVariant.Price}`;

            document.getElementById("dynamic-stock").innerText =
                `${selectedVariant.stock || selectedVariant.Stock} available`;
        } else {
            //document.getElementById("dynamic-price").innerText = "$0";
            //document.getElementById("dynamic-stock").innerText = "Out of stock";
        }
    }


    // ================= INIT =================
    updateAvailableOptions();



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
    document.querySelector(".product-add-btn")?.addEventListener("click", async function () {

        if (!selectedVariant) {
            toastr.error("Vui lòng chọn thuộc tính");
            return;
        }

        const qty = document.getElementById("quantity").value;
        const id = selectedVariant.id || selectedVariant.Id;
        const stock = selectedVariant.stock || selectedVariant.Stock || 0;

        if (qty > stock) {
            toastr.error("Vượt quá tồn kho!");
            return;
        } else if (qty <= 0 || isNaN(qty)) {
            toastr.error("Số lượng không hợp lệ!");
            return;
        }

        try {
            const data = await postAddToCart(id, qty);

            if (data.success) {

                const cartQty = document.getElementById("cart-qty");
                if (cartQty) cartQty.innerText = data.quantity;

                toastr.success("Đã thêm vào giỏ hàng!");
            } else {
                toastr.error(data.message || "Không thể thêm vào giỏ hàng!");
            }

        } catch (err) {
            console.error(err);
            toastr.error("Lỗi server!");
        }

    });

    // ================= BUY =================
    document.querySelector(".product-buy-btn")?.addEventListener("click", async function () {

        if (!selectedVariant) {
            toastr.error("Vui lòng chọn thuộc tính");
            return;
        }

        const qty = document.getElementById("quantity").value;
        const id = selectedVariant.id || selectedVariant.Id;
        const stock = selectedVariant.stock || selectedVariant.Stock || 0;

        if (qty > stock) {
            toastr.error("Vượt quá tồn kho!");
            return;
        } else if (qty <= 0 || isNaN(qty)){ 
            toastr.error("Số lượng không hợp lệ!");
            return;
        }

        try {
            const data = await postAddToCart(id, qty);

            if (!data.success) {
                toastr.error(data.message || "Không thể thêm vào giỏ hàng!");
                return;
            }
        } catch (err) {
            console.error(err);
            toastr.error("Lỗi server!");
            return;
        }

        window.location.href = "/Cart";
    });



    toastr.options = {
        closeButton: true,
        progressBar: true,
        positionClass: "toast-top-right",
        timeOut: "800"
    };

});
