// src/pages/ThanksPage.tsx
import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

interface User {
  email: string;
  firstname: string;
  lastname: string;
}

export default function ThanksPage() {
  const location = useLocation();
  const navigate = useNavigate();
  const user: User = location.state?.user;

  if (!user) {
    return <div className="py-10 text-center">Không có dữ liệu. Vui lòng quay lại form.</div>;
  }

  return (
    <div className="min-h-screen px-4 py-12 bg-gradient-to-br from-green-50 to-teal-100">
      <div className="max-w-md p-8 mx-auto bg-white shadow-lg rounded-xl">
        <h1 className="mb-6 text-2xl font-bold text-green-800">Thanks for joining our email list</h1>

        <p className="mb-6 text-gray-700">Here is the information that you entered:</p>

        <div className="mb-8 space-y-3">
          <div className="flex justify-between">
            <span className="font-medium text-gray-600">Email:</span>
            <span className="text-gray-900">{user.email}</span>
          </div>
          <div className="flex justify-between">
            <span className="font-medium text-gray-600">First Name:</span>
            <span className="text-gray-900">{user.firstname}</span>
          </div>
          <div className="flex justify-between">
            <span className="font-medium text-gray-600">Last Name:</span>
            <span className="text-gray-900">{user.lastname}</span>
          </div>
        </div>

        <p className="mb-6 text-sm text-gray-600">
          To enter another email address, click on the Back button in your browser or the Return button shown below.
        </p>

        <button
          onClick={() => navigate('/join')}
          className="w-full py-3 font-semibold text-white transition bg-teal-600 rounded-lg hover:bg-teal-700"
        >
          Return
        </button>
      </div>
    </div>
  );
}