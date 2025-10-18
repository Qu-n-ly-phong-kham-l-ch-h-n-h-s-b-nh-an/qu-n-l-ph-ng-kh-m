// =================================================================
// SCRIPT CHO VAI TRÒ BỆNH NHÂN (HOÀN CHỈNH - CÓ NHẮC LỊCH FRONTEND)
// =================================================================

// --- LƯỢC BỎ: const API_BASE_URL đã được khai báo trong app.js ---

// --- GLOBAL STATE ---
const patientState = {
    patientProfile: null,
    appointments: [], // Bao gồm cả lịch sắp tới và đã qua
    remindedAppointmentIds: [] // Lưu ID các lịch đã nhắc trong session này
};

// --- INITIALIZATION ---
document.addEventListener('DOMContentLoaded', () => {
    // 1. Kiểm tra xác thực và vai trò
    const user = JSON.parse(sessionStorage.getItem('user'));
    if (!user || user.role.toLowerCase() !== 'patient') {
        alert('Bạn không có quyền truy cập trang này.');
        window.location.href = '../Login/index.html';
        return;
    }
    // QUAN TRỌNG: Kiểm tra xem user có patientId không
    if (!user.patientId) {
         console.error("Login response did not include patientId for Patient user.");
         showPatientMessage("Không thể tải thông tin bệnh nhân. Tài khoản này có thể chưa được liên kết với hồ sơ. Vui lòng liên hệ phòng khám.", true);
         // Vẫn thiết lập header để người dùng có thể đăng xuất
         setupHeader(user);
         // Làm rỗng khu vực profile và appointment
         const profileDiv = document.getElementById('patient-profile');
         const upcomingDiv = document.getElementById('upcoming-appointments');
         const pastDiv = document.getElementById('past-appointments');
         if(profileDiv) profileDiv.innerHTML = '<p class="text-red-600">Lỗi tải thông tin.</p>';
         if(upcomingDiv) upcomingDiv.innerHTML = '<p class="text-red-600">Lỗi tải lịch hẹn.</p>';
         if(pastDiv) pastDiv.innerHTML = '<p class="text-red-600">Lỗi tải lịch hẹn.</p>';
         return; // Dừng khởi tạo nếu không có patientId
    }

    // 2. Thiết lập giao diện chung (Header) và nút đóng modal
    setupHeader(user);

    // 3. Khởi chạy logic của trang
    initPatientDashboard(user.patientId);
});

// Hàm thiết lập Header và nút đóng modal
function setupHeader(user) {
    const usernameDisplay = document.getElementById('username-display');
    const logoutButton = document.getElementById('logout-button');
    if (usernameDisplay) usernameDisplay.textContent = `Chào, ${user.username}!`;
    if (logoutButton) logoutButton.addEventListener('click', () => {
        sessionStorage.clear();
        window.location.href = '../Login/index.html';
    });
    // Gán sự kiện đóng modal chung
    const closeMsgBtn = document.getElementById('close-message-modal-btn');
    if(closeMsgBtn) closeMsgBtn.addEventListener('click', closePatientMessageModal);
}

// Khởi tạo dashboard
async function initPatientDashboard(patientId) {
    setupPatientEventListeners(); // Gán sự kiện chuyển tab
    try {
        // Tải song song thông tin cá nhân và lịch hẹn
        await Promise.all([
            fetchPatientProfile(patientId),
            fetchPatientAppointments()
        ]);

        // Hiển thị dữ liệu
        displayPatientProfile();
        displayAppointments('upcoming'); // Hiển thị tab "Sắp tới" mặc định

        // Bật chức năng nhắc lịch trên frontend
        startAppointmentReminderCheck();

    } catch (error) {
        // Hiển thị lỗi tổng quát nếu có lỗi xảy ra trong quá trình tải dữ liệu
        showPatientMessage(`Lỗi tải dữ liệu trang: ${error.message}`, true);
        // Làm rỗng các khu vực dữ liệu
         const profileDiv = document.getElementById('patient-profile');
         const upcomingDiv = document.getElementById('upcoming-appointments');
         const pastDiv = document.getElementById('past-appointments');
         if(profileDiv) profileDiv.innerHTML = '<p class="text-red-600">Lỗi tải thông tin.</p>';
         if(upcomingDiv) upcomingDiv.innerHTML = '<p class="text-red-600">Lỗi tải lịch hẹn.</p>';
         if(pastDiv) pastDiv.innerHTML = '<p class="text-red-600">Lỗi tải lịch hẹn.</p>';
    }
}

// Gán sự kiện chuyển tab
function setupPatientEventListeners() {
    const upcomingTab = document.getElementById('tab-upcoming');
    const pastTab = document.getElementById('tab-past');
    if(upcomingTab) upcomingTab.addEventListener('click', () => switchTab('upcoming'));
    if(pastTab) pastTab.addEventListener('click', () => switchTab('past'));
}

// --- DATA FETCHING ---

// Lấy thông tin hồ sơ bệnh nhân
async function fetchPatientProfile(patientId) {
    try {
        patientState.patientProfile = await fetchAPI(`/api/Patients/${patientId}`, 'GET');
        if (!patientState.patientProfile) {
             throw new Error("Không tìm thấy hồ sơ bệnh nhân.");
        }
    } catch (error) {
        console.error("Lỗi fetchPatientProfile:", error);
        throw new Error(`Không thể tải thông tin cá nhân: ${error.message}`);
    }
}

// Lấy danh sách lịch hẹn (backend tự lọc)
async function fetchPatientAppointments() {
    try {
        const appointments = await fetchAPI('/api/Appointments', 'GET');
        patientState.appointments = appointments || [];
        // Gọi kiểm tra nhắc lịch ngay sau khi tải xong lịch hẹn
        checkAndShowReminders();
    } catch (error) {
        console.error("Lỗi fetchPatientAppointments:", error);
        throw new Error(`Không thể tải lịch hẹn: ${error.message}`);
    }
}

// --- UI RENDERING ---

// Hiển thị thông tin hồ sơ bệnh nhân
function displayPatientProfile() {
    const profile = patientState.patientProfile;
    if (!profile) {
        console.warn("Không có dữ liệu profile để hiển thị.");
         const profileDiv = document.getElementById('patient-profile');
         if(profileDiv) profileDiv.innerHTML = '<p class="text-red-600">Không tải được thông tin.</p>';
        return;
    };

    const setText = (id, value) => {
        const element = document.getElementById(id);
        if (element) element.textContent = value || '-';
    };
    const formatDateForDisplay = (dateString) => {
        if (!dateString) return '-';
        try { return new Date(dateString).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' }); }
        catch { return '-'; }
    };

    setText('profile-name', profile.fullName);
    setText('profile-dob', formatDateForDisplay(profile.dob));
    setText('profile-gender', profile.gender);
    setText('profile-phone', profile.phone);
    setText('profile-email', profile.email);
    setText('profile-address', profile.address);
    setText('profile-history', profile.medicalHistory);
}

// Hiển thị danh sách lịch hẹn (sắp tới hoặc đã qua)
function displayAppointments(type = 'upcoming') {
    const upcomingContainer = document.getElementById('upcoming-appointments');
    const pastContainer = document.getElementById('past-appointments');
    if (!upcomingContainer || !pastContainer) return;

    const now = new Date();
    // Lọc danh sách lịch hẹn
    const filteredAppointments = patientState.appointments.filter(app => {
        if (!app || !app.appointmentDate) return false;
        try {
            const appDate = new Date(app.appointmentDate);
            const statusLower = app.status ? app.status.toLowerCase() : '';
            const isPastOrCompleted = appDate < now || ['cancelled', 'completed', 'đã hủy', 'đã khám'].includes(statusLower);
            return type === 'upcoming' ? !isPastOrCompleted : isPastOrCompleted;
        } catch(e) { console.error("Lỗi xử lý ngày tháng:", app.appointmentDate, e); return false; }
    });

    let targetContainer;
    if (type === 'upcoming') {
        targetContainer = upcomingContainer;
        filteredAppointments.sort((a, b) => new Date(a.appointmentDate) - new Date(b.appointmentDate)); // Gần nhất lên đầu
    } else {
        targetContainer = pastContainer;
        filteredAppointments.sort((a, b) => new Date(b.appointmentDate) - new Date(a.appointmentDate)); // Mới nhất lên đầu
    }

    targetContainer.innerHTML = ''; // Xóa nội dung cũ

    if (filteredAppointments.length === 0) {
        targetContainer.innerHTML = `<p class="text-gray-500 italic">Không có lịch hẹn nào.</p>`;
        return;
    }

    // Tạo HTML cho từng lịch hẹn
    filteredAppointments.forEach(app => {
        const div = document.createElement('div');
        div.className = 'p-4 border rounded-lg bg-gray-50 shadow-sm';
        let dateStr = 'N/A', timeStr = 'N/A';
        try {
             const appDate = new Date(app.appointmentDate);
             dateStr = appDate.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
             timeStr = appDate.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
        } catch(e) { console.error("Lỗi định dạng ngày giờ:", app.appointmentDate, e); }

        let statusClass = 'text-gray-600 bg-gray-100';
        let statusText = app.status || 'Không rõ';
        const statusLower = statusText.toLowerCase();
         switch (statusLower) {
            case 'scheduled': case 'đã đặt': statusClass = 'text-blue-600 bg-blue-100'; statusText = 'Đã đặt lịch'; break;
            case 'completed': case 'đã khám': statusClass = 'text-green-600 bg-green-100'; statusText = 'Đã khám'; break;
            case 'cancelled': case 'đã hủy': statusClass = 'text-red-600 bg-red-100'; statusText = 'Đã hủy'; break;
        }

        div.innerHTML = `
            <div class="flex justify-between items-start mb-1">
                <p class="font-semibold text-indigo-700">Bác sĩ: ${app.doctorName || 'N/A'}</p>
                <span class="text-xs font-medium ${statusClass} px-2 py-0.5 rounded-full">${statusText}</span>
            </div>
             <p class="text-sm text-gray-700 mb-2">Thời gian: <span class="font-medium">${timeStr} - ${dateStr}</span></p>
            ${app.notes ? `<p class="text-sm text-gray-500 mt-1 p-2 bg-white rounded border border-gray-200">Ghi chú: ${app.notes}</p>` : ''}
        `;
        targetContainer.appendChild(div);
    });
}

// Chuyển đổi giữa các tab lịch hẹn
function switchTab(tabId) {
    const upcomingTab = document.getElementById('tab-upcoming');
    const pastTab = document.getElementById('tab-past');
    const upcomingContent = document.getElementById('upcoming-appointments');
    const pastContent = document.getElementById('past-appointments');

    if (!upcomingTab || !pastTab || !upcomingContent || !pastContent) {
         console.error("Không tìm thấy các thành phần tab.");
         return;
    }

    const isActive = tabId === 'upcoming';

    // Cập nhật giao diện nút tab
    upcomingTab.classList.toggle('tab-active', isActive);
    upcomingTab.classList.toggle('text-gray-500', !isActive);
    // upcomingTab.classList.toggle('hover:text-gray-700', !isActive); // Bỏ hover khi active
    // upcomingTab.classList.toggle('hover:border-gray-300', !isActive);

    pastTab.classList.toggle('tab-active', !isActive);
    pastTab.classList.toggle('text-gray-500', isActive);
    // pastTab.classList.toggle('hover:text-gray-700', isActive); // Bỏ hover khi active
    // pastTab.classList.toggle('hover:border-gray-300', isActive);


    // Hiển thị/ẩn nội dung
    upcomingContent.classList.toggle('hidden', !isActive);
    pastContent.classList.toggle('hidden', isActive);

    // Tải lại dữ liệu cho tab được chọn
    displayAppointments(tabId);
}

// --- HELPER FUNCTIONS ---
function showPatientMessage(message, isError = false) {
    const modal = document.getElementById('message-modal');
    const text = document.getElementById('message-text');
    if (modal && text) {
        text.textContent = message;
        text.style.color = isError ? 'red' : 'black';
        modal.classList.add('active');
    } else {
        alert(message); // Fallback
    }
}

function closePatientMessageModal() {
    const modal = document.getElementById('message-modal');
    if(modal) {
        modal.classList.remove('active');
    }
}

// =================================================================
// CHỨC NĂNG NHẮC LỊCH HẸN (FRONTEND)
// =================================================================

// =================================================================
// CHỨC NĂNG NHẮC LỊCH HẸN (FRONTEND)
// =================================================================

let reminderIntervalId = null;

function startAppointmentReminderCheck() {
    console.log("Bắt đầu kiểm tra nhắc lịch hẹn (mỗi 1 phút)...");
    if (reminderIntervalId) {
        clearInterval(reminderIntervalId);
    }
    setTimeout(checkAndShowReminders, 5000); // Chạy lần đầu sau 5 giây
    // Chạy định kỳ mỗi 1 phút (Bạn có thể tăng lên, ví dụ: 15 * 60000 cho 15 phút)
    reminderIntervalId = setInterval(checkAndShowReminders, 60000);
}

function checkAndShowReminders() {
    if (!patientState.appointments || patientState.appointments.length === 0) {
        // console.log("Không có lịch hẹn để kiểm tra nhắc nhở."); // Có thể bỏ log này
        return;
    }

    console.log("Đang kiểm tra nhắc lịch hẹn (24h tới)..."); // Sửa log
    const now = new Date();
    // === SỬA ĐỔI DÒNG NÀY: Thay đổi khoảng thời gian ===
    const reminderWindowMillis = 24 * 60 * 60 * 1000; // 24 giờ (tính bằng mili giây)

    const upcomingReminders = patientState.appointments.filter(appt => {
        if (!appt || !appt.appointmentDate || !appt.AppointmentID) return false;
        try {
            const apptTime = new Date(appt.appointmentDate);
            const timeDiff = apptTime.getTime() - now.getTime();
            const statusLower = appt.status ? appt.status.toLowerCase() : '';

            // Điều kiện nhắc: Trong tương lai, trong vòng 24h tới, chưa hủy/xong, chưa nhắc
            return timeDiff > 0 &&
                   timeDiff <= reminderWindowMillis && // Kiểm tra trong vòng 24h
                   !['cancelled', 'completed', 'đã hủy', 'đã khám'].includes(statusLower) &&
                   !patientState.remindedAppointmentIds.includes(appt.AppointmentID);
        } catch(e) {
            console.error("Lỗi khi kiểm tra lịch hẹn:", appt, e);
            return false;
        }
    });

    if (upcomingReminders.length > 0) {
        console.log(`Tìm thấy ${upcomingReminders.length} lịch hẹn cần nhắc trong 24h tới.`); // Sửa log
        upcomingReminders.forEach(appt => {
            showReminderToast(appt);
            patientState.remindedAppointmentIds.push(appt.AppointmentID);
        });
    } else {
        console.log("Không có lịch hẹn mới nào cần nhắc trong 24h tới."); // Sửa log
    }
}

// Hiển thị thông báo (toast) trên giao diện
function showReminderToast(appointment) {
    const reminderArea = document.getElementById('reminder-area');
    if (!reminderArea) {
        console.error("Không tìm thấy #reminder-area để hiển thị toast.");
        return;
    }

    const toast = document.createElement('div');
    // Sử dụng class đã định nghĩa trong <style> của HTML
    toast.className = 'reminder-toast';

    let timeStr = 'N/A', dateStr = 'N/A';
     try {
         const apptTime = new Date(appointment.appointmentDate);
         timeStr = apptTime.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
         dateStr = apptTime.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
     } catch(e) {
          console.error("Lỗi định dạng ngày giờ cho toast:", appointment.appointmentDate, e);
     }

    toast.innerHTML = `
        <div>
            <p class="font-bold">Nhắc lịch hẹn sắp tới!</p>
            <p class="text-sm">Bạn có lịch hẹn với BS ${appointment.doctorName || 'N/A'} lúc ${timeStr} ngày ${dateStr}.</p>
        </div>
        <button type="button">&times;</button> `;

    // Thêm sự kiện để đóng toast khi nhấn nút X
    const closeButton = toast.querySelector('button');
     if (closeButton) {
         closeButton.addEventListener('click', (e) => {
              e.stopPropagation(); // Ngăn sự kiện click khác nếu có
              toast.remove();
         });
     }


    // Tự động ẩn toast sau một khoảng thời gian (ví dụ: 15 giây)
    const autoCloseTimeout = setTimeout(() => {
        // Kiểm tra xem toast còn trong DOM không trước khi xóa
        if (toast && toast.parentNode === reminderArea) {
             toast.remove();
        }
    }, 15000); // 15 giây

     // Xóa timeout nếu người dùng tự đóng toast
     if (closeButton) {
         closeButton.addEventListener('click', () => {
              clearTimeout(autoCloseTimeout);
         });
     }

    // Thêm toast vào đầu danh sách để cái mới nhất hiện trên cùng
    reminderArea.prepend(toast);
}


// --- LƯỢC BỎ: fetchAPI và handleResponseError đã có trong app.js ---