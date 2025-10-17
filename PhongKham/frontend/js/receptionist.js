document.addEventListener('DOMContentLoaded', () => {

    // Shared initialization for all receptionist pages
    const user = JSON.parse(sessionStorage.getItem('user'));
    if (!user || user.role.toLowerCase() !== 'receptionist') {
        alert('Bạn không có quyền truy cập trang này. Vui lòng đăng nhập với vai trò Lễ tân.');
        window.location.href = '../Login/index.html';
        return;
    }

    // Setup common elements like username display and logout button
    const usernameDisplay = document.getElementById('username-display');
    const logoutButton = document.getElementById('logout-button');
    if(usernameDisplay) usernameDisplay.textContent = `Chào, ${user.username}!`;
    if(logoutButton) logoutButton.addEventListener('click', () => {
        sessionStorage.clear();
        window.location.href = '../Login/index.html';
    });

    // Page-specific initialization based on body ID
    const pageId = document.body.id;
    switch (pageId) {
        case 'appointments-page':
            initAppointmentsPage();
            break;
        case 'patients-page':
            initPatientsPage();
            break;
        case 'invoices-page':
            initInvoicesPage();
            break;
    }
});

// --- GLOBAL STATE ---
const state = {
    appointments: [],
    patients: [],
    doctors: [],
    unpaidInvoices: [],
    paidInvoices: [],
    patientAccounts: [],
    editingId: null,
};

// =================================================================
// APPOINTMENTS PAGE LOGIC
// =================================================================
async function initAppointmentsPage() {
    setupAppointmentEventListeners();
    document.getElementById('appointment-date-filter').valueAsDate = new Date();
    
    const today = new Date().toISOString().split('T')[0];
    document.getElementById('appointment-date').min = today;

    generateTimeSlots();

    await Promise.all([fetchDoctors(), fetchPatients(), fetchAppointments()]);
    
    populateSelectOptions('doctor-filter', state.doctors, 'doctorID', 'fullName', 'Tất cả bác sĩ');
    populateSelectOptions('appointment-doctor', state.doctors, 'doctorID', 'fullName', 'Chọn bác sĩ');
    populateSelectOptions('appointment-patient', state.patients, 'patientID', 'fullName', 'Chọn bệnh nhân');
    
    refreshAppointmentsDisplay();
}

function setupAppointmentEventListeners() {
    document.getElementById('open-appointment-modal-btn').addEventListener('click', openAppointmentModalForCreate);
    document.getElementById('close-appointment-modal-btn').addEventListener('click', closeAppointmentModal);
    document.getElementById('appointment-form').addEventListener('submit', handleAppointmentFormSubmit);
    document.getElementById('appointment-date-filter').addEventListener('change', refreshAppointmentsDisplay);
    document.getElementById('doctor-filter').addEventListener('change', refreshAppointmentsDisplay);
    document.getElementById('appointments-table-body').addEventListener('click', handleAppointmentsTableActions);
    document.getElementById('close-message-modal-btn').addEventListener('click', closeMessageModal);
}

function generateTimeSlots() {
    const timeSelect = document.getElementById('appointment-time');
    timeSelect.innerHTML = '';
    for (let hour = 7; hour < 18; hour++) {
        for (let minute of ['00', '30']) {
            if (hour === 17 && minute === '30') continue;
            const time = `${hour.toString().padStart(2, '0')}:${minute}`;
            const option = document.createElement('option');
            option.value = time;
            option.textContent = time;
            timeSelect.appendChild(option);
        }
    }
}

async function handleAppointmentFormSubmit(e) {
    e.preventDefault();
    const date = document.getElementById('appointment-date').value;
    const time = document.getElementById('appointment-time').value;
    
    const appointmentData = {
        patientID: parseInt(document.getElementById('appointment-patient').value),
        doctorID: parseInt(document.getElementById('appointment-doctor').value),
        appointmentDate: `${date}T${time}:00`,
        notes: document.getElementById('appointment-notes').value
    };

    try {
        if (state.editingId) {
            await fetchAPI(`/api/Appointments/${state.editingId}`, 'PUT', appointmentData);
            showMessage("Cập nhật lịch hẹn thành công!");
        } else {
            await fetchAPI('/api/Appointments', 'POST', appointmentData);
            showMessage("Tạo lịch hẹn mới thành công!");
        }
        closeAppointmentModal();
        await fetchAppointments();
        refreshAppointmentsDisplay();
    } catch (error) {
        showMessage(error.message || "Lỗi khi lưu lịch hẹn.");
    }
}

function openAppointmentModalForCreate() {
    document.getElementById('appointment-form').reset();
    document.getElementById('appointment-modal-title').textContent = "Tạo Lịch hẹn mới";
    state.editingId = null;
    document.getElementById('appointment-modal').classList.remove('hidden');
    document.getElementById('appointment-modal').style.display = 'flex';
}

function openAppointmentModalForEdit(appointmentId) {
    const app = state.appointments.find(a => a.appointmentID === appointmentId);
    if (!app) return;

    state.editingId = appointmentId;
    document.getElementById('appointment-modal-title').textContent = "Sửa Lịch hẹn";
    document.getElementById('appointment-id').value = app.appointmentID;
    document.getElementById('appointment-patient').value = app.patientID;
    document.getElementById('appointment-doctor').value = app.doctorID;
    
    const appDate = new Date(app.appointmentDate);
    document.getElementById('appointment-date').value = appDate.toISOString().split('T')[0];
    document.getElementById('appointment-time').value = appDate.toTimeString().slice(0, 5);
    
    document.getElementById('appointment-notes').value = app.notes || '';
    document.getElementById('appointment-modal').classList.remove('hidden');
    document.getElementById('appointment-modal').style.display = 'flex';
}

function handleAppointmentsTableActions(e) {
    const target = e.target;
    const id = parseInt(target.dataset.id);
    if (target.classList.contains('edit-appointment-btn')) {
        openAppointmentModalForEdit(id);
    }
    if (target.classList.contains('cancel-appointment-btn')) {
        if (confirm('Bạn có chắc chắn muốn hủy lịch hẹn này?')) {
            cancelAppointment(id);
        }
    }
}

async function cancelAppointment(id) {
    try {
        await fetchAPI(`/api/Appointments/${id}/cancel`, 'PATCH');
        showMessage("Hủy lịch hẹn thành công.");
        await fetchAppointments();
        refreshAppointmentsDisplay();
    } catch (error) {
        showMessage(error.message || "Lỗi khi hủy lịch hẹn.");
    }
}

function displayAppointments(appointmentsToDisplay) {
    const tableBody = document.getElementById('appointments-table-body');
    
    if (appointmentsToDisplay.length === 0) {
        tableBody.innerHTML = `<tr><td colspan="5" class="text-center py-4">Không có lịch hẹn nào cho ngày này.</td></tr>`;
        return;
    }

    tableBody.innerHTML = appointmentsToDisplay.map(app => {
        const patient = state.patients.find(p => p.patientID === app.patientID);
        const doctor = state.doctors.find(d => d.doctorID === app.doctorID);
        const appointmentDateTime = new Date(app.appointmentDate);
        const time = appointmentDateTime.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });

        const actions = app.status.toLowerCase() !== 'cancelled' 
            ? `<button data-id="${app.appointmentID}" class="edit-appointment-btn text-indigo-600 hover:text-indigo-900 font-medium mr-3">Sửa</button>
               <button data-id="${app.appointmentID}" class="cancel-appointment-btn text-red-600 hover:text-red-900 font-medium">Hủy</button>`
            : '';

        let statusClass = '';
        switch (app.status.toLowerCase()) {
            case 'scheduled': statusClass = 'text-blue-600'; break;
            case 'completed': statusClass = 'text-green-600'; break;
            case 'cancelled': statusClass = 'text-red-600'; break;
            default: statusClass = 'text-gray-600';
        }

        return `
            <tr class="hover:bg-gray-50">
                <td class="px-6 py-4 font-bold">${time}</td>
                <td class="px-6 py-4">${patient ? patient.fullName : 'Không rõ'}</td>
                <td class="px-6 py-4">${doctor ? doctor.fullName : 'Không rõ'}</td>
                <td class="px-6 py-4"><span class="font-semibold ${statusClass}">${app.status}</span></td>
                <td class="px-6 py-4 text-sm font-medium">${actions}</td>
            </tr>`;
    }).join('');
}

// =================================================================
// PATIENTS PAGE LOGIC
// =================================================================
async function initPatientsPage() {
    setupPatientEventListeners();
    await Promise.all([fetchPatients(), fetchPatientAccounts()]);
    displayPatients();
}

function setupPatientEventListeners() {
    document.getElementById('open-patient-create-modal-btn').addEventListener('click', openPatientModalForCreate);
    document.getElementById('open-patient-update-modal-btn').addEventListener('click', openPatientModalForUpdate);
    document.getElementById('close-patient-modal-btn').addEventListener('click', closePatientModal);
    document.getElementById('patient-form').addEventListener('submit', handlePatientFormSubmit);
    document.getElementById('patient-select').addEventListener('change', populateUpdateForm);
    document.getElementById('close-message-modal-btn').addEventListener('click', closeMessageModal);
    
    document.getElementById('patients-table-body').addEventListener('click', handlePatientRowClick);
    document.getElementById('close-patient-detail-modal-btn').addEventListener('click', closePatientDetailModal);
}

function openPatientModalForCreate() {
    state.editingId = null;
    document.getElementById('patient-form').reset();
    document.getElementById('patient-modal-title').textContent = "Thêm Bệnh nhân mới";
    
    document.getElementById('patient-select-container').classList.add('hidden');
    document.getElementById('patient-fullname').readOnly = false;
    document.getElementById('account-create-fields').classList.remove('hidden');
    document.getElementById('account-link-fields').classList.add('hidden');

    document.getElementById('patient-modal').classList.remove('hidden');
    document.getElementById('patient-modal').style.display = 'flex';
}

function openPatientModalForUpdate() {
    state.editingId = null;
    document.getElementById('patient-form').reset();
    document.getElementById('patient-modal-title').textContent = "Cập nhật Hồ sơ Bệnh nhân";

    document.getElementById('patient-select-container').classList.remove('hidden');
    document.getElementById('patient-fullname').readOnly = false;
    document.getElementById('account-create-fields').classList.add('hidden');
    document.getElementById('account-link-fields').classList.remove('hidden');

    populateSelectOptions('patient-select', state.patients, 'patientID', 'fullName', 'Chọn bệnh nhân...');
    document.getElementById('account-select').innerHTML = '<option value="">-- Vui lòng chọn bệnh nhân --</option>';
    
    document.getElementById('patient-modal').classList.remove('hidden');
    document.getElementById('patient-modal').style.display = 'flex';
}

function populateUpdateForm() {
    const patientId = document.getElementById('patient-select').value;
    const accountSelect = document.getElementById('account-select');

    if (!patientId) {
        document.getElementById('patient-form').reset();
        state.editingId = null;
        accountSelect.innerHTML = '<option value="">-- Vui lòng chọn bệnh nhân --</option>';
        return;
    }

    state.editingId = parseInt(patientId);
    const patient = state.patients.find(p => p.patientID === state.editingId);
    if (patient) {
        document.getElementById('patient-fullname').value = patient.fullName;
        document.getElementById('patient-dob').value = patient.dob ? new Date(patient.dob).toISOString().split('T')[0] : '';
        document.getElementById('patient-gender').value = patient.gender || 'Nam';
        document.getElementById('patient-phone').value = patient.phone || '';
        document.getElementById('patient-email').value = patient.email || '';
        document.getElementById('patient-address').value = patient.address || '';
        document.getElementById('patient-medical-history').value = patient.medicalHistory || '';
        
        const otherLinkedAccountIds = state.patients
            .filter(p => p.patientID !== patient.patientID && p.accountID != null)
            .map(p => p.accountID);

        const availableAccounts = state.patientAccounts.filter(acc => 
            !otherLinkedAccountIds.includes(acc.accountID)
        );

        populateSelectOptions('account-select', availableAccounts, 'accountID', 'username', 'Không liên kết');
        
        accountSelect.value = patient.accountID || "";
    }
}

async function handlePatientFormSubmit(e) {
    e.preventDefault();
    
    try {
        if (state.editingId) { // UPDATE MODE
            const accountIdValue = document.getElementById('account-select').value;
            const patientUpdateData = {
                fullName: document.getElementById('patient-fullname').value,
                dob: document.getElementById('patient-dob').value || null,
                gender: document.getElementById('patient-gender').value,
                phone: document.getElementById('patient-phone').value || null,
                email: document.getElementById('patient-email').value || null,
                address: document.getElementById('patient-address').value || null,
                medicalHistory: document.getElementById('patient-medical-history').value || null,
                accountID: accountIdValue ? parseInt(accountIdValue) : null,
            };
            await fetchAPI(`/api/Patients/${state.editingId}`, 'PUT', patientUpdateData);
            showMessage("Cập nhật bệnh nhân thành công!");
        } else { // CREATE MODE
            const patientCreateData = {
                fullName: document.getElementById('patient-fullname').value,
                dob: document.getElementById('patient-dob').value || null,
                gender: document.getElementById('patient-gender').value,
                phone: document.getElementById('patient-phone').value || null,
                email: document.getElementById('patient-email').value || null,
                address: document.getElementById('patient-address').value || null,
                medicalHistory: document.getElementById('patient-medical-history').value || null,
                accountUsername: document.getElementById('patient-username').value || null,
                accountPassword: document.getElementById('patient-password').value || null,
            };
            await fetchAPI('/api/Patients', 'POST', patientCreateData);
            showMessage("Thêm bệnh nhân mới thành công!");
        }

        closePatientModal();
        await Promise.all([fetchPatients(), fetchPatientAccounts()]);
        displayPatients();

    } catch (error) {
        showMessage(error.message || "Lỗi khi lưu thông tin bệnh nhân.");
    }
}

function displayPatients() {
    const tableBody = document.getElementById('patients-table-body');
    tableBody.innerHTML = state.patients.map(p => `
        <tr class="hover:bg-gray-100 cursor-pointer patient-row" data-id="${p.patientID}">
            <td class="px-6 py-4 font-medium text-gray-900">${p.fullName}</td>
            <td class="px-6 py-4">${p.dob ? new Date(p.dob).toLocaleDateString('vi-VN') : 'Chưa cập nhật'}</td>
            <td class="px-6 py-4">${p.gender || 'Chưa cập nhật'}</td>
            <td class="px-6 py-4">${p.phone || 'Chưa cập nhật'}</td>
        </tr>`).join('');
}

function handlePatientRowClick(e) {
    const row = e.target.closest('.patient-row');
    if (!row) return;

    const patientId = parseInt(row.dataset.id);
    const patient = state.patients.find(p => p.patientID === patientId);

    if (patient) {
        openPatientDetailModal(patient);
    }
}

function openPatientDetailModal(patient) {
    document.getElementById('detail-patient-name').textContent = patient.fullName;
    document.getElementById('detail-patient-dob').textContent = patient.dob ? new Date(patient.dob).toLocaleDateString('vi-VN') : 'N/A';
    document.getElementById('detail-patient-gender').textContent = patient.gender || 'N/A';
    document.getElementById('detail-patient-phone').textContent = patient.phone || 'N/A';
    document.getElementById('detail-patient-email').textContent = patient.email || 'N/A';
    document.getElementById('detail-patient-address').textContent = patient.address || 'N/A';
    document.getElementById('detail-patient-medical-history').textContent = patient.medicalHistory || 'N/A';

    const linkedAccount = state.patientAccounts.find(acc => acc.accountID === patient.accountID);
    document.getElementById('detail-patient-account').textContent = linkedAccount ? linkedAccount.username : 'Chưa liên kết';
    
    document.getElementById('patient-detail-modal').classList.remove('hidden');
    document.getElementById('patient-detail-modal').style.display = 'flex';
}

// =================================================================
// INVOICES PAGE LOGIC (No changes below)
// =================================================================
async function initInvoicesPage() {
    document.querySelectorAll('.invoice-tab-btn').forEach(btn => btn.addEventListener('click', handleInvoiceTabSwitch));
    document.getElementById('invoices-table-body').addEventListener('click', handleInvoicesTableActions);
    document.getElementById('close-invoice-detail-modal-btn').addEventListener('click', closeInvoiceDetailModal);
    document.getElementById('close-message-modal-btn').addEventListener('click', closeMessageModal);
    
    await Promise.all([fetchUnpaidInvoices(), fetchPaidInvoices()]);
    displayInvoices('unpaid');
}

async function handleInvoiceTabSwitch(e) {
    document.querySelectorAll('.invoice-tab-btn').forEach(btn => {
        btn.classList.remove('tab-active');
        btn.classList.add('text-gray-500');
    });
    e.target.classList.add('tab-active');
    e.target.classList.remove('text-gray-500');
    
    if (e.target.id === 'tab-unpaid') {
        document.getElementById('invoice-action-header').style.display = 'table-cell';
        displayInvoices('unpaid');
    } else {
        document.getElementById('invoice-action-header').style.display = 'none';
        displayInvoices('paid');
    }
}

function displayInvoices(type) {
    const invoices = (type === 'unpaid') ? state.unpaidInvoices : state.paidInvoices;
    const tableBody = document.getElementById('invoices-table-body');
    tableBody.innerHTML = invoices.length === 0
        ? `<tr><td colspan="5" class="text-center py-4">Không có hóa đơn nào.</td></tr>`
        : invoices.map(inv => `
            <tr class="hover:bg-gray-50">
                <td class="px-6 py-4 font-medium">
                    <a href="#" data-id="${inv.invoiceID}" class="view-invoice-detail text-indigo-600 hover:underline">#${inv.invoiceID}</a>
                </td>
                <td class="px-6 py-4">${inv.patientName}</td>
                <td class="px-6 py-4">${formatCurrency(inv.totalAmount)}</td>
                <td class="px-6 py-4">${new Date(inv.createdAt).toLocaleDateString('vi-VN')}</td>
                <td class="px-6 py-4">
                    ${type === 'unpaid'
                        ? `<button data-id="${inv.invoiceID}" class="pay-invoice-btn bg-green-500 hover:bg-green-600 text-white font-bold py-1 px-3 rounded">Thanh toán</button>`
                        : `<span class="text-green-600 font-semibold">Đã thanh toán</span>`
                    }
                </td>
            </tr>`).join('');
}

function handleInvoicesTableActions(e) {
    const target = e.target.closest('[data-id]');
    if (!target) return;
    const id = parseInt(target.dataset.id);

    if (target.classList.contains('pay-invoice-btn')) {
        if (confirm(`Xác nhận thanh toán cho hóa đơn #${id}?`)) processPayment(id);
    } else if (target.classList.contains('view-invoice-detail')) {
        e.preventDefault();
        openInvoiceDetailModal(id);
    }
}

async function processPayment(id) {
    try {
        await fetchAPI(`/api/Invoices/${id}/payment`, 'PATCH', { paymentMethod: 'Tiền mặt' });
        showMessage("Thanh toán hóa đơn thành công!");
        await Promise.all([fetchUnpaidInvoices(), fetchPaidInvoices()]);
        displayInvoices('unpaid');
    } catch (error) {
        showMessage(error.message || "Lỗi khi xử lý thanh toán.");
    }
}

async function openInvoiceDetailModal(invoiceId) {
    try {
        const data = await fetchAPI(`/api/Invoices/${invoiceId}`);
        const content = document.getElementById('invoice-detail-content');
        document.getElementById('detail-invoice-id').textContent = `#${data.invoiceID}`;

        let itemsHtml = data.prescriptionItems.length > 0 ? data.prescriptionItems.map(item => `
            <tr>
                <td class="py-2">${item.drugName}</td>
                <td class="py-2 text-center">${item.quantity}</td>
                <td class="py-2 text-right">${formatCurrency(item.price)}</td>
                <td class="py-2 text-right">${formatCurrency(item.quantity * item.price)}</td>
            </tr>
        `).join('') : '<tr><td colspan="4" class="py-2 text-center text-gray-500">Không có thuốc được kê.</td></tr>';

        content.innerHTML = `
            <div class="grid grid-cols-2 gap-4 text-sm">
                <p><strong>Bệnh nhân:</strong> ${data.patientName}</p>
                <p><strong>Ngày khám:</strong> ${new Date(data.encounterDate).toLocaleString('vi-VN')}</p>
                <p><strong>Bác sĩ:</strong> ${data.doctorName}</p>
                <p><strong>Trạng thái:</strong> <span class="font-semibold ${data.status === 'Đã thanh toán' ? 'text-green-600' : 'text-red-600'}">${data.status}</span></p>
            </div>
            <div class="border-t pt-4 mt-4">
                <p><strong>Chẩn đoán:</strong> ${data.diagnosisDescription || 'N/A'}</p>
            </div>
            <div class="border-t pt-4 mt-4">
                <h4 class="font-bold mb-2">Chi tiết Chi phí</h4>
                <div class="flex justify-between"><p>Phí khám:</p> <p>${formatCurrency(data.serviceFee)}</p></div>
                <div class="flex justify-between"><p>Tiền thuốc:</p> <p>${formatCurrency(data.drugFee)}</p></div>
                <div class="flex justify-between font-bold text-lg border-t mt-2 pt-2"><p>Tổng cộng:</p> <p>${formatCurrency(data.totalAmount)}</p></div>
            </div>
            <div class="border-t pt-4 mt-4">
                <h4 class="font-bold mb-2">Đơn thuốc</h4>
                <table class="w-full text-sm">
                    <thead><tr class="border-b"><th class="text-left py-1">Tên thuốc</th><th class="text-center py-1">SL</th><th class="text-right py-1">Đơn giá</th><th class="text-right py-1">Thành tiền</th></tr></thead>
                    <tbody>${itemsHtml}</tbody>
                </table>
            </div>
        `;
        document.getElementById('invoice-detail-modal').classList.remove('hidden');
        document.getElementById('invoice-detail-modal').style.display = 'flex';
    } catch (error) {
        showMessage("Không thể tải chi tiết hóa đơn.");
    }
}

// =================================================================
// SHARED FUNCTIONS
// =================================================================
async function fetchAPI(endpoint, method = 'GET', body = null) {
    const token = sessionStorage.getItem('jwt_token');
    const headers = { 'Content-Type': 'application/json' };
    if (token) headers['Authorization'] = `Bearer ${token}`;

    const config = { method, headers };
    if (body) config.body = JSON.stringify(body);

    config.cache = 'no-cache'; 

    const response = await fetch(`${API_BASE_URL}${endpoint}`, config);
    if (!response.ok) await handleResponseError(response);
    const contentType = response.headers.get("content-type");
    if (contentType && contentType.includes("application/json")) return response.json();
    return null;
}

async function handleResponseError(response) {
    let errorMessage = `Lỗi ${response.status}: ${response.statusText || 'Không thể kết nối đến server.'}`;
    try {
        const errorData = await response.json();
        if (errorData && errorData.message) errorMessage = errorData.message;
    } catch (e) { /* Ignore */ }
    throw new Error(errorMessage);
}

async function fetchAppointments() { state.appointments = await fetchAPI('/api/Appointments'); }
async function fetchPatients() { state.patients = await fetchAPI('/api/Patients'); }
async function fetchDoctors() { state.doctors = await fetchAPI('/api/Doctors'); }
async function fetchUnpaidInvoices() { state.unpaidInvoices = await fetchAPI('/api/Invoices?status=Chưa thanh toán'); }
async function fetchPaidInvoices() { state.paidInvoices = await fetchAPI('/api/Invoices?status=Đã thanh toán'); }

async function fetchPatientAccounts() {
    const allAccounts = await fetchAPI('/api/Accounts');
    if(allAccounts) {
        state.patientAccounts = allAccounts.filter(acc => acc.role === 'Patient');
    }
}

function refreshAppointmentsDisplay() {
    const dateFilter = document.getElementById('appointment-date-filter').value;
    const doctorFilter = document.getElementById('doctor-filter').value;
    let filtered = state.appointments;
    if (dateFilter) filtered = filtered.filter(app => app.appointmentDate.startsWith(dateFilter));
    if (doctorFilter) filtered = filtered.filter(app => app.doctorID == doctorFilter);
    displayAppointments(filtered);
}

function populateSelectOptions(selectId, data, valueField, textField, defaultOption) {
    const select = document.getElementById(selectId);
    select.innerHTML = defaultOption ? `<option value="">${defaultOption}</option>` : '';
    data.forEach(item => {
        select.innerHTML += `<option value="${item[valueField]}">${item[textField]}</option>`;
    });
}

function showMessage(message) {
    document.getElementById('message-text').textContent = message;
    document.getElementById('message-modal').classList.remove('hidden');
    document.getElementById('message-modal').style.display = 'flex';
}

function closeMessageModal() { 
    const modal = document.getElementById('message-modal');
    modal.classList.add('hidden');
    modal.style.display = 'none';
}
function closeAppointmentModal() { 
    const modal = document.getElementById('appointment-modal');
    modal.classList.add('hidden');
    modal.style.display = 'none';
}
function closePatientModal() { 
    const modal = document.getElementById('patient-modal');
    modal.classList.add('hidden');
    modal.style.display = 'none';
}
function closePatientDetailModal() { 
    const modal = document.getElementById('patient-detail-modal');
    modal.classList.add('hidden');
    modal.style.display = 'none';
}
function closeInvoiceDetailModal() { 
    const modal = document.getElementById('invoice-detail-modal');
    modal.classList.add('hidden');
    modal.style.display = 'none';
}

function formatCurrency(value) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
}