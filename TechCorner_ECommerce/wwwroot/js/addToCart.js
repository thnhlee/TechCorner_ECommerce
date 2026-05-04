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

            if (!data || !data.variants || data.variants.length === 0) {
                toastr.error("Sản phẩm hết hàng!");
                return;
            }

            state.product = data;
            state.variants = data.variants;
            state.selected = {};
            state.selectedVariant = null;

            renderBaseUI();
            renderAttributes();
            autoSelectFirst();
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

        toggleAddButton(false);
    }

    /* ================= ATTRIBUTE MAP ================= */
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

            let html = `<div class="attr-group"><b>${name}</b><br>`;

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

        document.querySelectorAll(".attr-btn").forEach(btn => {

            btn.onclick = () => {

                const name = btn.dataset.name;
                const value = btn.dataset.value;

                state.selected[name] = value;

                document.querySelectorAll(`[data-name="${name}"]`)
                    .forEach(b => b.classList.remove("active"));

                btn.classList.add("active");

                findVariant();
                updateAvailable();
            };
        });

        document.getElementById("plus").onclick = () => {
            let q = document.getElementById("qty");
            q.value++;
        };

        document.getElementById("minus").onclick = () => {
            let q = document.getElementById("qty");
            if (q.value > 1) q.value--;
        };

        document.getElementById("addToCartBtn").onclick = addToCart;
    }

    /* ================= AUTO SELECT ================= */
    function autoSelectFirst() {

        const v = state.variants[0];
        if (!v) return;

        state.selectedVariant = v;

        (v.attributes || []).forEach(a => {

            const name = a.name || a.Name;
            const value = a.value || a.Value;

            state.selected[name] = value;

            document.querySelectorAll(`[data-name="${name}"]`)
                .forEach(btn => {
                    if (btn.dataset.value === value) {
                        btn.classList.add("active");
                    }
                });
        });

        findVariant();
        updateAvailable();
    }

    /* ================= FIND SKU ================= */
    function findVariant() {

        state.selectedVariant = state.variants.find(v => {

            return (v.attributes || []).every(a => {
                const name = a.name || a.Name;
                const value = a.value || a.Value;

                return state.selected[name] === value;
            });
        });

        if (!state.selectedVariant) {
            toggleAddButton(false);
            return;
        }

        document.getElementById("popup-price").innerText =
            `$${state.selectedVariant.price}`;

        document.getElementById("popup-stock").innerText =
            `Stock: ${state.selectedVariant.stockQuantity}`;

        toggleAddButton(true);
    }

    /* ================= VALID OPTIONS ================= */
    function updateAvailable() {

        document.querySelectorAll(".attr-btn").forEach(btn => {

            const temp = { ...state.selected };
            temp[btn.dataset.name] = btn.dataset.value;

            const valid = state.variants.some(v => {

                return (v.attributes || []).every(a => {
                    const name = a.name || a.Name;
                    const value = a.value || a.Value;

                    return temp[name] === value;
                });
            });

            btn.disabled = !valid;
        });
    }

    /* ================= ADD TO CART ================= */
    async function addToCart() {

        if (!state.selectedVariant) {
            toastr.error("Vui lòng chọn thuộc tính");
            return;
        }

        const qty = parseInt(document.getElementById("qty").value);

        if (qty <= 0) {
            toastr.error("Số lượng không hợp lệ");
            return;
        }

        if (qty > state.selectedVariant.stockQuantity) {
            toastr.error("Vượt quá tồn kho!");
            return;
        }

        const id = state.selectedVariant.id || state.selectedVariant.Id;

        try {
            // 🔥 FIX: dùng productId
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

    /* ================= CART UI ================= */
    function updateCart(qty) {
        const el = document.getElementById("cart-qty");
        if (el) el.innerText = qty;
    }

    /* ================= BUTTON STATE ================= */
    function toggleAddButton(enable) {
        const btn = document.getElementById("addToCartBtn");
        if (btn) btn.disabled = !enable;
    }

    /* ================= MODAL ================= */
    function show() {
        const el = document.getElementById("variantModal");

        if (!modal) {
            modal = new bootstrap.Modal(el);
        }

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

            const container = document.getElementById("attribute-container");
            if (container) container.innerHTML = "";

            const qty = document.getElementById("qty");
            if (qty) qty.value = 1;
        });
    });

    document.getElementById("popup-close")?.addEventListener("click", close);

    return {
        open,
        close
    };

})();

/* ================= TOAST ================= */
toastr.options = {
    closeButton: true,
    progressBar: true,
    positionClass: "toast-top-right",
    timeOut: "700"
};