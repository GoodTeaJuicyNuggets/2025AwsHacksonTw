const productGrid = document.getElementById('product-grid');
const productCategorySelect = document.getElementById('product-category');
const imageSourceSelect = document.getElementById('image-source');
const selectedCategoryFiltersContainer = document.getElementById('selected-category-filters');
const selectedImageFiltersContainer = document.getElementById('selected-image-filters');

// 用於存儲已選的篩選條件
let selectedCategories = [];
let selectedImageSources = [];

// 監聽產品類別篩選選項的變化
productCategorySelect.addEventListener('change', function () {
    const selectedValues = Array.from(productCategorySelect.selectedOptions).map(option => option.value);

    // 找出新增的選項
    const newSelections = selectedValues.filter(value => !selectedCategories.includes(value));
    selectedCategories = selectedValues;

    // 更新篩選標籤，避免重複加入
    newSelections.forEach(value => {
        if (!isTagAlreadyAdded(value, selectedCategoryFiltersContainer)) {
            createFilterBadge(value, selectedCategories, selectedCategoryFiltersContainer, productCategorySelect);
        }
    });

    filterProducts();
});

// 監聽圖片來源篩選選項的變化
imageSourceSelect.addEventListener('change', function () {
    const selectedValues = Array.from(imageSourceSelect.selectedOptions).map(option => option.value);

    // 找出新增的選項
    const newSelections = selectedValues.filter(value => !selectedImageSources.includes(value));
    selectedImageSources = selectedValues;

    // 更新篩選標籤，避免重複加入
    newSelections.forEach(value => {
        if (!isTagAlreadyAdded(value, selectedImageFiltersContainer)) {
            createFilterBadge(value, selectedImageSources, selectedImageFiltersContainer, imageSourceSelect);
        }
    });

    filterProducts();
});

// 創建篩選標籤
function createFilterBadge(value, filterArray, container, selectElement) {
    const span = document.createElement('span');
    span.textContent = value.charAt(0).toUpperCase() + value.slice(1).replace('-', ' ');
    span.setAttribute('data-value', value);
    span.classList.add('filter-badge');

    const removeBtn = document.createElement('span');
    removeBtn.textContent = '×';
    removeBtn.classList.add('remove-btn');
    removeBtn.addEventListener('click', function () {
        // 從對應的篩選條件中移除該值
        const index = filterArray.indexOf(value);
        if (index > -1) {
            filterArray.splice(index, 1);
        }

        // 從多選框中取消選中該值
        Array.from(selectElement.options).forEach(option => {
            if (option.value === value) {
                option.selected = false;
            }
        });

        // 移除標籤並更新篩選
        container.removeChild(span);
        filterProducts();
    });

    span.appendChild(removeBtn);
    container.appendChild(span);
}

// 檢查標籤是否已經存在
function isTagAlreadyAdded(value, container) {
    return Array.from(container.children).some(tag => tag.getAttribute('data-value') === value);
}

// 根據篩選條件顯示或隱藏產品
function filterProducts() {
    Array.from(productGrid.children).forEach(product => {
        const productCategory = product.getAttribute('data-category');
        const imageSource = product.getAttribute('data-image-source');

        const matchesCategory =
            selectedCategories.length === 0 || selectedCategories.includes(productCategory);
        const matchesImageSource =
            selectedImageSources.length === 0 || selectedImageSources.includes(imageSource);

        if (matchesCategory && matchesImageSource) {
            product.style.display = 'block';
        } else {
            product.style.display = 'none';
        }
    });
}
