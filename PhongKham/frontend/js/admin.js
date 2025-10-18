// =================================================================
// SCRIPT CHO VAI TRÒ ADMIN (ĐÃ SỬA LỖI TRÙNG LẶP VÀ CẬP NHẬT LOGIC KHO THEO TYPE)
// =================================================================

// --- LƯỢC BỎ: const API_BASE_URL đã được khai báo trong app.js ---

// --- GLOBAL STATE ---
// === SỬA ĐỔI: Thêm editingDoctorId vào global state ===
const adminState = {
    allUsers: [],
    allDoctors: [],
    allSpecialties: [],
    drugStock: [],
    allDrugs: [],
    revenueReport: [],
    editingDrugId: null,
    editingDoctorId: null // <-- BỔ SUNG: Lưu ID bác sĩ đang sửa
};

// --- INITIALIZATION ---
document.addEventListener('DOMContentLoaded', () => {
    // 1. Kiểm tra xác thực và vai trò
    const user = JSON.parse(sessionStorage.getItem('user'));
    if (!user || user.role.toLowerCase() !== 'admin') {
        alert('Bạn không có quyền truy cập trang này.');
        window.location.href = '../Login/index.html';
        return;
    }

    // 2. Thiết lập giao diện chung
    const usernameDisplay = document.getElementById('username-display');
    const logoutButton = document.getElementById('logout-button');
    if (usernameDisplay) usernameDisplay.textContent = `Chào, ${user.username}!`;
    if (logoutButton) logoutButton.addEventListener('click', () => {
        sessionStorage.clear();
        window.location.href = '../Login/index.html';
    });

    // 3. Khởi chạy logic theo từng trang
    const pageId = document.body.id;
    switch (pageId) {
        case 'admin-users-page':
            initUsersPage();
            break;
        case 'admin-doctors-page':
            initDoctorsPage();
            break;
        case 'admin-drugs-page':
            initDrugsPage();
            break;
        case 'admin-reports-page':
            initReportsPage();
            break;
    }

    // Gán sự kiện đóng modal chung
    const closeMsgBtn = document.getElementById('close-message-modal-btn');
    if(closeMsgBtn) closeMsgBtn.addEventListener('click', closeMessageModal);
});

// --- HELPER FUNCTIONS ---
function showAdminMessage(message, isError = false) {
    const modal = document.getElementById('message-modal');
    const text = document.getElementById('message-text');
    if (modal && text) {
        text.textContent = message;
        text.style.color = isError ? 'red' : 'black';
        modal.classList.add('active');
    } else {
        alert(message);
    }
}

function closeMessageModal() {
    const modal = document.getElementById('message-modal');
    if(modal) {
        modal.classList.remove('active');
    }
}
function formatCurrency(value) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value || 0);
}
function formatDate(dateString) {
    if (!dateString) return 'N/A';
    try {
        const date = new Date(dateString);
        if (isNaN(date.getTime())) {
            return dateString;
        }
        return date.toLocaleString('vi-VN');
    } catch {
        return dateString;
    }
}

// =================================================================
// CHỨC NĂNG 1: QUẢN LÝ TÀI KHOẢN (users.html)
// =================================================================
async function initUsersPage() {
    await fetchAllUsers();
    displayUsers();
    setupUsersEventListeners();
}

async function fetchAllUsers() {
    try {
        adminState.allUsers = await fetchAPI('/api/Accounts', 'GET');
    } catch (error) {
        console.error("Lỗi fetchAllUsers:", error);
        showAdminMessage(`Không thể tải danh sách tài khoản: ${error.message}`, true);
    }
}

function displayUsers() {
    const tableBody = document.getElementById('users-table-body');
    if (!tableBody) return;
    tableBody.innerHTML = '';

    if (!adminState.allUsers || adminState.allUsers.length === 0) {
        tableBody.innerHTML = `<tr><td colspan="5" class="text-center py-4 text-gray-500">Không có tài khoản nào.</td></tr>`;
        return;
    }

    adminState.allUsers.forEach(user => {
        const tr = document.createElement('tr');
        tr.className = "hover:bg-gray-50";
        tr.innerHTML = `
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${user.accountID}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">${user.username}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${user.role}</td>
            <td class="px-6 py-4 whitespace-nowrap">
                <span class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${user.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}">
                    ${user.isActive ? 'Hoạt động' : 'Đã khóa'}
                </span>
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm font-medium space-x-3">
                 <button data-id="${user.accountID}" data-active="${user.isActive}" class="toggle-active-btn ${user.isActive ? 'text-red-600 hover:text-red-900' : 'text-green-600 hover:text-green-900'}">
                    ${user.isActive ? 'Khóa TK' : 'Mở khóa'}
                </button>
                 <button data-id="${user.accountID}" data-username="${user.username}" class="change-password-btn text-blue-600 hover:text-blue-900">
                    Đổi MK
                </button>
            </td>
        `;
        tableBody.appendChild(tr);
    });
}

function setupUsersEventListeners() {
    const tableBody = document.getElementById('users-table-body');
    const passwordForm = document.getElementById('password-form');
    const closePasswordModalBtn = document.getElementById('close-password-modal-btn');

    if (tableBody) {
        tableBody.addEventListener('click', async (event) => {
            const button = event.target;
            const userId = button.dataset.id;

            if (button.classList.contains('toggle-active-btn')) {
                const currentStatus = button.dataset.active === 'true';
                const newStatus = !currentStatus;
                if (confirm(`Bạn có chắc muốn ${newStatus ? 'mở khóa' : 'khóa'} tài khoản ID ${userId}?`)) {
                    await toggleUserActiveStatus(userId, newStatus);
                }
            } else if (button.classList.contains('change-password-btn')) {
                const username = button.dataset.username;
                openPasswordModal(userId, username);
            }
        });
    }
    if (passwordForm) passwordForm.addEventListener('submit', handleChangePasswordSubmit);
    if (closePasswordModalBtn) closePasswordModalBtn.addEventListener('click', closePasswordModal);
}

async function toggleUserActiveStatus(userId, newStatus) {
    try {
        await fetchAPI(`/api/Accounts/${userId}/active?value=${newStatus}`, 'PATCH');
        showAdminMessage(`Tài khoản ID ${userId} đã được ${newStatus ? 'mở khóa' : 'khóa'} thành công.`);
        await fetchAllUsers();
        displayUsers();
    } catch (error) {
        console.error("Lỗi toggleUserActiveStatus:", error);
        showAdminMessage(`Lỗi khi thay đổi trạng thái: ${error.message}`, true);
    }
}

function openPasswordModal(accountId, username) {
    document.getElementById('password-account-id').value = accountId;
    document.getElementById('password-username').textContent = username;
    document.getElementById('password-form').reset();
    document.getElementById('password-modal').classList.add('active');
}

function closePasswordModal() {
    document.getElementById('password-modal').classList.remove('active');
}

async function handleChangePasswordSubmit(event) {
    event.preventDefault();
    const accountId = document.getElementById('password-account-id').value;
    const newPassword = document.getElementById('new-password').value;
    const confirmPassword = document.getElementById('confirm-password').value;

    if (newPassword !== confirmPassword) {
        showAdminMessage("Mật khẩu xác nhận không khớp.", true);
        return;
    }
     if (newPassword.length < 6) {
         showAdminMessage("Mật khẩu mới phải có ít nhất 6 ký tự.", true);
         return;
    }

    try {
        await fetchAPI(`/api/Accounts/${accountId}/password`, 'PATCH', { newPassword });
        showAdminMessage(`Đổi mật khẩu cho tài khoản ID ${accountId} thành công.`);
        closePasswordModal();
    } catch (error) {
        console.error("Lỗi handleChangePasswordSubmit:", error);
        showAdminMessage(`Lỗi khi đổi mật khẩu: ${error.message}`, true);
    }
}


// =================================================================
// CHỨC NĂNG 2: QUẢN LÝ BÁC SĨ (doctors.html) - CẬP NHẬT
// =================================================================
async function initDoctorsPage() {
    setupDoctorsEventListeners();
    await Promise.all([fetchAllDoctors(), fetchAllSpecialties()]);
    displayDoctors();
    populateSelectOptions('doctor-specialty', adminState.allSpecialties, 'specialtyID', 'specialtyName', 'Chọn chuyên khoa');
}

async function fetchAllDoctors() {
    try {
        adminState.allDoctors = await fetchAPI('/api/Doctors', 'GET');
    } catch (error) {
        console.error("Lỗi fetchAllDoctors:", error);
        showAdminMessage(`Không thể tải danh sách bác sĩ: ${error.message}`, true);
        adminState.allDoctors = []; // Đảm bảo là mảng rỗng nếu lỗi
    }
}

async function fetchAllSpecialties() {
    try {
        adminState.allSpecialties = await fetchAPI('/api/Specialties', 'GET');
    } catch (error) {
        console.error("Lỗi fetchAllSpecialties:", error);
        showAdminMessage(`Không thể tải danh sách chuyên khoa: ${error.message}`, true);
        adminState.allSpecialties = []; // Đảm bảo là mảng rỗng nếu lỗi
    }
}

function displayDoctors() {
    const tableBody = document.getElementById('doctors-table-body');
    if (!tableBody) return;
    tableBody.innerHTML = '';

    if (!adminState.allDoctors || adminState.allDoctors.length === 0) {
        tableBody.innerHTML = `<tr><td colspan="5" class="text-center py-4 text-gray-500">Chưa có bác sĩ nào.</td></tr>`;
        return;
    }

    adminState.allDoctors.forEach(doc => {
        const tr = document.createElement('tr');
        tr.className = "hover:bg-gray-100 doctor-row";
        tr.dataset.id = doc.doctorID;
        tr.innerHTML = `
            <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">${doc.fullName}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${doc.specialtyName || 'Chưa cập nhật'}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${doc.phone || 'N/A'}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${doc.email || 'N/A'}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${doc.username || 'Chưa liên kết'}</td>
        `;
        tableBody.appendChild(tr);
    });
}

// === SỬA ĐỔI: Thêm sự kiện cho nút Cập nhật và dropdown chọn bác sĩ ===
function setupDoctorsEventListeners() {
    const openCreateModalBtn = document.getElementById('open-doctor-create-modal-btn');
    const openUpdateModalBtn = document.getElementById('open-doctor-update-modal-btn'); // Nút cập nhật mới
    const closeModalBtn = document.getElementById('close-doctor-modal-btn');
    const doctorForm = document.getElementById('doctor-form');
    const doctorsTableBody = document.getElementById('doctors-table-body');
    const closeDetailModalBtn = document.getElementById('close-doctor-detail-modal-btn');
    const closeDetailModalBtnAlt = document.getElementById('close-doctor-detail-modal-btn-alt');
    const doctorSelect = document.getElementById('doctor-select'); // Dropdown chọn bác sĩ

    if(openCreateModalBtn) openCreateModalBtn.addEventListener('click', openDoctorModalForCreate); // Mở modal để tạo
    if(openUpdateModalBtn) openUpdateModalBtn.addEventListener('click', openDoctorModalForUpdate); // Mở modal để cập nhật
    if(closeModalBtn) closeModalBtn.addEventListener('click', closeDoctorModal);
    if(doctorForm) doctorForm.addEventListener('submit', handleDoctorFormSubmit); // Xử lý cả thêm và sửa

    if (doctorsTableBody) doctorsTableBody.addEventListener('click', handleDoctorRowClick);
    if(closeDetailModalBtn) closeDetailModalBtn.addEventListener('click', closeDoctorDetailModal);
    if(closeDetailModalBtnAlt) closeDetailModalBtnAlt.addEventListener('click', closeDoctorDetailModal);
    if (doctorSelect) doctorSelect.addEventListener('change', populateDoctorUpdateForm); // Khi chọn bác sĩ trong dropdown
}

function handleDoctorRowClick(event) {
    const row = event.target.closest('.doctor-row');
    if (row && row.dataset.id) {
        const doctorId = parseInt(row.dataset.id);
        openDoctorDetailModal(doctorId);
    }
}

function openDoctorDetailModal(doctorId) {
    const doctor = adminState.allDoctors.find(d => d.doctorID === doctorId);
    if (!doctor) {
        showAdminMessage("Không tìm thấy thông tin chi tiết của bác sĩ.", true);
        return;
    }
    // Điền thông tin
    document.getElementById('detail-doctor-name').textContent = doctor.fullName || '-';
    document.getElementById('detail-doctor-specialty').textContent = doctor.specialtyName || 'Chưa cập nhật';
    document.getElementById('detail-doctor-phone').textContent = doctor.phone || 'N/A';
    document.getElementById('detail-doctor-email').textContent = doctor.email || 'N/A';
    document.getElementById('detail-doctor-account').textContent = doctor.username || 'Chưa liên kết';
    document.getElementById('detail-doctor-created').textContent = formatDate(doctor.createdAt);
    // Hiển thị modal
    document.getElementById('doctor-detail-modal').classList.add('active');
}

function closeDoctorDetailModal() {
    document.getElementById('doctor-detail-modal').classList.remove('active');
}

// === HÀM MỚI: Mở modal ở chế độ THÊM MỚI ===
function openDoctorModalForCreate() {
    adminState.editingDoctorId = null; // Đảm bảo không có ID đang sửa
    document.getElementById('doctor-form').reset();
    document.getElementById('doctor-modal-title').textContent = "Thêm Bác sĩ mới";
    document.getElementById('doctor-id').value = '';

    // Ẩn dropdown chọn bác sĩ, hiển thị phần tạo tài khoản
    document.getElementById('doctor-select-container').classList.add('hidden');
    document.getElementById('account-create-fields').classList.remove('hidden');

    // Reset và yêu cầu nhập username/password
     document.getElementById('doctor-username').required = true;
     document.getElementById('doctor-password').required = true;


    const modal = document.getElementById('doctor-modal');
    modal.classList.add('active');
}

// === HÀM MỚI: Mở modal ở chế độ CẬP NHẬT ===
function openDoctorModalForUpdate() {
    adminState.editingDoctorId = null; // Reset ID đang sửa
    document.getElementById('doctor-form').reset();
    document.getElementById('doctor-modal-title').textContent = "Cập nhật thông tin Bác sĩ";
    document.getElementById('doctor-id').value = '';

    // Hiển thị dropdown chọn bác sĩ, ẩn phần tạo tài khoản
    document.getElementById('doctor-select-container').classList.remove('hidden');
    document.getElementById('account-create-fields').classList.add('hidden');

     // Không yêu cầu nhập username/password khi cập nhật
     document.getElementById('doctor-username').required = false;
     document.getElementById('doctor-password').required = false;

    // Điền danh sách bác sĩ vào dropdown
    populateSelectOptions('doctor-select', adminState.allDoctors, 'doctorID', 'fullName', '--- Chọn bác sĩ ---');

    const modal = document.getElementById('doctor-modal');
    modal.classList.add('active');
}

// === HÀM MỚI: Điền form khi chọn bác sĩ từ dropdown ===
function populateDoctorUpdateForm() {
    const doctorId = document.getElementById('doctor-select').value;
    if (!doctorId) {
        adminState.editingDoctorId = null;
        document.getElementById('doctor-form').reset(); // Reset form nếu chọn "-- Chọn bác sĩ ---"
        document.getElementById('doctor-id').value = '';
        return;
    }

    const doctor = adminState.allDoctors.find(d => d.doctorID == doctorId);
    if (doctor) {
        adminState.editingDoctorId = doctor.doctorID; // Lưu ID đang sửa
        document.getElementById('doctor-id').value = doctor.doctorID;
        document.getElementById('doctor-fullname').value = doctor.fullName;
        document.getElementById('doctor-specialty').value = doctor.specialtyID || ''; // Set specialty ID
        document.getElementById('doctor-phone').value = doctor.phone || '';
        document.getElementById('doctor-email').value = doctor.email || '';
    } else {
         adminState.editingDoctorId = null;
         document.getElementById('doctor-id').value = '';
    }
}


function closeDoctorModal() {
     const modal = document.getElementById('doctor-modal');
     modal.classList.remove('active');
}

// === SỬA ĐỔI: Xử lý cả Thêm và Sửa ===
async function handleDoctorFormSubmit(event) {
     event.preventDefault();

     // Lấy dữ liệu chung
     const doctorData = {
        fullName: document.getElementById('doctor-fullname').value,
        specialtyID: document.getElementById('doctor-specialty').value ? parseInt(document.getElementById('doctor-specialty').value) : null,
        phone: document.getElementById('doctor-phone').value || null,
        email: document.getElementById('doctor-email').value || null,
    };

    try {
        if (adminState.editingDoctorId) {
            // --- Chế độ SỬA ---
             // Backend API PUT /api/Doctors/{id} chỉ cần thông tin hồ sơ
            await fetchAPI(`/api/Doctors/${adminState.editingDoctorId}`, 'PUT', doctorData);
            showAdminMessage("Cập nhật thông tin bác sĩ thành công!");
        } else {
            // --- Chế độ THÊM MỚI ---
            // Thêm thông tin tài khoản vào doctorData
             doctorData.accountUsername = document.getElementById('doctor-username').value;
             doctorData.accountPassword = document.getElementById('doctor-password').value;

            // Kiểm tra mật khẩu có đủ dài không (vì required=false khi sửa)
             if (!doctorData.accountPassword || doctorData.accountPassword.length < 6) {
                 showAdminMessage("Mật khẩu phải có ít nhất 6 ký tự.", true);
                 document.getElementById('doctor-password').focus();
                 return;
             }

            await fetchAPI('/api/Doctors', 'POST', doctorData);
            showAdminMessage("Thêm bác sĩ mới thành công!");
        }

        closeDoctorModal();
        // Tải lại danh sách sau khi thêm hoặc sửa
        await fetchAllDoctors();
        displayDoctors();
         // Cập nhật lại dropdown chọn bác sĩ (nếu đang mở modal sửa)
         if (document.getElementById('doctor-select-container').classList.contains('hidden') === false) {
              populateSelectOptions('doctor-select', adminState.allDoctors, 'doctorID', 'fullName', '--- Chọn bác sĩ ---');
         }

    } catch (error) {
        console.error("Lỗi handleDoctorFormSubmit:", error);
        showAdminMessage(`Lỗi khi lưu thông tin bác sĩ: ${error.message}`, true);
    }
}

// =================================================================
// CHỨC NĂNG 3: QUẢN LÝ THUỐC & KHO (drugs.html) - CẬP NHẬT LOGIC KHO THEO TYPE
// =================================================================
async function initDrugsPage() {
    setupDrugsEventListeners();
    await Promise.all([fetchDrugStock(), fetchAllDrugsForSelect()]);
    displayDrugStock();
    populateSelectOptions('adjust-drug-select', adminState.allDrugs, 'drugID', 'drugName', 'Chọn thuốc...');
}

async function fetchDrugStock() {
    try {
        adminState.drugStock = await fetchAPI('/api/stock', 'GET');
    } catch (error) {
        console.error("Lỗi fetchDrugStock:", error);
        showAdminMessage(`Không thể tải báo cáo tồn kho: ${error.message}`, true);
    }
}
async function fetchAllDrugsForSelect() {
    try {
        adminState.allDrugs = await fetchAPI('/api/drugs', 'GET');
    } catch (error) {
        console.error("Lỗi fetchAllDrugsForSelect:", error);
        showAdminMessage(`Không thể tải danh sách thuốc: ${error.message}`, true);
         adminState.allDrugs = [];
    }
}

function displayDrugStock() {
    const tableBody = document.getElementById('stock-table-body');
    if (!tableBody) return;
    tableBody.innerHTML = '';

    if (!adminState.drugStock || adminState.drugStock.length === 0) {
        tableBody.innerHTML = `<tr><td colspan="5" class="text-center py-4 text-gray-500">Chưa có dữ liệu tồn kho.</td></tr>`;
        return;
    }

    adminState.drugStock.forEach(stock => {
        const tr = document.createElement('tr');
        tr.className = "hover:bg-gray-50";
        tr.innerHTML = `
            <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">${stock.drugName}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${stock.unit || 'N/A'}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 text-right">${formatCurrency(stock.price)}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm font-semibold text-gray-900 text-right">${stock.quantityAvailable}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                <button data-id="${stock.drugID}" class="edit-drug-btn text-indigo-600 hover:text-indigo-900">
                    Sửa TT
                </button>
            </td>
        `;
        tableBody.appendChild(tr);
    });
}

function setupDrugsEventListeners() {
    const adjustForm = document.getElementById('stock-adjust-form');
    const openDrugModalBtn = document.getElementById('open-drug-modal-btn');
    const closeDrugModalBtn = document.getElementById('close-drug-modal-btn');
    const drugForm = document.getElementById('drug-form');
    const stockTableBody = document.getElementById('stock-table-body');

    if (adjustForm) adjustForm.addEventListener('submit', handleStockAdjustSubmit);
    if (openDrugModalBtn) openDrugModalBtn.addEventListener('click', openDrugModalForCreate);
    if (closeDrugModalBtn) closeDrugModalBtn.addEventListener('click', closeDrugModal);
    if (drugForm) drugForm.addEventListener('submit', handleDrugFormSubmit);
    if (stockTableBody) stockTableBody.addEventListener('click', (event) => {
        if (event.target.classList.contains('edit-drug-btn')) {
            const drugId = event.target.dataset.id;
            openDrugModalForEdit(drugId);
        }
    });
}

// === SỬA ĐỔI: Hàm xử lý điều chỉnh kho (theo logic Type: import/export) ===
async function handleStockAdjustSubmit(event) {
    event.preventDefault();
    const drugId = document.getElementById('adjust-drug-select').value;
    const quantityInput = document.getElementById('adjust-quantity');
    const quantityValue = quantityInput.value;
    // Lấy giá trị type từ radio button đang được chọn
    const typeRadio = document.querySelector('input[name="adjustType"]:checked');
    const type = typeRadio ? typeRadio.value : null; // Lấy 'import' hoặc 'export'

    if (!drugId) {
        showAdminMessage("Vui lòng chọn thuốc để điều chỉnh.", true);
        return;
    }
    if (!type) { // Kiểm tra xem có chọn type chưa (dù mặc định là import)
         showAdminMessage("Vui lòng chọn loại điều chỉnh (Nhập kho/Xuất kho).", true);
         return;
    }

    const quantity = parseInt(quantityValue); // Số lượng (luôn dương)

    // Kiểm tra có phải là số nguyên dương không
    if (isNaN(quantity) || quantity <= 0) {
        showAdminMessage("Vui lòng nhập số lượng là một số nguyên dương.", true);
        quantityInput.focus();
        return;
    }

    // Gửi đi đối tượng khớp với StockAdjustRequest mới (có Type)
    const adjustData = {
        drugID: parseInt(drugId),
        quantity: quantity, // Quantity luôn dương
        type: type          // 'import' hoặc 'export'
    };

    try {
        await fetchAPI('/api/stock/adjust', 'POST', adjustData); // Gọi API backend
        showAdminMessage("Điều chỉnh tồn kho thành công!");
        document.getElementById('stock-adjust-form').reset(); // Reset form
         // Reset radio button về mặc định (import)
        const importRadio = document.querySelector('input[name="adjustType"][value="import"]');
        if (importRadio) importRadio.checked = true;
        await fetchDrugStock(); // Tải lại dữ liệu tồn kho
        displayDrugStock(); // Hiển thị lại bảng
    } catch (error) {
         console.error("Lỗi handleStockAdjustSubmit:", error);
         // Hiển thị lỗi cụ thể từ backend nếu có
         showAdminMessage(`Lỗi khi điều chỉnh tồn kho: ${error.message}`, true);
    }
}


function openDrugModalForCreate() {
    adminState.editingDrugId = null;
    document.getElementById('drug-form').reset();
    document.getElementById('drug-modal-title').textContent = "Thêm Thuốc mới";
    document.getElementById('drug-id').value = '';
    document.getElementById('drug-modal').classList.add('active');
}

function openDrugModalForEdit(drugId) {
    const drug = adminState.allDrugs.find(d => d.drugID == drugId);
    if (!drug) {
        showAdminMessage("Không tìm thấy thông tin thuốc để sửa.", true);
        return;
    }
    adminState.editingDrugId = drugId;
    document.getElementById('drug-modal-title').textContent = "Sửa thông tin Thuốc";
    document.getElementById('drug-id').value = drug.drugID;
    document.getElementById('drug-name').value = drug.drugName;
    document.getElementById('drug-unit').value = drug.unit || '';
    document.getElementById('drug-price').value = drug.price;
    document.getElementById('drug-modal').classList.add('active');
}

function closeDrugModal() {
    document.getElementById('drug-modal').classList.remove('active');
}

async function handleDrugFormSubmit(event) {
    event.preventDefault();
    const drugData = {
        drugName: document.getElementById('drug-name').value,
        unit: document.getElementById('drug-unit').value || null,
        price: parseFloat(document.getElementById('drug-price').value)
    };

    try {
        if (adminState.editingDrugId) {
            await fetchAPI(`/api/drugs/${adminState.editingDrugId}`, 'PUT', drugData);
            showAdminMessage("Cập nhật thông tin thuốc thành công!");
        } else {
            await fetchAPI('/api/drugs', 'POST', drugData);
            showAdminMessage("Thêm thuốc mới thành công!");
        }
        closeDrugModal();
        // Tải lại cả danh sách thuốc và tồn kho sau khi thêm/sửa
        await Promise.all([fetchDrugStock(), fetchAllDrugsForSelect()]);
        displayDrugStock();
        // Cập nhật lại dropdown điều chỉnh kho
        populateSelectOptions('adjust-drug-select', adminState.allDrugs, 'drugID', 'drugName', 'Chọn thuốc...');
    } catch (error) {
         console.error("Lỗi handleDrugFormSubmit:", error);
         showAdminMessage(`Lỗi khi lưu thuốc: ${error.message}`, true);
    }
}


// =================================================================
// CHỨC NĂNG 4: XEM BÁO CÁO (reports.html)
// =================================================================
function initReportsPage() {
     setupReportsEventListeners();
    const today = new Date();
    const firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
    document.getElementById('start-date').valueAsDate = firstDayOfMonth;
    document.getElementById('end-date').valueAsDate = today;
}

function setupReportsEventListeners() {
    const viewReportBtn = document.getElementById('view-report-btn');
    if(viewReportBtn) viewReportBtn.addEventListener('click', handleViewReport);
}

async function handleViewReport() {
     const startDate = document.getElementById('start-date').value;
    const endDate = document.getElementById('end-date').value;

    if (!startDate || !endDate) {
        showAdminMessage("Vui lòng chọn cả ngày bắt đầu và ngày kết thúc.", true);
        return;
    }
    if (new Date(startDate) > new Date(endDate)) {
        showAdminMessage("Ngày bắt đầu không được lớn hơn ngày kết thúc.", true);
        return;
    }

    await fetchRevenueReport(startDate, endDate);
    displayRevenueReport();
}

async function fetchRevenueReport(startDate, endDate) {
     try {
        const url = `/api/Reports/doctor-revenue?startDate=${startDate}&endDate=${endDate}`;
        adminState.revenueReport = await fetchAPI(url, 'GET');
    } catch (error) {
        console.error("Lỗi fetchRevenueReport:", error);
        showAdminMessage(`Không thể tải báo cáo: ${error.message}`, true);
        adminState.revenueReport = [];
    }
}

function displayRevenueReport() {
    const tableBody = document.getElementById('report-table-body');
    if (!tableBody) return;
    tableBody.innerHTML = '';

    if (!adminState.revenueReport || adminState.revenueReport.length === 0) {
        tableBody.innerHTML = `<tr><td colspan="5" class="text-center py-4 text-gray-500">Không có dữ liệu doanh thu cho khoảng thời gian này.</td></tr>`;
        return;
    }

    adminState.revenueReport.forEach(row => {
        const tr = document.createElement('tr');
        tr.className = "hover:bg-gray-50";
        tr.innerHTML = `
            <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">${row.doctorName}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${row.specialtyName || 'N/A'}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 text-right">${row.totalEncounters}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900 text-right">${formatCurrency(row.totalRevenue)}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 text-right">${formatCurrency(row.averageFee)}</td>
        `;
        tableBody.appendChild(tr);
    });
}


// --- Utility Functions ---
function populateSelectOptions(selectId, data, valueField, textField, defaultOption) {
    const select = document.getElementById(selectId);
    if (!select) return;
    select.innerHTML = defaultOption ? `<option value="">${defaultOption}</option>` : '';
    if (data && Array.isArray(data)) {
        data.forEach(item => {
            select.innerHTML += `<option value="${item[valueField]}">${item[textField]}</option>`;
        });
    } else {
        console.warn(`Dữ liệu cho select '${selectId}' không hợp lệ hoặc rỗng.`);
    }
}

// --- LƯỢC BỎ: fetchAPI và handleResponseError đã có trong app.js ---