// src/pages/JoinEmailList.tsx
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';

interface User {
  email: string;
  firstname: string;
  lastname: string;
}

export default function JoinEmailList() {
  const [email, setEmail] = useState('');
  const [firstname, setFirstname] = useState('');
  const [lastname, setLastname] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email || !firstname || !lastname) {
      alert('Vui lòng nhập đầy đủ thông tin');
      return;
    }

    setLoading(true);

    try {
      // ĐÃ SỬA: DÙNG host.docker.internal
      const response = await fetch('http://localhost:8081/api/user', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, firstname, lastname }),
      });

      const data = await response.json();

      if (response.ok) {
        navigate('/thanks', {
          state: { user: data.data },
        });
      } else {
        alert(data.message || 'Lỗi khi gửi');
      }
    } catch (error) {
      alert('Không thể kết nối đến server');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen px-4 py-12 bg-gradient-to-br from-blue-50 to-indigo-100">
      <div className="max-w-md p-8 mx-auto bg-white shadow-lg rounded-xl">
        <h1 className="mb-2 text-2xl font-bold text-indigo-800">Join our email list</h1>
        <p className="mb-6 text-gray-600">
          To join our email list, enter your name and email address below.
        </p>

        <form onSubmit={handleSubmit} className="space-y-5">
          <div>
            <label className="block mb-1 text-sm font-medium text-gray-700">Email:</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
              placeholder="you@example.com"
            />
          </div>

          <div>
            <label className="block mb-1 text-sm font-medium text-gray-700">First Name:</label>
            <input
              type="text"
              value={firstname}
              onChange={(e) => setFirstname(e.target.value)}
              required
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
              placeholder="Nguyễn"
            />
          </div>

          <div>
            <label className="block mb-1 text-sm font-medium text-gray-700">Last Name:</label>
            <input
              type="text"
              value={lastname}
              onChange={(e) => setLastname(e.target.value)}
              required
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
              placeholder="Văn A"
            />
          </div>

          <div className="pt-4">
            <button
              type="submit"
              disabled={loading}
              className="w-full py-3 font-semibold text-white transition bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-60"
            >
              {loading ? 'Đang gửi...' : 'Join Now'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}