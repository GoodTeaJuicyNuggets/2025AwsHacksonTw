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
        if (!shouldShow) {
            item.classList.remove('selected');
            const radio = item.querySelector('.product-radio');
            if (radio) {
                radio.checked = false;
            }
        }
    });
}

// 僅根據 selectedImageSources 控制 .source-divider 顯示
function filterByImageSource() {
    const dividers = document.querySelectorAll('.source-divider');

    dividers.forEach(divider => {
        const source = divider.getAttribute('data-source');
        const shouldShow = selectedImageSources.length === 0 || selectedImageSources.includes(source);
        divider.style.display = shouldShow ? '' : 'none';
        if (!shouldShow) {
            const cols = divider.querySelectorAll('.col');
            cols.forEach(col => {
                col.classList.remove('selected');
                const radio = col.querySelector('.product-radio');
                if (radio) {
                    radio.checked = false;
                }
            });
        }
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

document.addEventListener('DOMContentLoaded', function () {
    // 點擊產品卡片切換選擇狀態
    document.querySelectorAll('.product-item').forEach(item => {
        item.addEventListener('click', function () {
            const radio = this.querySelector('.product-radio');
            const dataId = this.getAttribute('data-id');

            if (radio.checked) {
                radio.checked = false;
                this.classList.remove('selected');
            } else {
                if (new SelectedProducts().get().length >= maxSelection) {
                    alert(`最多只能選取 ${maxSelection} 張圖片`);
                    return;
                }
                radio.checked = true;
                this.classList.add('selected');
            }
        });
    });

    // 點擊 "選取" 按鈕
    document.getElementById('select-button').addEventListener('click', function () {
        const selectedProductDetails = [];
        new SelectedProducts().get().forEach(item => {
            selectedProductDetails.push({
                ImageId: item.getAttribute('data-id'),
                Name: item.getAttribute('data-name'),
                Category: item.getAttribute('data-category'),
                Source: item.getAttribute('data-source'),
                ImageUrl: item.getAttribute('data-imageurl'),
                Prompt: item.getAttribute('data-prompt')
            });
        });

        if (selectedProductDetails.length === 0) {
            console.log('未選取任何圖片，操作已略過。');
            return;
        }
        else {
            if (confirm('是否確定選定圖片?')) {
                // 把選取的產品資料回傳給 opener（原本的父頁）
                window.opener.receiveData(selectedProductDetails);

                // 關掉自己
                window.close();
            }
        }
    });
});

class SelectedProducts {
    constructor() {
        this.selectedItems = document.querySelectorAll('.product-item.selected');
    }

    get() {
        return this.selectedItems;
    }
}