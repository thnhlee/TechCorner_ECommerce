//window.createVariantManager = function (variants) {

//    let selected = {};
//    let selectedVariant = null;

//    function normalizeAttrs(v) {
//        return v.attributes || v.Attributes || [];
//    }

//    function getAttrMap() {
//        const map = {};

//        variants.forEach(v => {
//            normalizeAttrs(v).forEach(a => {
//                const name = a.name || a.Name;
//                const value = a.value || a.Value;

//                if (!map[name]) map[name] = new Set();
//                map[name].add(value);
//            });
//        });

//        return map;
//    }

//    function select(name, value) {
//        selected[name] = value;
//        return findVariant();
//    }

//    function findVariant() {

//        selectedVariant = variants.find(v => {
//            return normalizeAttrs(v).every(a => {
//                const name = a.name || a.Name;
//                const value = a.value || a.Value;

//                return selected[name] === value;
//            });
//        });

//        return selectedVariant;
//    }

//    function isValidOption(name, value) {

//        const temp = { ...selected };
//        temp[name] = value;

//        return variants.some(v => {
//            return normalizeAttrs(v).every(a => {
//                const n = a.name || a.Name;
//                const val = a.value || a.Value;

//                return temp[n] === val;
//            });
//        });
//    }

//    function autoSelectFirst() {
//        const v = variants[0];
//        if (!v) return;

//        normalizeAttrs(v).forEach(a => {
//            const name = a.name || a.Name;
//            const value = a.value || a.Value;
//            selected[name] = value;
//        });

//        return findVariant();
//    }

//    return {
//        getAttrMap,
//        select,
//        findVariant,
//        isValidOption,
//        autoSelectFirst,
//        getSelected: () => selected,
//        getVariant: () => selectedVariant
//    };
//};