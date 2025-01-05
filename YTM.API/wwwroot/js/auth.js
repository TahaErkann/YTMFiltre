document.addEventListener('DOMContentLoaded', function() {
    const loginForm = document.getElementById('loginForm');
    const registerForm = document.getElementById('registerForm');

    if (loginForm) {
        loginForm.addEventListener('submit', handleLogin);
    }

    if (registerForm) {
        registerForm.addEventListener('submit', handleRegister);
    }

    const token = localStorage.getItem('token');
    const userRole = localStorage.getItem('userRole');

    if (token && userRole) {
        console.log('User already logged in, redirecting...');
        if (userRole === 'Admin') {
            window.location.replace('/admin/index.html');
        } else {
            window.location.replace('/customer/index.html');
        }
    }
});

async function handleLogin(event) {
    event.preventDefault();
    
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;

    try {
        console.log('Login attempt for:', email);

        const response = await fetch('/api/auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ email, password })
        });

        console.log('Response status:', response.status);

        const data = await response.json();
        console.log('Response data:', data);

        if (!response.ok) {
            throw new Error(data.message || 'Giriş başarısız');
        }

        console.log('Login successful, role:', data.role);

        localStorage.setItem('token', data.token);
        localStorage.setItem('userRole', data.role);

        if (data.role === 'Admin') {
            console.log('Redirecting to admin panel...');
            window.location.replace('/admin/index.html');
        } else {
            console.log('Redirecting to customer dashboard...');
            window.location.replace('/customer/index.html');
        }
    } catch (error) {
        console.error('Login error:', error);
        alert('Giriş başarısız: ' + error.message);
    }
}

async function handleRegister(event) {
    event.preventDefault();
    
    const name = document.getElementById('name').value;
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;
    const confirmPassword = document.getElementById('confirmPassword').value;

    if (password !== confirmPassword) {
        alert('Şifreler eşleşmiyor!');
        return;
    }

    try {
        const response = await fetch('/api/auth/register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                name,
                email,
                password,
                role: 'Customer' // Varsayılan rol
            })
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Kayıt başarısız');
        }

        alert('Kayıt başarılı! Giriş yapabilirsiniz.');
        window.location.href = '/login.html';
    } catch (error) {
        alert('Kayıt başarısız: ' + error.message);
    }
} 