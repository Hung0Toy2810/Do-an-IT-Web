// src/pages/admin/CustomerManagement.tsx
import { useState, useEffect } from 'react';
import {
  Search,
  Lock,
  Unlock,
  Package,
  Eye,
  ChevronLeft,
  ChevronRight,
  X,
} from 'lucide-react';
import { format } from 'date-fns';
import { notify } from '@/utils/notify';
import { customerApi } from '@/services/customerApi';

interface Customer {
  id: string;
  customerName: string;
  phoneNumber: string;
  email: string;
  avtURL: string;
  status: boolean;
  totalInvoices: number;
}

interface Invoice {
  id: number;
  trackingCode: string;
  createdAt: string;
  status: string;
  totalAmount: number;
  paymentMethod: string;
  totalItems: number;
}

interface InvoiceDetail {
  id: number;
  trackingCode: string;
  createdAt: string;
  statusText: string;
  statusBadgeColor: string;
  totalAmount: number;
  paymentMethod: string;
  receiverName: string;
  receiverPhone: string;
  shippingAddress: {
    detailAddress: string;
    wardsName: string;
    districtName: string;
    provinceName: string;
  };
  carrier?: string;
  items: Array<{
    invoiceDetailId: number;
    productName: string;
    productSlug: string;
    variantSlug: string;
    quantity: number;
    unitPrice: number;
    originalPrice: number;
    attributes: Array<{ attributeName: string; attributeValue: string }>;
  }>;
  statusHistories: Array<{
    statusText: string;
    createdAt: string;
    note?: string;
  }>;
}

export default function CustomerManagement() {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | null>(null);
  const [selectedInvoice, setSelectedInvoice] = useState<InvoiceDetail | null>(null);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<boolean | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(false);
  const [showInvoices, setShowInvoices] = useState(false);

  const fetchCustomers = async () => {
    setLoading(true);
    try {
      const res = await customerApi.getCustomers(page, search, statusFilter);
      setCustomers(res.data.items || []);
      setTotalPages(Math.ceil(res.data.totalCount / 20));
    } catch (err: any) {
      notify.error(err.message || 'Lấy danh sách khách hàng thất bại');
    } finally {
      setLoading(false);
    }
  };

  const fetchCustomerInvoices = async (customerId: string) => {
    try {
      const res = await customerApi.getCustomerInvoices(customerId);
      setInvoices(res.data.invoices || []);
      setShowInvoices(true);
    } catch (err: any) {
      notify.error(err.message || 'Lấy đơn hàng thất bại');
    }
  };

  const fetchInvoiceDetail = async (invoiceId: number) => {
    try {
      const res = await customerApi.getInvoiceDetail(invoiceId);
      setSelectedInvoice(res.data);
    } catch (err: any) {
      notify.error(err.message || 'Không thể lấy chi tiết đơn hàng');
    }
  };

  const toggleCustomerStatus = async (id: string) => {
    try {
      await customerApi.toggleCustomerStatus(id);
      notify.success('Cập nhật trạng thái thành công');
      fetchCustomers();
    } catch (err: any) {
      notify.error(err.message || 'Cập nhật thất bại');
    }
  };

  useEffect(() => {
    fetchCustomers();
  }, [page, search, statusFilter]);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="p-6 bg-white border shadow-lg border-violet-100/50 rounded-2xl">
        <h1 className="mb-2 text-2xl font-bold text-transparent lg:text-3xl bg-gradient-to-r from-violet-700 to-violet-900 bg-clip-text">
          Quản lý khách hàng
        </h1>
        <p className="text-sm font-medium text-gray-600">
          Quản lý thông tin khách hàng, trạng thái tài khoản và lịch sử mua hàng
        </p>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap gap-4 p-6 bg-white border shadow-lg border-violet-100/50 rounded-2xl">
        <div className="flex-1 min-w-64">
          <div className="relative">
            <Search className="absolute w-5 h-5 text-gray-400 -translate-y-1/2 left-3 top-1/2" />
            <input
              type="text"
              placeholder="Tìm tên, số điện thoại, email..."
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
      </div>

      {/* Customers Table */}
      <div className="overflow-hidden bg-white border shadow-lg border-violet-100/50 rounded-2xl">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gradient-to-r from-violet-50 to-purple-50">
              <tr>
                <th className="px-6 py-4 text-sm font-semibold text-left text-violet-900">Khách hàng</th>
                <th className="px-6 py-4 text-sm font-semibold text-left text-violet-900">Liên hệ</th>
                <th className="px-6 py-4 text-sm font-semibold text-center text-violet-900">Tổng đơn</th>
                <th className="px-6 py-4 text-sm font-semibold text-center text-violet-900">Trạng thái</th>
                <th className="px-6 py-4 text-sm font-semibold text-center text-violet-900">Hành động</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {loading ? (
                <tr>
                  <td colSpan={5} className="py-16 text-center text-gray-500">Đang tải...</td>
                </tr>
              ) : customers.length === 0 ? (
                <tr>
                  <td colSpan={5} className="py-16 text-center text-gray-500">Không tìm thấy khách hàng nào</td>
                </tr>
              ) : (
                customers.map((customer) => (
                  <tr key={customer.id} className="transition-colors hover:bg-violet-50/30">
                    <td className="px-6 py-5">
                      <div className="flex items-center gap-4">
                        <img
                          src={customer.avtURL || '/default-avatar.png'}
                          alt={customer.customerName}
                          className="object-cover w-12 h-12 border-2 rounded-full border-violet-200"
                        />
                        <div>
                          <p className="font-semibold text-gray-900">{customer.customerName}</p>
                          <p className="text-sm text-gray-500">ID: {customer.id.slice(0, 8)}...</p>
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-5">
                      <div>
                        <p className="font-medium text-gray-900">{customer.phoneNumber}</p>
                        <p className="text-sm text-gray-500">{customer.email || 'Chưa có email'}</p>
                      </div>
                    </td>
                    <td className="px-6 py-5 text-center">
                      <span className="inline-flex items-center gap-2 px-4 py-2 font-semibold rounded-full bg-violet-100 text-violet-800">
                        <Package className="w-4 h-4" />
                        {customer.totalInvoices}
                      </span>
                    </td>
                    <td className="px-6 py-5 text-center">
                      <span className={`inline-flex items-center gap-2 px-4 py-2 rounded-full font-medium ${
                        customer.status ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                      }`}>
                        {customer.status ? 'Hoạt động' : 'Bị khóa'}
                      </span>
                    </td>
                    <td className="px-6 py-5 text-center">
                      <div className="flex items-center justify-center gap-3">
                        <button
                          onClick={() => {
                            setSelectedCustomer(customer);
                            fetchCustomerInvoices(customer.id);
                          }}
                          className="p-3 transition-all rounded-xl bg-violet-100 hover:bg-violet-200 text-violet-700"
                          title="Xem đơn hàng"
                        >
                          <Eye className="w-5 h-5" />
                        </button>
                        <button
                          onClick={() => toggleCustomerStatus(customer.id)}
                          className={`p-3 rounded-xl transition-all ${
                            customer.status
                              ? 'bg-red-100 hover:bg-red-200 text-red-700'
                              : 'bg-green-100 hover:bg-green-200 text-green-700'
                          }`}
                          title={customer.status ? 'Khóa tài khoản' : 'Mở khóa'}
                        >
                          {customer.status ? <Lock className="w-5 h-5" /> : <Unlock className="w-5 h-5" />}
                        </button>
                      </div>
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
            <p className="text-sm text-gray-600">Trang {page} / {totalPages}</p>
            <div className="flex gap-2">
              <button
                onClick={() => setPage(Math.max(1, page - 1))}
                disabled={page === 1}
                className="p-3 transition-all bg-white border border-gray-200 rounded-xl hover:bg-violet-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <ChevronLeft className="w-5 h-5" />
              </button>
              <button
                onClick={() => setPage(Math.min(totalPages, page + 1))}
                disabled={page === totalPages}
                className="p-3 transition-all bg-white border border-gray-200 rounded-xl hover:bg-violet-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <ChevronRight className="w-5 h-5" />
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Modal: Danh sách đơn hàng của khách – DẠNG BẢNG CHUYÊN NGHIỆP */}
      {showInvoices && selectedCustomer && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm">
          <div className="w-full max-w-7xl max-h-[95vh] m-4 bg-white rounded-3xl shadow-2xl overflow-hidden">
            {/* Header */}
            <div className="sticky top-0 z-10 flex items-center justify-between p-6 bg-gradient-to-r from-violet-700 to-violet-900">
              <div>
                <h2 className="text-2xl font-bold text-white">
                  Đơn hàng của {selectedCustomer.customerName}
                </h2>
                <p className="text-sm text-violet-100">
                  {selectedCustomer.phoneNumber} • Tổng: {invoices.length} đơn hàng
                </p>
              </div>
              <button
                onClick={() => {
                  setShowInvoices(false);
                  setSelectedInvoice(null);
                }}
                className="flex items-center justify-center text-white transition-all w-11 h-11 rounded-xl bg-white/20 hover:bg-white/30"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            {/* Table */}
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-violet-50">
                  <tr>
                    <th className="px-6 py-4 text-xs font-semibold text-left uppercase text-violet-900">ID</th>
                    <th className="px-6 py-4 text-xs font-semibold text-left uppercase text-violet-900">Mã vận đơn</th>
                    <th className="px-6 py-4 text-xs font-semibold text-left uppercase text-violet-900">Ngày đặt</th>
                    <th className="px-6 py-4 text-xs font-semibold text-center uppercase text-violet-900">Trạng thái</th>
                    <th className="px-6 py-4 text-xs font-semibold text-center uppercase text-violet-900">Sản phẩm</th>
                    <th className="px-6 py-4 text-xs font-semibold text-right uppercase text-violet-900">Tổng tiền</th>
                    <th className="px-6 py-4 text-xs font-semibold text-center uppercase text-violet-900">Hành động</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {invoices.length === 0 ? (
                    <tr>
                      <td colSpan={7} className="py-16 text-center text-gray-500">
                        Khách hàng chưa có đơn hàng nào
                      </td>
                    </tr>
                  ) : (
                    invoices.map((invoice) => (
                      <tr
                        key={invoice.id}
                        className="transition-colors cursor-pointer hover:bg-violet-50/50"
                        onClick={() => fetchInvoiceDetail(invoice.id)}
                      >
                        <td className="px-6 py-4 text-sm font-medium text-gray-600">#{invoice.id}</td>
                        <td className="px-6 py-4">
                          <p className="font-semibold text-gray-900">#{invoice.trackingCode}</p>
                        </td>
                        <td className="px-6 py-4 text-sm text-gray-600">
                          {format(new Date(invoice.createdAt), 'dd/MM/yyyy HH:mm')}
                        </td>
                        <td className="px-6 py-4 text-center">
                          <span className="px-3 py-1 text-xs font-medium text-gray-800 bg-gray-100 rounded-full">
                            {invoice.status}
                          </span>
                        </td>
                        <td className="px-6 py-4 font-medium text-center text-gray-700">
                          {invoice.totalItems}
                        </td>
                        <td className="px-6 py-4 font-bold text-right text-violet-700">
                          {invoice.totalAmount.toLocaleString('vi-VN')}₫
                        </td>
                        <td className="px-6 py-4 text-center">
                          <button
                            onClick={(e) => {
                              e.stopPropagation();
                              fetchInvoiceDetail(invoice.id);
                            }}
                            className="p-2.5 rounded-lg bg-violet-100 hover:bg-violet-200 text-violet-700 transition-all"
                          >
                            <Eye className="w-4 h-4" />
                          </button>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* Modal: Chi tiết đơn hàng */}
      {selectedInvoice && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm">
          <div className="w-full max-w-5xl max-h-[95vh] m-4 overflow-y-auto bg-white rounded-3xl shadow-2xl">
            <div className="sticky top-0 z-10 flex items-center justify-between p-6 bg-gradient-to-r from-violet-700 to-violet-900 rounded-t-3xl">
              <div>
                <h2 className="text-2xl font-bold text-white">
                  Đơn hàng #{selectedInvoice.trackingCode}
                </h2>
                <p className="mt-1 text-sm font-medium text-violet-100">
                  ID: {selectedInvoice.id} • {format(new Date(selectedInvoice.createdAt), 'dd/MM/yyyy HH:mm')}
                </p>
              </div>
              <button
                onClick={() => setSelectedInvoice(null)}
                className="flex items-center justify-center text-white transition-all w-11 h-11 rounded-xl bg-white/20 hover:bg-white/30"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            <div className="p-8 space-y-8">
              <div className="grid grid-cols-1 gap-8 md:grid-cols-2">
                <div>
                  <h3 className="mb-4 text-lg font-bold text-violet-900">Thông tin nhận hàng</h3>
                  <table className="w-full text-sm">
                    <tbody className="divide-y divide-gray-200">
                      <tr>
                        <td className="py-2 font-medium text-gray-600">Người nhận</td>
                        <td className="py-2 text-gray-900">{selectedInvoice.receiverName}</td>
                      </tr>
                      <tr>
                        <td className="py-2 font-medium text-gray-600">Số điện thoại</td>
                        <td className="py-2 text-gray-900">{selectedInvoice.receiverPhone}</td>
                      </tr>
                      <tr>
                        <td className="py-2 font-medium text-gray-600">Địa chỉ giao</td>
                        <td className="py-2 text-gray-900">
                          {selectedInvoice.shippingAddress.detailAddress}, {selectedInvoice.shippingAddress.wardsName}, {selectedInvoice.shippingAddress.districtName}, {selectedInvoice.shippingAddress.provinceName}
                        </td>
                      </tr>
                      {selectedInvoice.carrier && (
                        <tr>
                          <td className="py-2 font-medium text-gray-600">Vận chuyển</td>
                          <td className="py-2 text-gray-900 uppercase">{selectedInvoice.carrier}</td>
                        </tr>
                      )}
                    </tbody>
                  </table>
                </div>

                <div>
                  <h3 className="mb-4 text-lg font-bold text-violet-900">Thông tin thanh toán</h3>
                  <table className="w-full text-sm">
                    <tbody className="divide-y divide-gray-200">
                      <tr>
                        <td className="py-2 font-medium text-gray-600">Phương thức</td>
                        <td className="py-2 font-semibold text-gray-900">{selectedInvoice.paymentMethod}</td>
                      </tr>
                      <tr>
                        <td className="py-2 font-medium text-gray-600">Trạng thái</td>
                        <td className="py-2">
                          <span className={`px-3 py-1 text-xs font-medium rounded-full ${
                            selectedInvoice.statusBadgeColor === 'success' ? 'bg-green-100 text-green-800' :
                            selectedInvoice.statusBadgeColor === 'warning' ? 'bg-yellow-100 text-yellow-800' :
                            'bg-gray-100 text-gray-800'
                          }`}>
                            {selectedInvoice.statusText}
                          </span>
                        </td>
                      </tr>
                      <tr>
                        <td className="py-2 font-medium text-gray-600">Tổng tiền</td>
                        <td className="py-2 text-xl font-bold text-violet-700">
                          {selectedInvoice.totalAmount.toLocaleString('vi-VN')}₫
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>

              <div>
                <h3 className="mb-4 text-lg font-bold text-violet-900">Sản phẩm trong đơn</h3>
                <div className="overflow-x-auto border rounded-xl border-violet-100">
                  <table className="w-full">
                    <thead className="bg-violet-50">
                      <tr>
                        <th className="px-4 py-3 text-xs font-semibold text-left uppercase text-violet-900">STT</th>
                        <th className="px-4 py-3 text-xs font-semibold text-left uppercase text-violet-900">Sản phẩm</th>
                        <th className="px-4 py-3 text-xs font-semibold text-center uppercase text-violet-900">Thuộc tính</th>
                        <th className="px-4 py-3 text-xs font-semibold text-center uppercase text-violet-900">SL</th>
                        <th className="px-4 py-3 text-xs font-semibold text-right uppercase text-violet-900">Đơn giá</th>
                        <th className="px-4 py-3 text-xs font-semibold text-right uppercase text-violet-900">Thành tiền</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-100">
                      {selectedInvoice.items.map((item, index) => (
                        <tr key={item.invoiceDetailId} className="hover:bg-violet-50/50">
                          <td className="px-4 py-4 text-sm text-center">{index + 1}</td>
                          <td className="px-4 py-4">
                            <div>
                              <p className="font-medium text-gray-900">{item.productName}</p>
                              <p className="text-xs text-gray-500">Slug: {item.productSlug}</p>
                            </div>
                          </td>
                          <td className="px-4 py-4 text-xs text-center">
                            {item.attributes.map((attr, i) => (
                              <div key={i}>
                                {attr.attributeName}: <span className="font-medium">{attr.attributeValue}</span>
                              </div>
                            ))}
                          </td>
                          <td className="px-4 py-4 font-semibold text-center">{item.quantity}</td>
                          <td className="px-4 py-4 text-right text-gray-700">
                            {item.unitPrice.toLocaleString('vi-VN')}₫
                          </td>
                          <td className="px-4 py-4 font-bold text-right text-violet-700">
                            {(item.quantity * item.unitPrice).toLocaleString('vi-VN')}₫
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>

              <div>
                <h3 className="mb-4 text-lg font-bold text-violet-900">Lịch sử trạng thái</h3>
                <div className="space-y-3">
                  {selectedInvoice.statusHistories.map((h, i) => (
                    <div key={i} className="flex items-center gap-4 p-4 bg-gray-50 rounded-xl">
                      <div className="flex items-center justify-center flex-shrink-0 w-10 h-10 text-sm font-bold text-white rounded-full bg-violet-600">
                        {i + 1}
                      </div>
                      <div className="flex-1">
                        <p className="font-semibold text-gray-900">{h.statusText}</p>
                        <p className="text-sm text-gray-500">
                          {format(new Date(h.createdAt), 'dd/MM/yyyy HH:mm:ss')}
                          {h.note && <span className="ml-2 italic">– {h.note}</span>}
                        </p>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}