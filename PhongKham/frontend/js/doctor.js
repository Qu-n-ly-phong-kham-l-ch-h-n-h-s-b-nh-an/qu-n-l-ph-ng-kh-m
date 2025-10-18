// =================================================================
// SCRIPT HOÀN CHỈNH CHO VAI TRÒ BÁC SĨ (ĐÃ CẢI TIẾN)
// =================================================================

const API_BASE_URL = 'https://localhost:7220'; // <-- Đảm bảo URL này chính xác

// --- GLOBAL STATE ---
const state = {
    todaysAppointments: [],
    allDrugs: [],
    currentEncounter: {
        appointmentId: null,
        prescriptionItems: [] // Lưu trữ { drugID, quantity, usage, drugName, price }
    }
};

// --- INITIALIZATION ---
document.addEventListener('DOMContentLoaded', () => {
    // 1. Kiểm tra xác thực và vai trò
    const user = JSON.parse(sessionStorage.getItem('user'));
    if (!user || user.role.toLowerCase() !== 'doctor') {
        alert('Bạn không có quyền truy cập trang này.');
        window.location.href = '../Login/index.html'; // Điều hướng về trang chọn vai trò
        return;
    }
    
    // 2. Thiết lập giao diện chung
    document.getElementById('username-display').textContent = `Chào, ${user.username}!`;
    document.getElementById('logout-button').addEventListener('click', () => {
        sessionStorage.clear();
        window.location.href = '../Login/index.html';
    });
    
    // 3. Khởi chạy logic của trang
    initDoctorDashboard();
});

/**
 * Khởi tạo Dashboard: Tải dữ liệu cần thiết và gán sự kiện
 */
async function initDoctorDashboard() {
    setupEventListeners();
    try {
        // Tải song song lịch hẹn và danh sách thuốc
        await Promise.all([
            fetchTodaysAppointments(),
            fetchAllDrugs()
        ]);
        
        // Chỉ hiển thị sau khi đã có dữ liệu
        displayAppointments();
        // Điền dữ liệu vào dropdown chọn thuốc
        populateSelectOptions('drug-select', state.allDrugs, 'drugID', 'drugName', '--- Chọn loại thuốc ---');

    } catch (error) {
        showMessage(`Lỗi tải dữ liệu ban đầu: ${error.message}`);
    }
}

/**
 * Gán các sự kiện click cho các nút bấm
 */
function setupEventListeners() {
    // Nút đóng modal
    document.getElementById('close-encounter-modal-btn').addEventListener('click', closeEncounterModal);
    document.getElementById('close-message-modal-btn').addEventListener('click', closeMessageModal);
    
    // Nút thêm thuốc
    document.getElementById('add-drug-btn').addEventListener('click', handleAddDrug);
    
    // Submit form khám bệnh
    document.getElementById('encounter-form').addEventListener('submit', handleEncounterSubmit);

    // === SỬA ĐỔI 3: Thêm sự kiện 'input' cho ô Phí dịch vụ ===
    document.getElementById('encounter-fee').addEventListener('input', () => {
        // Cập nhật hiển thị phí dịch vụ và tổng tiền
        const fee = parseFloat(document.getElementById('encounter-fee').value) || 0;
        const formatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' });
        document.getElementById('service-fee-display').textContent = formatter.format(fee);
        updateTotalFees(); // Gọi hàm tính tổng
    });
}

// --- DATA FETCHING (LẤY DỮ LIỆU) ---

/**
 * Chức năng 1: Lấy lịch hẹn của bác sĩ
 * API: GET /api/Appointments
 */
async function fetchTodaysAppointments() {
    try {
        const data = await fetchAPI('/api/Appointments', 'GET');
        // Backend tự lọc theo DoctorID của người đăng nhập
        // Chúng ta lọc thêm ở frontend để chỉ hiển thị các lịch "Đã đặt"
        state.todaysAppointments = data.filter(appt => appt.status.toLowerCase() === 'đã đặt');
    } catch (error) {
        console.error('Lỗi fetchTodaysAppointments:', error);
        showMessage(error.message);
    }
}

/**
 * Lấy danh sách thuốc để kê đơn
 * API: GET /api/drugs
 */
async function fetchAllDrugs() {
    try {
        // API này cần vai trò Doctor (đã kiểm tra trong DrugController.cs của bạn)
        state.allDrugs = await fetchAPI('/api/drugs', 'GET');
    } catch (error) {
        console.error('Lỗi fetchAllDrugs:', error);
        showMessage(`Không thể tải danh sách thuốc: ${error.message}`);
    }
}

// --- UI RENDERING (HIỂN THỊ GIAO DIỆN) ---

/**
 * Hiển thị danh sách lịch hẹn ra cột bên trái
 */
function displayAppointments() {
    const listElement = document.getElementById('appointments-list');
    listElement.innerHTML = ''; // Xóa danh sách cũ

    if (state.todaysAppointments.length === 0) {
        listElement.innerHTML = '<p class="text-gray-500">Không có lịch hẹn nào "Đã đặt".</p>';
        return;
    }

    state.todaysAppointments.forEach(appt => {
        const button = document.createElement('button');
        button.className = "text-left p-3 border rounded-lg hover:bg-gray-50 w-full";
        button.innerHTML = `
            <strong class="text-indigo-600">${appt.patientName}</strong>
            <p class="text-sm text-gray-700">Giờ hẹn: ${formatTime(appt.appointmentDate)}</p>
        `;
        // Gán sự kiện click để mở modal
        button.addEventListener('click', () => openEncounterModal(appt));
        listElement.appendChild(button);
    });
}

/**
 * === SỬA ĐỔI 4: Cập nhật hàm render danh sách thuốc ===
 * Hiển thị danh sách thuốc trong đơn, bao gồm giá và nút xóa
 */
function renderPrescriptionList() {
    const listElement = document.getElementById('prescription-list');
    listElement.innerHTML = ''; // Xóa cũ

    if (state.currentEncounter.prescriptionItems.length === 0) {
        listElement.innerHTML = '<li class="text-gray-500 italic">Chưa có thuốc nào được thêm.</li>';
        return;
    }

    state.currentEncounter.prescriptionItems.forEach((item, index) => {
        const li = document.createElement('li');
        li.className = "flex justify-between items-center py-1";
        // Hiển thị cả giá tiền của thuốc
        li.innerHTML = `
            <span>
                <strong>${item.drugName}</strong> (SL: ${item.quantity}) - ${item.price.toLocaleString('vi-VN')} VNĐ/đv
                <br>
                <span class="text-sm text-gray-600">${item.usage}</span>
            </span>
            <button type="button" class="text-red-500 hover:text-red-700 font-semibold px-2" data-index="${index}">Xóa</button>
        `;
        
        // Gán sự kiện cho nút xóa
        li.querySelector('button').addEventListener('click', (e) => {
            const indexToRemove = parseInt(e.currentTarget.getAttribute('data-index'));
            removeDrugFromPrescription(indexToRemove);
        });
        
        listElement.appendChild(li);
    });
}


// --- EVENT HANDLERS (XỬ LÝ SỰ KIỆN) ---

/**
 * === SỬA ĐỔI 6: Cập nhật hàm mở Modal ===
 * Mở modal khám bệnh và reset hiển thị chi phí
 */
function openEncounterModal(appointment) {
    // 1. Reset trạng thái
    state.currentEncounter = {
        appointmentId: appointment.appointmentID,
        prescriptionItems: []
    };
    
    // 2. Reset form
    document.getElementById('encounter-form').reset();
    const defaultFee = 100000;
    document.getElementById('encounter-fee').value = defaultFee; // Set phí khám mặc định
    
    renderPrescriptionList(); // Xóa danh sách thuốc

    // 3. Điền thông tin bệnh nhân
    document.getElementById('encounter-patient-name').textContent = appointment.patientName;
    
    // 4. Reset hiển thị chi phí
    const formatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' });
    document.getElementById('service-fee-display').textContent = formatter.format(defaultFee);
    document.getElementById('drug-fee-display').textContent = formatter.format(0);
    document.getElementById('total-fee-display').textContent = formatter.format(defaultFee);

    // 5. Hiển thị modal
    document.getElementById('encounter-modal').classList.add('active');
}

function closeEncounterModal() {
    document.getElementById('encounter-modal').classList.remove('active');
}

/**
 * === SỬA ĐỔI 2: Cập nhật hàm thêm thuốc ===
 * Thêm một loại thuốc vào đơn thuốc (trong state)
 */
function handleAddDrug() {
    const drugId = document.getElementById('drug-select').value;
    const quantity = parseInt(document.getElementById('drug-quantity').value);
    const usage = document.getElementById('drug-usage').value;

    if (!drugId) {
        showMessage("Vui lòng chọn một loại thuốc.");
        return;
    }
    if (isNaN(quantity) || quantity <= 0) {
        showMessage("Số lượng thuốc phải là số dương.");
        return;
    }
    if (!usage) {
        showMessage("Vui lòng nhập cách dùng thuốc.");
        return;
    }

    // Xóa bỏ kiểm tra thuốc trùng lặp
    
    // Lấy thông tin thuốc (bao gồm cả giá) từ state.allDrugs
    const selectedDrug = state.allDrugs.find(d => d.drugID == drugId);
    if (!selectedDrug) {
        showMessage("Lỗi: Không tìm thấy thông tin thuốc.");
        return;
    }

    // Thêm vào state (bao gồm cả price và drugName)
    state.currentEncounter.prescriptionItems.push({
        drugID: parseInt(drugId),
        quantity: quantity,
        usage: usage,
        drugName: selectedDrug.drugName, // Để hiển thị
        price: selectedDrug.price        // Để tính tiền
    });

    // Cập nhật UI (hiển thị danh sách thuốc)
    renderPrescriptionList();
    
    // Cập nhật tổng tiền
    updateTotalFees();

    // Reset các ô nhập
    document.getElementById('drug-select').value = '';
    document.getElementById('drug-quantity').value = '';
    document.getElementById('drug-usage').value = '';
}

/**
 * === SỬA ĐỔI 5: Cập nhật hàm xóa thuốc ===
 * Xóa một loại thuốc khỏi đơn (trong state) bằng index
 */
function removeDrugFromPrescription(indexToRemove) {
    if (indexToRemove > -1 && indexToRemove < state.currentEncounter.prescriptionItems.length) {
        state.currentEncounter.prescriptionItems.splice(indexToRemove, 1);
    }
    
    renderPrescriptionList(); // Cập nhật lại UI
    updateTotalFees(); // Cập nhật tổng tiền
}

/**
 * Chức năng 2: Gửi thông tin khám bệnh lên server
 * API: POST /api/Encounters/complete
 */
async function handleEncounterSubmit(event) {
    event.preventDefault(); // Ngăn form reload trang
    
    // 1. Lấy dữ liệu từ form
    const notes = document.getElementById('encounter-notes').value;
    const diagnosis = document.getElementById('encounter-diagnosis').value;
    const fee = parseFloat(document.getElementById('encounter-fee').value);

    // 2. Tạo body request (khớp với CompleteEncounterRequest)
    const body = {
        appointmentID: state.currentEncounter.appointmentId,
        examinationNotes: notes,
        diagnosisDescription: diagnosis,
        serviceFee: fee,
        // Chỉ gửi đi các trường cần thiết cho backend
        prescriptionItems: state.currentEncounter.prescriptionItems.map(item => ({
            drugID: item.drugID,
            quantity: item.quantity,
            usage: item.usage
        }))
    };

    // 3. Gửi API
    try {
        const result = await fetchAPI('/api/Encounters/complete', 'POST', body);
        
        // 4. Xử lý thành công
        closeEncounterModal();
        showMessage(result.message || "Hoàn tất khám bệnh thành công!");
        
        // 5. Tải lại danh sách lịch hẹn
        await fetchTodaysAppointments();
        displayAppointments();

    } catch (error) {
        console.error('Lỗi handleEncounterSubmit:', error);
        showMessage(error.message);
    }
}

/**
 * === SỬA ĐỔI 7: Thêm hàm mới để tính tổng chi phí ===
 * Tính toán và cập nhật chi phí trên UI
 */
function updateTotalFees() {
    // 1. Lấy phí dịch vụ từ ô input
    const serviceFeeInput = document.getElementById('encounter-fee');
    const serviceFee = parseFloat(serviceFeeInput.value) || 0;

    // 2. Tính tổng tiền thuốc từ state
    const drugFee = state.currentEncounter.prescriptionItems.reduce((total, item) => {
        return total + (item.price * item.quantity);
    }, 0);

    // 3. Tính tổng cộng
    const totalAmount = serviceFee + drugFee;

    // 4. Cập nhật giao diện
    const formatter = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' });

    document.getElementById('service-fee-display').textContent = formatter.format(serviceFee);
    document.getElementById('drug-fee-display').textContent = formatter.format(drugFee);
    document.getElementById('total-fee-display').textContent = formatter.format(totalAmount);
}


// --- HELPER FUNCTIONS (HÀM HỖ TRỢ) ---

function formatTime(dateTimeString) {
    const date = new Date(dateTimeString);
    return date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
}

function populateSelectOptions(selectId, data, valueField, textField, defaultOption) {
    const select = document.getElementById(selectId);
    if (!select) return;
    select.innerHTML = defaultOption ? `<option value="">${defaultOption}</option>` : '';
    if (data) {
        data.forEach(item => {
            select.innerHTML += `<option value="${item[valueField]}">${item[textField]}</option>`;
        });
    }
}

function showMessage(message) {
    const modal = document.getElementById('message-modal');
    const text = document.getElementById('message-text');
    if (modal && text) {
        text.textContent = message;
        modal.classList.add('active');
    }
}

function closeMessageModal() {
    document.getElementById('message-modal').classList.remove('active');
}

// --- HÀM GỌI API (LẤY TỪ FILE doctor.js CỦA BẠN) ---
async function fetchAPI(endpoint, method = 'GET', body = null) {
    const token = sessionStorage.getItem('jwt_token');
    const headers = {
        'Content-Type': 'application/json',
    };
    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }

    const config = {
        method,
        headers,
    };
    if (body) {
        config.body = JSON.stringify(body);
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, config);
        if (!response.ok) {
            await handleResponseError(response);
        }
        
        const contentType = response.headers.get("content-type");
        if (contentType && contentType.indexOf("application/json") !== -1) {
            return response.json();
        } 
        return { message: "Thao tác thành công (không có nội dung trả về)." }; // Trả về thông điệp mặc định nếu 200 OK nhưng không có JSON

    } catch (error) {
        console.error(`API Fetch Error (${method} ${endpoint}):`, error);
        if (error.message.includes("Failed to fetch")) {
            throw new Error("Lỗi kết nối: Không thể kết nối đến máy chủ.");
        }
        throw error;
    }
}

async function handleResponseError(response) {
    let errorMessage = `Lỗi ${response.status}: ${response.statusText || 'Lỗi không xác định.'}`;
    try {
        const errorData = await response.json();
        if (errorData && errorData.message) {
            errorMessage = errorData.message;
        }
    } catch (e) { /* Ignore if no JSON body */ }
    throw new Error(errorMessage);
}