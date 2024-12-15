'use client';

import { useState } from 'react';

export default function TestAuth() {
    const [response, setResponse] = useState<string | null>(null);

    const testAuth = async () => {
        try {
            const res = await fetch('http://localhost:5000/api/users/me', {
                method: 'GET',
                credentials: 'include', // Important to include cookies for authenticated requests
                headers: {
                    'Content-Type': 'application/json',
                },
            });

            if (res.ok) {
                const data = await res.json();
                setResponse(`User ID: ${data.userId}, Email: ${data.email}`);
            } else if (res.status === 401) {
                setResponse('Unauthorized: Please log in first.');
            } else {
                setResponse(`Error: ${res.status} ${res.statusText}`);
            }
        } catch (error) {
            if (error instanceof Error) {
                setResponse(`Error: ${error.message}`);
            } else {
                setResponse('An unknown error occurred.');
            }
        }
    };

    return (
        <div className="flex flex-col items-center justify-center min-h-screen p-4">
            <h1 className="text-2xl font-bold mb-4">Test Auth Protection</h1>
            <button
                onClick={testAuth}
                className="bg-blue-500 text-white px-4 py-2 rounded"
            >
                Test Auth
            </button>
            {response && <p className="mt-4 text-center">{response}</p>}
        </div>
    );
}
