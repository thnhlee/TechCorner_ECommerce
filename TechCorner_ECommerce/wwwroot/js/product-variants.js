(function () {
    function findValue(attributes, id) {
        const valueId = parseInt(id);

        for (const attr of attributes) {
            const found = attr.Values.find(v => v.Id === valueId);
            if (found) return found;
        }

        return null;
    }

    function cartesian(arrays) {
        return arrays.reduce((a, b) => a.flatMap(d => b.map(e => [...d, e])), [[]]);
    }

    function selectedAttributeGroups() {
        const selected = {};

        document.querySelectorAll(".attr-checkbox:checked").forEach(cb => {
            const attrId = parseInt(cb.dataset.attr);

            if (!selected[attrId]) selected[attrId] = [];

            selected[attrId].push(parseInt(cb.value));
        });

        return Object.values(selected);
    }

    function renderAttributes(attributes, catId) {
        const container = document.getElementById("attributeContainer");
        if (!container) return;

        const filtered = attributes.filter(a => a.CategoryId === parseInt(catId));

        if (filtered.length === 0) {
            container.innerHTML = `<p class="text-muted">No attributes for this category</p>`;
            return;
        }

        container.innerHTML = filtered.map(attr => `
            <div class="mb-3 attribute-group" data-category="${attr.CategoryId}">
                <label class="fw-bold mb-2 d-block">${attr.Name}</label>
                <div class="ms-2 mt-1">
                    ${attr.Values.map(val => `
                        <div class="form-check form-check-inline">
                            <input type="checkbox"
                                   class="attr-checkbox"
                                   data-attr="${attr.Id}"
                                   value="${val.Id}" />
                            <label class="form-check-label">${val.Value}</label>
                        </div>
                    `).join("")}
                </div>
            </div>
        `).join("");
    }

    function loadSubcategories(subCategories, catId, selectedSubId) {
        const subSelect = document.getElementById("subCategorySelect");
        if (!subSelect) return;

        subSelect.innerHTML = `<option value="">Select subcategory</option>`;

        subCategories
            .filter(x => x.CategoryId === parseInt(catId))
            .forEach(x => {
                const selected = x.Id === parseInt(selectedSubId) ? "selected" : "";
                subSelect.innerHTML += `<option value="${x.Id}" ${selected}>${x.Name}</option>`;
            });
    }

    function setupImagePreview(options) {
        const imageInput = document.querySelector(options.inputSelector);
        const preview = document.getElementById(options.previewId);

        if (!imageInput || !preview) return;

        let selectedFiles = [];

        function updateFileInput() {
            const dataTransfer = new DataTransfer();
            selectedFiles.forEach(file => dataTransfer.items.add(file));
            imageInput.files = dataTransfer.files;
        }

        function renderImagePreview() {
            preview.innerHTML = "";

            selectedFiles.forEach((file, index) => {
                if (file.size > 2 * 1024 * 1024) {
                    alert(file.name + " quá lớn (>2MB)");
                    selectedFiles.splice(index, 1);
                    updateFileInput();
                    return;
                }

                const url = URL.createObjectURL(file);
                const wrapper = document.createElement("div");
                const img = document.createElement("img");
                const btn = document.createElement("button");

                wrapper.className = "image-preview-item";
                img.src = url;
                img.className = "image-preview-img";
                btn.type = "button";
                btn.innerHTML = "&times;";
                btn.className = "image-remove-btn";
                btn.onclick = function () {
                    URL.revokeObjectURL(url);
                    selectedFiles.splice(index, 1);
                    renderImagePreview();
                };

                wrapper.appendChild(img);
                wrapper.appendChild(btn);
                preview.appendChild(wrapper);
            });

            updateFileInput();
        }

        imageInput.addEventListener("change", function (e) {
            selectedFiles = Array.from(e.target.files);
            renderImagePreview();
        });
    }

    function initAddProduct(options) {
        const categorySelect = document.getElementById("categorySelect");

        window.generateVariants = function () {
            const attrArrays = selectedAttributeGroups();

            if (attrArrays.length === 0) {
                alert("Chọn ít nhất 1 attribute");
                return;
            }

            const combos = cartesian(attrArrays);
            const header = document.getElementById("variantHeader");
            const body = document.querySelector("#variantTable tbody");

            header.innerHTML = `
                <th class="fw-bold">Price</th>
                <th class="fw-bold">Stock</th>
                <th class="fw-bold">Attributes</th>
            `;

            body.innerHTML = "";

            combos.forEach((combo, index) => {
                const attrNames = combo
                    .map(valId => findValue(options.attributes, valId)?.Value)
                    .filter(Boolean)
                    .join(" / ");

                const hiddenInputs = combo
                    .map(valId => findValue(options.attributes, valId))
                    .filter(Boolean)
                    .map(val => `
                        <input type="hidden"
                               name="Variants[${index}].AttributeValueIds"
                               value="${val.Id}" />
                    `)
                    .join("");

                body.innerHTML += `
                    <tr>
                        <td><input name="Variants[${index}].Price" class="form-control" /></td>
                        <td><input name="Variants[${index}].StockQuantity" class="form-control" /></td>
                        <td>${attrNames}${hiddenInputs}</td>
                    </tr>
                `;
            });
        };

        categorySelect?.addEventListener("change", function () {
            const catId = parseInt(this.value);

            loadSubcategories(options.subCategories, catId);
            renderAttributes(options.attributes, catId);

            document.querySelector("#variantTable tbody").innerHTML = "";
            document.getElementById("variantHeader").innerHTML =
                `<th class="fw-bold">Price</th><th class="fw-bold">Stock</th>`;
        });

        setupImagePreview({
            inputSelector: 'input[type="file"]',
            previewId: "preview"
        });
    }

    function initEditProduct(options) {
        const categorySelect = document.getElementById("categorySelect");
        let variantIndex = options.variantIndex;
        const existingVariantKeys = new Set(options.existingVariantKeys);
        const generatedVariantKeys = new Set();

        loadSubcategories(options.subCategories, categorySelect?.value, options.selectedSubId);
        renderAttributes(options.attributes, parseInt(categorySelect?.value));

        window.generateVariants = function () {
            const attrArrays = selectedAttributeGroups();

            if (attrArrays.length === 0) {
                alert("Chọn ít nhất 1 attribute");
                return;
            }

            appendNewVariants(cartesian(attrArrays));
        };

        function appendNewVariants(combos) {
            const body = document.querySelector("#variantTable tbody");
            let addedCount = 0;
            let duplicatedCount = 0;

            combos.forEach(combo => {
                const comboKey = combo.map(x => parseInt(x)).sort((a, b) => a - b).join(",");

                if (existingVariantKeys.has(comboKey) || generatedVariantKeys.has(comboKey)) {
                    duplicatedCount++;
                    return;
                }

                const attrNames = combo.map(id => findValue(options.attributes, id)?.Value).join(" / ");
                const hiddenInputs = combo.map(valId => `
                    <input type="hidden"
                           name="Variants[${variantIndex}].AttributeValueIds"
                           value="${valId}" />
                `).join("");

                body.innerHTML += `
                    <tr>
                        <td>
                            <input type="hidden" name="Variants[${variantIndex}].ProductId" value="0" />
                            <input name="Variants[${variantIndex}].Price" class="form-control" />
                        </td>
                        <td><input name="Variants[${variantIndex}].StockQuantity" class="form-control" /></td>
                        <td>${attrNames}${hiddenInputs}</td>
                    </tr>
                `;

                generatedVariantKeys.add(comboKey);
                addedCount++;
                variantIndex++;
            });

            if (addedCount === 0 && duplicatedCount > 0) {
                alert("Không generate được vì combo attribute này đã tồn tại.");
                return;
            }

            if (duplicatedCount > 0) {
                alert(`Đã bỏ qua ${duplicatedCount} combo bị trùng.`);
            }
        }

        setupImagePreview({
            inputSelector: 'input[type="file"]',
            previewId: "newPreview"
        });

        setupDeleteHandlers();
    }

    function setupDeleteHandlers() {
        let deleteImageId = 0;
        let deleteId = 0;

        window.openDeleteImageModal = function (id) {
            deleteImageId = id;
            new bootstrap.Modal(document.getElementById("deleteImageModal")).show();
        };

        window.openDeleteModal = function (id) {
            deleteId = id;
            new bootstrap.Modal(document.getElementById("deleteModal")).show();
        };

        document.getElementById("confirmDeleteImage")?.addEventListener("click", function () {
            const formData = new FormData();
            formData.append("id", deleteImageId);

            fetch("/Admin/Product/DeleteImage", {
                method: "POST",
                headers: { RequestVerificationToken: getToken() },
                body: formData
            })
                .then(r => r.json())
                .then(res => {
                    if (res.success) {
                        document.getElementById(`image-${deleteImageId}`)?.remove();
                        bootstrap.Modal.getInstance(document.getElementById("deleteImageModal")).hide();
                    }
                    else {
                        alert(res.message);
                    }
                });
        });

        document.getElementById("confirmDelete")?.addEventListener("click", function () {
            const formData = new FormData();
            formData.append("id", deleteId);

            fetch("/Admin/Product/DeleteVariant", {
                method: "POST",
                headers: { RequestVerificationToken: getToken() },
                body: formData
            })
                .then(r => r.json())
                .then(res => {
                    if (res.success) {
                        location.reload();
                    }
                    else {
                        alert(res.message);
                    }
                });
        });
    }

    window.ProductVariants = {
        initAddProduct,
        initEditProduct
    };
})();
