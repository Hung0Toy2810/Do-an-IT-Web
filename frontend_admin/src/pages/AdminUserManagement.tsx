// src/pages/admin/AdminUserManagement.tsx
import { useState, useEffect } from 'react';
import { Search, Lock, Unlock, Shield, ChevronLeft, ChevronRight, UserPlus } from 'lucide-react';
import { notify } from '@/utils/notify';

interface Admin {
  id: string;
  username: string;
  status: boolean;
}

export default function AdminUserManagement() {
  const [admins, setAdmins] = useState<Admin[]>([]);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<boolean | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(false);

  const API_BASE = 'http://localhost:5067';

  const authFetch = async (url: string, options: RequestInit = {}) => {
    const token = localStorage.getItem('auth_token') || document.cookie
      .split('; ')
      .find(row => row.startsWith('auth_token='))
      ?.split('=')[1];

    const response = await fetch(`${API_BASE}${url}`, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...(token && { Authorization: `Bearer ${token}` }),
        ...options.headers,
      },
    });

    if (response.status === 401 || response.status === 403) {
      notify.error('Phiên đăng nhập hết hạn');
      setTimeout(() => { window.location.href = '/admin-login'; }, 1500);
      throw new Error('Unauthorized');
    }

    if (!response.ok) {
      const err = await response.json().catch(() => ({}));
      throw new Error(err.message || 'Lỗi server');
    }

    return response.json();
  };

  const fetchAdmins = async () => {
    setLoading(true);
    try {
      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: '20',
        ...(search && { search }),
        ...(statusFilter !== null && { status: statusFilter.toString() }),
      });

      const res = await authFetch(`/api/admin/admins?${params}`);
      setAdmins(res.data.items || []);
      setTotalPages(res.data.totalPages || 1);
    } catch (err: any) {
      notify.error(err.message || 'Không thể tải danh sách quản trị viên');
    } finally {
      setLoading(false);
    }
  };

  const toggleAdminStatus = async (id: string) => {
    if (confirm('Bạn có chắc muốn thay đổi trạng thái tài khoản này?')) {
      try {
        await authFetch(`/api/admin/admins/${id}/toggle-status`, { method: 'POST' });
        notify.success('Cập nhật trạng thái thành công');
        fetchAdmins();
      } catch (err: any) {
        notify.error(err.message || 'Cập nhật thất bại');
      }
    }
  };

  useEffect(() => {
    fetchAdmins();
  }, [page, search, statusFilter]);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="p-6 bg-white border shadow-lg border-violet-100/50 rounded-2xl">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="mb-2 text-2xl font-bold text-transparent lg:text-3xl bg-gradient-to-r from-violet-700 to-violet-900 bg-clip-text">
              Quản lý quản trị viên
            </h1>
            <p className="text-sm font-medium text-gray-600">
              Quản lý tài khoản admin, phân quyền và bảo mật hệ thống
            </p>
          </div>
          <div className="flex items-center justify-center w-16 h-16 shadow-xl bg-gradient-to-br from-violet-600 to-violet-800 rounded-2xl">
            <Shield className="text-white w-9 h-9" />
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap gap-4 p-6 bg-white border shadow-lg border-violet-100/50 rounded-2xl">
        <div className="flex-1 min-w-64">
          <div className="relative">
            <Search className="absolute w-5 h-5 text-gray-400 -translate-y-1/2 left-3 top-1/2" />
            <input
              type="text"
              placeholder="Tìm theo tên đăng nhập..."
              value={search}
              onChange={(e) => {
                setSearch(e.target.value);
                setPage(1);
              }}
              className="w-full py-3 pl-10 pr-4 transition-all border border-gray-200 outline-none rounded-xl focus:ring-2 focus:ring-violet-500 focus:border-violet-500"
            />
          </div>
        </div>

        <select
          value={statusFilter === null ? '' : statusFilter.toString()}
          onChange={(e) => {
            setStatusFilter(e.target.value === '' ? null : e.target.value === 'true');
            setPage(1);
          }}
          className="px-5 py-3 border border-gray-200 outline-none rounded-xl focus:ring-2 focus:ring-violet-500"
        >
          <option value="">Tất cả trạng thái</option>
          <option value="true">Đang hoạt động</option>
          <option value="false">Bị khóa</option>
        </select>

        <button className="flex items-center gap-2 px-6 py-3 font-semibold text-white transition-all shadow-lg rounded-xl bg-gradient-to-r from-violet-600 to-violet-800 hover:from-violet-700 hover:to-violet-900 hover:shadow-violet-500/25">
          <UserPlus className="w-5 h-5" />
          Thêm quản trị viên
        </button>
      </div>

      {/* Admins Table – ĐÃ BỎ CỘT NGÀY TẠO */}
      <div className="overflow-hidden bg-white border shadow-lg border-violet-100/50 rounded-2xl">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gradient-to-r from-violet-50 to-purple-50">
              <tr>
                <th className="px-6 py-4 text-sm font-semibold text-left text-violet-900">Quản trị viên</th>
                <th className="px-6 py-4 text-sm font-semibold text-left text-violet-900">ID</th>
                <th className="px-6 py-4 text-sm font-semibold text-center text-violet-900">Trạng thái</th>
                <th className="px-6 py-4 text-sm font-semibold text-center text-violet-900">Hành động</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {loading ? (
                <tr>
                  <td colSpan={4} className="py-16 text-center text-gray-500">Đang tải...</td>
                </tr>
              ) : admins.length === 0 ? (
                <tr>
                  <td colSpan={4} className="py-16 text-center text-gray-500">
                    <div className="flex flex-col items-center">
                      <Shield className="w-16 h-16 mb-4 text-gray-300" />
                      <p>Chưa có quản trị viên nào</p>
                    </div>
                  </td>
                </tr>
              ) : (
                admins.map((admin) => (
                  <tr key={admin.id} className="transition-colors hover:bg-violet-50/30">
                    <td className="px-6 py-5">
                      <div className="flex items-center gap-4">
                        <div className="flex items-center justify-center w-12 h-12 text-xl font-bold text-white shadow-lg rounded-xl bg-gradient-to-br from-violet-600 to-violet-800">
                          {admin.username.charAt(0).toUpperCase()}
                        </div>
                        <div>
                          <p className="font-semibold text-gray-900">{admin.username}</p>
                          <p className="text-sm text-gray-500">Quản trị viên</p>
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-5">
                      <code className="px-3 py-1 font-mono text-xs rounded-lg text-violet-700 bg-violet-100">
                        {admin.id.slice(0, 8)}...
                      </code>
                    </td>
                    <td className="px-6 py-5 text-center">
                      <span className={`inline-flex items-center gap-2 px-4 py-2 rounded-full font-medium ${
                        admin.status
                          ? 'bg-green-100 text-green-800'
                          : 'bg-red-100 text-red-800'
                      }`}>
                        {admin.status ? 'Hoạt động' : 'Bị khóa'}
                      </span>
                    </td>
                    <td className="px-6 py-5 text-center">
                      <button
                        onClick={() => toggleAdminStatus(admin.id)}
                        className={`p-3 rounded-lg transition-all ${
                          admin.status
                            ? 'bg-red-100 hover:bg-red-200 text-red-700'
                            : 'bg-green-100 hover:bg-green-200 text-green-700'
                        }`}
                        title={admin.status ? 'Khóa tài khoản' : 'Mở khóa'}
                      >
                        {admin.status ? <Lock className="w-5 h-5" /> : <Unlock className="w-5 h-5" />}
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="flex items-center justify-between px-6 py-4 border-t border-gray-100">
            <p className="text-sm text-gray-600">
              Trang {page} / {totalPages}
            </p>
            <div className="flex gap-2">
              <button
                onClick={() => setPage(Math.max(1, page - 1))}
                disabled={page === 1}
                className="p-3 transition-all bg-white border border-gray-200 rounded-lg hover:bg-violet-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <ChevronLeft className="w-5 h-5" />
              </button>
              <button
                onClick={() => setPage(Math.min(totalPages, page + 1))}
                disabled={page === totalPages}
                className="p-3 transition-all bg-white border border-gray-200 rounded-lg hover:bg-violet-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <ChevronRight className="w-5 h-5" />
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}