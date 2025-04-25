const imageSourceSelect = document.getElementById('image-source');
const productCategorySelect = document.getElementById('product-category');

const selectedImageFiltersContainer = document.getElementById('selected-image-filters');
const selectedCategoryFiltersContainer = document.getElementById('selected-category-filters');

let selectedImageSources = [];
let selectedCategories = [];

// 判斷 badge 是否已經存在
function isTagAlreadyAdded(value, container) {
    return Array.from(container.children).some(child => child.getAttribute('data-value') === value);
}

// 建立 badge 元素
function createFilterBadge(value, filterArray, container, selectElement) {
    const span = document.createElement('span');
    span.className = 'filter-badge';
    span.setAttribute('data-value', value);
    span.textContent = value;

    const removeBtn = document.createElement('button');
    removeBtn.innerHTML = '&times;';
    removeBtn.className = 'remove-btn';
    span.appendChild(removeBtn);

    removeBtn.addEventListener('click', function () {
        const index = filterArray.indexOf(value);
        if (index > -1) {
            filterArray.splice(index, 1);
        }

        Array.from(selectElement.options).forEach(option => {
            if (option.value === value) {
                option.selected = false;
            }
        });

        container.removeChild(span);

        if (container.id === 'selected-category-filters') {
            selectedCategories = selectedCategories.filter(val => val !== value);
            filterByProductCategory();
        } else if (container.id === 'selected-image-filters') {
            selectedImageSources = selectedImageSources.filter(val => val !== value);
            filterByImageSource();
        }
    });

    container.appendChild(span);
}

// 僅根據 selectedCategories 控制 .col 顯示
function filterByProductCategory() {
    const items = document.querySelectorAll('.col');
    items.forEach(item => {
        const itemCategory = item.getAttribute('data-category');
        const shouldShow = selectedCategories.length === 0 || selectedCategories.includes(itemCategory);
        item.style = shouldShow ? '' : 'display:none!important';
    });
}

// 僅根據 selectedImageSources 控制 .source-divider 顯示
function filterByImageSource() {
    const dividers = document.querySelectorAll('.source-divider');
    dividers.forEach(divider => {
        const source = divider.getAttribute('data-source');
        const shouldShow = selectedImageSources.length === 0 || selectedImageSources.includes(source);
        divider.style.display = shouldShow ? '' : 'none';
    });
}

// Product Category 篩選器邏輯
productCategorySelect.addEventListener('change', function () {
    const selectedValues = Array.from(productCategorySelect.selectedOptions).map(option => option.value);
    selectedValues.forEach(value => {
        if (!selectedCategories.includes(value)) {
            selectedCategories.push(value);
        }
    });

    selectedValues.forEach(value => {
        if (!isTagAlreadyAdded(value, selectedCategoryFiltersContainer)) {
            createFilterBadge(value, selectedCategories, selectedCategoryFiltersContainer, productCategorySelect);
        }
    });

    filterByProductCategory();
});

// Image Source 篩選器邏輯
imageSourceSelect.addEventListener('change', function () {
    const selectedValues = Array.from(imageSourceSelect.selectedOptions).map(option => option.value);
    selectedValues.forEach(value => {
        if (!selectedImageSources.includes(value)) {
            selectedImageSources.push(value);
        }
    });

    selectedValues.forEach(value => {
        if (!isTagAlreadyAdded(value, selectedImageFiltersContainer)) {
            createFilterBadge(value, selectedImageSources, selectedImageFiltersContainer, imageSourceSelect);
        }
    });

    filterByImageSource();
});