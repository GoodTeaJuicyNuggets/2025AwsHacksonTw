const uploadArea = document.getElementById('uploadArea');
const previewImage = document.getElementById('previewImage');
const textBox = document.getElementById('textBox');
const generateButton = document.getElementById('generateButton');
const actionDropdown = document.getElementById('actionDropdown');

// Handle drag-and-drop or click upload
uploadArea.addEventListener('click', () => {
    const fileInput = document.createElement('input');
    fileInput.type = 'file';
    fileInput.accept = 'image/*';
    fileInput.onchange = (event) => {
        const file = event.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = (e) => {
                previewImage.src = e.target.result;
                previewImage.style.display = 'block';
            };
            reader.readAsDataURL(file);
            uploadArea.textContent = `Selected: ${file.name}`;
        }
    };
    fileInput.click();
});

uploadArea.addEventListener('dragover', (event) => {
    event.preventDefault();
    uploadArea.classList.add('border-primary');
});

uploadArea.addEventListener('dragleave', () => {
    uploadArea.classList.remove('border-primary');
});

uploadArea.addEventListener('drop', (event) => {
    event.preventDefault();
    const file = event.dataTransfer.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = (e) => {
            previewImage.src = e.target.result;
            previewImage.style.display = 'block';
        };
        reader.readAsDataURL(file);
        uploadArea.textContent = `Selected: ${file.name}`;
    }
});
