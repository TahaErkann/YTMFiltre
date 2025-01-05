async function login(email, password) {
    try {
        const response = await fetch('/api/auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ email, password })
        });

        if (!response.ok) {
            throw new Error('Giriş başarısız');
        }

        const data = await response.json();
        localStorage.setItem('token', data.token);
        localStorage.setItem('userRole', data.role);
        localStorage.setItem('userId', data.userId);

        if (data.role === 'Customer') {
            window.location.href = '/customer/index.html';
        } else if (data.role === 'Admin') {
            window.location.href = '/admin/index.html';
        }
    } catch (error) {
        console.error('Login error:', error);
        showToast('Giriş başarısız: ' + error.message, 'error');
    }
} 