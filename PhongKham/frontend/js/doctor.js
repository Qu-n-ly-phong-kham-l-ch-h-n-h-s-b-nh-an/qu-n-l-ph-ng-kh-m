// =================================================================
// SCRIPT HOÀN CHỈNH CHO VAI TRÒ BÁC SĨ (ĐÃ SỬA LỖI)
// =================================================================

const API_BASE_URL = 'https://localhost:7220'; // <-- Đảm bảo URL này chính xác

// --- GLOBAL STATE ---
const state = {
    todaysAppointments: [],
    allDrugs: [],
    currentEncounter: {
        appointmentId: null,
        prescriptionItems: []
    }
};

// --- INITIALIZATION ---
document.addEventListener('DOMContentLoaded', () => {
    const user = JSON.parse(sessionStorage.getItem('user'));
    if (!user || user.role.toLowerCase() !== 'doctor') {
        alert('Bạn không có quyền truy cập trang này.');
        window.location.href = '../Login/index.html';
        return;
    }
    
    document.getElementById('username-display').textContent = `Chào, ${user.username}!`;
    document.getElementById('logout-button').addEventListener('click', () => {
        sessionStorage.clear();
        window.location.href = '../Login/index.html';
    });
    
    initDoctorDashboard();
});

async function initDoctorDashboard() {
    setupEventListeners();
    try {
        await Promise.all([
            fetchTodaysAppointments(),
            fetchAllDrugs()
        ]);
        displayAppointments();
        populateSelectOptions('drug-select', state.allDrugs, 'drugID', 'drugName', 'Chọn một loại thuốc...');
    } catch (error) {
        showMessage("Lỗi khi tải dữ liệu: " + error.message);
    }
}

function setupEventListeners() {
    document.getElementById('appointments-list').addEventListener('click', handleAppointmentClick);
    document.getElementById('close-encounter-modal-btn').addEventListener('click', closeEncounterModal);
    document.getElementById('cancel-encounter-btn').addEventListener('click', closeEncounterModal);
    document.getElementById('encounter-form').addEventListener('submit', handleEncounterFormSubmit);
    document.getElementById('add-drug-btn').addEventListener('click', addDrugToPrescription);
    document.getElementById('prescription-items-body').addEventListener('click', handlePrescriptionActions);
    document.getElementById('close-message-modal-btn').addEventListener('click', closeMessageModal);
}

// --- API FETCHING ---
async function fetchTodaysAppointments() {
    const today = new Date().toISOString().split('T')[0];
    const appointments = await fetchAPI(`/api/Appointments?date=${today}&status=Đã đặt`);
    state.todaysAppointments = appointments || [];
}

async function fetchAllDrugs() {
    const drugs = await fetchAPI('/api/drugs');
    state.allDrugs = drugs || [];
}

// --- UI RENDERING ---
function displayAppointments() {
    const list = document.getElementById('appointments-list');
    const noAppMsg = document.getElementById('no-appointments-msg');
    list.innerHTML = ''; 

    if (state.todaysAppointments.length === 0) {
        noAppMsg.textContent = "Không có lịch hẹn nào hôm nay.";
        noAppMsg.classList.remove('hidden');
        return;
    }

    noAppMsg.classList.add('hidden');
    state.todaysAppointments.forEach(app => {
        const item = document.createElement('div');
        item.className = 'p-4 border-b hover:bg-indigo-50 cursor-pointer';
        item.dataset.appointmentId = app.appointmentID;
        item.innerHTML = `
            <div class="flex justify-between items-center">
                <div>
                    <p class="font-bold text-lg text-indigo-700">${app.patientName}</p>
                    <p class="text-sm text-gray-600">Thời gian: ${new Date(app.appointmentDate).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}</p>
                </div>
                <span class="text-indigo-600 font-semibold">&#9654;</span>
            </div>
        `;
        list.appendChild(item);
    });
}

function displayPrescriptionItems() {
    const tableBody = document.getElementById('prescription-items-body');
    tableBody.innerHTML = '';
    if (state.currentEncounter.prescriptionItems.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="4" class="p-4 text-center text-gray-500">Chưa có thuốc nào được thêm.</td></tr>';
        return;
    }
    
    state.currentEncounter.prescriptionItems.forEach((item, index) => {
        const drug = state.allDrugs.find(d => d.drugID === item.drugID);
        const row = `
            <tr class="text-sm">
                <td class="px-4 py-2">${drug?.drugName || 'Không tìm thấy thuốc'}</td>
                <td class="px-4 py-2">${item.quantity}</td>
                <td class="px-4 py-2">${item.usage}</td>
                <td class="px-4 py-2 text-right">
                    <button type="button" data-index="${index}" class="remove-drug-btn text-red-500 hover:text-red-700 font-semibold">Xóa</button>
                </td>
            </tr>
        `;
        tableBody.innerHTML += row;
    });
}

// --- EVENT HANDLERS ---
function handleAppointmentClick(e) {
    const appointmentDiv = e.target.closest('[data-appointment-id]');
    if (appointmentDiv) {
        openEncounterModal(parseInt(appointmentDiv.dataset.appointmentId));
    }
}

function openEncounterModal(appointmentId) {
    const appointment = state.todaysAppointments.find(app => app.appointmentID === appointmentId);
    if (!appointment) return;

    document.getElementById('encounter-form').reset();
    state.currentEncounter = { appointmentId, prescriptionItems: [] };
    
    document.getElementById('encounter-appointment-id').value = appointmentId;
    document.getElementById('modal-patient-name').textContent = appointment.patientName;
    document.getElementById('modal-appointment-time').textContent = new Date(appointment.appointmentDate).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });

    displayPrescriptionItems();
    document.getElementById('encounter-modal').classList.add('active');
}

function closeEncounterModal() {
    document.getElementById('encounter-modal').classList.remove('active');
}

function addDrugToPrescription() {
    const drugId = parseInt(document.getElementById('drug-select').value);
    const quantity = parseInt(document.getElementById('drug-quantity').value);
    const usage = document.getElementById('drug-usage').value;

    if (!drugId || !quantity || quantity <= 0) {
        showMessage("Vui lòng chọn thuốc và nhập số lượng hợp lệ.");
        return;
    }

    state.currentEncounter.prescriptionItems.push({ drugID: drugId, quantity, usage });
    displayPrescriptionItems();

    document.getElementById('drug-select').value = '';
    document.getElementById('drug-quantity').value = 1;
    document.getElementById('drug-usage').value = '';
}

function handlePrescriptionActions(e) {
    if (e.target.classList.contains('remove-drug-btn')) {
        const index = parseInt(e.target.dataset.index);
        state.currentEncounter.prescriptionItems.splice(index, 1);
        displayPrescriptionItems();
    }
}

async function handleEncounterFormSubmit(e) {
    e.preventDefault();
    const submitButton = e.target.querySelector('button[type="submit"]');
    submitButton.disabled = true;
    submitButton.textContent = "Đang xử lý...";
    
    const encounterData = {
        appointmentID: state.currentEncounter.appointmentId,
        examinationNotes: document.getElementById('examination-notes').value,
        diagnosisDescription: document.getElementById('diagnosis-description').value,
        serviceFee: parseFloat(document.getElementById('service-fee').value),
        prescriptionItems: state.currentEncounter.prescriptionItems
    };

    try {
        await fetchAPI('/api/Encounters/complete', 'POST', encounterData);
        showMessage("Hoàn tất lần khám thành công!");
        closeEncounterModal();
        await fetchTodaysAppointments();
        displayAppointments();
    } catch (error) {
        showMessage(error.message || "Đã xảy ra lỗi khi hoàn tất khám.");
    } finally {
        submitButton.disabled = false;
        submitButton.textContent = "Hoàn tất Khám";
    }
}


// =================================================================
// SHARED HELPER FUNCTIONS (Copied from app.js)
// =================================================================
async function fetchAPI(endpoint, method = 'GET', body = null) {
    const token = sessionStorage.getItem('jwt_token');
    const headers = { 'Content-Type': 'application/json' };
    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }
    const config = { method, headers };
    if (body) {
        config.body = JSON.stringify(body);
    }
    try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, config);
        if (!response.ok) {
            await handleResponseError(response);
        }
        const contentType = response.headers.get("content-type");
        if (contentType && contentType.includes("application/json")) {
            return response.json();
        }
        return null;
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
    const modal = document.getElementById('message-modal');
    if (modal) {
        modal.classList.remove('active');
    }
}