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

    const response = await fetch(`${API_BASE_URL}${endpoint}`, config);
    if (!response.ok) {
        await handleResponseError(response);
    }
    // Trả về JSON nếu có nội dung, nếu không trả về null
    const contentType = response.headers.get("content-type");
    if (contentType && contentType.indexOf("application/json") !== -1) {
        return response.json();
    } 
    return null;
}

// --- HÀM HELPER ĐỂ XỬ LÝ LỖI RESPONSE ---
async function handleResponseError(response) {
    let errorMessage = `Lỗi ${response.status}: ${response.statusText || 'Không thể kết nối đến server.'}`;
    try {
        const errorData = await response.json();
        if (errorData && errorData.message) {
            errorMessage = errorData.message;
        }
    } catch (jsonError) {
        console.error("Không thể phân tích phản hồi lỗi dưới dạng JSON.", jsonError);
    }
    throw new Error(errorMessage);
}


// --- LOGIC CHO TRANG ĐĂNG NHẬP (login.html) ---
const loginForm = document.getElementById('login-form');
if (loginForm) {
    const urlParams = new URLSearchParams(window.location.search);
    const role = urlParams.get('role');
    const roleName = urlParams.get('roleName');

    if (role && roleName) {
        document.getElementById('login-role-title').textContent = roleName;
        if (role === 'Patient') {
            document.getElementById('register-link-container').classList.remove('hidden');
        }
    } else {
        window.location.href = 'index.html';
    }

    loginForm.addEventListener('submit', async (event) => {
        event.preventDefault();
        const loginError = document.getElementById('login-error');
        const submitButton = loginForm.querySelector('button[type="submit"]');
        loginError.textContent = '';
        submitButton.disabled = true;
        submitButton.textContent = 'Đang xử lý...';

        const username = document.getElementById('login-username').value;
        const password = document.getElementById('login-password').value;

        try {
            const data = await fetchAPI('/api/accounts/role-login', 'POST', { username, password, role });
            
            sessionStorage.setItem('jwt_token', data.token);
            sessionStorage.setItem('user', JSON.stringify(data.user));

            // =============================================================
            // ✅ SỬA LỖI: LOGIC ĐIỀU HƯỚNG THEO VAI TRÒ
            // =============================================================
            switch (data.user.role) {
                case 'Admin':
                    window.location.href = '../Admin/dashboard.html'; // Sửa thành trang của Admin
                    break;
                case 'Receptionist':
                    window.location.href = '../LeTan/index.html'; // Sửa thành trang của Lễ tân
                    break;
                case 'Doctor':
                    window.location.href = '../BacSi/dashboard.html'; // Sửa thành trang của Bác sĩ
                    break;
                case 'Patient':
                    window.location.href = '../BenhNhan/dashboard.html'; // Sửa thành trang của Bệnh nhân
                    break;
                default:
                    // Trang mặc định nếu vai trò không xác định
                    window.location.href = 'dashboard.html';
                    break;
            }
            // =============================================================

        } catch (error) {
            console.error('Lỗi đăng nhập:', error);
            if (error.message.includes("Failed to fetch")) {
                loginError.textContent = 'Lỗi kết nối: Không thể kết nối đến máy chủ.';
            } else {
                loginError.textContent = error.message;
            }
        } finally {
            submitButton.disabled = false;
            submitButton.textContent = 'Đăng nhập';
        }
    });
}


// --- LOGIC CHO TRANG ĐĂNG KÝ (register.html) ---
const registerForm = document.getElementById('register-form');
if (registerForm) {
    registerForm.addEventListener('submit', async (event) => {
        event.preventDefault();
        const registerMessage = document.getElementById('register-message');
        const submitButton = registerForm.querySelector('button[type="submit"]');
        registerMessage.textContent = '';
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

        try {
            await fetchAPI('/api/accounts/register', 'POST', { username, password, role: 'Patient' });
            
            registerMessage.style.color = 'green';
            registerMessage.textContent = 'Đăng ký thành công! Đang chuyển đến trang đăng nhập...';
            registerForm.reset();
            setTimeout(() => {
                window.location.href = 'login.html?role=Patient&roleName=Bệnh nhân';
            }, 2000);

        } catch (error) {
            console.error('Lỗi đăng ký:', error);
            registerMessage.style.color = 'red';
            if (error.message.includes("Failed to fetch")) {
                registerMessage.textContent = 'Lỗi kết nối: Không thể kết nối đến máy chủ.';
            } else {
                registerMessage.textContent = error.message;
            }
        } finally {
            if (registerMessage.style.color !== 'green') {
                submitButton.disabled = false;
                submitButton.textContent = 'Đăng ký';
            }
        }
    });
}

// --- LOGIC CHO TRANG DASHBOARD CHUNG (dashboard.html) ---
const welcomeMessage = document.getElementById('welcome-message');
if (welcomeMessage) {
    const user = JSON.parse(sessionStorage.getItem('user'));
    if (!user) {
        window.location.href = 'index.html';
    } else {
        welcomeMessage.textContent = `Chào mừng, ${user.username}!`;
    }

    document.getElementById('logout-button').addEventListener('click', () => {
        sessionStorage.removeItem('jwt_token');
        sessionStorage.removeItem('user');
        window.location.href = 'index.html';
    });
}