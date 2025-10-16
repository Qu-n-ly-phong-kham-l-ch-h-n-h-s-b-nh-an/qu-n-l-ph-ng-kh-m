// js/auth.js
const token = localStorage.getItem("token");
if (!token) {
  alert("Bạn cần đăng nhập trước!");
  window.location.href = "index.html";
}
