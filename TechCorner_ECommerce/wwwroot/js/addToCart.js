const PopupProduct = (() => {

    let state = {
        variants: [],
        selected: {},
        selectedVariant: null,
        product: null
    };

    let modal;

    /* ================= OPEN ================= */
    async function open(productId) {
        try {
            const res = await fetch(`/Product/GetVariants?productId=${productId}`);
            const data = await res.json();

            if (!data?.variants?.length) {
                toastr.error("Sản phẩm hết hàng!");
                return;
            }

            state.product = data;
            state.variants = data.variants;
            state.selected = {};
            state.selectedVariant = null;

            renderBaseUI();
            renderAttributes();
            updateAvailable();

            show();

        } catch (err) {
            console.error(err);
            toastr.error("Lỗi tải sản phẩm!");
        }
    }

    /* ================= BASE UI ================= */
    function renderBaseUI() {
        document.getElementById("popup-name").innerText = state.product.name || "";

        const img = document.getElementById("popup-img");
        img.src = state.product.image || "/images/no-image.png";

        document.getElementById("popup-price").innerText = "";
        document.getElementById("popup-stock").innerText = "";

        document.getElementById("qty").value = 1;

        
    }

    /* ================= BUILD ATTR MAP ================= */
    function getAttrMap() {
        const map = {};

        state.variants.forEach(v => {
            (v.attributes || []).forEach(a => {
                const name = a.name || a.Name;
                const value = a.value || a.Value;

                if (!map[name]) map[name] = new Set();
                map[name].add(value);
            });
        });

        return map;
    }

    /* ================= RENDER ATTR ================= */
    function renderAttributes() {
        const container = document.getElementById("attribute-container");
        container.innerHTML = "";

        const map = getAttrMap();

        Object.keys(map).forEach(name => {

            let html = `<div><b>${name}</b><br>`;

            map[name].forEach(val => {
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

        bindEvents();
    }

    /* ================= EVENTS ================= */
    function bindEvents() {

        // ATTR CLICK
        document.querySelectorAll(".attr-btn").forEach(btn => {

            btn.onclick = () => {

                const name = btn.dataset.name;
                const value = btn.dataset.value;

                // toggle giống detail
                if (state.selected[name] === value) {
                    delete state.selected[name];
                    btn.classList.remove("active");
                } else {
                    state.selected[name] = value;

                    document.querySelectorAll(`[data-name="${name}"]`)
                        .forEach(b => b.classList.remove("active"));

                    btn.classList.add("active");
                }

                updateAvailable();
                findVariant();
            };
        });

        // +
        document.getElementById("plus").onclick = () => {
            const q = document.getElementById("qty");

            const stock = state.selectedVariant?.stockQuantity
                || state.selectedVariant?.stock
                || state.selectedVariant?.Stock
                || Infinity;

            if (parseInt(q.value) < stock) {
                q.value++;
            }
        };

        // -
        document.getElementById("minus").onclick = () => {
            const q = document.getElementById("qty");
            if (q.value > 1) q.value--;
        };

        document.getElementById("addToCartBtn").onclick = addToCart;
    }

    /* ================= FIND VARIANT ================= */
    function findVariant() {

        state.selectedVariant = state.variants.find(v => {

            return (v.attributes || []).every(a => {
                const name = a.name || a.Name;
                const value = a.value || a.Value;

                return state.selected[name] === value;
            });

        }) || null;



        const price = state.selectedVariant.price || state.selectedVariant.Price;
        const stock = state.selectedVariant.stockQuantity || state.selectedVariant.stock || state.selectedVariant.Stock || 0;

        document.getElementById("popup-price").innerText = `Price: $${price}`;
        document.getElementById("popup-stock").innerText = `Stock: ${stock}`;

       
    }

    /* ================= UPDATE AVAILABLE ================= */
    function updateAvailable() {

        document.querySelectorAll(".attr-btn").forEach(btn => {

            const name = btn.dataset.name;
            const value = btn.dataset.value;

            const isAvailable = state.variants.some(v => {

                const attrs = v.attributes || [];

                return attrs.every(a => {
                    const n = a.name || a.Name;
                    const val = a.value || a.Value;

                    if (n === name) return val === value;

                    if (state.selected[n] && state.selected[n] !== val) return false;

                    return true;
                });
            });


            if (isAvailable) {
                btn.classList.remove("disabled");
            } else {
                btn.classList.add("disabled");
            }
        });
    }

    /* ================= ADD TO CART ================= */
    async function addToCart() {

        if (!state.selectedVariant) {
            toastr.error("Vui lòng chọn thuộc tính");
            return;
        }

        const qty = parseInt(document.getElementById("qty").value);

        const stock = state.selectedVariant.stockQuantity
            || state.selectedVariant.stock
            || state.selectedVariant.Stock
            || 0;

        if (isNaN(qty) || qty <= 0) {
            toastr.error("Số lượng không hợp lệ!");
            return;
        }

        if (qty > stock) {
            toastr.error("Vượt quá tồn kho!");
            return;
        }

        const id = state.selectedVariant.id || state.selectedVariant.Id;

        try {
            const res = await fetch(`/Cart/AddToCart?productId=${id}&quantity=${qty}`);
            const data = await res.json();

            if (data.success) {
                updateCart(data.quantity);
                close();
                toastr.success("Đã thêm vào giỏ hàng!");
            }

        } catch (err) {
            console.error(err);
            toastr.error("Lỗi server!");
        }
    }

    /* ================= UPDATE CART ================= */
    function updateCart(qty) {
        const el = document.getElementById("cart-qty");
        if (el) el.innerText = qty;
    }

    /* ================= BUTTON ================= */


    /* ================= MODAL ================= */
    function show() {
        const el = document.getElementById("variantModal");

        if (!modal) modal = new bootstrap.Modal(el);
        modal.show();
    }

    function close() {
        if (modal) modal.hide();
    }

    /* ================= RESET ================= */
    document.addEventListener("DOMContentLoaded", function () {

        const modalEl = document.getElementById("variantModal");
        if (!modalEl) return;

        modalEl.addEventListener("hidden.bs.modal", () => {

            state.selected = {};
            state.selectedVariant = null;

            document.getElementById("attribute-container").innerHTML = "";
            document.getElementById("qty").value = 1;
        });
    });

    document.getElementById("popup-close")?.addEventListener("click", close);

    return { open, close };

})();

/* ================= TOAST ================= */
toastr.options = {
    closeButton: true,
    progressBar: true,
    positionClass: "toast-top-right",
    timeOut: "700"
};