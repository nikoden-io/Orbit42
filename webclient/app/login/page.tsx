'use client';

import { useState } from 'react';

export default function Login() {
    const [email, setEmail] = useState('test@user.be');
    const [password, setPassword] = useState('password');
    const [message, setMessage] = useState('');

    const handleLogin = async () => {
        try {
            const response = await fetch('http://localhost:5000/api/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                credentials: 'include', // Important to allow cookies to be set
                body: JSON.stringify({ email, password }),
            });

            if (response.ok) {
                setMessage('Login successful! Check your cookies for "refresh_token".');
            } else {
                const errorData = await response.json();
                setMessage(`Login failed: ${errorData.message || 'Unknown error'}`);
            }
        } catch (error) {
            if (error instanceof Error) {
                setMessage(`Error: ${error.message}`);
            } else {
                setMessage('An unknown error occurred.');
            }
        }
    };

    return (
        <div className="flex flex-col items-center justify-center min-h-screen p-4">
            <h1 className="text-2xl font-bold mb-4">Login</h1>
            <div className="flex flex-col gap-4">
                <input
                    type="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder="Email"
                    className="p-2 border rounded"
                />
                <input
                    type="password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    placeholder="Password"
                    className="p-2 border rounded"
                />
                <button
                    onClick={handleLogin}
                    className="bg-blue-500 text-white px-4 py-2 rounded"
                >
                    Login
                </button>
                {message && <p className="mt-4 text-center">{message}</p>}
            </div>
        </div>
    );
}
