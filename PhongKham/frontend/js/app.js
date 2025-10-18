const API_BASE_URL = 'https://localhost:7220'; // <-- Đảm bảo URL này chính xác

// --- HÀM HELPER ĐỂ GỌI API AN TOÀN ---
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

    // Thêm cache: 'no-cache' để luôn lấy dữ liệu mới nhất
    config.cache = 'no-cache';

    try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, config);
        if (!response.ok) {
            // Ném lỗi đã được xử lý từ handleResponseError
            await handleResponseError(response);
        }
        // Trả về JSON nếu có nội dung, nếu không trả về đối tượng thông báo thành công mặc định
        const contentType = response.headers.get("content-type");
        if (contentType && contentType.indexOf("application/json") !== -1) {
            return response.json();
        }
        // Nếu không có JSON (ví dụ: 204 No Content), trả về thông báo thành công
        return { message: "Thao tác thành công." };
    } catch (error) {
        console.error(`API Fetch Error (${method} ${endpoint}):`, error);
        // Xử lý lỗi mạng riêng biệt
        if (error.message.includes("Failed to fetch")) {
            throw new Error("Lỗi kết nối: Không thể kết nối đến máy chủ.");
        }
        // Ném lại lỗi đã được chuẩn hóa hoặc lỗi từ handleResponseError
        throw error;
    }
}

// --- HÀM HELPER ĐỂ XỬ LÝ LỖI RESPONSE ---
async function handleResponseError(response) {
    let errorMessage = `Lỗi ${response.status}: ${response.statusText || 'Lỗi không xác định.'}`;
    try {
        // Cố gắng đọc lỗi dạng JSON từ backend
        const errorData = await response.json();
        // Ưu tiên message lỗi từ backend nếu có
        if (errorData && errorData.message) {
            errorMessage = errorData.message;
        }
    } catch (jsonError) {
        // Nếu backend trả về lỗi không phải JSON (vd: lỗi 500 HTML), giữ nguyên errorMessage ban đầu
        console.warn("Phản hồi lỗi không phải JSON hoặc không thể phân tích.", jsonError);
    }
    // Ném lỗi để hàm gọi fetchAPI có thể bắt được
    throw new Error(errorMessage);
}


// --- LOGIC CHO TRANG ĐĂNG NHẬP (login.html) ---
const loginForm = document.getElementById('login-form');
if (loginForm) {
    const urlParams = new URLSearchParams(window.location.search);
    const role = urlParams.get('role');
    const roleName = urlParams.get('roleName');
    const loginRoleTitle = document.getElementById('login-role-title');
    const registerLinkContainer = document.getElementById('register-link-container');


    if (role && roleName && loginRoleTitle) {
        loginRoleTitle.textContent = roleName;
        // Chỉ hiển thị link đăng ký khi đăng nhập với vai trò Bệnh nhân
        if (role === 'Patient' && registerLinkContainer) {
            registerLinkContainer.classList.remove('hidden');
        }
    } else {
        // Nếu không có role hoặc roleName, quay về trang chọn vai trò
        console.warn("Missing role or roleName in URL parameters for login page.");
        window.location.href = 'index.html'; // Hoặc trang chọn vai trò của bạn
    }

    loginForm.addEventListener('submit', async (event) => {
        event.preventDefault();
        const loginError = document.getElementById('login-error');
        const submitButton = loginForm.querySelector('button[type="submit"]');

        if (!loginError || !submitButton) {
             console.error("Login form elements not found!");
             return;
        }

        loginError.textContent = '';
        submitButton.disabled = true;
        submitButton.textContent = 'Đang xử lý...';

        const username = document.getElementById('login-username').value;
        const password = document.getElementById('login-password').value;

        try {
            // Gọi API đăng nhập theo vai trò
            const data = await fetchAPI('/api/accounts/role-login', 'POST', { username, password, role });

            // Kiểm tra xem API có trả về token và user không
             if (!data || !data.token || !data.user) {
                 throw new Error("Phản hồi đăng nhập không hợp lệ từ máy chủ.");
             }

             // *** QUAN TRỌNG: LƯU THÔNG TIN USER VÀO SESSION STORAGE ***
            sessionStorage.setItem('jwt_token', data.token);
            // Lưu toàn bộ đối tượng user (bao gồm cả patientId nếu có)
            sessionStorage.setItem('user', JSON.stringify(data.user));

            // Điều hướng dựa trên vai trò trả về từ API
            switch (data.user.role) {
                case 'Admin':
                    window.location.href = '../Admin/dashboard.html';
                    break;
                case 'Receptionist':
                    window.location.href = '../LeTan/index.html'; // Trang chính của Lễ tân
                    break;
                case 'Doctor':
                    window.location.href = '../BacSi/dashboard.html';
                    break;
                case 'Patient':
                     // Kiểm tra lại patientId trước khi chuyển trang (phòng trường hợp backend không trả về)
                     if (!data.user.patientId) {
                          console.error("Patient login successful, but patientId is missing in the response.");
                          // Hiển thị lỗi thân thiện hơn cho bệnh nhân
                          loginError.textContent = "Tài khoản chưa liên kết hồ sơ. Vui lòng liên hệ lễ tân.";
                          // Xóa thông tin đăng nhập để tránh lỗi sau này
                          sessionStorage.removeItem('jwt_token');
                          sessionStorage.removeItem('user');
                          // Không chuyển trang, giữ lại ở trang login
                          submitButton.disabled = false; // Bật lại nút
                          submitButton.textContent = 'Đăng nhập';
                          return; // Dừng thực thi
                     }
                    window.location.href = '../BenhNhan/dashboard.html';
                    break;
                 case 'Pharmacist': // Thêm vai trò Dược sĩ nếu có
                     // window.location.href = '../DuocSi/dashboard.html'; // Ví dụ
                     console.warn("Chưa cấu hình trang cho Dược sĩ.");
                     loginError.textContent = "Vai trò Dược sĩ chưa được hỗ trợ giao diện.";
                     sessionStorage.removeItem('jwt_token'); // Xóa token vì không có trang để vào
                     sessionStorage.removeItem('user');
                     break;
                default:
                    console.warn(`Vai trò không xác định: ${data.user.role}. Chuyển về trang chọn vai trò.`);
                    // Có thể xóa token và user ở đây nếu không muốn họ vào trang dashboard chung
                    // sessionStorage.removeItem('jwt_token');
                    // sessionStorage.removeItem('user');
                    window.location.href = 'index.html'; // Trang chọn vai trò
                    break;
            }
            // Không cần bật lại nút nếu chuyển trang thành công

        } catch (error) {
            console.error('Lỗi đăng nhập:', error);
            // Hiển thị lỗi đã được chuẩn hóa từ fetchAPI/handleResponseError
            loginError.textContent = error.message;
            // Luôn bật lại nút nếu có lỗi
            submitButton.disabled = false;
            submitButton.textContent = 'Đăng nhập';
        }
        // Bỏ finally vì nút chỉ bật lại khi có lỗi
    });
}


// --- LOGIC CHO TRANG ĐĂNG KÝ (register.html) ---
const registerForm = document.getElementById('register-form');
if (registerForm) {
    registerForm.addEventListener('submit', async (event) => {
        event.preventDefault();
        const registerMessage = document.getElementById('register-message');
        const submitButton = registerForm.querySelector('button[type="submit"]');

         if (!registerMessage || !submitButton) {
             console.error("Register form elements not found!");
             return;
        }


        registerMessage.textContent = ''; // Xóa thông báo cũ
        submitButton.disabled = true;
        submitButton.textContent = 'Đang đăng ký...';

        const username = document.getElementById('register-username').value;
        const password = document.getElementById('register-password').value;
        const confirmPassword = document.getElementById('register-confirm-password').value;

        if (password !== confirmPassword) {
            registerMessage.style.color = 'red';
            registerMessage.textContent = 'Mật khẩu nhập lại không khớp.';
            submitButton.disabled = false;
            submitButton.textContent = 'Đăng ký';
            return;
        }
         // Kiểm tra độ dài mật khẩu trước khi gửi API
         if (password.length < 6) {
             registerMessage.style.color = 'red';
             registerMessage.textContent = 'Mật khẩu phải có ít nhất 6 ký tự.';
             submitButton.disabled = false;
             submitButton.textContent = 'Đăng ký';
             return;
         }


        try {
            // Gọi API đăng ký, vai trò mặc định là 'Patient' ở backend
            const result = await fetchAPI('/api/accounts/register', 'POST', { username, password /* Bỏ role: 'Patient' vì backend tự xử lý */ });

            registerMessage.style.color = 'green';
            registerMessage.textContent = result?.message || 'Đăng ký thành công! Đang chuyển đến trang đăng nhập...'; // Hiển thị thông báo từ API nếu có
            registerForm.reset();
            // Đợi 2 giây rồi chuyển trang
            setTimeout(() => {
                // Chuyển hướng đến trang login với tham số của Patient
                window.location.href = 'login.html?role=Patient&roleName=Bệnh nhân';
            }, 2000);
            // Không cần bật lại nút vì đã thành công và chuyển trang

        } catch (error) {
            console.error('Lỗi đăng ký:', error);
            registerMessage.style.color = 'red';
            // Hiển thị lỗi đã được chuẩn hóa từ fetchAPI/handleResponseError
            registerMessage.textContent = error.message;
            // Bật lại nút nếu đăng ký thất bại
             submitButton.disabled = false;
             submitButton.textContent = 'Đăng ký';
        }
        // Bỏ finally vì nút chỉ bật lại khi có lỗi
    });
}

// --- LOGIC CHO TRANG DASHBOARD CHUNG (dashboard.html - Trang chào mừng đơn giản) ---
// Trang này có thể không cần thiết nữa nếu các vai trò đều có trang riêng
const welcomeMessage = document.getElementById('welcome-message');
if (welcomeMessage) {
    const user = JSON.parse(sessionStorage.getItem('user'));
    if (!user) {
         // Nếu chưa đăng nhập, quay về trang chọn vai trò
        window.location.href = 'index.html';
    } else {
        welcomeMessage.textContent = `Chào mừng, ${user.username}!`;
         // Thêm nút đăng xuất nếu cần
         const logoutBtn = document.getElementById('logout-button');
         if(logoutBtn) {
              logoutBtn.addEventListener('click', () => {
                   sessionStorage.removeItem('jwt_token');
                   sessionStorage.removeItem('user');
                   window.location.href = 'index.html'; // Quay về trang chọn vai trò
              });
         }
    }
}