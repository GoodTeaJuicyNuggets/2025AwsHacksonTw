const uploadArea = document.getElementById('uploadArea');
const previewImage = document.getElementById('previewImage');
const textBox = document.getElementById('textBox');
const importButton = document.getElementById('importButton');
const uploadButton = document.getElementById('uploadButton');
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

// Handle import button click
importButton.addEventListener('click', () => {
    alert('Importing website images...');
    // Add logic to fetch and display website images
});

// Handle upload button click
uploadButton.addEventListener('click', () => {
    const text = textBox.value;
    if (!text) {
        alert('Please enter some text before uploading.');
        return; const uploadArea = document.getElementById('uploadArea');
        const previewImage = document.getElementById('previewImage');
        const textBox = document.getElementById('textBox');
        const importButton = document.getElementById('importButton');
        const uploadButton = document.getElementById('uploadButton');
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

        // Handle import button click
        importButton.addEventListener('click', () => {
            alert('Importing website images...');
            // Add logic to fetch and display website images
        });

        // Handle upload button click
        uploadButton.addEventListener('click', () => {
            const text = textBox.value;
            if (!text) {
                alert('Please enter some text before uploading.');
                return;
            }
            alert('Uploading image and text...');
            // Add logic to upload the image and text
        });

        // Handle generate button click
        generateButton.addEventListener('click', () => {
            const text = textBox.value;
            if (!text) {
                alert('Please enter some text to generate.');
                return;
            }
            alert(`Generating content for: ${text}`);
            // Add logic to generate content based on the text
        });

        // Handle action dropdown change
        actionDropdown.addEventListener('change', () => {
            const selectedAction = actionDropdown.value;
            console.log(`Selected Action: ${selectedAction}`);
            // Add logic to handle the selected action
        });

    }
    alert('Uploading image and text...');
    // Add logic to upload the image and text
});

// Handle generate button click
generateButton.addEventListener('click', () => {
    const text = textBox.value;
    if (!text) {
        alert('Please enter some text to generate.');
        return;
    }
    alert(`Generating content for: ${text}`);
    // Add logic to generate content based on the text
});

// Handle action dropdown change
actionDropdown.addEventListener('change', () => {
    const selectedAction = actionDropdown.value;
    console.log(`Selected Action: ${selectedAction}`);
    // Add logic to handle the selected action
});
