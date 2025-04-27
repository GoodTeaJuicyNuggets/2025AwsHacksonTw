const uploadArea = document.getElementById('uploadArea');
const previewImage = document.getElementById('previewImage');
const textBox = document.getElementById('textBox');
const generateButton = document.getElementById('generateButton');
const actionDropdown = document.getElementById('actionDropdown');

// Handle drag-and-drop or click upload
//uploadArea.addEventListener('click', () => {
//    const fileInput = document.createElement('input');
//    fileInput.type = 'file';
//    fileInput.accept = 'image/*';
//    fileInput.onchange = (event) => {
//        const file = event.target.files[0];
//        if (file) {
//            const reader = new FileReader();
//            reader.onload = (e) => {
//                previewImage.src = e.target.result;
//                previewImage.style.display = 'block';
//            };
//            reader.readAsDataURL(file);
//            uploadArea.textContent = `Selected: ${file.name}`;
//        }
//    };
//    fileInput.click();
//});

//uploadArea.addEventListener('dragover', (event) => {
//    event.preventDefault();
//    uploadArea.classList.add('border-primary');
//});

//uploadArea.addEventListener('dragleave', () => {
//    uploadArea.classList.remove('border-primary');
//});

//uploadArea.addEventListener('drop', (event) => {
//    event.preventDefault();
//    const file = event.dataTransfer.files[0];
//    if (file) {
//        const reader = new FileReader();
//        reader.onload = (e) => {
//            previewImage.src = e.target.result;
//            previewImage.style.display = 'block';
//        };
//        reader.readAsDataURL(file);
//        uploadArea.textContent = `Selected: ${file.name}`;
//    }
//});
document.addEventListener('DOMContentLoaded', function () {
    const txtPrompt = document.getElementById('txtPrompt');
    const btnSendPrompt = document.getElementById('btnSendPrompt');
    const btnCreateImage = document.getElementById('btnCreateImage');
    const chatMessages = document.getElementById('chatMessages');

    // 将消息新增到聊天框
    function addChatMessage(message) {
        if (message.trim() === '') return;

        // 创建用户消息元素
        const userMessageElement = document.createElement('div');
        userMessageElement.className = 'message user-message'; // 使用 user-message 样式
        userMessageElement.textContent = message;

        // 将用户消息添加到聊天框
        chatMessages.appendChild(userMessageElement);

        // 创建机器人消息元素
        const botMessageElement = document.createElement('div');
        botMessageElement.className = 'message bot-message'; // 使用 bot-message 样式

        // 设置机器人消息内容，包含按钮
        const uniqueId = `generateButton-${Date.now()}`; // 生成唯一的 ID
        botMessageElement.innerHTML = `請確認您將以 "${message}" 進行圖片生成，請點擊 <button class="btn btn-link generate-button" data-id="${uniqueId}">這裡</button> 進行生成`;

        // 将机器人消息添加到聊天框
        chatMessages.appendChild(botMessageElement);

        // 滚动到底部
        chatMessages.scrollTop = chatMessages.scrollHeight;

        // 为所有生成按钮绑定点击事件
        const generateButtons = document.querySelectorAll('.generate-button');
        generateButtons.forEach(button => {
            button.addEventListener('click', function () {
                btnCreateImage.click(); // 触发隐藏的提交按钮
            });
        });
    }

    // 按下 Enter 键时触发
    txtPrompt.addEventListener('keydown', function (event) {
        if (event.key === 'Enter') {
            event.preventDefault(); // 阻止默认的表单提交行为
            addChatMessage(txtPrompt.value); // 新增聊天消息
        }
    });

    // 点击按钮时触发
    btnSendPrompt.addEventListener('click', function () {
        addChatMessage(txtPrompt.value);
    });
});



document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('CoolerMasterAiAgentForm');
    const loadingIcon = document.getElementById('loadingIcon');

    form.addEventListener('submit', function () {
        // 显示加载图标
        loadingIcon.classList.remove('d-none');
    });
});

