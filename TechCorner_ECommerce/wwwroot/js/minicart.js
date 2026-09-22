async function refreshCartPage() {
    try {
        const [itemsRes, summaryRes] = await Promise.all([
            fetch("/Cart/GetCartItems", {
                cache: "no-store"
            }),
            fetch("/Cart/GetCartSummary", {
                cache: "no-store"
            })
        ]);

        if (!itemsRes.ok || !summaryRes.ok) {
            return;
        }

        const html = await itemsRes.text();
        const data = await summaryRes.json();

        // Update cart items
        const oldItems = document.getElementById("cart-items");

        if (oldItems) {
            oldItems.outerHTML = html;
        }

        if (!data.success) return;

        // Update mini cart quantity
        const cartQty = document.getElementById("cart-qty");

        if (cartQty) {
            cartQty.innerText = data.quantity;
        }

        // Update subtotal / total
        const value = "$" + Number(data.subtotal).toLocaleString("en-US", {
            minimumFractionDigits: 0,
            maximumFractionDigits: 2
        });

        const subtotalEl = document.getElementById("cart-subtotal");
        const totalEl = document.getElementById("cart-total");

        if (subtotalEl) {
            subtotalEl.innerText = value;
        }

        if (totalEl) {
            totalEl.innerText = value;
        }

    } catch (err) {
        console.error("Refresh cart error:", err);
    }
}

window.addEventListener("pageshow", refreshCartPage);
