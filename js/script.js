let registeredUser = null;

// ==== ĐĂNG KÝ ====
function showRegisterForm(){ document.getElementById("registerModal").style.display="block"; }
function closeRegisterForm(){ document.getElementById("registerModal").style.display="none"; }
function submitRegister(){
  const name=document.getElementById("regName").value;
  const email=document.getElementById("regEmail").value;
  const pass=document.getElementById("regPass").value;
  if(!name||!email||!pass){ alert("Vui lòng nhập đủ thông tin!"); return; }
  registeredUser={name,email,pass};
  alert("Đăng ký thành công!"); closeRegisterForm();
}

// ==== ĐĂNG NHẬP ====
function showLoginForm(){ document.getElementById("loginModal").style.display="block"; }
function closeLoginForm(){ document.getElementById("loginModal").style.display="none"; }
function submitLogin(){
  const email=document.getElementById("loginEmail").value;
  const pass=document.getElementById("loginPass").value;
  if(!registeredUser){ alert("Chưa có tài khoản!"); return; }
  if(email===registeredUser.email && pass===registeredUser.pass){
    document.getElementById("authBox").style.display="none";
    document.getElementById("loginModal").style.display="none";
    document.getElementById("userBox").style.display="block";
    document.getElementById("usernameDisplay").textContent="Xin chào, "+registeredUser.name;
  } else alert("Sai email hoặc mật khẩu!");
}

// ==== FORM ĐẶT LỊCH ====
const today=new Date().toISOString().split("T")[0];
document.getElementById("dateInput").setAttribute("min",today);
const timeInput=document.getElementById("timeInput");
for(let h=8;h<=19;h++){
  let opt=document.createElement("option");
  opt.value=`${h.toString().padStart(2,"0")}:00`;
  opt.textContent=opt.value;
  timeInput.appendChild(opt);
}
const nextBtn=document.getElementById("nextBtn");
function checkValid(){
  if(document.getElementById("dateInput").value && timeInput.value){
    nextBtn.disabled=false;
  } else nextBtn.disabled=true;
}
document.getElementById("dateInput").addEventListener("change",checkValid);
timeInput.addEventListener("change",checkValid);
nextBtn.addEventListener("click",()=>{
  document.getElementById("step1").style.display="none";
  document.getElementById("step2").style.display="block";
});

let currentIndex = 0;
const slides = document.querySelectorAll('.news-slider .slide');

function showSlide(index) {
  slides.forEach((slide, i) => {
    slide.classList.remove('active');
    if (i === index) slide.classList.add('active');
  });
}

function nextSlide() {
  currentIndex = (currentIndex + 1) % slides.length;
  showSlide(currentIndex);
}

// tự động chuyển sau 4 giây
setInterval(nextSlide, 4000);


