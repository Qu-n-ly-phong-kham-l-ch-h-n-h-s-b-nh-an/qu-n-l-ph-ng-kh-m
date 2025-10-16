// Lấy role từ query
const params = new URLSearchParams(window.location.search);
const role = params.get("role") || "User";
document.getElementById("role-title").innerText = `Đăng nhập - ${role}`;

async function login() {
  const username = document.getElementById("username").value;
  const password = document.getElementById("password").value;
  const error = document.getElementById("error");
  error.textContent = "";

  if (!username || !password) {
    error.textContent = "Vui lòng nhập đủ thông tin!";
    return;
  }

  const data = {
    username: username,
    passwordHash: password, // ⚠️ nếu backend chưa mã hóa, gửi thẳng
  };

  try {
    const res = await fetch("http://localhost:5236/api/accounts/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });

    if (!res.ok) throw new Error("Sai tài khoản hoặc mật khẩu");
    const result = await res.json();

    // Lưu token và role
    localStorage.setItem("token", result.token);
    localStorage.setItem("role", result.role);

    // Điều hướng đến trang phù hợp
    switch (result.role) {
      case "Admin": window.location.href = "admin.html"; break;
      case "Doctor": window.location.href = "doctor.html"; break;
      case "Receptionist": window.location.href = "receptionist.html"; break;
      case "Pharmacist": window.location.href = "pharmacist.html"; break;
      case "Patient": window.location.href = "patient.html"; break;
      default: alert("Không xác định vai trò người dùng!");
    }

  } catch (err) {
    error.textContent = err.message;
  }
}
